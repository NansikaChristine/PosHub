using System;
using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class VehicleDto
    {
        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("make")]
        public string Make { get; set; }

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("trackingUrl")]
        public string TrackingUrl { get; set; }
    }
}
