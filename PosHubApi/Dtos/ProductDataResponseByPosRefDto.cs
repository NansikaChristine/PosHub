using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class ProductDataResponseByPosRefDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("accountId")]
        public string AccountId { get; set; }

        [JsonPropertyName("posReference")]
        public string PosReference { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("posVersion")]
        public string PosVersion { get; set; }

        [JsonPropertyName("originalImageUrl")]
        public string OriginalImageUrl { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("inStorePrice")]
        public decimal? InStorePrice { get; set; }

        [JsonPropertyName("taxRate")]
        public decimal TaxRate { get; set; }

        [JsonPropertyName("isTaxIncluded")]
        public bool IsTaxIncluded { get; set; } = true;

        [JsonPropertyName("containsAlcohol")]
        public bool ContainsAlcohol { get; set; }

        [JsonPropertyName("containsTobacco")]
        public bool ContainsTobacco { get; set; }

        [JsonPropertyName("isBikeFriendly")]
        public bool IsBikeFriendly { get; set; }

        [JsonPropertyName("showOnline")]
        public bool ShowOnline { get; set; }

        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; } = new();

        [JsonPropertyName("position")]
        public int Position { get; set; }

        [JsonPropertyName("nutritionalInfo")]
        public NutritionalInfoDto NutritionalInfo { get; set; } = new();

        [JsonPropertyName("serviceAvailability")]
        public List<ServiceAvailabilityDto> ServiceAvailability { get; set; } = new();

        [JsonPropertyName("modifierGroups")]
        public List<string> ModifierGroups { get; set; } = new();

        [JsonPropertyName("selections")]
        public List<SelectionDto> Selections { get; set; } = new();

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public string UpdatedAt { get; set; }

        [JsonPropertyName("locationId")]
        public string LocationId { get; set; }

        [JsonPropertyName("taxRateIds")]
        public List<string> TaxRateIds { get; set; }
    }
}
