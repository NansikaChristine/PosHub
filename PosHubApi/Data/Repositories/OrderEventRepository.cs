using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
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
        private readonly PosHubAuthDA _posHubAuthDA;
         private readonly string _baseUrl;
        private readonly WebhookEventDA _webhookEventDA;
        private readonly IMapper _mapper;

        public OrderEventRepository(HttpClient httpClient, IConfiguration configuration, ApiErrorDA apiErrorDA, OrderEventDA orderEventDA, IMapper mapper, LogsDA logsDA, PosHubAuthDA posHubAuthDA, WebhookEventDA webhookEventDA)
        {
            _httpClient = httpClient;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _apiErrorDA = apiErrorDA;
            _orderEventDA = orderEventDA;
            _posHubAuthDA = posHubAuthDA;
            _webhookEventDA = webhookEventDA;
            _mapper = mapper;
            _baseUrl = configuration.GetSection("PosHubUrl").Value;
            _logsDA = logsDA;
        }

        public async Task<OrderEventDto> UpdateOrderEventByOrderIdAsync(string orderId, string status, string cancellationReason, string apiCall)
        {
            OrderWebhookEventResponseDto existingDto = await _orderEventDA.GetOrderEventFromNewStateAsync(orderId, apiCall);
            if (existingDto == null || string.IsNullOrWhiteSpace(existingDto.NewState?.Id))
                return null;
            
            ClientsDto client = await _posHubAuthDA.GetClientDetailsByClientIdAsync(existingDto.ApplicationId, apiCall);

            existingDto.NewState.Status = status;
            existingDto.NewState.CancellationReason = cancellationReason;

            string jsonContent = JsonSerializer.Serialize(existingDto, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
            string url = $"http://localhost:5091/api/OrderEvent/UpdateOrderEvent/{orderId}";
            StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(url, content);
            
            // string url = $"{_baseUrl}/v1/accounts/{existingDto.AccountId}/locations/{existingDto.LocationId}/orders/{orderId}";
            // var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
            // {
            //     Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            // };
            // request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "");
            // HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                // OrderWebhookEventRequestDto existOrderDto = await _orderEventDA.GetOrderEventAsync(orderId, apiCall);
                // string reqForUpdate = await response.Content.ReadAsStringAsync();
                // OrderEventDto webhookEvent = JsonSerializer.Deserialize<OrderEventDto>(reqForUpdate);
                // existOrderDto.NewState = webhookEvent;
                // if (existOrderDto != null)
                // {
                //     await _webhookEventDA.OrderWebhookEvent(existOrderDto, apiCall);
                // }
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "UpdateOrderEventByOrderIdAsync",
                    IsSuccess = true,
                    FailMessage = "",
                    RequestBody = "",
                    ApplicationId = existingDto.ApplicationId,
                });
                Stream responseStream = await response.Content.ReadAsStreamAsync();

                OrderEventDto updatedDto = await JsonSerializer.DeserializeAsync<OrderEventDto>(responseStream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return updatedDto;
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "UpdateOrderEventByOrderIdAsync",
                    IsSuccess = false,
                    FailMessage = $"Unauthorized - {response.ReasonPhrase}. Response body: {errorContent}",
                    RequestBody = jsonContent,
                    ApplicationId = existingDto.ApplicationId
                });
                return null;
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "UpdateOrderEventByOrderIdAsync",
                    IsSuccess = false,
                    FailMessage = $"NotFound - {response.ReasonPhrase}. Response body: {errorContent}",
                    RequestBody = jsonContent,
                    ApplicationId = existingDto.ApplicationId
                });
                return null;
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "UpdateOrderEventByOrderIdAsync",
                    IsSuccess = false,
                    FailMessage = $"Failed to update order event. Status: {response.StatusCode}, Error: {error}",
                    RequestBody = jsonContent,
                    ApplicationId = existingDto.ApplicationId
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