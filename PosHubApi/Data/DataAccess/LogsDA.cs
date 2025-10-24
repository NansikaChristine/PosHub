using System;
using System.Collections.Generic;
using System.Data;
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

        #region InsertOrUpdateApiError
        public async Task InsertLogAsync(LogModel log)
        {
            using (SqlConnection conn = new SqlConnection(_defaultConnectionString))
            {
                await conn.OpenAsync();

                string checkQuery = @"
                        SELECT TOP 1 Id, Count
                        FROM Logs with (nolock)
                        WHERE Url = @Url AND Event = @Event 
                        AND ApplicationId = @ApplicationId AND UniqueId = @UniqueId AND FailMessage = @FailMessage ";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.Add(new SqlParameter("@Url", SqlDbType.NVarChar) { Value = (object)log.Url ?? DBNull.Value });
                    checkCmd.Parameters.Add(new SqlParameter("@Event", SqlDbType.NVarChar) { Value = (object)log.Event ?? DBNull.Value });
                    checkCmd.Parameters.Add(new SqlParameter("@ApplicationId", SqlDbType.NVarChar) { Value = (object)log.ApplicationId ?? DBNull.Value });
                    checkCmd.Parameters.Add(new SqlParameter("@UniqueId", SqlDbType.NVarChar) { Value = (object)log.UniqueId ?? DBNull.Value });
                    checkCmd.Parameters.Add(new SqlParameter("@FailMessage", SqlDbType.NVarChar) { Value = (object)log.FailMessage ?? DBNull.Value });

                    long existingId = 0;
                    int existingCount = 0;

                    using (SqlDataReader reader = await checkCmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            existingId = reader.GetInt64(0);
                            existingCount = reader.GetInt32(1);
                        }
                    }

                    if (existingId > 0)
                    {
                        string updateQuery = @"
                        UPDATE Logs SET 
                        [IsSuccess] = @IsSuccess,
                        [Failmessage] = @Failmessage,
                        [RequestModel] = @RequestModel,
                        [UpdatedAt] = @UpdatedAt,
                        [Count] = @Count
                        WHERE Id = @Id";

                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@Count", existingCount + 1 );
                            updateCmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now );
                            updateCmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = existingId });
                            updateCmd.Parameters.Add(new SqlParameter("@RequestModel", SqlDbType.NVarChar) { Value = log.RequestBody });
                            updateCmd.Parameters.Add(new SqlParameter("@FailMessage", SqlDbType.NVarChar) { Value = (object)log.FailMessage ?? DBNull.Value });
                            updateCmd.Parameters.AddWithValue("@IsSuccess", log.IsSuccess);

                            await updateCmd.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        string insertQuery = @"
                        INSERT INTO Logs ([Url], [Event], [IsSuccess], [Failmessage], [RequestModel], [ApplicationId], [UniqueId], [Count])
                        VALUES (@Url, @Event, @IsSuccess, @Failmessage, @RequestModel, @ApplicationId, @UniqueId, @Count); ";

                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@Url", (object)log.Url ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@Event", (object)log.Event ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@IsSuccess", log.IsSuccess);
                            insertCmd.Parameters.AddWithValue("@Failmessage", (object)log.FailMessage ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@RequestModel", log.RequestBody);
                            insertCmd.Parameters.AddWithValue("@ApplicationId", log.ApplicationId);
                            insertCmd.Parameters.AddWithValue("@UniqueId", (object)log.UniqueId ?? DBNull.Value );
                            insertCmd.Parameters.AddWithValue("@Count", 1 );

                            await insertCmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
        }
        #endregion InsertOrUpdateApiError
    }
}