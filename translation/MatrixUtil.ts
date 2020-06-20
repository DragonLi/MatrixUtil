module MatrixUtil {
    
    export class Matrix {
        
        private rowVec: number[][];
        
        private rowCount: number;
        
        private columnCount: number;
        
        public constructor (rowCount: number, columnCount: number) {
            this.rowCount = rowCount;
            this.columnCount = columnCount;
            this.rowVec = new Array(this.rowCount);
            for (let i = 0; (i < this.rowCount); ++i) {
                this.rowVec[i] = new Array(this.columnCount);
            }
        }
        
        public MultiplyScalar(val: number): Matrix {
            let r = new Matrix(this.rowCount, this.columnCount);
            for (let i = 0; i < this.rowCount; ++i) {
                let thisRow = this.rowVec[i];
                let otherRow = r.rowVec[i];
                for (let j = 0; j < this.columnCount; ++j) {
                    otherRow[j] = thisRow[j] * val;
                }
            }
            return r;
        }
        
        public PartialAdd(centerRow: number, centerCol: number, other: Matrix) {
            PackedMatrix.RangeCheck(centerRow, this.rowCount);
            PackedMatrix.RangeCheck(centerCol, this.columnCount);
            let width = other.columnCount;
            let height = other.rowCount;
            let [startRow, lastRow, otherRowStart] = PackedMatrix.GetRange(centerRow, height, this.rowCount);
            let [startCol, lastCol, otherColStart] = PackedMatrix.GetRange(centerCol, width, this.columnCount);
            for (let i = startRow, k = otherRowStart; i < lastRow; ++k, ++i) {
                let thisRow = this.rowVec[i];
                let otherRow = other.rowVec[k];
                for (let j = startCol, l = otherColStart; j < lastCol; ++l, ++j) {
                    thisRow[j] += otherRow[l];
                }
            }
        }
        
        public DivideBy(other: Matrix) {
            if (((this.rowCount != other.rowCount) 
                        || (this.columnCount != other.columnCount))) {
                throw new Error("size not match!");
            }
            
            for (let i = 0; (i < this.rowCount); i++) {
                let thisRow = this.rowVec[i];
                let otherRow = other.rowVec[i];
                for (let j = 0; (j < this.columnCount); j++) {
                    // test other.rowVec[i, j] == 0
                    thisRow[j] = Math.abs(otherRow[j]) < 1E-45 ? 1 : thisRow[j] / otherRow[j];
                }
            }
        }
    }

    export class PackedMatrix {
        
        private linearData: number[];
        
        private rowCount: number;
        
        private columnCount: number;

        public static RangeCheck(val: number, maxExclusive: number) {
            if (((val < 0)  || (val >= maxExclusive))) {
                throw new Error("out of range");
            }
        }
        
        public static GetRange(center: number, len: number, max: number): [number,number,number] {
            let half = (len / 2);
            let otherStart = (center - half);
            let otherEnd = (otherStart + len);
            let start: number =  otherStart < 0 ? 0 : otherStart;
            
            otherStart = (start - otherStart);
            let end: number = otherEnd > max ? max : otherEnd;            
            return [start, end, otherStart];
        }

        public constructor (rowCount: number, columnCount: number) {
            this.rowCount = rowCount;
            this.columnCount = columnCount;
            this.linearData = new Array((this.rowCount * this.columnCount));
        }
        
        public MultiplyScalar(val: number): PackedMatrix {
            let r = new PackedMatrix(this.rowCount, this.columnCount);
            for (let i=0, len: number = this.linearData.length; (i < len); i++) {
                r.linearData[i] = (this.linearData[i] * val);
            }
            
            return r;
        }
        
        public PartialAdd(centerRow: number, centerCol: number, other: PackedMatrix) {
            PackedMatrix.RangeCheck(centerRow, this.rowCount);
            PackedMatrix.RangeCheck(centerCol, this.columnCount);
            let width = other.columnCount;
            let height = other.rowCount;
            let [startRow, lastRow, otherRowStart] = PackedMatrix.GetRange(centerRow, height, this.rowCount);
            let [startCol, lastCol, otherColStart] = PackedMatrix.GetRange(centerCol, width, this.columnCount);

            for (let i = startRow * this.columnCount + startCol,
                len = lastRow * this.columnCount + startCol,
                k = otherRowStart * width + otherColStart,
                delta = lastCol - startCol;
                i < len;
                i += this.columnCount, k += width)
            {
                for (let j = i, updateLen = i + delta, l = k; j < updateLen; ++j, ++l)
                {
                    this.linearData[j] += other.linearData[l];
                }
            }
        }
        
        public DivideBy(other: PackedMatrix) {
            if (((this.rowCount != other.rowCount) 
                        || (this.columnCount != other.columnCount))) {
                throw new Error("size not match!");
            }
            
            for (let i = 0, len = this.linearData.length; i < len; i++)
            {
                //test other.linearData[i] == 0
                this.linearData[i] = Math.abs(other.linearData[i]) < 1E-45 ? 1 : this.linearData[i] / other.linearData[i];
            }
            
        }
    }
}