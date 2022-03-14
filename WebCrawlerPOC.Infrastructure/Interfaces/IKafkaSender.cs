namespace WebCrawlerPOC.Infrastructure.Interfaces
{
    public interface IKafkaSender
    {
        string Topic { get; set; }
    }
}
