using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PosHubApi.Dtos
{
    public class ModifierDto
    {
        public string PosReference { get; set; }
        public List<ModifierDto> Modifiers { get; set; } = new();
        public string Name { get; set; }
        public string Description { get; set; }
        public string PosVersion { get; set; }
        public string OriginalImageUrl { get; set; }
        public int Price { get; set; }
        public int? InStorePrice { get; set; }
        public decimal TaxRate { get; set; }
        public bool IsTaxIncluded { get; set; } = true;
        public bool ContainsAlcohol { get; set; }
        public bool ContainsTobacco { get; set; }
        public bool IsBikeFriendly { get; set; }
        public bool ShowOnline { get; set; }
        public int Position { get; set; }
        public int MinPermitted { get; set; }
        public int MaxPermitted { get; set; }
        public string DietaryRestriction { get; set; }
        public string Spiciness { get; set; }
        public List<string> Additives { get; set; } = new();
        public List<string> Allergens { get; set; } = new();
        public NutritionalInfoDto NutritionalInfo { get; set; } = new();
        public List<SelectionDto> Selections { get; set; } = new();

    }
}