using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class ProductResponseByPosRefDto
    {
        [JsonPropertyName("hasNextPage")]
        public bool HasNextPage { get; set; }

        [JsonPropertyName("data")]
        public List<ProductDataResponseByPosRefDto> Data { get; set; }
    }
}
