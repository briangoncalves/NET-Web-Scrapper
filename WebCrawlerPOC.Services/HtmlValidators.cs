using System;
using System.Net;
using WebCrawlerPOC.Services.Interfaces;

namespace WebCrawlerPOC.Services
{
    public class HtmlValidators : IHtmlValidators
    {
        public bool CheckURLValid(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) && uriResult.Scheme == Uri.UriSchemeHttps;
        }

        public bool CheckURLExists(string url)
        {
            var webRequest = System.Net.WebRequest.Create(url);
            webRequest.Method = "HEAD";
            try
            {
                using (var response = (System.Net.HttpWebResponse)webRequest.GetResponse())
                {
                    if (response.StatusCode.ToString() == "OK")
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        client.Headers.Add("User-Agent: Mozilla/ 5.0(Windows NT 10.0; Win64; x64; rv: 79.0) Gecko/20100101 Firefox/79.0");
                        var html = client.DownloadString(url);
                        return !string.IsNullOrEmpty(html);
                    }
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
