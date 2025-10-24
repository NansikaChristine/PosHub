using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using PosHubApi.Data.DataAccess;
using PosHubApi.Data.Interfaces;
using PosHubApi.Dtos;
using PosHubApi.Models;

namespace PosHubApi.Data.Repositories
{
    public class LocationOrdersRepository :ILocationOrdersRepository
    {
        private readonly HttpClient _httpClient;
        private readonly PosHubAuthDA _posHubAuthDA;
        private readonly ApiErrorDA _apiErrorDA;
        private readonly LogsDA _logsDA;
        private readonly string _baseUrl;

        public LocationOrdersRepository(HttpClient httpClient, IConfiguration configuration,
         PosHubAuthDA posHubAuthDA, ApiErrorDA apiErrorDA, LogsDA logsDA)
        {
            _httpClient = httpClient;
            _posHubAuthDA = posHubAuthDA;
            _apiErrorDA = apiErrorDA;
            _logsDA = logsDA;
            _baseUrl = configuration.GetSection("PosHubUrl").Value;

        }

        public async Task<OrderEventDto> GetOrderByOrderId(string applicationId, string orderId, string apiCall)
        {
            ClientsDto client = await _posHubAuthDA.GetClientDetailsByClientIdAsync(applicationId, apiCall);
            string url = $"{_baseUrl}/v1/accounts/{client.AccountId}/locations/{client.LocationId}/orders/{orderId}";
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", client.AccessToken);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync();
                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = url,
                        Event = "GetOrderByOrderId",
                        IsSuccess = false,
                        FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}",
                        RequestBody = body,
                        ApplicationId = applicationId,
                        UniqueId = orderId

                    });
                    await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                    {
                        ErrorMessage = $"API call failed with status {response.StatusCode}. Body: {body}",
                        ApiCall = apiCall,
                        MethodName = nameof(GetOrderByOrderId),
                        ErrorOccurredDateTime = DateTime.Now,
                        ClientId = applicationId
                    });
                    return new OrderEventDto();
                }

                string json = await response.Content.ReadAsStringAsync();

                OrderResponseDto orderResponse = JsonSerializer.Deserialize<OrderResponseDto>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "GetOrderByOrderId",
                    IsSuccess = true,
                    FailMessage = "",
                    RequestBody = "",
                    ApplicationId = applicationId,
                    UniqueId = orderId
                });
                return orderResponse.Data ?? new OrderEventDto();
            }
            catch (Exception ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "GetOrderByOrderId",
                    IsSuccess = false,
                    FailMessage = ex.Message,
                    RequestBody = "",
                    ApplicationId = applicationId,
                    UniqueId = orderId
                });
                ApiErrorMessageModel error = new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(GetOrderByOrderId),
                    ErrorOccurredDateTime = DateTime.Now,
                    ClientId = applicationId
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return new OrderEventDto();
            }
        }
    
    }
}