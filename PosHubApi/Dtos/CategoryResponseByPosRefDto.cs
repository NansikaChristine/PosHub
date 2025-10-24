using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PosHubApi.Dtos
{
    public class CategoryResponseByPosRefDto
    {
        [JsonPropertyName("hasNextPage")]
        public bool HasNextPage { get; set; }

        [JsonPropertyName("data")]
        public List<CategoryDataResponseByPosRefDto> Data { get; set; }
    }
}