using WebCrawlerPOC.Infrastructure.Interfaces;

namespace WebCrawlerPOC.Infrastructure
{
    public class KafkaSender : IKafkaSender
    {
        public string Topic { get; set; }
    }
}
