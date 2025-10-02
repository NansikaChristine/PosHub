using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PosHubApi.Dtos;

namespace PosHubApi.Data.Interfaces
{
    public interface IWebhookEventRepository
    {
        Task<bool> OrderWebhookEvent(OrderWebhookEventRequestDto xWebhookRequest, string apiCall);
        Task<bool> ValidateSignature(string xWebhookSignature, string body, string apiCall);
        
    }
}