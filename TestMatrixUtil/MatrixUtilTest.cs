using System;
using System.Collections.Generic;

namespace MatrixUtil
{
    public class MatrixUtilTest
    {
        const int maxWidth = 6;
        const int maxHeight = 6;
        const float scoreProbability = 0.3f;
        const int distancePower = 5;
        const float power = distancePower / 2.0f;
        const int kernelSize = 3;

        public static void TestMatrix()
        {
            //提前计算卷积用到矩阵（权重矩阵）
            var kernel = PrepareKernel();

            var width = maxWidth;
            var height = maxHeight;
            //准备评分点 每个屏幕坐标有30%的机会填上一个评分
            var scoreList = PrepareScoreList(height, width);

            TestMatrix(height, width, scoreList, kernel);
        }

        private static Matrix TestMatrix(int height, int width, List<(int, int, float)> scoreList, Matrix kernel)
        {
            //全局的分子矩阵（对应屏幕大小）
            var nominatorAll = new Matrix(height, width);
            //全局的分母矩阵（对应屏幕大小）
            var denominatorAll = new Matrix(height, width);
            foreach (var (rowInd, colInd, score) in scoreList)
            {
                //计算分子的增量：就是权重矩阵与评分的标量乘法
                var nominator = kernel.MultiplyScalar(score);
                //分母增量就是权重矩阵自身
                var denominator = kernel;
                //把分子增量加到全局的分子矩阵中
                nominatorAll.PartialAdd(rowInd, colInd, nominator);
                //把分母增量加到全局的分母矩阵中
                denominatorAll.PartialAdd(rowInd, colInd, denominator);
            }

            Console.WriteLine("nominator All:");
            nominatorAll.DebugPrint();
            Console.WriteLine("denominator All:");
            denominatorAll.DebugPrint();

            var idwWithKernel = nominatorAll;
            //全局分子矩阵除以全局分母矩阵，得到每个点的插值
            idwWithKernel.DivideBy(denominatorAll);
            Console.WriteLine("idwWithKernel:");
            idwWithKernel.DebugPrint();
            Console.WriteLine();
            return idwWithKernel;
        }

        public static void TestPackedMatrix()
        {
            var kernel = PreparePackedKernel();

            var width = maxWidth;
            var height = maxHeight;
            var scoreList = PrepareScoreList(height, width);

            TestPackedMatrix(height, width, scoreList, kernel);
        }

        private static PackedMatrix TestPackedMatrix(int height, int width, List<(int, int, float)> scoreList, PackedMatrix kernel)
        {
            var nominatorAll = new PackedMatrix(height, width);
            var denominatorAll = new PackedMatrix(height, width);
            foreach (var (rowInd, colInd, score) in scoreList)
            {
                var nominator = kernel.MultiplyScalar(score);
                var denominator = kernel;
                nominatorAll.PartialAdd(rowInd, colInd, nominator);
                denominatorAll.PartialAdd(rowInd, colInd, denominator);
            }

            Console.WriteLine("nominator packed All:");
            nominatorAll.DebugPrint();
            Console.WriteLine("denominator packed All:");
            denominatorAll.DebugPrint();
            
            var idwWithKernel = nominatorAll;
            idwWithKernel.DivideBy(denominatorAll);
            Console.WriteLine("idwWithPackedKernel:");
            idwWithKernel.DebugPrint();
            Console.WriteLine();
            return idwWithKernel;
        }

        public static void TestAll()
        {
            var kernel = PrepareKernel();

            var width = maxWidth;
            var height = maxHeight;
            var scoreList = PrepareScoreList(height, width);

            var idwWithKernel = TestMatrix(height, width, scoreList, kernel);

            var packedKernel = ConvertKernelRepresentation(kernel);
            var idwWithPackedKernel = TestPackedMatrix(height, width, scoreList, packedKernel);

            Compare(idwWithKernel, idwWithPackedKernel);
        }

        private static List<(int, int, float)> PrepareScoreList(int height, int width)
        {
            var r = new Random();
            Console.WriteLine("prepare score list:");
            var scoreList = new List<(int, int, float)>();
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    if (!(r.NextDouble() < scoreProbability)) continue;
                    var tuple = (i, j, r.Next(1, 100));
                    scoreList.Add(tuple);
                    Console.WriteLine(tuple);
                }
            }

            Console.WriteLine("score list size: "+scoreList.Count);
            return scoreList;
        }

        private static Matrix PrepareKernel()
        {
            var kernel = new Matrix(kernelSize, kernelSize);
            for (int i = 0, half = kernelSize / 2; i < kernelSize; i++)
            {
                for (var j = 0; j < kernelSize; j++)
                {
                    //每个点的权重是距离倒数再五次方 相当于距离平方再算2.5次方
                    var t = (float) (Math.Pow(Math.Abs(i - half), power) + Math.Pow(Math.Abs(j - half), power));
                    //test t == 0
                    kernel[i, j] = t < float.Epsilon ? 1 : 1/t;
                }
            }
            Console.WriteLine("kernel:");
            kernel.DebugPrint();
            return kernel;
        }

        private static PackedMatrix PreparePackedKernel()
        {
            var kernel = new PackedMatrix(kernelSize, kernelSize);
            for (int i = 0, half = kernelSize / 2; i < kernelSize; i++)
            {
                for (var j = 0; j < kernelSize; j++)
                {
                    var t = (float) (Math.Pow(Math.Abs(i - half), power) + Math.Pow(Math.Abs(j - half), power));
                    //test t == 0
                    kernel[i, j] = t < float.Epsilon ? 1 : 1/t;
                }
            }

            Console.WriteLine("packed kernel:");
            kernel.DebugPrint();
            return kernel;
        }

        private static PackedMatrix ConvertKernelRepresentation(Matrix k)
        {
            var r = new PackedMatrix(k.RowCount,k.ColumnCount);
            for (int i = 0,rLen=k.RowCount; i <rLen ; i++)
            {
                for (int j = 0,cLen=k.ColumnCount; j < cLen; j++)
                {
                    r[i, j] = k[i, j];
                }
            }

            Console.WriteLine("packed kernel:");
            r.DebugPrint();
            return r;
        }

        private static void Compare(Matrix k, PackedMatrix r)
        {
            for (int i = 0,rLen=k.RowCount; i <rLen ; i++)
            {
                for (int j = 0,cLen=k.ColumnCount; j < cLen; j++)
                {
                    if (Math.Abs(r[i, j] - k[i, j]) > float.Epsilon)
                        throw new Exception("bug");
                }
            }
            Console.WriteLine("Compare successfully passed");
        }
    }
}