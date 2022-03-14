using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;

namespace WebCrawlerPOC.Domain
{
    [BsonCollection("url-validation")]
    [BsonIgnoreExtraElements]
    public class UrlValidation : IDocument
    {
        [JsonIgnore]
        public ObjectId Id { get; set; }

        [BsonElement("url")]
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [BsonElement("valid")]
        [JsonPropertyName("valid")]
        public bool Valid { get; set; }

        [BsonElement("expireDate")]
        [JsonPropertyName("expireDate")]
        public DateTime ExpireDate { get; set; }
    }
}
