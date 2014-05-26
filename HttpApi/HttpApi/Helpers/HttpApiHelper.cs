using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpApi.Helpers
{
    public static class HttpApiHelper
    {
        public static async Task<string> RunApiMethod(string baseUrl, Dictionary<string, string> parameters)
        {
            var urlParameters = string.Join("&", parameters.Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value)));
            var url = string.Format("{0}?{1}", baseUrl, urlParameters);
            var request = WebRequest.Create(url);
            var response = await request.GetResponseAsync();
            var responseStream = response.GetResponseStream();
            if (responseStream == null)
                return null;
            var ms = new MemoryStream();
            responseStream.CopyTo(ms);
            return Encoding.UTF8.GetString(ms.GetBuffer());
        }
    }
}
