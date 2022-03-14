using System.Collections.Generic;

namespace WebCrawlerPOC.Infrastructure.Interfaces
{
    public interface IKafkaConfig
    {
        string Host { get; set; }
        IEnumerable<IKafkaSender> Sender { get; set; }
        IEnumerable<IKafkaListener> Listeners { get; set; }
        string Source { get; set; }
    }
}
