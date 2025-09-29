using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PosHubApi.Dtos
{
    public class CatalogProductsResponseDto
    {
        public bool HasNextPage { get; set; }
        public List<ProductDto> Data { get; set; } = new();
        public string NextPageKey { get; set; }
    }
}