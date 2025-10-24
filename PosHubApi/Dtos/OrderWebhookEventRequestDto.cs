using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class OrderWebhookEventRequestDto
    {
        [JsonPropertyName("eventId")]
        public string EventId { get; set; }


        [JsonPropertyName("accountId")]
        public string AccountId { get; set; }


        [JsonPropertyName("clientId")]
        public string ClientId { get; set; }


        [JsonPropertyName("orderId")]
        public string OrderId { get; set; }


        [JsonPropertyName("locationId")]
        public string LocationId { get; set; }


        [JsonPropertyName("eventTime")]
        public DateTime? EventTime { get; set; }


        [JsonPropertyName("connectionId")]
        public string ConnectionId { get; set; }


        [JsonPropertyName("eventType")]
        public string EventType { get; set; }


        [JsonPropertyName("objectType")]
        public string ObjectType { get; set; }


        [JsonPropertyName("newState")]
        public OrderEventDto NewState { get; set; }

        [JsonPropertyName("new_state")]
        public OrderEventDto New_State { get; set; }


        [JsonPropertyName("previousState")]
        public OrderEventDto PreviousState { get; set; }

        [JsonPropertyName("previous_state")]
        public OrderEventDto Previous_State { get; set; }
    }
}