using Confluent.Kafka;
using System;

namespace WebCrawlerPOC.Domain
{
    public enum ReturnStatus
    {
        Success = 0,
        ErrorUnknown = -1,
        ErrorHostNotFound = -2
    }

    public class KafkaReturnValue
    {
        public ReturnStatus Status { get; set; }
        public Exception Exception { get; set; }
        public DeliveryResult<string, string> DeliveryResult { get; set; }
    }
}
