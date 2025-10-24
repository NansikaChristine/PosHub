using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PosHubApi.Dtos
{
    public class CategoryDataResponseByPosRefDto
    {

        [JsonPropertyName("posVersion")]
        public string PosVersion { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("originalImageUrl")]
        public string OriginalImageUrl { get; set; }

        [JsonPropertyName("accountId")]
        public string AccountId { get; set; }

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("showOnline")]
        public bool ShowOnline { get; set; }

        [JsonPropertyName("locationId")]
        public string LocationId { get; set; }

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("serviceAvailability")]
        public List<ServiceAvailabilityDto> ServiceAvailability { get; set; } = new();

        [JsonPropertyName("posReference")]
        public string PosReference { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("position")]
        public int Position { get; set; }
        
        [JsonPropertyName("updatedAt")]
        public string UpdatedAt { get; set; }

    }
}