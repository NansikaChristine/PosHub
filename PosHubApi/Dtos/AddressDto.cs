using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class AddressDto
    {
        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("googlePlaceId")]
        public string GooglePlaceId { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("addressComponents")]
        public AddressComponentsDto AddressComponents { get; set; }
    }
}
