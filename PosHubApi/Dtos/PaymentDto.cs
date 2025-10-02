using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class PaymentDto
    {
        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
