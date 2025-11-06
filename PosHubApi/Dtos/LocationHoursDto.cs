using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Text.Json.Serialization;

namespace PosHubApi.Dtos
{
    public class LocationHoursDto
    {
        [JsonPropertyName("businessHours")]
        public List<ServiceAvailabilityDto> BusinessHours { get; set; }
    }
}