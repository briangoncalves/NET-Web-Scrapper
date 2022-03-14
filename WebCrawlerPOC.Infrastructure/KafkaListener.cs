using System.Collections.Generic;
using WebCrawlerPOC.Infrastructure.Interfaces;

namespace WebCrawlerPOC.Infrastructure
{
    public class KafkaListener : IKafkaListener
    {
        public string ConsumerGroupId { get; set; }
        public IEnumerable<string> Topics { get; set; }
    }
}
