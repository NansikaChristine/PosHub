using PosHubApi.Dtos;

namespace PosHubApi.Data.Interfaces
{
    public interface IPosHubAuthRepository
    {
        Task<TokenResponseDto> GetAccessTokenAsync(TokenRequestDto requestDto, string apiCall);
        Task<TokenResponseDto> RefreshAccessTokenAsync(RefreshAccessTokenRequestDto request, string apiCall);
        Task<bool> SaveOrUpdateAccountLocationAsync(AccountLocationDto dto, string apiCall);
        Task<List<ClientsDto>> GetClientsDetails(string apiCall);
        Task<ClientsDto> GetClientDetailsByClientIdAsync(string applicationId, string apiCall);
    }
}