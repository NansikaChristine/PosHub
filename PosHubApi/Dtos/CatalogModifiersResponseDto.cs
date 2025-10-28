using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PosHubApi.Dtos;

namespace PosHubApi.Dtos
{
    public class CatalogModifiersResponseDto
    {
        [JsonPropertyName("hasNextPage")]
        public bool HasNextPage { get; set; }

        [JsonPropertyName("data")]
        public List<ModifierDto> Data { get; set; } = new();

        [JsonPropertyName("nextPageKey")]
        public string NextPageKey { get; set; }
    }
}
