using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class DiscountDto
    {
        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
