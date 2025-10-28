using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        public async Task<bool> UpdateOrderEventByOrderIdAsync(string orderId, string status, string cancellationReason, string apiCall)
        {
            OrderWebhookEventResponseDto existingDto = await _orderEventDA.GetOrderEventFromNewStateAsync(orderId, apiCall);
            if (existingDto == null || string.IsNullOrWhiteSpace(existingDto.NewState?.Id))
                return false;

            ClientsDto client = await _posHubAuthDA.GetClientDetailsByClientIdAsync(existingDto.ApplicationId, apiCall);

            UpdateOrderEventRequestDto dto = new UpdateOrderEventRequestDto
            {
                Notes = existingDto.NewState.Notes,
                OrderNumber = existingDto.NewState.OrderNumber,
                SourceDeviceType = "POS",
                Timezone = existingDto.NewState.Timezone,
                EstimatedDeliveryTime = existingDto.NewState.EstimatedDeliveryTime,
                Payments = existingDto.NewState.Payments ?? new List<PaymentDto>(),
                SubTotal = existingDto.NewState.SubTotal,
                DriverStatus = "ASSIGNED",
                FulfillmentType = existingDto.NewState.FulfillmentType,
                TableName = existingDto.NewState.TableName,
                TotalTax = existingDto.NewState.TotalTax,
                Total = existingDto.NewState.Total,
                Discounts = existingDto.NewState.Discounts ?? new List<DiscountDto>(),
                Currency = existingDto.NewState.Currency,
                EstimatedPickupTime = existingDto.NewState.EstimatedPickupTime,
                Delivery = existingDto.NewState.Delivery,
                CancellationReason = IsValidCancellationReason(cancellationReason) ? cancellationReason : null,
                Tax = existingDto.NewState.Tax ?? new List<TaxDto>(),
                FriendlyId = string.IsNullOrWhiteSpace(existingDto.NewState.FriendlyId) ? string.Empty : existingDto.NewState.FriendlyId,
                PlacedOn = existingDto.NewState.PlacedOn,
                IsPaid = existingDto.NewState.IsPaid,
                Charges = existingDto.NewState.Charges ?? new List<ChargeDto>(),
                Driver = existingDto.NewState.Driver,
                IsScheduledOrder = existingDto.NewState.IsScheduledOrder,
                TableId = existingDto.NewState.TableId,
                PartnerId = existingDto.NewState.PartnerId,
                SourceName = existingDto.NewState.SourceName,
                Items = existingDto.NewState.Items ?? new List<ItemDto>(),
                Customer = existingDto.NewState.Customer,
                Status = status
            };

            string jsonContent = JsonSerializer.Serialize(dto, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
            // string url = $"http://localhost:5091/api/OrderEvent/UpdateOrderEvent/{orderId}";
            // StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            // HttpResponseMessage response = await _httpClient.PutAsync(url, content);

            string url = $"{_baseUrl}/v1/accounts/{existingDto.AccountId}/locations/{existingDto.LocationId}/orders/{orderId}";
            // Console.WriteLine(url);
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", client.AccessToken);
            HttpResponseMessage response = await _httpClient.SendAsync(request);

            // Console.WriteLine(jsonContent);

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
                    UniqueId = orderId
                });

                return true;
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
                    ApplicationId = existingDto.ApplicationId,
                    UniqueId = orderId
                });
                return false;
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
                    ApplicationId = existingDto.ApplicationId,
                    UniqueId = orderId
                });
                return false;
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
                    ApplicationId = existingDto.ApplicationId,
                    UniqueId = orderId
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

        private bool IsValidCancellationReason(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) return false;

            string[] validReasons = new[] {
                "OUT_OF_STOCK",
                "STORE_CLOSED",
                "TOO_BUSY",
                "CUSTOMER_CANCELLED",
                "OTHER"
            };

            return validReasons.Contains(reason.Trim().ToUpperInvariant());
        }


    }
}