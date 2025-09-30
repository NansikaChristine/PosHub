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
            CatalogImportEntityDto catalog = await _catalogRrepository.GetPullCatalogAsync(apiCall);

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

        [HttpGet("getCatalogProducts/{applicationId}/{limit}")]
        public async Task<ActionResult<CatalogProductsResponseDto>> GetCatalogProducts(string applicationId, string limit)
        {
            try
            {
                string apiCall = $"Catalog/getCatalogProducts";
                ClientsDto clientDetail = await _authRepository.GetClientDetailsByClientIdAsync(applicationId, apiCall);

                CatalogProductsResponseDto products = await _catalogRrepository.GetCatalogProducts(clientDetail, limit, apiCall);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("getCatalogProductByProductId/{applicationId}/{productId}")]
        public async Task<ActionResult<ProductDto>> GetCatalogProductByProductId(string applicationId, string productId)
        {
            try
            {
                string apiCall = $"Catalog/getCatalogProductByProductId";
                ClientsDto clientDetail = await _authRepository.GetClientDetailsByClientIdAsync(applicationId, apiCall);

                ProductDto product = await _catalogRrepository.GetCatalogProductByProductId(clientDetail, productId, apiCall);
                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("updateCatalogProductByProductId/{applicationId}/{productId}")]
        public async Task<ActionResult<ProductDto>> UpdateCatalogProductByProductId(string applicationId, ProductUpdateRequestDto product, string productId)
        {
            Console.WriteLine("applicatinId: " + applicationId);
            Console.WriteLine("productId: " + productId);
            try
            {
                string apiCall = $"Catalog/updateCatalogProductByProductId";
                ClientsDto clientDetail = await _authRepository.GetClientDetailsByClientIdAsync(applicationId, apiCall);

                ProductDto productRes = await _catalogRrepository.UpdateCatalogProductByProductId(clientDetail, product, productId, apiCall);
                return Ok(productRes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("createCatalogProduct/{applicationId}")]
        public async Task<ActionResult<ProductDto>> CreateCatalogProductByProductId(string applicationId, ProductDto product)
        {
            try
            {
                string apiCall = $"Catalog/createCatalogProduct";
                ClientsDto clientDetail = await _authRepository.GetClientDetailsByClientIdAsync(applicationId, apiCall);

                ProductDto productRes = await _catalogRrepository.CreateCatalogProductByProductId(clientDetail, product, apiCall);
                return Ok(productRes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("deleteCatalogProductByProductId/{applicationId}/{productId}")]
        public async Task<ActionResult> DeleteCatalogProductByProductId(string applicationId, string productId)
        {
            try
            {
                string apiCall = $"Catalog/deleteCatalogProductByProductId";
                ClientsDto clientDetail = await _authRepository.GetClientDetailsByClientIdAsync(applicationId, apiCall);

                bool response = await _catalogRrepository.DeleteCatalogProductByProductId(clientDetail, productId, apiCall);
                if (!response)
                    return BadRequest("Failed to delete catalog product to PosHub.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    
    }
}