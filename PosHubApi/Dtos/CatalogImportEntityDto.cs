using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PosHubApi.Dtos
{
    public class CatalogImportEntityDto
    {
        public LocationDto Location { get; set; }
        public List<CategoryDto> Categories { get; set; }
        public List<ProductDto> Products { get; set; }
        public List<ModifierGroupDto> ModifierGroups { get; set; }
        public List<ModifierDto> Modifiers { get; set; }
        public string ErrorMessage  { get; set; } = "";
    }
}