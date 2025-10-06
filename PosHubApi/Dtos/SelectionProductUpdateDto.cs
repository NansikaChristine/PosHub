using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class SelectionProductUpdateDto
    {
        [JsonPropertyName("catalogModifierGroupId")]
        public string PosReference { get; set; }
        [JsonPropertyName("minPermitted")]
        public int MinPermitted { get; set; }
        [JsonPropertyName("maxPermitted")]
        public int MaxPermitted { get; set; }
        [JsonPropertyName("modifiers")]
        public List<ModifierProductUpdateDto> Modifiers { get; set; } = new();
    }
}