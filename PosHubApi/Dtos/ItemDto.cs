using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class ItemDto
    {
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("posReference")]
        public string PosReference { get; set; }

        [JsonPropertyName("partnerId")]
        public string PartnerId { get; set; }

        [JsonPropertyName("parentPosReference")]
        public string ParentPosReference { get; set; }

        [JsonPropertyName("menuCategoryId")]
        public string MenuCategoryId { get; set; }

        [JsonPropertyName("taxRateIds")]
        public List<string> TaxRateIds { get; set; }

        [JsonPropertyName("options")]
        public List<ItemOptionDto> Options { get; set; }

        [JsonPropertyName("customerNotes")]
        public string CustomerNotes { get; set; }
    }
}
