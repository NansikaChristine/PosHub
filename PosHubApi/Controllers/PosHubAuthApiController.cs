using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PosHubApi.Data.Interfaces;
using PosHubApi.Dtos;

namespace PosHubApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PosHubAuthApiController : ControllerBase
    {
        private readonly IPosHubAuthRepository _authRepository;

        public PosHubAuthApiController(IPosHubAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [HttpGet("apps")]
        public async Task<ActionResult<ClientsDto>> GetClientsDetails()
        {
            try
            {
                string apiCall = $"PosHubAuthApi/apps";
                List<ClientsDto> clientsDetails = await _authRepository.GetClientsDetails(apiCall);
                return Ok(clientsDetails);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("code")]
        public async Task<ActionResult> GetAuthCode(string code, string account_id,string location_id, string applicationId, string connectionId)
        {
            var redirectUrl = "http://localhost:4200";
            try
            { 
                string apiCall = $"PosHubAuthApi/code";
                ClientsDto clientDetail = await _authRepository.GetClientDetailsByClientIdAsync(applicationId, apiCall);
                
                TokenRequestDto requestDto = new TokenRequestDto();
                requestDto.Grant_Type = "client_credentials";
                requestDto.Client_Id = applicationId;
                requestDto.Client_Secret = clientDetail.ClientSecret;
                requestDto.Scope = "provisioning connections.write catalogs.read catalogs.write orders.read orders.write locations.read locations.write connections.read";

                string apiCall1 = $"PosHubAuthApi/token/save";
                TokenResponseDto token = await _authRepository.GetAccessTokenAsync(requestDto, apiCall1);

                AccountLocationDto dto = new AccountLocationDto();
                dto.Code = code;
                dto.AccountId = account_id;
                dto.LocationId = location_id;
                dto.ApplicationId = applicationId;
                dto.ConnectionId = connectionId;
                dto.AccessToken = token.AccessToken;
                dto.RefreshToken = token.RefreshToken;
                dto.Authorized = true;

                string apiCall2 = "PosHubAuthApi/saveOrUpdateAccountLocation";
                bool result = await _authRepository.SaveOrUpdateAccountLocationAsync(dto, apiCall2);

                if (result)
                {
                    return Redirect($"{redirectUrl}?status=success");
                }
                else
                {
                    return Redirect($"{redirectUrl}?status=error&message={Uri.EscapeDataString("No rows affected")}");
                }
            }
            catch (Exception ex)
            {
                return Redirect($"{redirectUrl}?status=error&message={Uri.EscapeDataString(ex.Message)}");
            }

        }

        [HttpPost("token/save")]
        public async Task<ActionResult<TokenResponseDto>> GetToken([FromBody] TokenRequestDto requestDto)
        {
            try
            {
                string apiCall = $"PosHubAuthApi/token/save";
                TokenResponseDto token = await _authRepository.GetAccessTokenAsync(requestDto, apiCall);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("refresh_access_token")]
        public async Task<IActionResult> RefreshAccessToken(string applicationId)
        {
            string apiCall = $"PosHubAuthApi/refresh_access_token";
            ClientsDto clientDetail = await _authRepository.GetClientDetailsByClientIdAsync(applicationId, apiCall);
            
            RefreshAccessTokenRequestDto requestDto = new RefreshAccessTokenRequestDto();
            requestDto.Grant_Type = "refresh_token";
            requestDto.Client_Id = applicationId;
            requestDto.Client_Secret = clientDetail.ClientSecret;
            requestDto.Refresh_Token = clientDetail.RefreshToken;

            TokenResponseDto token = await _authRepository.RefreshAccessTokenAsync(requestDto, apiCall);

            AccountLocationDto dto = new AccountLocationDto();
            dto.Code = clientDetail.Code;
            dto.AccountId = clientDetail.AccountId;
            dto.LocationId = clientDetail.LocationId;
            dto.ApplicationId = applicationId;
            dto.ConnectionId = clientDetail.ConnectionId;
            dto.AccessToken = token.AccessToken;
            dto.RefreshToken = token.RefreshToken;
            dto.Authorized = clientDetail.Authorized;

            string apiCall2 = "PosHubAuthApi/saveOrUpdateAccountLocation";
            bool result = await _authRepository.SaveOrUpdateAccountLocationAsync(dto, apiCall2);

            return Ok(token);
        }

        [HttpPost("saveOrUpdateAccountLocation")]
        public async Task<IActionResult> SaveOrUpdateAccountLocation([FromBody] AccountLocationDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { error = "Invalid data." });
            }

            try
            {
                string apiCall = "PosHubAuthApi/saveOrUpdateAccountLocation";
                bool result = await _authRepository.SaveOrUpdateAccountLocationAsync(dto, apiCall);

                if (result)
                    return Ok(new { message = "Account location saved successfully" });
                else
                    return StatusCode(500, new { error = "No rows affected" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
    }
}