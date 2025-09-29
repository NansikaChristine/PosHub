using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PosHubApi.Dtos
{
    public class NutritionalInfoDto
    {
        public RangeDto Kilojoules { get; set; } = new();
        public RangeDto Calories { get; set; } = new();
        public RangeDto Carbohydrates { get; set; } = new();
        public RangeDto Protein { get; set; } = new();
        public RangeDto SaturatedFat { get; set; } = new();
        public RangeDto Salt { get; set; } = new();
        public RangeDto Sugar { get; set; } = new();
    }
}