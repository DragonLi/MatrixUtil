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

        public static (int, int, int) GetRange(int center, int len, int max)
        {
            var half = len / 2;
            var otherStart = center - half;
            var otherEnd = otherStart + len;
            var start = otherStart < 0 ? 0 : otherStart;
            otherStart = start - otherStart;

            var end = otherEnd > max ? max : otherEnd;
            //otherEnd = len - (otherEnd - end);
            //otherEnd - otherStart = len -(center - half +len - end) - (start -(center - half))
            //=half-center + end-start +(center-half) = end - start
            return (start, end, otherStart);
        }

        public Matrix(int rowCount, int columnCount)
        {
            this.rowCount = rowCount;
            this.columnCount = columnCount;
            rowVec = new float[rowCount, columnCount];
        }

        public Matrix MultiplyScalar(float val)
        {
            var r = new Matrix(rowCount, columnCount);
            for (var i = 0; i < rowCount; i++)
            {
                for (var j = 0; j < columnCount; j++)
                {
                    r.rowVec[i, j] = rowVec[i, j] * val;
                }
            }

            return r;
        }

        public void PartialAdd(int centerRow, int centerCol, Matrix other)
        {
            RangeCheck(centerRow, rowCount);
            RangeCheck(centerCol, columnCount);
            var width = other.columnCount;
            var height = other.rowCount;
            var (startRow, lastRow, otherRowStart) = GetRange(centerRow, height, rowCount);
            var (startCol, lastCol, otherColStart) = GetRange(centerCol, width, columnCount);

            for (int i = startRow, k = otherRowStart; i < lastRow; ++k, ++i)
            {
                for (int j = startCol, l = otherColStart; j < lastCol; ++l, ++j)
                {
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