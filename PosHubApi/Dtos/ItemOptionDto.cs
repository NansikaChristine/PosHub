using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class ItemOptionDto
    {
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("menuModifierGroupId")]
        public string MenuModifierGroupId { get; set; }

        [JsonPropertyName("taxRateIds")]
        public List<string> TaxRateIds { get; set; }

        [JsonPropertyName("modifierGroupName")]
        public string ModifierGroupName { get; set; }

        [JsonPropertyName("posReference")]
        public string PosReference { get; set; }

        [JsonPropertyName("parentPosReference")]
        public string ParentPosReference { get; set; }

        [JsonPropertyName("parentModifierPosReference")]
        public string ParentModifierPosReference { get; set; }

        [JsonPropertyName("parentMenuModifierGroupId")]
        public string ParentMenuModifierGroupId { get; set; }

        [JsonPropertyName("parentMenuModifierId")]
        public string ParentMenuModifierId { get; set; }

        [JsonPropertyName("parentModifierGroupPosReference")]
        public string ParentModifierGroupPosReference { get; set; }

        [JsonPropertyName("partnerId")]
        public string PartnerId { get; set; }
    }
}
