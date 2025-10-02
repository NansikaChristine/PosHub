using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class LocationDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("posReference")]
        public string PosReference { get; set; }

        [JsonPropertyName("deliveryDefaultPickupInstructions")]
        public string DeliveryDefaultPickupInstructions { get; set; }

        [JsonPropertyName("taxRates")]
        public List<TaxRateDto> TaxRates { get; set; }

        [JsonPropertyName("priceLists")]
        public List<PriceListDto> PriceLists { get; set; }

        [JsonPropertyName("paymentTypes")]
        public List<PaymentTypeDto> PaymentTypes { get; set; }

        [JsonPropertyName("availability")]
        public ServiceAvailabilityDto Availability { get; set; }

        [JsonPropertyName("businessHours")]
        public List<ServiceAvailabilityDto> BusinessHours { get; set; }

        [JsonPropertyName("delivery")]
        public List<ServiceAvailabilityDto> Delivery { get; set; }

        [JsonPropertyName("collection")]
        public List<ServiceAvailabilityDto> Collection { get; set; }
    }
}