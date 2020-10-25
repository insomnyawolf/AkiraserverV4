using System;
using System.Collections.Generic;
using System.Text;

namespace AkiraserverV4.Http.Helper
{
    public static class Data
    {
        public static byte[] ToByteArray(this string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }

        public static bool PatternEquals<T>(this IList<T> source, IList<T> pattern) where T : IEquatable<T>
        {
            if(source.Count != pattern.Count)
            {
                return false;
            }

            for (int i = 0; i < pattern.Count; ++i)
            {
                if (!source[i].Equals(pattern[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool EndsWith<T>(this IList<T> source, IList<T> pattern) where T : IEquatable<T>
        {
            int patternIndex = 0;
            for (int i = source.Count - pattern.Count; i < source.Count; ++i)
            {
                if (!source[i].Equals(pattern[patternIndex++]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool EndsWithOffset<T>(this IList<T> source, IList<T> pattern, int offset) where T : IEquatable<T>
        {
            int startPos = source.Count - pattern.Count - offset;

            if (startPos < 0)
            {
                return false;
            }

            int patternIndex = 0;
            for (int i = startPos; i < source.Count; ++i)
            {
                if (!source[i].Equals(pattern[patternIndex++]))
                {
                    return false;
                }
            }

            return true;
        }

        public static T[] ShiftLeft<T>(this T[] arr, int shifts, T[] tempBuffer)
        {
            Array.Copy(arr, shifts, tempBuffer, 0, arr.Length - shifts);
            return tempBuffer;
        }

        //public static void ShiftRight<T>(this T[] arr, int shifts)
        //{
        //    Array.Copy(arr, 0, arr, shifts, arr.Length - shifts);
        //}

        // Can't Understand The Logic Behind

        //public static void ShiftBlockLeft(this int[] arr, int shifts)
        //{
        //    var size = sizeof(int);
        //    var dest = (arr.Length - shifts) * size;
        //    Buffer.BlockCopy(arr, shifts, arr, 0, dest);
        //}

        //public static void ShiftBlockRight(this int[] arr, int shifts)
        //{
        //    var size = sizeof(int);
        //    var origin = shifts * size;
        //    var dest = (arr.Length - shifts) * size;
        //    Buffer.BlockCopy(arr, 0, arr, origin, dest);
        //}
    }
}