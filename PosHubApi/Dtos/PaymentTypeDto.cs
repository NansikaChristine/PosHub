using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class PaymentTypeDto
    {
        [JsonPropertyName("isDefault")]
        public bool IsDefault { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("posReference")]
        public string PosReference { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }
}
