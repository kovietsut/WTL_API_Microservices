using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.SeedWork
{
    public static class Util
    {
        public static string GenerateCacheKeyFromRequest(HttpRequest httpRequest)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"{httpRequest.Path}?");
            var queryParameters = httpRequest.Query.OrderBy(x => x.Key).ToList();
            if (queryParameters.Count > 0)
            {
                for (var i = 0; i < queryParameters.Count; i++)
                {
                    var (key, value) = queryParameters[i];
                    keyBuilder.Append($"{key}={value}");
                    if (i < queryParameters.Count - 1)
                    {
                        keyBuilder.Append("&");
                    }
                }
            }
            return keyBuilder.ToString();
        }

        public static List<long> SplitStringToArray(string ids)
        {
            return ids.Split(",").Select(long.Parse).ToList();
        }
    }
}
