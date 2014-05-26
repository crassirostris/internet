using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using HttpApi.Protocols;
using Newtonsoft.Json.Linq;

namespace HttpApi.Helpers
{
    internal static class VkHelper
    {
        private static readonly Regex[] dirtyRegexes = new []
        {
            new Regex(@"^\d+\."),
            new Regex(@""), 
            new Regex(@"\(.*\)"), 
            new Regex(@"\[.*\]"), 
            new Regex(@"[^\w\s]"), 
        };
        private static readonly Regex splitRegex = new Regex(@"\s");
        private const int VkAuthAppId = 4381995;
        private const string VkAuthScript = @"<script>window.location.replace(window.location.href.replace('#', '?'))</script>";
        private const string VkAuthCloseScript = @"<script>window.close()</script>";
        private const string VkAuthScope = "audio";
        private const string VkAuthRedirectUri = "http://localhost:8080/";
        private const string VkAuthDisplay = "page";
        private const string VkApiAuthFormatString = @"https://oauth.vk.com/authorize?client_id={0}&scope={1}&redirect_uri={2}&display={3}&response_type=token";
        private const string VkApiRequestFormatString = @"https://api.vk.com/method/{0}";

        public static string GetToken()
        {
            var httpListener = new HttpListener();
            var encodedUrl = string.Format(VkApiAuthFormatString,
                HttpUtility.UrlEncode(VkAuthAppId.ToString()),
                HttpUtility.UrlEncode(VkAuthScope),
                HttpUtility.UrlEncode(VkAuthRedirectUri),
                HttpUtility.UrlEncode(VkAuthDisplay));
            Process.Start(encodedUrl);
            httpListener.Prefixes.Add(@"http://+:8080/");
            httpListener.Start();
            while (true)
            {
                var context = httpListener.GetContext();
                try
                {
                    if (!context.Request.QueryString.AllKeys.Contains("access_token"))
                    {
                        var bytes = Encoding.UTF8.GetBytes(VkAuthScript);
                        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                    }
                    else
                    {
                        var bytes = Encoding.UTF8.GetBytes(VkAuthCloseScript);
                        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                        return context.Request.QueryString["access_token"];
                    }
                }
                finally 
                {
                    context.Response.Close();
                }
            }
        }

        private static string CleanTitle(string dirty)
        {
            var result = dirtyRegexes.Aggregate(dirty, (current, regex) => regex.Replace(current, ""));
            result = result.Replace("_", " ");
            return string.Join(" ", splitRegex.Split(result).Where(s => s != String.Empty));
        }

        public static async Task<Song[]> GetSongs(string token, int? id = null)
        {
            var parameters = new Dictionary<string, string>();
            if (id != null)
                parameters["uid"] = id.Value.ToString();
            parameters["access_token"] = token;
            var methodResult = await HttpApiHelper.RunApiMethod(string.Format(VkApiRequestFormatString, "audio.get"), parameters);
            if (methodResult == null)
                return null;
            var responseJson = JObject.Parse(methodResult);
            var songs = responseJson["response"];
            return songs.Select(o => new Song
            {
                Artist = CleanTitle((string)o["artist"]), 
                Title = CleanTitle((string)o["title"])
            }).ToArray();
        }
    }
}
