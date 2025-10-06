using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class PriceListDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("posReference")]
        public string PosReference { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("percentage")]
        public int Percentage { get; set; } = 0;

        [JsonPropertyName("products")]
        public List<ProductDto> Products { get; set; }
    }
}
