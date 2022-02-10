using System.Collections.Generic;
using Newtonsoft.Json;

namespace hutel.Models
{
    public class TagDataContract
    {
        [JsonProperty(Required = Required.Always)]
        public string Id { get; set; }

        private bool _isSensitive;

        [JsonProperty(Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsSensitive
        {
            get => this._isSensitive ? (bool?)true : null;
            set { this._isSensitive = value == true; }
        }

        [JsonProperty(Required = Required.Always)]
        public List<TagFieldDataContract> Fields { get; set; }
    }

    public class TagFieldDataContract
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Type { get; set; }

        public List<string> Values { get; set; } // for enum
    }
}