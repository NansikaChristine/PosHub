using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class TaxRateDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("posReference")]
        public string PosReference { get; set; }

        [JsonPropertyName("rate")]
        public int Rate { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
