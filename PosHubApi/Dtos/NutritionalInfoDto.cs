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
        public string DietaryRestriction { get; set; }
        public string Spiciness { get; set; }
        public List<string> Additives { get; set; }
        public List<string> Allergens { get; set; }
    }
}