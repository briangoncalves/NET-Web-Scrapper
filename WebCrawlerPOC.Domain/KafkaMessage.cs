using System;

namespace WebCrawlerPOC.Domain
{
    public class KafkaMessage
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string SpecVersion { get; set; }
        public string Source { get; set; }
        public string Subject { get; set; }
        public DateTime Time { get; set; }
        public string DataContentType { get; set; }
        public object Data { get; set; }
    }
}
