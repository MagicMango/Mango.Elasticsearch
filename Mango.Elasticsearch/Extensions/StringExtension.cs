using System;

namespace Mango.Elasticsearch.Extensions
{
    public static class StringExtension
    {
        public static string ToLowerCamelCase(this string unformated)
        {
            return Char.ToLowerInvariant(unformated[0]) + unformated.Substring(1);
        }
    }
}
