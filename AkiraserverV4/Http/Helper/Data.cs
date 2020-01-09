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

        public static List<byte[]> Separate(this byte[] source, byte[] pattern, int? times = null)
        {
            var Parts = new List<byte[]>();

            byte[] Part;
            int Index = 0;
            int patternIndex = 0;

            for (int I = 0; I < source.Length; ++I)
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
                    patternIndex++;
                }
            }

            Part = new byte[source.Length - Index];
            Array.Copy(source, Index, Part, 0, Part.Length);
            Parts.Add(Part);
            return Parts;
        }

        public static List<List<byte>> Separate(this List<byte> source, byte[] pattern, int? times = null)
        {
            var Parts = new List<List<byte>>();

            List<byte> Part;
            int Index = 0;
            int patternIndex = 1;

            for (int I = 0; I < source.Count; ++I)
            {
                if (times.HasValue && times.Value < patternIndex)
                {
                    break;
                }

                if (PatternEquals(source, pattern, I))
                {
                    Part = source.GetRange(Index, I - Index);
                    Parts.Add(Part);
                    Index = I + pattern.Length;
                    I += pattern.Length - 1;
                    patternIndex++;
                }
            }

            Part = source.GetRange(Index, source.Count - Index);
            Parts.Add(Part);
            return Parts;
        }

        private static bool PatternEquals(IList<byte> source, byte[] separator, int index)
        {
            for (int i = 0; i < separator.Length; ++i)
            {
                if (index + i >= source.Count || source[index + i] != separator[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}