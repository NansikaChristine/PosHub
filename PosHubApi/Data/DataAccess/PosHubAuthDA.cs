using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using PosHubApi.Dtos;
using PosHubApi.Models;

namespace PosHubApi     .Data.DataAccess
{
    public class PosHubAuthDA
    {
        private readonly string _defaultConnectionString;
        private readonly ApiErrorDA _apiErrorDA;
        public PosHubAuthDA(IConfiguration configuration, ApiErrorDA apiErrorDA)
        {
            _defaultConnectionString = configuration.GetConnectionString("DefaultConnection");
            _apiErrorDA = apiErrorDA;
        }

        #region UpdateOrInsertTokenLog
        public async Task<bool> UpdateOrInsertTokenLog(TokenLogModel tokenLog, string apiCall)
        {
            string sql = @"
                    MERGE TokenLog AS target
                    USING (SELECT @ClientId AS ClientId, @ClientSecret AS ClientSecret) AS source
                    ON target.ClientId = source.ClientId AND target.ClientSecret = source.ClientSecret
                    WHEN MATCHED THEN 
                        UPDATE SET
                            Scope = @Scope,
                            GrantType = @GrantType,
                            AccessToken = @AccessToken,
                            RefreshToken = @RefreshToken,
                            TokenType = @TokenType,
                            ExpiresIn = @ExpiresIn,
                            RequestedAt = @RequestedAt,
                            ExpiresAt = @ExpiresAt
                    WHEN NOT MATCHED THEN
                        INSERT (ClientId, ClientSecret, Scope, GrantType, AccessToken, RefreshToken, TokenType, ExpiresIn, RequestedAt, ExpiresAt)
                        VALUES (@ClientId, @ClientSecret, @Scope, @GrantType, @AccessToken, @RefreshToken, @TokenType, @ExpiresIn, @RequestedAt, @ExpiresAt);";

            try
            {
                using (SqlConnection conn = new SqlConnection(_defaultConnectionString))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        // #region @ClientId
                        // SqlParameter param = command.CreateParameter();
                        // param.ParameterName = "@DeviceId";
                        // param.Value = tokenLog.ClientId;
                        // param.DbType = DbType.String;
                        // param.Size = 20;
                        // command.Parameters.Add(param);
                        // #endregion @ClientId
                        // #region @ClientId
                        // SqlParameter param = command.CreateParameter();
                        // param.ParameterName = "@DeviceId";
                        // param.Value = tokenLog.ClientId;
                        // param.DbType = DbType.String;
                        // param.Size = 20;
                        // command.Parameters.Add(param);
                        // #endregion @ClientId
                        cmd.Parameters.AddWithValue("@ClientId", tokenLog.ClientId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ClientSecret", tokenLog.ClientSecret ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Scope", tokenLog.Scope ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@GrantType", tokenLog.GrantType ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@AccessToken", tokenLog.AccessToken ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@RefreshToken", tokenLog.RefreshToken ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@TokenType", tokenLog.TokenType ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ExpiresIn", tokenLog.ExpiresIn);
                        cmd.Parameters.AddWithValue("@RequestedAt", tokenLog.RequestedAt);
                        cmd.Parameters.AddWithValue("@ExpiresAt", tokenLog.ExpiresAt);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
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
                    MethodName = nameof(UpdateOrInsertTokenLog),
                    ErrorOccurredDateTime = DateTime.Now
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                throw;
            }
        }
        #endregion UpdateOrInsertTokenLog

        #region UpdateOrInsertAccountLocation
        public async Task<bool> UpdateOrInsertAccountLocation(AccountLocationDto dto, string apiCall)
        {
            string sql = @"
                MERGE AccountLocation AS target
                USING (SELECT 
                        @AccountId AS AccountId,
                        @ApplicationId AS ApplicationId,
                        @LocationId AS LocationId
                    ) AS source
                ON target.AccountId = source.AccountId
                AND target.ApplicationId = source.ApplicationId
                AND target.LocationId = source.LocationId
                WHEN MATCHED THEN
                    UPDATE SET
                        ConnectionId = @ConnectionId,
                        Code = @Code,
                        UpdatedAt = GETDATE(),
                        Authorized = @Authorized,
                        AccessToken = @AccessToken,
                        RefreshToken = @RefreshToken
                WHEN NOT MATCHED THEN
                    INSERT (AccountId, LocationId, ApplicationId, ConnectionId, Code, CreatedAt, UpdatedAt, Authorized, AccessToken, RefreshToken)
                    VALUES (@AccountId, @LocationId, @ApplicationId, @ConnectionId, @Code, GETDATE(), GETDATE(), @Authorized, @AccessToken, @RefreshToken);";

            try
            {
                using (SqlConnection conn = new SqlConnection(_defaultConnectionString))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@AccountId", dto.AccountId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@LocationId", dto.LocationId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ApplicationId", dto.ApplicationId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ConnectionId", dto.ConnectionId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Code", dto.Code ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Authorized", dto.Authorized);
                        cmd.Parameters.AddWithValue("@AccessToken", dto.AccessToken ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@RefreshToken", dto.RefreshToken ?? (object)DBNull.Value);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
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
                    MethodName = nameof(UpdateOrInsertAccountLocation),
                    ErrorOccurredDateTime = DateTime.Now
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                throw;
            }
        }
        #endregion UpdateOrInsertAccountLocation

        #region  GetClientsDetails
        public async Task<List<ClientsDto>> GetClientsDetails(string apiCall)
        {
            List<ClientsDto> clients = new List<ClientsDto>();

            string Sql = @"
                SELECT c.ClientId,c.ClientName,c.ClientSecret,c.RedirectUrl,c.SyncUrl,al.CreatedAt,al.UpdatedAt,
                    al.AccountId,al.LocationId,c.ClientId,al.Authorized,al.ConnectionId
                FROM Clients c LEFT JOIN AccountLocation al
                ON c.ClientId = al.ApplicationId ";

            try
            {
                using (SqlConnection connection = new SqlConnection(_defaultConnectionString))
                {
                    #region Execute SQL
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(Sql, connection))
                    {
                        command.CommandTimeout = 60 * 60;

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            #region Fill category
                            while (await reader.ReadAsync())
                            {
                                {
                                    ClientsDto client = new ClientsDto();
                                    try { client.ClientId = reader.GetString(0); } catch { }
                                    try { client.ClientName = reader.GetString(1); } catch { }
                                    try { client.ClientSecret = reader.GetString(2); } catch { }
                                    try { client.RedirectUrl = reader.GetString(3); } catch { }
                                    try { client.SyncUrl = reader.GetString(4); } catch { }
                                    try { client.CreatedAt = reader.GetDateTime(5); } catch { }
                                    try { client.UpdatedAt = reader.GetDateTime(6); } catch { }
                                    try { client.AccountId = reader.GetString(7); } catch { }
                                    try { client.LocationId = reader.GetString(8); } catch { }
                                    try { client.ApplicationId = reader.GetString(9); } catch { }
                                    try { client.Authorized = reader.GetBoolean(10); } catch { }
                                    try { client.ConnectionId = reader.GetString(11); } catch { }

                                    clients.Add(client);
                                }
                            }
                            #endregion Fill category
                        }
                    }
                    return clients;
                    #endregion Execute SQL
                }
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
                    MethodName = nameof(GetClientsDetails),
                    ErrorOccurredDateTime = DateTime.Now
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                throw;
            }
        }
        
        #endregion  GetClientsDetails

        #region  GetClientDetailsByClientIdAsync
        public async Task<ClientsDto> GetClientDetailsByClientIdAsync(string applicationId, string apiCall)
        {
            ClientsDto client = new ClientsDto();
 
            string Sql = @"
                SELECT c.ClientId,c.ClientName,c.ClientSecret,c.RedirectUrl,c.SyncUrl,al.CreatedAt,al.UpdatedAt,
                    al.AccountId,al.LocationId,c.ClientId,al.Authorized,al.ConnectionId,al.AccessToken,al.RefreshToken,al.Code
                FROM Clients c LEFT JOIN AccountLocation al
                ON c.ClientId = al.ApplicationId
				where c.ClientId = @ApplicationId";

            #region Execute SQL
            try
            {
                using (SqlConnection connection = new SqlConnection(_defaultConnectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(Sql, connection))
                    {
                        #region Param
                        #region @ApplicationId
                        SqlParameter param  = new SqlParameter();
                        param.ParameterName="@ApplicationId";
                        param.Value=applicationId;
                        param.DbType=DbType.String;
                        param.Size=-1;
                        command.Parameters.Add(param);
                        #endregion @ApplicationId

                        #endregion Param

                        command.CommandTimeout = 60 * 60;

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            #region Fill category
                            while (await reader.ReadAsync())
                            {
                                try { client.ClientId = reader.GetString(0); } catch { }
                                try { client.ClientName = reader.GetString(1); } catch { }
                                try { client.ClientSecret = reader.GetString(2); } catch { }
                                try { client.RedirectUrl = reader.GetString(3); } catch { }
                                try { client.SyncUrl = reader.GetString(4); } catch { }
                                try { client.CreatedAt = reader.GetDateTime(5); } catch { }
                                try { client.UpdatedAt = reader.GetDateTime(6); } catch { }
                                try { client.AccountId = reader.GetString(7); } catch { }
                                try { client.LocationId = reader.GetString(8); } catch { }
                                try { client.ApplicationId = reader.GetString(9); } catch { }
                                try { client.Authorized = reader.GetBoolean(10); } catch { }
                                try { client.ConnectionId = reader.GetString(11); } catch { }
                                try { client.AccessToken = reader.GetString(12); } catch { }
                                try { client.RefreshToken = reader.GetString(13); } catch { }
                                try { client.Code = reader.GetString(14); } catch { }
                            }
                            #endregion Fill category
                        }
                    }
                    #endregion Execute SQL
                    return client;
                }
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
                    MethodName = nameof(GetClientDetailsByClientIdAsync),
                    ErrorOccurredDateTime = DateTime.Now
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                throw;
            }
        }
        
        #endregion  GetClientDetailsByClientIdAsync
    }
}