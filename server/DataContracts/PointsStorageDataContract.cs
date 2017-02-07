using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace hutel.Models
{
    public class PointsStorageDataContract : List<PointWithIdDataContract>
    {
    }

    public class PointWithIdDataContract
    {
        [JsonProperty(Required = Required.Always)]
        public Guid Id { get; set; }
        
        [JsonProperty(Required = Required.Always)]
        public string TagId { get; set; }
        
        [JsonProperty(Required = Required.Always)]
        public string Date { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> Extra { get; set; }
    }
}
