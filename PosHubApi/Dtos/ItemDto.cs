using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class ItemDto
    {
        [JsonPropertyName("quantity")]
         int Quantity { get; set; }

        [JsonPropertyName("price")]
         int Price { get; set; }

        [JsonPropertyName("name")]
         string Name { get; set; }

        [JsonPropertyName("posReference")]
         string PosReference { get; set; }

        [JsonPropertyName("partnerId")]
         string PartnerId { get; set; }

        [JsonPropertyName("parentPosReference")]
         string ParentPosReference { get; set; }

        [JsonPropertyName("menuCategoryId")]
         string MenuCategoryId { get; set; }

        [JsonPropertyName("taxRateIds")]
         List<string> TaxRateIds { get; set; }

        [JsonPropertyName("options")]
         List<ItemOptionDto> Options { get; set; }

        [JsonPropertyName("customerNotes")]
         string CustomerNotes { get; set; }
    }
}
