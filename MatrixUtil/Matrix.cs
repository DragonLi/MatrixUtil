using System;

namespace MatrixUtil
{
    public class Matrix
    {
        private readonly float[,] rowVec;
        private readonly int rowCount;
        private readonly int columnCount;

        public static void RangeCheck(int val, int maxExclusive)
        {
            if (val < 0 || val >= maxExclusive)
                throw new Exception("out of range");
        }

        //对横坐标或者纵坐标的算法是一样的:center是卷积矩阵的中点位置,len是卷积矩阵的宽度（长度）,max是屏幕坐标的最大值
        public static (int, int, int) GetRange(int center, int len, int max)
        {
            var half = len / 2;
            var otherStart = center - half;//这一步计算出卷积矩阵的起始位置
            var otherEnd = otherStart + len;//这一步计算卷积矩阵的终结位置
            var start = otherStart < 0 ? 0 : otherStart;//如果起始位置小于0,要截取
            otherStart = start - otherStart;//这一步计算出截取的个数，截取个数刚好是卷积矩阵对应需要开始的位置，在这个位置之前的数据被截取，是不需要更新到全局分子/分母矩阵中的

            var end = otherEnd > max ? max : otherEnd;//这一步控制卷积矩阵终结位置不能超过屏幕的最大坐标值
            //otherEnd = len - (otherEnd - end);
            //otherEnd - otherStart = len -(center - half +len - end) - (start -(center - half))
            //=half-center + end-start +(center-half) = end - start
            return (start, end, otherStart);
        }

        public Matrix(int rowCount, int columnCount)
        {
            this.rowCount = rowCount;
            this.columnCount = columnCount;
            //初始化的时候矩阵每个点都为0
            rowVec = new float[rowCount, columnCount];
        }

        public Matrix MultiplyScalar(float val)
        {
            var r = new Matrix(rowCount, columnCount);
            for (var i = 0; i < rowCount; i++)
            {
                for (var j = 0; j < columnCount; j++)
                {
                    //逐个点进行标量乘法
                    r.rowVec[i, j] = rowVec[i, j] * val;
                }
            }

            return r;
        }

        public void PartialAdd(int centerRow, int centerCol, Matrix other)
        {
            //检查下标范围是否越界
            RangeCheck(centerRow, rowCount);
            RangeCheck(centerCol, columnCount);
            var width = other.columnCount;
            var height = other.rowCount;
            //计算卷积矩阵落在全局屏幕上的有效范围
            var (startRow, lastRow, otherRowStart) = GetRange(centerRow, height, rowCount);
            var (startCol, lastCol, otherColStart) = GetRange(centerCol, width, columnCount);

            for (int i = startRow, k = otherRowStart; i < lastRow; ++k, ++i)
            {
                for (int j = startCol, l = otherColStart; j < lastCol; ++l, ++j)
                {
                    //按照找到的全局矩阵开始更新的位置，以及对应卷积矩阵开始的位置，逐个点对齐进行更新
                    rowVec[i, j] += other.rowVec[k, l];
                }
            }
        }

        public void DivideBy(Matrix other)
        {
            if (rowCount != other.rowCount || columnCount != other.columnCount)
                throw new Exception("size not match!");
            for (var i = 0; i < rowCount; i++)
            {
                for (var j = 0; j < columnCount; j++)
                {
                    //test other.rowVec[i, j] == 0
                    rowVec[i, j] = Math.Abs(other.rowVec[i, j]) < float.Epsilon ? 1 : rowVec[i, j] / other.rowVec[i, j];
                }
            }
        }

        public float this[int rowInd, int colInd]
        {
            get => rowVec[rowInd, colInd];
            set => rowVec[rowInd, colInd] = value;
        }

        public int RowCount => rowCount;

        public int ColumnCount => columnCount;

        public void DebugPrint()
        {
            for (var i = 0; i < rowCount; i++)
            {
                for (var j = 0; j < columnCount - 1; j++)
                {
                    Console.Out.Write(rowVec[i, j]);
                    Console.Out.Write(", ");
                }

                Console.Out.WriteLine(rowVec[i, columnCount - 1]);
            }
        }
    }
}