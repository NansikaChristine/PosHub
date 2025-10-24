namespace PosHubApi.Dtos
{
    public class UpdateOrderEventRequestUIDto
    {
        public string OrderId { get; set; }
        public string Status { get; set; }
        public string CancellationReason { get; set; }
    }
}