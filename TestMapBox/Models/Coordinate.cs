using Newtonsoft.Json;

namespace TestMapBox.Models
{
    public class Coordinate
    {
        [JsonProperty("lng")]
        public double lng { get; set; }

        [JsonProperty("lat")]
        public double lat { get; set; }
    }
}
