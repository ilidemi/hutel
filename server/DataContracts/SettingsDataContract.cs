using Newtonsoft.Json;

namespace hutel.Models
{

    public class SettingsDataContract
    {
        [JsonProperty(Required = Required.Always)]
        public bool SensitiveHidden { get; set; }
    }
}
