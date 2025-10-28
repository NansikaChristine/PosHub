using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class ModifierDataResponseByPosRefDto
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

        [JsonPropertyName("posVersion")]
        public string PosVersion { get; set; }

        [JsonPropertyName("originalImageUrl")]
        public string OriginalImageUrl { get; set; }

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }

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

        [JsonPropertyName("position")]
        public int Position { get; set; }

        [JsonPropertyName("nutritionalInfo")]
        public NutritionalInfoDto NutritionalInfo { get; set; } = new();

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public string UpdatedAt { get; set; }

        [JsonPropertyName("locationId")]
        public string LocationId { get; set; }

        [JsonPropertyName("taxRateIds")]
        public List<string> TaxRateIds { get; set; }

        [JsonPropertyName("minPermitted")]
        public int MinPermitted { get; set; }

        [JsonPropertyName("maxPermitted")]
        public int MaxPermitted { get; set; }

    }
}
