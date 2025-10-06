namespace PosHubApi.Dtos
{
    public class ServiceAvailabilityDto
    {
        public string Weekday { get; set; }
        public List<TimePeriodDto> TimePeriods { get; set; } = new();
    }
}