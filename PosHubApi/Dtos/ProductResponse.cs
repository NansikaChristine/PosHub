using System.Text.Json.Serialization;
using PosHubApi.Dtos;

public class ProductResponse
{
    [JsonPropertyName("data")]
    public ProductDto Data { get; set; }
}
