namespace PosHubApi.Dtos
{
    public class ModifierGroupDto
    {
        public string PosReference { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PosVersion { get; set; }
        public int Position { get; set; }
        public int MinPermitted { get; set; }
        public int MaxPermitted { get; set; }
        public List<ModifierDto> Modifiers { get; set; } = new();
    }
}