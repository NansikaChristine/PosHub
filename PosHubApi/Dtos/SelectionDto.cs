using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PosHubApi.Dtos
{
    public class SelectionDto
    {
        [JsonPropertyName("catalogModifierGroupId")]
        public string Id { get; set; }
        [JsonPropertyName("posReference")]
        public string PosReference { get; set; }
        [JsonPropertyName("minPermitted")]
        public int MinPermitted { get; set; }
        [JsonPropertyName("maxPermitted")]
        public int MaxPermitted { get; set; }
        [JsonPropertyName("modifiers")]
        public List<ModifierDto> Modifiers { get; set; } = new();
        // [JsonPropertyName("selections")]
        // public List<SelectionDto> Selections { get; set; } = new(); 
    }
}