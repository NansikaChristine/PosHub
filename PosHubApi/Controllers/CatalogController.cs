using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PosHubApi.Data.Interfaces;
using PosHubApi.Dtos;

namespace PosHubApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly ICatalogRepository _catalogRrepository;
        private readonly IPosHubAuthRepository _authRepository;

        public CatalogController(ICatalogRepository catalogRrepository, IPosHubAuthRepository authRepository)
        {
            _catalogRrepository = catalogRrepository;
            _authRepository = authRepository;
        }

        [HttpPost("pull")]
        public async Task<ActionResult<CatalogImportEntityDto>> PullCatalog()
        {
            string apiCall = $"Catalog/syncCatalogToPosHub";
            var catalog = await _catalogRrepository.GetPullCatalogAsync(apiCall);

            return Ok(catalog);
        }

        [HttpPost("syncCatalogToPosHub/{applicationId}")]
        public async Task<ActionResult> SyncCatalogToPosHub(string applicationId)
        {
            string apiCall = $"Catalog/syncCatalogToPosHub";
            ClientsDto clientDetail = await _authRepository.GetClientDetailsByClientIdAsync(applicationId, apiCall);

            bool success = await _catalogRrepository.SyncCatalogToPosHub(clientDetail, apiCall);

            if (!success)
                return BadRequest("Failed to sync catalog to PosHub.");

            return Ok(success);
        }

        [HttpGet("catalogProducts/{applicationId}/{limit}")]
        public async Task<ActionResult<CatalogProductsResponseDto>> GetCatalogProducts(string applicationId, string limit)
        {
            try
            {
                string apiCall = $"Catalog/catalogProducts";
                ClientsDto clientDetail = await _authRepository.GetClientDetailsByClientIdAsync(applicationId, apiCall);
                
                CatalogProductsResponseDto products = await _catalogRrepository.GetCatalogProducts(clientDetail, limit, apiCall);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}