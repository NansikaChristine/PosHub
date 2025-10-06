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
                INSERT INTO [dbo].[Logs] ([Url], [Event], [IsSuccess], [Failmessage])
                VALUES (@Url, @Event, @IsSuccess, @Failmessage);";

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