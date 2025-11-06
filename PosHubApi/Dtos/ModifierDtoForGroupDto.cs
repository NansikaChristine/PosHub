using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class ModifierDtoForGroupDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}