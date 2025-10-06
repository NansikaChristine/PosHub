using PosHubApi.Dtos;

namespace PosHubApi.Data.Interfaces
{
    public interface IOrderEventRepository
    {
        Task<OrderEventDto> UpdateOrderEventByOrderIdAsync(string orderId, string status, string cancellationReason, string apiCall);
        Task<OrderEventDto> UpdateOrderEventNewStateAsync(string orderId, OrderWebhookEventResponseDto updateDto, string apiCall);
    }
}