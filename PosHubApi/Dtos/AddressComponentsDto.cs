using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class AddressComponentsDto
    {
        [JsonPropertyName("area")]
        public string Area { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("flatNo")]
        public string FlatNo { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; }

        [JsonPropertyName("houseNo")]
        public string HouseNo { get; set; }

        [JsonPropertyName("addressLine1")]
        public string AddressLine1 { get; set; }

        [JsonPropertyName("addressLine2")]
        public string AddressLine2 { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }
    }
}
