
using System.Text.Json;
using Microsoft.Data.SqlClient;
using PosHubApi.Dtos;
using PosHubApi.Models;

namespace PosHubApi.Data.DataAccess
{
    public class OrderEventDA
    {
        private readonly string _defaultConnectionString;
        private readonly ApiErrorDA _apiErrorDA;
        public OrderEventDA(IConfiguration configuration, ApiErrorDA apiErrorDA)
        {
            _defaultConnectionString = configuration.GetConnectionString("DefaultConnection");
            _apiErrorDA = apiErrorDA;
        }

        #region GetOrderEventFromNewState
        public async Task<OrderWebhookEventResponseDto> GetOrderEventFromNewStateAsync(string orderId, string apiCall)
        {
            string sql = @"
                SELECT TOP 1 [NewState], AccountId, LocationId, ClientId
                FROM [dbo].[OrderWebhookEvents] WITH (NOLOCK)
                WHERE [OrderId] = @OrderId AND [NewState] IS NOT NULL";

            try
            {
                using (SqlConnection connection = new SqlConnection(_defaultConnectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandTimeout = 60 * 60;
                        command.Parameters.AddWithValue("@OrderId", orderId);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string json = reader.IsDBNull(0) ? null : reader.GetString(0);
                                string accountId = reader.IsDBNull(1) ? null : reader.GetString(1);
                                string locationId = reader.IsDBNull(2) ? null : reader.GetString(2);
                                string applicationId = reader.IsDBNull(3) ? null : reader.GetString(3);

                                if (!string.IsNullOrWhiteSpace(json))
                                {
                                    try
                                    {
                                        OrderEventDto orderEvent = JsonSerializer.Deserialize<OrderEventDto>(json, new JsonSerializerOptions
                                        {
                                            PropertyNameCaseInsensitive = true
                                        });

                                        return new OrderWebhookEventResponseDto
                                        {
                                            AccountId = accountId,
                                            LocationId = locationId,
                                            NewState = orderEvent ?? new OrderEventDto(),
                                            ApplicationId = applicationId
                                        };
                                    }
                                    catch (Exception deserializationEx)
                                    {
                                        await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                                        {
                                            ErrorMessage = deserializationEx.Message,
                                            ErrorSource = deserializationEx.Source,
                                            StackTrace = deserializationEx.StackTrace,
                                            InnerErrorMessage = deserializationEx.InnerException?.Message ?? "",
                                            ApiCall = apiCall,
                                            MethodName = nameof(GetOrderEventFromNewStateAsync),
                                            ErrorOccurredDateTime = DateTime.Now,
                                            ClientId = applicationId
                                        });

                                        throw;
                                    }
                                }
                            }

                            return new OrderWebhookEventResponseDto
                            {
                                AccountId = null,
                                LocationId = null,
                                NewState = new OrderEventDto(),
                                ApplicationId = ""
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(GetOrderEventFromNewStateAsync),
                    ErrorOccurredDateTime = DateTime.Now
                });

                throw;
            }
        }

        #endregion GetOrderEventsFromNewState

        #region UpdateNewStateByOrderIdAsync
        public async Task<bool> UpdateNewStateByOrderIdAsync(string orderId, string newStateJson, string apiCall)
        {
            string sql = @"
                UPDATE [dbo].[OrderWebhookEvents]
                SET [NewState] = @NewState, [UpdatedAt] = getdate()
                WHERE [OrderId] = @OrderId";

            try
            {
                using (SqlConnection connection = new SqlConnection(_defaultConnectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@OrderId", orderId);
                        command.Parameters.AddWithValue("@NewState", newStateJson ?? (object)DBNull.Value);

                        int affectedRows = await command.ExecuteNonQueryAsync();
                        return affectedRows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(UpdateNewStateByOrderIdAsync),
                    ErrorOccurredDateTime = DateTime.Now
                });

                throw;
            }
        }
        #endregion
        
        #region GetOrderEvent
        public async Task<OrderWebhookEventRequestDto> GetOrderEventAsync(string orderId, string apiCall)
        {
            string sql = @"
                SELECT TOP 1 [NewState], AccountId, LocationId, ClientId, EventId, EventTime, ConnectionId, EventType,
                [PreviousState], ObjectType 
                FROM [dbo].[OrderWebhookEvents] WITH (NOLOCK)
                WHERE [OrderId] = @OrderId AND [NewState] IS NOT NULL";

            try
            {
                using (SqlConnection connection = new SqlConnection(_defaultConnectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandTimeout = 60 * 60;
                        command.Parameters.AddWithValue("@OrderId", orderId);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string jsonNewState = reader.IsDBNull(0) ? null : reader.GetString(0);
                                string accountId = reader.IsDBNull(1) ? null : reader.GetString(1);
                                string locationId = reader.IsDBNull(2) ? null : reader.GetString(2);
                                string clientId = reader.IsDBNull(3) ? null : reader.GetString(3);
                                string eventId = reader.IsDBNull(4) ? null : reader.GetString(4);
                                DateTime? eventTime = reader.IsDBNull(5) ? null : reader.GetDateTime(5);
                                string connectionId = reader.IsDBNull(6) ? null : reader.GetString(6);
                                string eventType = reader.IsDBNull(7) ? null : reader.GetString(7);
                                string jsonPreviousState = reader.IsDBNull(8) ? null : reader.GetString(8);
                                string objectType = reader.IsDBNull(9) ? null : reader.GetString(9);

                                if (!string.IsNullOrWhiteSpace(jsonNewState))
                                {
                                    try
                                    {
                                        OrderEventDto orderEventNewState = JsonSerializer.Deserialize<OrderEventDto>(jsonNewState, new JsonSerializerOptions
                                        {
                                            PropertyNameCaseInsensitive = true
                                        });
                                        OrderEventDto orderEventPreviousState = JsonSerializer.Deserialize<OrderEventDto>(jsonPreviousState, new JsonSerializerOptions
                                        {
                                            PropertyNameCaseInsensitive = true
                                        });

                                        return new OrderWebhookEventRequestDto
                                        {
                                            AccountId = accountId,
                                            LocationId = locationId,
                                            NewState = orderEventNewState ?? new OrderEventDto(),
                                            ClientId = clientId,
                                            EventId = eventId,
                                            EventTime = eventTime,
                                            ConnectionId = connectionId,
                                            EventType = eventType,
                                            PreviousState = orderEventPreviousState ?? new OrderEventDto(),
                                            ObjectType = objectType,
                                            OrderId = orderId
                                        };
                                    }
                                    catch (Exception deserializationEx)
                                    {
                                        await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                                        {
                                            ErrorMessage = deserializationEx.Message,
                                            ErrorSource = deserializationEx.Source,
                                            StackTrace = deserializationEx.StackTrace,
                                            InnerErrorMessage = deserializationEx.InnerException?.Message ?? "",
                                            ApiCall = apiCall,
                                            MethodName = nameof(GetOrderEventFromNewStateAsync),
                                            ErrorOccurredDateTime = DateTime.Now,
                                            ClientId = clientId
                                        });

                                        throw;
                                    }
                                }
                            }

                            return new OrderWebhookEventRequestDto
                            {
                                AccountId = null,
                                LocationId = null,
                                NewState = new OrderEventDto(),
                                ClientId = null,
                                EventId = null,
                                EventTime = null,
                                ConnectionId = null,
                                EventType = null,
                                PreviousState = new OrderEventDto(),
                                ObjectType = null,
                                OrderId = null
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(GetOrderEventFromNewStateAsync),
                    ErrorOccurredDateTime = DateTime.Now
                });

                throw;
            }
        }
        #endregion GetOrderEvent

    }
}