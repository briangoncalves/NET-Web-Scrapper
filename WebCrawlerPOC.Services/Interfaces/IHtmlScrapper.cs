using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebCrawlerPOC.Services.Interfaces
{
    public interface IHtmlScrapper
    {
        Task<string> DownloadHtml(string url);
        IEnumerable<string> GetListOfUrls(string html);
        IEnumerable<string> GetListOfImages(string html);
    }
}