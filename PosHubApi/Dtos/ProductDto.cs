using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PosHubApi.Dtos
{
    public class ProductDto
    {
        public string Id { get; set; }
        public string PosReference { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string PosVersion { get; set; }
        public string OriginalImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal? InStorePrice { get; set; }
        public decimal TaxRate { get; set; }
        public bool IsTaxIncluded { get; set; } = true;
        public bool ContainsAlcohol { get; set; }
        public bool ContainsTobacco { get; set; }
        public bool IsBikeFriendly { get; set; }
        public bool ShowOnline { get; set; }
        public List<string> Categories { get; set; } = new();
        public int Position { get; set; }
        public NutritionalInfoDto NutritionalInfo { get; set; } = new();
        public string DietaryRestriction { get; set; }
        public string Spiciness { get; set; }
        public List<string> Additives { get; set; } = new();
        public List<string> Allergens { get; set; } = new();
        public List<ServiceAvailabilityDto> ServiceAvailability { get; set; } = new();
        public List<string> ModifierGroups { get; set; } = new();
        public List<SelectionDto> Selections { get; set; } = new();

    }
}