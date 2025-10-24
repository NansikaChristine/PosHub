namespace PosHubApi.Dtos
{
    public class CategoryDto
    {
        public string Id { get; set; }
        public string PosReference { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OriginalImageUrl { get; set; }
        public bool ShowOnline { get; set; }
        public string PosVersion { get; set; }
        public int Position { get; set; }
        public List<ServiceAvailabilityDto> ServiceAvailability { get; set; } = new();
    }
}