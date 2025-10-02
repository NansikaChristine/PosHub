using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class DeliveryDto
    {
        [JsonPropertyName("instructions")]
        public string Instructions { get; set; }

        [JsonPropertyName("address")]
        public AddressDto Address { get; set; }

        [JsonPropertyName("deliveryType")]
        public string DeliveryType { get; set; }
    }
}
