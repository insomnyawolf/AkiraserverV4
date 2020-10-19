namespace Extensions
{
    public static class StringExtensions
    {
        public static string RemoveAtEnd(this string input, string pattern)
        {
            int index = input.LastIndexOf(pattern);
            if (index > 0)
            {
                return input.Substring(0, index);
            }
            return input;
        }

        public static string RemoveAtStart(this string input, string pattern)
        {
            if (input.StartsWith(pattern))
            {
                return input[pattern.Length..];
            }
            return input;
        }
    }
}