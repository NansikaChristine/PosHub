using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text.Json;
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

        public CatalogController(ICatalogRepository catalogRrepository)
        {
            _catalogRrepository = catalogRrepository;
        }

        [HttpPost("pull")]
        public async Task<ActionResult<CatalogImportEntityDto>> PullCatalog()
        {
            string apiCall = $"Catalog/pull";
            (CatalogImportEntityDto, bool) catalog = await _catalogRrepository.GetPullCatalogAsync(apiCall);
            string json = JsonSerializer.Serialize(catalog.Item1);

            Console.WriteLine(json);

            if (!catalog.Item2) return BadRequest("Failed to pull catalog to PosHub.");
            else return Ok(catalog.Item1);
        }

        [HttpPost("syncCatalogToPosHub/{applicationId}")]
        public async Task<ActionResult> SyncCatalogToPosHub(string applicationId)
        {
            string apiCall = $"Catalog/syncCatalogToPosHub/{applicationId}";
            bool success = await _catalogRrepository.SyncCatalogToPosHub(applicationId, apiCall);

            if (!success)
                return BadRequest("Failed to sync catalog to PosHub.");

            return Ok(success);
        }

        [HttpGet("getCatalogProducts")]
        public async Task<ActionResult<List<ProductDto>>> GetCatalogProducts()
        {
            try
            {
                string apiCall = $"Catalog/getCatalogProducts";
                List<ProductDto> products = await _catalogRrepository.GetCatalogProducts(apiCall);
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
                string apiCall = $"Catalog/getCatalogProductByProductId/{applicationId}/{productId}";
                ProductDto product = await _catalogRrepository.GetCatalogProductByProductId(applicationId, productId, apiCall);
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
            try
            {
                string apiCall = $"Catalog/updateCatalogProductByProductId/{applicationId}/{productId}";
                ProductDto productRes = await _catalogRrepository.UpdateCatalogProductByProductId(applicationId, product, productId, apiCall);
                return Ok(productRes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("getCatalogProductByPosRefId/{applicationId}/{posRefId}")]
        public async Task<ActionResult<ProductDataResponseByPosRefDto>> GetCatalogProductByPosRefId(string applicationId, string posRefId)
        {
            try
            {
                string apiCall = $"Catalog/getCatalogProductByPosRefId/{applicationId}/{posRefId}";
                List<ProductDataResponseByPosRefDto> product = await _catalogRrepository.GetCatalogProductByPosRefId(applicationId, posRefId, apiCall);
                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("deleteCatalogProductByPosRefId/{applicationId}/{posRefId}")]
        public async Task<ActionResult<bool>> DeleteCatalogProductByPosRefId(string applicationId, string posRefId)
        {
            try
            {
                string apiCall = $"Catalog/deleteCatalogProductByPosRefId/{applicationId}/{posRefId}";
                bool response = await _catalogRrepository.DeleteCatalogProductByPosRefId(applicationId, posRefId, apiCall);

                if (response)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound(new { error = "PosHubProductId not found or already deleted." }); // HTTP 404
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message }); // HTTP 401
            }
            catch (SecurityException ex)
            {
                return Forbid(); // HTTP 403
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message }); // HTTP 500
            }
        }

        [HttpDelete("deleteCatalogCategoryByPosRefId/{applicationId}/{posRefId}")]
        public async Task<ActionResult<bool>> DeleteCatalogCategoryByPosRefId(string applicationId, string posRefId)
        {
            try
            {
                string apiCall = $"Catalog/deleteCatalogCategoryByPosRefId/{applicationId}/{posRefId}";
                bool response = await _catalogRrepository.DeleteCatalogCategoryByPosRefId(applicationId, posRefId, apiCall);

                if (response)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound(new { error = "PosHubCategoryId not found or already deleted." }); // HTTP 404
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message }); // HTTP 401
            }
            catch (SecurityException ex)
            {
                return Forbid(); // HTTP 403
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message }); // HTTP 500
            }
        }

        [HttpPatch("updateProductByPosRefId/{applicationId}")]
        public async Task<ActionResult> UpdateProductByPosRefId(ProductDto product,string applicationId)
        {
            try
            {
                string apiCall = $"Catalog/updateProductByPosRefId/{applicationId}";
                bool isUpdate = await _catalogRrepository.UpdateProductByPosRefId(applicationId, product, apiCall);
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    
        [HttpGet("getCatalogModifiers/{applicationId}/{limit}")]
        public async Task<ActionResult<CatalogModifiersResponseDto>> GetCatalogModifiers(string applicationId, string limit)
        {
            try
            {
                string apiCall = $"Catalog/getCatalogModifiers";
                CatalogModifiersResponseDto modifiers = await _catalogRrepository.GetCatalogModifiers(applicationId, limit, apiCall);
                return Ok(modifiers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("updateModifierByPosRefId/{applicationId}")]
        public async Task<ActionResult> UpdateModifierByPosRefId(ModifierDto modifier,string applicationId)
        {
            try
            {
                string apiCall = $"Catalog/updateModifierByPosRefId/{applicationId}";
                bool isUpdate = await _catalogRrepository.UpdateModifierByPosRefId(applicationId, modifier, apiCall);
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    
    
    
    }
}