using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PosHubApi.Dtos;

namespace PosHubApi.Data.Interfaces
{
    public interface ILocationOrdersRepository
    {
        Task<OrderEventDto> GetOrderByOrderId(string applicationId, string orderId, string apiCall);
        
    }
}