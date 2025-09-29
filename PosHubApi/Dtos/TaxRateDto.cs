using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PosHubApi.Dtos
{
    public class TaxRateDto
    {
        public string Name { get; set; }
        public string PosReference { get; set; }
        public int Rate { get; set; }
        public string Type { get; set; }
    }
}