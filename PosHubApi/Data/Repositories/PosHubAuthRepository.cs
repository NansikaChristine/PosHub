
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
         private readonly string _baseUrl;
        private readonly LogsDA _logsDA;
        private readonly ApiErrorDA _apiErrorDA;

        public PosHubAuthRepository(HttpClient httpClient, IConfiguration configuration, PosHubAuthDA posHubAuthDA, ApiErrorDA apiErrorDA, LogsDA logsDA)
        {
            _httpClient = httpClient;
            _posHubAuthDA = posHubAuthDA;
            _logsDA = logsDA;
            _baseUrl = configuration.GetSection("PosHubUrl").Value;
            _apiErrorDA = apiErrorDA;

        }

        public async Task<TokenResponseDto> GetAccessTokenAsync(TokenRequestDto requestDto, string apiCall)
        {
            string url = $"{_baseUrl}/oauth2/token";
            try
            {

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
                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = url,
                        Event = "Post",
                        IsSuccess = false,
                        FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}"
                    });
                    return new TokenResponseDto
                    {
                        ErrorMessage = $"Error fetching token: {response.StatusCode}, {errorBody}"
                    };
                }

                Stream responseStream = await response.Content.ReadAsStreamAsync();
                TokenResponseDto tokenResponse = await JsonSerializer.DeserializeAsync<TokenResponseDto>(responseStream);

                if (tokenResponse == null)
                {
                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = url,
                        Event = "Post",
                        IsSuccess = false,
                        FailMessage = $"Token response is null after successful status code: {(int)response.StatusCode} - {response.ReasonPhrase}"
                    });
                    return new TokenResponseDto
                    {
                        ErrorMessage = "Token response is null"
                    };
                }
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "Post",
                    IsSuccess = true,
                    FailMessage = ""
                });
                return tokenResponse;
            }
            catch (Exception ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "Post",
                    IsSuccess = false,
                    FailMessage = ex.Message
                });
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
            
            string _tokenUrl = $"{_baseUrl}/oauth2/token";
            try
            {
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
                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = _tokenUrl,
                        Event = "Post",
                        IsSuccess = false,
                        FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}"
                    });
                    return new TokenResponseDto
                    {
                        ErrorMessage = $"Error fetching refresh token: {response.StatusCode}, {errorBody}"
                    };
                }

                Stream responseStream = await response.Content.ReadAsStreamAsync();
                TokenResponseDto tokenResponse = await JsonSerializer.DeserializeAsync<TokenResponseDto>(responseStream);

                if (tokenResponse == null)
                {
                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = _tokenUrl,
                        Event = "Post",
                        IsSuccess = false,
                        FailMessage = $"Token response is null after successful status code: {(int)response.StatusCode} - {response.ReasonPhrase}"
                    });
                    return new TokenResponseDto
                    {
                        ErrorMessage = "Refresh Token response is null"
                    };
                }
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = _tokenUrl,
                    Event = "Post",
                    IsSuccess = true,
                    FailMessage = ""
                });

                return tokenResponse;
            }
            catch (Exception ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = _tokenUrl,
                    Event = "Post",
                    IsSuccess = false,
                    FailMessage = ex.Message
                });
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