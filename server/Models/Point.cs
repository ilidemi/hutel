using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace hutel.Models
{
    public class Point
    {
        public static readonly IList<string> ReservedFields =
            new List<string> { "id", "tagId", "date" };

        [JsonProperty(Required = Required.Always)]
        public Guid Id { get; set; }
        
        [JsonProperty(Required = Required.Always)]
        public string TagId { get; set; }
        
        [JsonProperty(Required = Required.Always)]
        public DateTime Date { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> Extra { get; set; }
    }
}