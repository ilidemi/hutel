using System.Collections.Generic;
using Newtonsoft.Json;

namespace hutel.Models
{
    public class PointJson
    {
        [JsonProperty(Required = Required.Always)]
        public string TagId { get; set; }
        
        [JsonProperty(Required = Required.Always)]
        public string Date { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> Extra { get; set; }
    }
}
