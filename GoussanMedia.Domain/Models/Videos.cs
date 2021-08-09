using Newtonsoft.Json;
using System;

namespace GoussanMedia.Domain.Models
{
    public class Videos
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "filename")]
        public string FileName { get; set; }

        [JsonProperty(PropertyName = "bloburi")]
        public string BlobUri { get; set; }

        [JsonProperty(PropertyName = "created")]
        public DateTime UploadDate { get; set; }

        [JsonProperty(PropertyName = "lastmodified")]
        public DateTimeOffset LastModified { get; set; }

        [JsonProperty(PropertyName = "filesize")]
        public long Size { get; set; }

        [JsonProperty(PropertyName = "extension")]
        public string Extension { get; set; }

        public int ErrorCode { get; set; }

        [JsonProperty(PropertyName = "completed")]
        public bool Completed { get; set; }
    }
}