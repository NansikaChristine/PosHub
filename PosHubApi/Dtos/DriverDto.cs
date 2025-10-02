using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class DriverDto
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonPropertyName("driverReference")]
        public string DriverReference { get; set; }

        [JsonPropertyName("vehicle")]
        public VehicleDto Vehicle { get; set; }
    }
}
