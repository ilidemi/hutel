using System.Collections.Generic;
using Newtonsoft.Json;

namespace hutel.Models
{
    public class TagJson
    {
        [JsonProperty(Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<TagFieldJson> Fields { get; set; }
    }

    public class TagFieldJson
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Type { get; set; }
        
        public List<string> Values { get; set; } // for enum
    }
}