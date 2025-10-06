using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class CatalogProductsResponseDto
    {
        [JsonPropertyName("hasNextPage")]
        public bool HasNextPage { get; set; }

        [JsonPropertyName("data")]
        public List<ProductDto> Data { get; set; } = new();

        [JsonPropertyName("nextPageKey")]
        public string NextPageKey { get; set; }
    }
}
