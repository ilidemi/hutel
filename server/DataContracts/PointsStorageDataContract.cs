using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace hutel.Models
{
    public class StoredPointDataContractCollection : List<StoredPointDataContract>
    {
    }

    public class StoredPointDataContract
    {
        [JsonProperty(Required = Required.Always)]
        public Guid Id { get; set; }
        
        [JsonProperty(Required = Required.Always)]
        public string TagId { get; set; }
        
        [JsonProperty(Required = Required.Always)]
        public string Date { get; set; }
        
        [JsonProperty(Required = Required.Always)]
        public string SubmitTimestamp { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> Extra { get; set; }
    }
}
