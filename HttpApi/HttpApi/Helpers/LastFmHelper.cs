using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HttpApi.Protocols;
using Newtonsoft.Json.Linq;

namespace HttpApi.Helpers
{
    internal static class LastFmHelper
    {
        private const string LastFmApiKey = @"65381ce693ef89cec1a27f1469d15ef0";
        private const string LastFmApiBaseUrl = @"http://ws.audioscrobbler.com/2.0/";

        public async static Task<string[]> GetTags(Song song)
        {
            var getTagsResult = await HttpApiHelper.RunApiMethod(LastFmApiBaseUrl, new Dictionary<string, string>
            {
                { "method", "track.getInfo" },
                { "artist", song.Artist },
                { "track", song.Title },
                { "api_key", LastFmApiKey },
                { "autocorrect", "1" },
                { "format", "json" },
            });
            var result = JObject.Parse(getTagsResult);
            if (result["error"] != null)
                return new string[0];
            var tags = result["track"]["toptags"];

            if (!tags.Any() || tags["tag"] == null)
                return new string[0];
            var tag = tags["tag"];
            return tag.ToString().First() == '[' 
                ? tag.Select(t => (string) t["name"]).ToArray() 
                : new[] { (string) tag["name"] };
        }
    }
}
