using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Utils
{
    public static class ArrayUtils
    {
        public static unsafe byte[] ConcatenatingArrays(this byte[] arr1, byte[] arr2)
        {
            var sizeArr1 = arr1.Length;
            var sizeArr2 = arr2.Length;
            var totalSize = sizeArr1 + sizeArr2;

            var concatenatedArr = new byte[sizeArr1 + sizeArr2];

            Buffer.BlockCopy(arr1, 0, concatenatedArr, 0, sizeArr1);
            Buffer.BlockCopy(arr2, 0, concatenatedArr, sizeArr1, sizeArr2);
            return concatenatedArr;
        }

        public static T[] SubArray<T>(this T[] data, int index)
        {
            var length = data.Length - index;
            T[] result = new T[length];
            Buffer.BlockCopy(data, index, result, 0, length);
            return result;
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Buffer.BlockCopy(data, index, result, 0, length);
            return result;
        }
    }
}
