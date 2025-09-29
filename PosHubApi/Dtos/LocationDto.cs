using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PosHubApi.Dtos
{
    public class LocationDto
    {
        public string Name { get; set; }
        public string PosReference { get; set; }
        public string DeliveryDefaultPickupInstructions { get; set; }

        public List<TaxRateDto> TaxRates { get; set; }
        public List<PriceListDto> PriceLists { get; set; }
        public List<PaymentTypeDto> PaymentTypes { get; set; }

        public ServiceAvailabilityDto Availability { get; set; }
        public List<ServiceAvailabilityDto> BusinessHours { get; set; }
        public List<ServiceAvailabilityDto> Delivery { get; set; }
        public List<ServiceAvailabilityDto> Collection { get; set; }
    }
}