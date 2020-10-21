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
    }
}