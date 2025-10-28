using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class ModifierUpdateRequestDto
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

        [JsonPropertyName("originalImageUrl")]
        public string OriginalImageUrl { get; set; }

        [JsonPropertyName("taxRate")]
        public decimal TaxRate { get; set; }

        [JsonPropertyName("isBikeFriendly")]
        public bool IsBikeFriendly { get; set; }

        [JsonPropertyName("showOnline")]
        public bool ShowOnline { get; set; }
        
        [JsonPropertyName("taxRateIds")]
        public List<string> TaxRateIds { get; set; } = new();

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("containsTobacco")]
        public bool ContainsTobacco { get; set; }

        [JsonPropertyName("position")]
        public int Position { get; set; }

        [JsonPropertyName("inStorePrice")]
        public int InStorePrice { get; set; }

        [JsonPropertyName("maxPermitted")]
        public int MaxPermitted { get; set; }

        [JsonPropertyName("minPermitted")]
        public int MinPermitted { get; set; }


    }
}
