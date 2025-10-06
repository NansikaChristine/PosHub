using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class OrderResponseDto
    {
        [JsonPropertyName("data")]
        public OrderEventDto Data { get; set; }
    }
}