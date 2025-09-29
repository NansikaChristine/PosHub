using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PosHubApi.Dtos
{
    public class PriceListDto
    {
        public string Name { get; set; }
        public string PosReference { get; set; }
        public string Type { get; set; } // FIXED or PERCENTAGE
        public int Percentage { get; set; } = 0;
        public List<ProductDto> Products { get; set; }
    }
}