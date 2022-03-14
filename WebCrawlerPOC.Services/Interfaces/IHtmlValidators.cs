namespace WebCrawlerPOC.Services.Interfaces
{
    public interface IHtmlValidators
    {
        bool CheckURLExists(string url);
        bool CheckURLValid(string url);
    }
}