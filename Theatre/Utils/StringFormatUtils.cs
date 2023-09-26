using System.Text;

namespace Theatre.Utils
{
    public static class StringFormatUtils
    {
        public static string LimitChars(this string str, int limit)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < limit; i++)
            {
                if (str.Length<limit)
                {
                    break;
                }
                builder.Append(str[i]);
            }

            if (builder.Length == 0)
            {
                return str;
            }
            return builder+"...";

        }

        public static string FormatModList(this string str, string author)
        {
            return str + " by: " + author;
        }
    }
}
