using Newtonsoft.Json;

namespace TestMapBox.Models
{
    public class Coordinate
    {
        [JsonProperty("lng")]
        public decimal lng { get; set; }

        [JsonProperty("lat")]
        public decimal lat { get; set; }
    }
}
