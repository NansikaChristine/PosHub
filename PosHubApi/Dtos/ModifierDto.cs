using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class ModifierDto
    {
        [JsonPropertyName("catalogModifierId")]
        public string PosReference { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PosVersion { get; set; }
        public string OriginalImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal? InStorePrice { get; set; }
        public decimal TaxRate { get; set; }
        public bool IsTaxIncluded { get; set; } = true;
        public bool ContainsAlcohol { get; set; }
        public bool ContainsTobacco { get; set; }
        public bool IsBikeFriendly { get; set; }
        public bool ShowOnline { get; set; }
        public int Position { get; set; }
        public int MinPermitted { get; set; }
        public int MaxPermitted { get; set; }
        public NutritionalInfoDto NutritionalInfo { get; set; } = new();
        public List<SelectionDto> Selections { get; set; } = new();

    }
}