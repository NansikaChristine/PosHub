using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PosHubApi.Data.DataAccess;
using PosHubApi.Data.Interfaces;
using PosHubApi.Dtos;
using PosHubApi.Models;

namespace PosHubApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderEventController : ControllerBase
    {
        private readonly IOrderEventRepository _orderEventRepository;
         private readonly ApiErrorDA _apiErrorDA;

        public OrderEventController(IOrderEventRepository orderEventRepository, ApiErrorDA apiErrorDA)
        {
            _orderEventRepository = orderEventRepository;
            _apiErrorDA = apiErrorDA;
        }

        [HttpPut("updateOrderEventByOrderId")]
        public async Task<IActionResult> UpdateOrderEventByOrderId([FromBody] UpdateOrderEventRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.OrderId))
                return BadRequest("OrderId is required.");
            string apiCall = $"OrderEvent/updateOrderEventByOrderId";
            try
            {
                OrderEventDto orderEvent = await _orderEventRepository.UpdateOrderEventByOrderIdAsync(dto.OrderId, dto.Status, dto.CancellationReason, apiCall);

                if (orderEvent == null || string.IsNullOrWhiteSpace(orderEvent.Id))
                    return NotFound(new { Message = $"No order event found for OrderId: {dto.OrderId}" });

                return Ok(orderEvent);
            }
            catch (Exception ex)
            {
                ApiErrorMessageModel error = new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(UpdateOrderEventByOrderId),
                    ErrorOccurredDateTime = DateTime.Now
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return StatusCode(500, new
                {
                    Error = "Internal Server Error",
                    Message = ex.Message
                });
            }
        }
        

        [HttpPut("UpdateOrderEvent/{orderId}")]
        public async Task<IActionResult> UpdateOrderEvent(string orderId, [FromBody]  OrderWebhookEventResponseDto updateDto)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                return BadRequest("Order ID is required.");

            if (updateDto == null)
                return BadRequest("Update payload is required.");

            try
            {
                OrderEventDto result = await _orderEventRepository.UpdateOrderEventNewStateAsync(orderId, updateDto, $"PUT /api/order-event/{orderId}");

                if (result == null || string.IsNullOrWhiteSpace(result.Id))
                    return NotFound($"Order with ID {orderId} was not found.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An unexpected error occurred.",
                    Error = ex.Message
                });
            }
        }
    }
}