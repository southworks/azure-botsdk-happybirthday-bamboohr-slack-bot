using System;
using System.Collections.Generic;
using System.Text;

namespace DataIngestionBambooAPI.Extensions
{
    public static class TextExtension
    {
        public static string RemoveTroublesomeCharacters(this string inString)
        {
            if (inString == null) return null;

            var newString = new StringBuilder();
            char ch;

            foreach (var t in inString)
            {
                ch = t;
                // remove any characters outside the valid UTF-8 range as well as all control characters
                // except tabs and new lines
                if ((ch < 0x00FD && ch > 0x001F) || ch == '\t' || ch == '\n' || ch == '\r')
                {
                    newString.Append(ch);
                }
            }
            newString.Replace(@"\u00a0", " ").Replace(@"\u200e", "").Replace(@"\u00e9", "é");
            return newString.ToString();
        }
    }
}
