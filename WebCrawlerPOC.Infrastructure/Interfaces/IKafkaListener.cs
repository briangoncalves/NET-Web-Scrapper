using System.Collections.Generic;

namespace WebCrawlerPOC.Infrastructure.Interfaces
{
    public interface IKafkaListener
    {
        string ConsumerGroupId { get; set; }
        IEnumerable<string> Topics { get; set; }
    }
}
