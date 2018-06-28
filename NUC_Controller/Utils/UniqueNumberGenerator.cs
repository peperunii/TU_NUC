namespace NUC_Controller.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class UNG
    {
        private UInt64 min;
        private UInt64 max;
        private UInt64 current;

        private SortedSet<UInt64> unusedNumbers;

        private SortedSet<UInt64> reservedNumbers;

        private string GetUnusedListIdsAsString()
        {
            var strIds = new StringBuilder();
            var curKeyPar = 0;
            foreach (var number in unusedNumbers)
            {
                strIds.Append(number);
                if (curKeyPar != unusedNumbers.Count - 1)
                {
                    strIds.Append(", ");
                }
                curKeyPar++;
            }

            return strIds.ToString();
        }

        private string GetReservedListIdsAsString()
        {
            var strIds = new StringBuilder();
            var curKeyPar = 0;
            foreach (var number in reservedNumbers)
            {
                strIds.Append(number);
                if (curKeyPar != reservedNumbers.Count - 1)
                {
                    strIds.Append(", ");
                }
                curKeyPar++;
            }

            return strIds.ToString();
        }

        public UNG(UInt64 min, UInt64 max)
        {
            if (max < min)
            {
                throw new ArgumentException("min shouldn't be larger than max");
            }

            this.min = min;
            this.max = max;
            this.current = min;

            this.unusedNumbers = new SortedSet<ulong>();
            this.reservedNumbers = new SortedSet<ulong>();
        }

        public bool IsReserved(UInt64 num)
        {
            return this.reservedNumbers.Contains(num);
        }

        public UInt16 NextUInt16(bool isLevel = false)
        {
            var nextNumber = (UInt16)this.InnerNext();

            return nextNumber;
        }

        public UInt32 NextUInt32()
        {
            return (UInt32)this.InnerNext();
        }

        public UInt64 NextUInt64()
        {
            return (UInt64)this.InnerNext();
        }

        private UInt64 InnerNext()
        {
            ulong numberToReturn = this.current;

            while (IsReserved(numberToReturn))
            {
                numberToReturn = numberToReturn + 1;
            }

            if (this.unusedNumbers.Count > 0)
            {
                foreach (var number in this.unusedNumbers)
                {
                    if (number < this.current)
                    {
                        if (!IsReserved(number))
                        {
                            numberToReturn = number;
                            break;
                        }
                    }
                }
            }
            else
            {
                this.current = numberToReturn;
            }

            return numberToReturn;
        }

        #region ReserveNumber

        public void ReserveNumber(UInt16 number)
        {
            if (reservedNumbers.Contains(number))
            {
                throw new ArgumentException("already reserved number");
            }
            reservedNumbers.Add(number);
            if (this.unusedNumbers.Contains(number))
            {
                this.unusedNumbers.Remove(number);
            }
        }

        public void ReserveNumber(UInt32 number)
        {
            reservedNumbers.Add(number);
            if (this.unusedNumbers.Contains(number))
            {
                this.unusedNumbers.Remove(number);
            }
        }

        public void ReserveNumber(UInt64 number)
        {
            reservedNumbers.Add(number);
            if (this.unusedNumbers.Contains(number))
            {
                this.unusedNumbers.Remove(number);
            }
        }

        #endregion

        #region ReleaseNumber

        public void ReleaseNumber(UInt16 num)
        {
            if (this.unusedNumbers.Contains((UInt64)num))
            {
                throw new ArgumentException("already released number");
            }

            this.unusedNumbers.Add((UInt64)num);
            if (this.reservedNumbers.Contains(num))
            {
                this.reservedNumbers.Remove(num);
            }
        }

        public void ReleaseNumber(UInt32 num)
        {
            if (this.unusedNumbers.Contains((UInt64)num))
            {
                throw new ArgumentException("already released number");
            }

            this.unusedNumbers.Add((UInt64)num);
            if (this.reservedNumbers.Contains(num))
            {
                this.reservedNumbers.Remove(num);
            }
        }

        public void ReleaseNumber(UInt64 num)
        {
            if (this.unusedNumbers.Contains(num))
            {
                throw new ArgumentException("already released number");
            }

            this.unusedNumbers.Add(num);
            if (this.reservedNumbers.Contains(num))
            {
                this.reservedNumbers.Remove(num);
            }
        }

        #endregion
    }
}
