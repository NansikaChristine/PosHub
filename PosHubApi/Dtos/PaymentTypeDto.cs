using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PosHubApi.Dtos
{
    public class PaymentTypeDto
    {
        public bool IsDefault { get; set; }
        public string Code { get; set; } // CASH, CARD, GIFT_CARD, OTHER
        public string Name { get; set; }
        public string PosReference { get; set; }
        public bool IsActive { get; set; }
    }
}