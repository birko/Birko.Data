using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Birko.Data.Helper
{
    public static class StringHelper
    {
        static readonly Regex WordDelimiters = new Regex(@"[\s—–_\/]", RegexOptions.Compiled);

        // characters that are not valid
        static readonly Regex InvalidChars = new Regex(@"[^a-z0-9\-]", RegexOptions.Compiled);

        // multiple hyphens
        static readonly Regex MultipleHyphens = new Regex(@"-{2,}", RegexOptions.Compiled);

        public static string GenerateSlug(string value)
        {
            // convert to lower case
            value = value.ToLowerInvariant();

            // remove diacritics (accents)
            value = RemoveDiacritics(value);

            // ensure all word delimiters are hyphens
            value = WordDelimiters.Replace(value, "-");

            // strip out invalid characters
            value = InvalidChars.Replace(value, "");

            // replace multiple hyphens (-) with a single hyphen
            value = MultipleHyphens.Replace(value, "-");

            // trim hyphens (-) from ends
            return value.Trim('-');
        }

        public static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string ToUrlSlug(string phrase)
        {
            string str = RemoveDiacritics(phrase).ToLower();
            //string str = phrase;
            // invalid chars
            str = Regex.Replace(str, @"[^a-z0-9\/\s-]", "");
            str = RemoveMultipleSpaces(str);
            // cut and trim
            str = str.Substring(0, str.Length <= 55 ? str.Length : 55).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens
            str = Regex.Replace(str, @"\/", "-"); // hyphens
            return str;
        }

        public static string RemoveMultipleSpaces(string str)
        {
            // convert multiple spaces into one space
            str = Regex.Replace(str, @"\s+", " ").Trim();
            return str;
        }

        public static string HashText(string text)
        {
            var data = Encoding.ASCII.GetBytes(text);
            var sha1 = new SHA1CryptoServiceProvider();
            var sha1data = sha1.ComputeHash(data);
            return Encoding.ASCII.GetString(sha1data);
        }
    }
}
