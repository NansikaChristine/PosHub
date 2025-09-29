
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using PosHubApi.Data.DataAccess;
using PosHubApi.Data.Interfaces;
using PosHubApi.Dtos;
using PosHubApi.Models;

namespace PosHubApi.Data.Repositories
{
    public class PosHubAuthRepository : IPosHubAuthRepository
    {
        private readonly HttpClient _httpClient;
        private readonly PosHubAuthDA _posHubAuthDA;
        private readonly ApiErrorDA _apiErrorDA;

        public PosHubAuthRepository(HttpClient httpClient, PosHubAuthDA posHubAuthDA, ApiErrorDA apiErrorDA)
        {
            _httpClient = httpClient;
            _posHubAuthDA = posHubAuthDA;
            _apiErrorDA = apiErrorDA;

        }

        public async Task<TokenResponseDto> GetAccessTokenAsync(TokenRequestDto requestDto, string apiCall)
        {
            try
            {
                string url = "https://api-sit-dr.stage.tryposhub.com/oauth2/token";

                Dictionary<string, string> formData = new Dictionary<string, string>
                {
                    { "grant_type", requestDto.Grant_Type },
                    { "client_id", requestDto.Client_Id },
                    { "client_secret", requestDto.Client_Secret },
                    { "scope", requestDto.Scope }
                };

                FormUrlEncodedContent content = new FormUrlEncodedContent(formData);
                HttpResponseMessage response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    return new TokenResponseDto
                    {
                        ErrorMessage = $"Error fetching token: {response.StatusCode}, {errorBody}"
                    };
                }

                Stream responseStream = await response.Content.ReadAsStreamAsync();
                TokenResponseDto tokenResponse = await JsonSerializer.DeserializeAsync<TokenResponseDto>(responseStream);
                
                if (tokenResponse == null)
                    return new TokenResponseDto
                    {
                        ErrorMessage = "Token response is null"
                    };

                // var requestedAt = DateTime.UtcNow;
                // var tokenLog = new TokenLogModel
                // {
                //     ClientId = requestDto.Client_Id,
                //     ClientSecret = requestDto.Client_Secret,
                //     Scope = requestDto.Scope,
                //     GrantType = requestDto.Grant_Type,
                //     AccessToken = tokenResponse.AccessToken,
                //     RefreshToken = tokenResponse.RefreshToken,
                //     TokenType = tokenResponse.TokenType,
                //     ExpiresIn = tokenResponse.ExpiresIn,
                //     RequestedAt = requestedAt,
                //     ExpiresAt = requestedAt.AddHours(24)
                // };

                // try
                // {
                //     bool isSuccess = await _posHubAuthDA.UpdateOrInsertTokenLog(tokenLog, apiCall);
                //     if (!isSuccess)
                //     {
                //         tokenResponse.ErrorMessage = "Token fetched successfully but failed to save in database.";
                //     }
                // }
                // catch (Exception ex)
                // {
                //     tokenResponse.ErrorMessage = $"Token fetched successfully but failed to save in database: {ex.Message}";
                // }

                return tokenResponse;
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
                    MethodName = nameof(GetAccessTokenAsync),
                    ErrorOccurredDateTime = DateTime.Now
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return new TokenResponseDto();
            }

        }
        public async Task<TokenResponseDto> RefreshAccessTokenAsync(RefreshAccessTokenRequestDto request, string apiCall)
        {
            try
            {
                string _tokenUrl = "https://api-sit-dr.stage.tryposhub.com/oauth2/token";
                Dictionary<string, string> formData = new Dictionary<string, string>
                {
                    { "grant_type", request.Grant_Type },
                    { "refresh_token", request.Refresh_Token },
                    { "client_id", request.Client_Id },
                    { "client_secret", request.Client_Secret }
                };

                HttpResponseMessage response = await _httpClient.PostAsync(_tokenUrl, new FormUrlEncodedContent(formData));
                response.EnsureSuccessStatusCode();

                if (!response.IsSuccessStatusCode)
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    return new TokenResponseDto
                    {
                        ErrorMessage = $"Error fetching refresh token: {response.StatusCode}, {errorBody}"
                    };
                }

                Stream responseStream = await response.Content.ReadAsStreamAsync();
                TokenResponseDto tokenResponse = await JsonSerializer.DeserializeAsync<TokenResponseDto>(responseStream);

                if (tokenResponse == null)
                    return new TokenResponseDto
                    {
                        ErrorMessage = "Refresh Token response is null"
                    };
                // var requestedAt = DateTime.UtcNow;
                // var tokenLog = new TokenLogModel
                // {
                //     ClientId = request.Client_Id,
                //     ClientSecret = request.Client_Secret,
                //     Scope = tokenResponse.Scope,
                //     GrantType = request.Grant_Type,
                //     AccessToken = tokenResponse.AccessToken,
                //     RefreshToken = tokenResponse.RefreshToken,
                //     TokenType = tokenResponse.TokenType,
                //     ExpiresIn = tokenResponse.ExpiresIn,
                //     RequestedAt = requestedAt,
                //     ExpiresAt = requestedAt.AddHours(24)
                // };

                // try
                // {
                //     bool isSuccess = await _posHubAuthDA.UpdateOrInsertTokenLog(tokenLog, apiCall);
                //     if (!isSuccess)
                //     {
                //         tokenResponse.ErrorMessage = "Refresh Token fetched successfully but failed to save in database.";
                //     }
                // }
                // catch (Exception ex)
                // {
                //     tokenResponse.ErrorMessage = $"Refresh Token fetched successfully but failed to save in database: {ex.Message}";
                // }

                return tokenResponse;
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
                    MethodName = nameof(RefreshAccessTokenAsync),
                    ErrorOccurredDateTime = DateTime.Now
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return new TokenResponseDto();
            }

        }
        public async Task<bool> SaveOrUpdateAccountLocationAsync(AccountLocationDto dto, string apiCall)
        {
            return await _posHubAuthDA.UpdateOrInsertAccountLocation(dto, apiCall);
        }
        public async Task<List<ClientsDto>> GetClientsDetails(string apiCall)
        {
            return await _posHubAuthDA.GetClientsDetails(apiCall);
        }
        public async Task<ClientsDto> GetClientDetailsByClientIdAsync(string applicationId, string apiCall)
        {
            return await _posHubAuthDA.GetClientDetailsByClientIdAsync(applicationId, apiCall);
        }
    }
}