using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class TaxDto
    {
        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
