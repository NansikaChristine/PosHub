using System.Data;
using Microsoft.Data.SqlClient;
using PosHubApi.Models;

namespace PosHubApi.Data.DataAccess
{
    public class ApiErrorDA
    {

        private readonly string _defaultConnectionString;

        public ApiErrorDA(IConfiguration configuration)
        {
            _defaultConnectionString = configuration.GetConnectionString("DefaultConnection");
        }

        #region InsertOrUpdateApiError
        public async Task InsertOrUpdateApiErrorAsync(ApiErrorMessageModel error)
        {
            using (SqlConnection conn = new SqlConnection(_defaultConnectionString))
            {
                await conn.OpenAsync();

                string checkQuery = @"
                        SELECT TOP 1 Id, Count
                        FROM ApiErrorLogs with (nolock)
                        WHERE ErrorMessage = @ErrorMessage AND ErrorSource = @ErrorSource 
                        AND StackTrace = @StackTrace AND MethodName = @MethodName AND ClientId = @ClientId ";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.Add(new SqlParameter("@ErrorMessage", SqlDbType.NVarChar) { Value = (object)error.ErrorMessage ?? DBNull.Value });
                    checkCmd.Parameters.Add(new SqlParameter("@ErrorSource", SqlDbType.NVarChar) { Value = (object)error.ErrorSource ?? DBNull.Value });
                    checkCmd.Parameters.Add(new SqlParameter("@StackTrace", SqlDbType.NVarChar) { Value = (object)error.StackTrace ?? DBNull.Value });
                    checkCmd.Parameters.Add(new SqlParameter("@MethodName", SqlDbType.NVarChar) { Value = (object)error.MethodName ?? DBNull.Value });
                    checkCmd.Parameters.Add(new SqlParameter("@ClientId", SqlDbType.NVarChar) { Value = (object)error.ClientId ?? DBNull.Value });

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
                                UPDATE ApiErrorLogs
                                SET Count = @Count,
                                    ErrorOccurredDateTime = @ErrorOccurredDateTime
                                WHERE Id = @Id";

                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                        {
                            updateCmd.Parameters.Add(new SqlParameter("@Count", SqlDbType.Int) { Value = existingCount + 1 });
                            updateCmd.Parameters.Add(new SqlParameter("@ErrorOccurredDateTime", SqlDbType.DateTime) { Value = DateTime.Now });
                            updateCmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = existingId });

                            await updateCmd.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        string insertQuery = @"
                                INSERT INTO ApiErrorLogs
                                (Count, ErrorMessage, ErrorSource, StackTrace, InnerErrorMessage, ApiCall, MethodName, ErrorOccurredDateTime, ClientId)
                                VALUES
                                (@Count, @ErrorMessage, @ErrorSource, @StackTrace, @InnerErrorMessage, @ApiCall, @MethodName, @ErrorOccurredDateTime, @ClientId)";

                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.Add(new SqlParameter("@Count", SqlDbType.Int) { Value = 1 });
                            insertCmd.Parameters.Add(new SqlParameter("@ErrorMessage", SqlDbType.NVarChar) { Value = (object)error.ErrorMessage ?? DBNull.Value });
                            insertCmd.Parameters.Add(new SqlParameter("@ErrorSource", SqlDbType.NVarChar) { Value = (object)error.ErrorSource ?? DBNull.Value });
                            insertCmd.Parameters.Add(new SqlParameter("@StackTrace", SqlDbType.NVarChar) { Value = (object)error.StackTrace ?? DBNull.Value });
                            insertCmd.Parameters.Add(new SqlParameter("@InnerErrorMessage", SqlDbType.NVarChar) { Value = (object)error.InnerErrorMessage ?? DBNull.Value });
                            insertCmd.Parameters.Add(new SqlParameter("@ApiCall", SqlDbType.NVarChar, 255) { Value = (object)error.ApiCall ?? DBNull.Value });
                            insertCmd.Parameters.Add(new SqlParameter("@MethodName", SqlDbType.NVarChar, 255) { Value = (object)error.MethodName ?? DBNull.Value });
                            insertCmd.Parameters.Add(new SqlParameter("@ErrorOccurredDateTime", SqlDbType.DateTime) { Value = DateTime.Now });
                            insertCmd.Parameters.Add(new SqlParameter("@ClientId", SqlDbType.NVarChar) { Value = (object)error.ClientId ?? DBNull.Value });

                            await insertCmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
        }
        #endregion InsertOrUpdateApiError

        #region GetApiErrors
        public async Task<List<ApiErrorMessageModel>> GetApiErrorReportDAAsync(DateTime startDate, DateTime endDate,string apiCall, string clientId)
        {
            try
            {
                List<ApiErrorMessageModel> errors = new List<ApiErrorMessageModel>();
                endDate = endDate.AddDays(1); 

                string sql = @"
                    SELECT [Id], [Count], [ErrorMessage], [ErrorSource], [StackTrace], 
                        [InnerErrorMessage], [ApiCall], [MethodName], [ErrorOccurredDateTime], [ClientId]
                    FROM [dbo].[ApiErrorLogs] WITH (NOLOCK)
                    WHERE [ErrorOccurredDateTime] >= @StartDate AND [ErrorOccurredDateTime] < @EndDate
                    ORDER BY [ErrorOccurredDateTime] DESC";

                using (SqlConnection conn = new SqlConnection(_defaultConnectionString))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandTimeout = 5 * 60;
                        cmd.Parameters.AddWithValue("@StartDate", startDate);
                        cmd.Parameters.AddWithValue("@EndDate", endDate);
                        using (SqlDataReader rdr = await cmd.ExecuteReaderAsync())
                        {
                            while (await rdr.ReadAsync())
                            {
                                ApiErrorMessageModel error = new ApiErrorMessageModel();

                                try { error.Id = rdr.GetInt64(0); } catch { }
                                try { error.Count = rdr.GetInt32(1); } catch { }
                                try { error.ErrorMessage = rdr.GetString(2); } catch { }
                                try { error.ErrorSource = rdr.GetString(3); } catch { }
                                try { error.StackTrace = rdr.GetString(4); } catch { }
                                try { error.InnerErrorMessage = rdr.GetString(5); } catch { }
                                try { error.ApiCall = rdr.GetString(6); } catch { }
                                try { error.MethodName = rdr.GetString(7); } catch { }
                                try { error.ErrorOccurredDateTime = rdr.GetDateTime(8); } catch { }
                                try { error.ClientId = rdr.GetString(9); } catch { }

                                errors.Add(error);
                            }
                        }
                    }
                }

                return errors;
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
                    MethodName = nameof(GetApiErrorReportDAAsync),
                    ErrorOccurredDateTime = DateTime.Now,
                    ClientId = clientId
                };

                await InsertOrUpdateApiErrorAsync(error);
                throw;
            }
        }

        #endregion GetApiErrors
    }
}