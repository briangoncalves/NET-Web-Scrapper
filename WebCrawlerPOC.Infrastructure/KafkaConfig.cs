using System.Collections.Generic;
using WebCrawlerPOC.Infrastructure.Interfaces;

namespace WebCrawlerPOC.Infrastructure
{
    public class KafkaConfig : IKafkaConfig
    {
        public string Host { get; set; }
        public IEnumerable<IKafkaSender> Sender { get; set; }
        public IEnumerable<IKafkaListener> Listeners { get; set; }
        public string Source { get; set; }
    }
}
