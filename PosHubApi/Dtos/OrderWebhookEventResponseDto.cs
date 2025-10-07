using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PosHubApi.Dtos
{
    public class OrderWebhookEventResponseDto
    {
        [JsonPropertyName("accountId")]
        public string AccountId { get; set; }

        
        [JsonPropertyName("locationId")]
        public string LocationId { get; set; }

        [JsonPropertyName("clientId")]
        public string ApplicationId { get; set; }


        [JsonPropertyName("newState")]
        public OrderEventDto NewState { get; set; }


    }
}