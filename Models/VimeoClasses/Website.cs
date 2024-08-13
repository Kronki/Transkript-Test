using JetBrains.Annotations;
using Newtonsoft.Json;

namespace TranskriptTest.Models.VimeoClasses
{
    public class Website
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "link")]
        public string Link { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }
}
