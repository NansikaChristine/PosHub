using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class ModifierResponseByPosRefDto
    {
        [JsonPropertyName("hasNextPage")]
        public bool HasNextPage { get; set; }

        [JsonPropertyName("data")]
        public List<ModifierDataResponseByPosRefDto> Data { get; set; }
    }
}
