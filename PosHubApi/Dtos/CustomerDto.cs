using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class CustomerDto
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("phonePin")]
        public string PhonePin { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }
    }
}
