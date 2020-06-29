using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mango.Elasticsearch.Extensions
{
    public static class StringExtension
    {
        public static string ToLowerCamelCase(this string unformated)
        {
            return Char.ToLowerInvariant(unformated[0]) + unformated.Substring(1);
        }

        public static string ToLowerInvariantElastic(this string toElasticQuery)
        {
            if (string.IsNullOrEmpty(toElasticQuery))
                return string.Empty;
            var words = toElasticQuery.Split(' ');
            return string.Join(" ", words.Select(x => x.ToElasticRegexWord()));
        }

        public static string ToElasticRegexWord(this string str)
        {
            if (string.IsNullOrEmpty(str) && str.Length > 2)
                return str;
            if (!Regex.IsMatch(str, @"^\d+$"))
                return "[" + str[0].ToString().ToLower() + str[0].ToString().ToUpper() + "]" + str.Substring(1);
            else
                return str;
        }
    }
}
