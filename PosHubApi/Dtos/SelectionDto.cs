using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class SelectionDto
    {
        [JsonPropertyName("catalogModifierGroupId")]
        public string PosReference { get; set; }
        [JsonPropertyName("minPermitted")]
        public int MinPermitted { get; set; }
        [JsonPropertyName("maxPermitted")]
        public int MaxPermitted { get; set; }
        [JsonPropertyName("modifiers")]
        public List<ModifierDto> Modifiers { get; set; } = new();
    }
}