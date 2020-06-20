using System;

namespace MatrixUtil
{
    public class PackedMatrix
    {
        private readonly float[] linearData;
        private readonly int rowCount;
        private readonly int columnCount;

        public PackedMatrix(int rowCount, int columnCount)
        {
            this.rowCount = rowCount;
            this.columnCount = columnCount;
            linearData = new float[rowCount * columnCount];
        }

        public PackedMatrix MultiplyScalar(float val)
        {
            var r = new PackedMatrix(rowCount, columnCount);
            for (int i = 0, len = linearData.Length; i < len; i++)
            {
                r.linearData[i] = linearData[i] * val;
            }

            return r;
        }

        public void PartialAdd(int centerRow, int centerCol, PackedMatrix other)
        {
            Matrix.RangeCheck(centerRow, rowCount);
            Matrix.RangeCheck(centerCol, columnCount);
            var width = other.columnCount;
            var height = other.rowCount;
            var (startRow, lastRow, otherRowStart) = Matrix.GetRange(centerRow, height, rowCount);
            var (startCol, lastCol, otherColStart) = Matrix.GetRange(centerCol, width, columnCount);


            for (int i = startRow * columnCount + startCol,
                len = lastRow * columnCount + startCol,
                k = otherRowStart * width + otherColStart,
                delta = lastCol - startCol;
                i < len;
                i += columnCount, k += width)
            {
                for (int j = i, updateLen = i + delta, l = k; j < updateLen; ++j, ++l)
                {
                    linearData[j] += other.linearData[l];
                }
            }


            // slow implementation
            /*
            for (int i = startRow,k=otherRowStart; i < lastRow; ++k,++i)
            {
                for (int j = startCol,l=otherColStart; j < lastCol; ++l,++j)
                {
                    this[i, j] += other[k, l];
                }
            }
            */
        }

        public void DivideBy(PackedMatrix other)
        {
            if (rowCount != other.rowCount || columnCount != other.columnCount)
                throw new Exception("size not match!");
            for (int i = 0, len = linearData.Length; i < len; i++)
            {
                //test other.linearData[i] == 0
                linearData[i] = Math.Abs(other.linearData[i]) < float.Epsilon ? 1 : linearData[i] / other.linearData[i];
            }
        }

        public float this[int rowInd, int colInd]
        {
            get => linearData[rowInd * columnCount + colInd];
            set => linearData[rowInd * columnCount + colInd] = value;
        }

        public int RowCount => rowCount;

        public int ColumnCount => columnCount;

        public void DebugPrint()
        {
            for (var i = 0; i < rowCount; i++)
            {
                for (var j = 0; j < columnCount - 1; j++)
                {
                    Console.Out.Write(this[i, j]);
                    Console.Out.Write(", ");
                }

                Console.Out.WriteLine(this[i, columnCount - 1]);
            }
        }
    }
}