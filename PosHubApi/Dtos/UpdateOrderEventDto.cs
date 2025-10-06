using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class UpdateOrderEventDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("notes")]
        public string Notes { get; set; }

        [JsonPropertyName("orderNumber")]
        public string OrderNumber { get; set; }

        [JsonPropertyName("sourceDeviceType")]
        public string SourceDeviceType { get; set; }

        [JsonPropertyName("estimatedDeliveryTime")]
        public DateTime? EstimatedDeliveryTime { get; set; }

        [JsonPropertyName("subTotal")]
        public decimal SubTotal { get; set; }

        [JsonPropertyName("totalTax")]
        public decimal? TotalTax { get; set; }

        [JsonPropertyName("placedOn")]
        public DateTime PlacedOn { get; set; }

        [JsonPropertyName("isPaid")]
        public bool IsPaid { get; set; }

        [JsonPropertyName("cancellationReason")]
        public string CancellationReason { get; set; }

        [JsonPropertyName("isScheduledOrder")]
        public bool IsScheduledOrder { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("fulfillmentType")]
        public string FulfillmentType { get; set; }

        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; }

        [JsonPropertyName("friendlyId")]
        public string FriendlyId { get; set; }

        [JsonPropertyName("tableName")]
        public string TableName { get; set; }

        [JsonPropertyName("tableId")]
        public int? TableId { get; set; }

        [JsonPropertyName("partnerId")]
        public string PartnerId { get; set; }

        [JsonPropertyName("estimatedPickupTime")]
        public DateTime? EstimatedPickupTime { get; set; }

        [JsonPropertyName("driverStatus")]
        public string DriverStatus { get; set; }

        [JsonPropertyName("customer")]
        public CustomerDto Customer { get; set; }

        [JsonPropertyName("delivery")]
        public DeliveryDto Delivery { get; set; }

        [JsonPropertyName("driver")]
        public DriverDto Driver { get; set; }

        [JsonPropertyName("discounts")]
        public List<DiscountDto> Discounts { get; set; }

        [JsonPropertyName("tax")]
        public List<TaxDto> Tax { get; set; }

        [JsonPropertyName("charges")]
        public List<ChargeDto> Charges { get; set; }

        [JsonPropertyName("payments")]
        public List<PaymentDto> Payments { get; set; }

        [JsonPropertyName("items")]
        public List<ItemDto> Items { get; set; }
    }
}
