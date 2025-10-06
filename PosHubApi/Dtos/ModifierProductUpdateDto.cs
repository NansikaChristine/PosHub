using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class ModifierProductUpdateDto
    {
        [JsonPropertyName("catalogModifierId")]
        public string PosReference { get; set; }

        [JsonPropertyName("maxPermitted")]
        public int MaxPermitted { get; set; }

        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonPropertyName("inStorePrice")]
        public int? InStorePrice { get; set; }

        [JsonPropertyName("showOnline")]
        public bool ShowOnline { get; set; }

        [JsonPropertyName("minPermitted")]
        public int MinPermitted { get; set; }

        [JsonPropertyName("selections")]
        public List<SelectionDto> Selections { get; set; } = new();
    }
}
