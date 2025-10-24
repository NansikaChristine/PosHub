using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using PosHubApi.Dtos;
using PosHubApi.Models;

namespace PosHubApi.Data.DataAccess
{
    public class WebhookEventDA
    {
        private readonly string _defaultConnectionString;
        private readonly ApiErrorDA _apiErrorDA;
        public WebhookEventDA(IConfiguration configuration, ApiErrorDA apiErrorDA)
        {
            _defaultConnectionString = configuration.GetConnectionString("DefaultConnection");
            _apiErrorDA = apiErrorDA;
        }

        #region OrderWebhookEvent
        public async Task<bool> OrderWebhookEvent(OrderWebhookEventRequestDto xWebhookRequest, string apiCall)
        {
            string sql = @"
                        IF EXISTS (SELECT 1 FROM OrderWebhookEvents WHERE EventId = @EventId and OrderId = @OrderId)
                        BEGIN
                            UPDATE OrderWebhookEvents
                            SET
                                AccountId = @AccountId,
                                ClientId = @ClientId,
                                LocationId = @LocationId,
                                EventTime = @EventTime,
                                ConnectionId = @ConnectionId,
                                EventType = @EventType,
                                ObjectType = @ObjectType,
                                NewState = @NewState,
                                PreviousState = @PreviousState,
                                Status = @Status,
                                SourceName =@SourceName,
                                Total = @Total
                            WHERE EventId = @EventId and OrderId = @OrderId and ObjectType = 'ORDER' 
                        END
                        ELSE IF @ObjectType = 'ORDER' 
                        BEGIN
                            INSERT INTO OrderWebhookEvents (
                                EventId, AccountId, ClientId, OrderId, LocationId,
                                EventTime, ConnectionId, EventType, ObjectType,
                                NewState, PreviousState, Status, SourceName, Total
                            )
                            VALUES (
                                @EventId, @AccountId, @ClientId, @OrderId, @LocationId,
                                @EventTime, @ConnectionId, @EventType, @ObjectType,
                                @NewState, @PreviousState, @Status, @SourceName, @Total
                            )
                        END  ";


            using (SqlConnection conn = new SqlConnection(_defaultConnectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@EventId", xWebhookRequest.EventId);
                    cmd.Parameters.AddWithValue("@AccountId", xWebhookRequest.AccountId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ClientId", xWebhookRequest.ClientId);
                    cmd.Parameters.AddWithValue("@OrderId", xWebhookRequest.OrderId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LocationId", xWebhookRequest.LocationId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EventTime", xWebhookRequest.EventTime);
                    cmd.Parameters.AddWithValue("@ConnectionId", xWebhookRequest.ConnectionId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EventType", xWebhookRequest.EventType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ObjectType", xWebhookRequest.ObjectType ?? (object)DBNull.Value);

                    string newStateJson = xWebhookRequest.NewState != null
                        ? JsonSerializer.Serialize(xWebhookRequest.NewState)
                        : null;

                    string new_StateJson = xWebhookRequest.New_State != null
                        ? JsonSerializer.Serialize(xWebhookRequest.New_State)
                        : null;

                    string prevStateJson = xWebhookRequest.PreviousState != null
                        ? JsonSerializer.Serialize(xWebhookRequest.PreviousState)
                        : null;

                    string prev_StateJson = xWebhookRequest.Previous_State != null
                        ? JsonSerializer.Serialize(xWebhookRequest.Previous_State)
                        : null;

                    newStateJson = string.IsNullOrWhiteSpace(newStateJson) ? new_StateJson : newStateJson;
                    prevStateJson = string.IsNullOrWhiteSpace(prevStateJson) ? prev_StateJson : prevStateJson;

                    string status = null;
                    string sourceName = null;
                    decimal? total = null;

                    if (!string.IsNullOrWhiteSpace(newStateJson))
                    {
                        try
                        {
                            JObject jsonObj = JObject.Parse(newStateJson);

                            status = (string)jsonObj["status"];
                            sourceName = (string)jsonObj["sourceName"];
                            total = (decimal?)jsonObj["total"];
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("JSON Parse Error: " + ex.Message);
                        }
                    }

                    cmd.Parameters.AddWithValue("@NewState", (object)newStateJson ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PreviousState", (object)prevStateJson ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", status ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SourceName", sourceName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Total", (total/100) ?? (object)DBNull.Value);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }

        }
        #endregion OrderWebhookEvent

    }
}