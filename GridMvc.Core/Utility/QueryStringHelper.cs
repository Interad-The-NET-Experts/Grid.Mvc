﻿using System.Collections.Specialized;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace GridMvc.Core.Utility
{
    public static class QueryStringHelper
    {
        public static string GetValue(this IQueryCollection strings, string key)
        {
            StringValues values;
            if (!strings.TryGetValue(key, out values))
                return string.Empty;

            return values[0];
        }

        public static NameValueCollection ToNameValueCollection(this IQueryCollection strings)
        {
            NameValueCollection collection = new NameValueCollection();
            foreach (string key in strings.Keys)
            {
                collection.Add(key, strings[key].ToString());
            }
            return collection;
        }
    }
}
