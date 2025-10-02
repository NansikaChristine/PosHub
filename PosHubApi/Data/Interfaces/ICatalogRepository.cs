using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PosHubApi.Dtos;

namespace PosHubApi.Data.Interfaces
{
    public interface ICatalogRepository
    {
        Task<CatalogImportEntityDto> GetPullCatalogAsync(string apiCall);
        Task<bool> SyncCatalogToPosHub(string applicationId, string apiCall);
        Task<CatalogProductsResponseDto> GetCatalogProducts(string applicationId, string limit, string apiCall);
        Task<ProductDto> GetCatalogProductByProductId(string applicationId, string productId, string apiCall);
        Task<ProductDto> UpdateCatalogProductByProductId(string applicationId, ProductUpdateRequestDto product,string productId, string apiCall);
        // Task<ProductDto> CreateCatalogProductByProductId(ClientsDto client, ProductDto product, string apiCall);
        // Task<bool> DeleteCatalogProductByProductId(ClientsDto client, string productId, string apiCall);
        Task<List<ProductDataResponseByPosRefDto>> GetCatalogProductByPosRefId(string applicationId, string posRefId, string apiCall);
    
    }
}