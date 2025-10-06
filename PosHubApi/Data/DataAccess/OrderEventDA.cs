
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
                SELECT TOP 1 [NewState], AccountId, LocationId
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

                                if (!string.IsNullOrWhiteSpace(json))
                                {
                                    try
                                    {
                                        var orderEvent = JsonSerializer.Deserialize<OrderEventDto>(json, new JsonSerializerOptions
                                        {
                                            PropertyNameCaseInsensitive = true
                                        });

                                        return new OrderWebhookEventResponseDto
                                        {
                                            AccountId = accountId,
                                            LocationId = locationId,
                                            NewState = orderEvent ?? new OrderEventDto()
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
                                            ErrorOccurredDateTime = DateTime.Now
                                        });

                                        throw;
                                    }
                                }
                            }

                            return new OrderWebhookEventResponseDto
                            {
                                AccountId = null,
                                LocationId = null,
                                NewState = new OrderEventDto()
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

    }
}