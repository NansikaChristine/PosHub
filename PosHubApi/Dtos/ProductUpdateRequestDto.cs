using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class ProductUpdateRequestDto
    {
        [JsonPropertyName("nutritionalInfo")]
        public NutritionalInfoDto NutritionalInfo { get; set; } = new();

        [JsonPropertyName("containsAlcohol")]
        public bool ContainsAlcohol { get; set; }

        [JsonPropertyName("isTaxIncluded")]
        public bool IsTaxIncluded { get; set; } = true;

        [JsonPropertyName("posVersion")]
        public string PosVersion { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("modifierGroups")]
        public List<string> ModifierGroups { get; set; } = new();

        [JsonPropertyName("parentId")]
        public string ParentId { get; set; }

        [JsonPropertyName("originalImageUrl")]
        public string OriginalImageUrl { get; set; }

        [JsonPropertyName("taxRate")]
        public decimal TaxRate { get; set; }

        [JsonPropertyName("isBikeFriendly")]
        public bool IsBikeFriendly { get; set; }

        [JsonPropertyName("showOnline")]
        public bool ShowOnline { get; set; }

        [JsonPropertyName("selections")]
        public List<SelectionProductUpdateDto> Selections { get; set; } = new();

        [JsonPropertyName("taxRateIds")]
        public List<string> TaxRateIds { get; set; } = new();

        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("containsTobacco")]
        public bool ContainsTobacco { get; set; }

        [JsonPropertyName("serviceAvailability")]
        public List<ServiceAvailabilityDto> ServiceAvailability { get; set; } = new();

        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; } = new();

        [JsonPropertyName("position")]
        public int Position { get; set; }

        [JsonPropertyName("inStorePrice")]
        public int InStorePrice { get; set; }
    }
}
