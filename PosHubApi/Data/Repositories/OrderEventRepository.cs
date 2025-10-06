using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using PosHubApi.Data.DataAccess;
using PosHubApi.Data.Interfaces;
using PosHubApi.Dtos;
using PosHubApi.Models;

namespace PosHubApi.Data.Repositories
{
    public class OrderEventRepository : IOrderEventRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _connectionString;
        private readonly ApiErrorDA _apiErrorDA;
        private readonly OrderEventDA _orderEventDA;
        private readonly LogsDA _logsDA;
         private readonly string _baseUrl;
        private readonly IMapper _mapper;

        public OrderEventRepository(HttpClient httpClient, IConfiguration configuration, ApiErrorDA apiErrorDA, OrderEventDA orderEventDA, IMapper mapper, LogsDA logsDA)
        {
            _httpClient = httpClient;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _apiErrorDA = apiErrorDA;
            _orderEventDA = orderEventDA;
            _mapper = mapper;
            _baseUrl = configuration.GetSection("PosHubUrl").Value;
            _logsDA = logsDA;
        }

        public async Task<OrderEventDto> UpdateOrderEventByOrderIdAsync(string orderId, string status, string cancellationReason, string apiCall)
        {
            OrderWebhookEventResponseDto existingDto = await _orderEventDA.GetOrderEventFromNewStateAsync(orderId, apiCall);

            if (existingDto == null || string.IsNullOrWhiteSpace(existingDto.NewState?.Id))
            return null;

            existingDto.NewState.Status = status;
            existingDto.NewState.CancellationReason = cancellationReason;

            var jsonContent = JsonSerializer.Serialize(existingDto, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
            // string url = $"http://localhost:5091/api/OrderEvent/UpdateOrderEvent/{orderId}";
            string url = $"{_baseUrl}/v1/accounts/{existingDto.AccountId}/locations/{existingDto.LocationId}/orders/{orderId}";

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // var response = await _httpClient.PutAsync(url, content);
            var response = await _httpClient.PatchAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "Patch",
                    IsSuccess = true,
                    FailMessage = ""
                });
                var responseStream = await response.Content.ReadAsStreamAsync();

                var updatedDto = await JsonSerializer.DeserializeAsync<OrderEventDto>(responseStream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return updatedDto;
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "Patch",
                    IsSuccess = false,
                    FailMessage = $"NotFound - {response.ReasonPhrase}. Response body: {errorContent}"
                });
                return null;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "Patch",
                    IsSuccess = false,
                    FailMessage = $"Failed to update order event. Status: {response.StatusCode}, Error: {error}"
                });

                throw new Exception($"Failed to update order event. Status: {response.StatusCode}, Error: {error}");
            }
        }


        public async Task<OrderEventDto> UpdateOrderEventNewStateAsync(string orderId, OrderWebhookEventResponseDto updateDto, string apiCall)
        {
            string newStateJson = JsonSerializer.Serialize(updateDto.NewState, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            bool isUpdated = await _orderEventDA.UpdateNewStateByOrderIdAsync(orderId, newStateJson, apiCall);

            if (isUpdated)
                return updateDto.NewState;

            return null;
        }

    }
}