using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using PosHubApi.Models;

namespace PosHubApi.Data.DataAccess
{
    public class LogsDA
    {
        private readonly string _defaultConnectionString;
        private readonly ApiErrorDA _apiErrorDA;
        public LogsDA(IConfiguration configuration, ApiErrorDA apiErrorDA)
        {
            _defaultConnectionString = configuration.GetConnectionString("DefaultConnection");
            _apiErrorDA = apiErrorDA;
        }

        public async Task<bool> InsertLogAsync(LogModel log)
        {
            const string sql = @"
                MERGE [dbo].[Logs] AS target
                USING (SELECT @Event AS Event, @ApplicationId AS ApplicationId) AS source
                ON target.Event = source.Event AND target.ApplicationId = source.ApplicationId
                WHEN MATCHED THEN
                    UPDATE SET 
                        [Url] = @Url,
                        [IsSuccess] = @IsSuccess,
                        [Failmessage] = @Failmessage,
                        [RequestModel] = @RequestModel,
                        [InsertedAt] = GetDate()
                WHEN NOT MATCHED THEN
                    INSERT ([Url], [Event], [IsSuccess], [Failmessage], [RequestModel], [ApplicationId])
                    VALUES (@Url, @Event, @IsSuccess, @Failmessage, @RequestModel, @ApplicationId); ";

            try
            {
                using (SqlConnection connection = new SqlConnection(_defaultConnectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Url", (object)log.Url ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Event", (object)log.Event ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IsSuccess", log.IsSuccess);
                        command.Parameters.AddWithValue("@Failmessage", (object)log.FailMessage ?? DBNull.Value);
                        command.Parameters.AddWithValue("@RequestModel", log.RequestBody);
                        command.Parameters.AddWithValue("@ApplicationId", log.ApplicationId);

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
                    ApiCall = "InsertLog",
                    MethodName = nameof(InsertLogAsync),
                    ErrorOccurredDateTime = DateTime.Now
                });

                throw;
            }
        }

    }
}