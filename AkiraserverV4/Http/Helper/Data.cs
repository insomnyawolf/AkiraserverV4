using System;
using System.Collections.Generic;
using System.Text;

namespace AkiraserverV4.Http.Helper
{
    public static class Data
    {
        public static byte[] ConcatByteArrays(params byte[][] arrays)
        {
            int lenght = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                lenght += arrays[i].Length;
            }

            byte[] rv = new byte[lenght];

            int offset = 0;

            for (int i = 0; i < arrays.Length; i++)
            {
                byte[] array = arrays[i];
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }

            return rv;
        }

        public static byte[] ToByteArray(this string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }

        public static byte[][] Separate(this byte[] source, byte[] pattern, int? times = null)
        {
            var Parts = new List<byte[]>();
            var Index = 0;
            byte[] Part;
            int patternIndex = 0;
            for (var I = 0; I < source.Length; ++I)
            {
                if (times.HasValue && times.Value < patternIndex)
                {
                    break;
                }

                if (PatternEquals(source, pattern, I))
                {
                    Part = new byte[I - Index];
                    Array.Copy(source, Index, Part, 0, Part.Length);
                    Parts.Add(Part);
                    Index = I + pattern.Length;
                    I += pattern.Length - 1;
                }
            }
            Part = new byte[source.Length - Index];
            Array.Copy(source, Index, Part, 0, Part.Length);
            Parts.Add(Part);
            return Parts.ToArray();
        }

        private static bool PatternEquals(byte[] source, byte[] separator, int index)
        {
            for (int i = 0; i < separator.Length; ++i)
            {
                if (index + i >= source.Length || source[index + i] != separator[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}