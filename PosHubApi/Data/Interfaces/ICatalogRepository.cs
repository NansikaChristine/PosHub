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
        Task<bool> SyncCatalogToPosHub(ClientsDto client, string apiCall);
        Task<CatalogProductsResponseDto> GetCatalogProducts(ClientsDto client, string limit, string apiCall);
        Task<ProductDto> GetCatalogProductByProductId(ClientsDto client, string productId, string apiCall);
        Task<ProductDto> UpdateCatalogProductByProductId(ClientsDto client, ProductUpdateRequestDto product,string productId, string apiCall);
        Task<ProductDto> CreateCatalogProductByProductId(ClientsDto client, ProductDto product, string apiCall);
        Task<bool> DeleteCatalogProductByProductId(ClientsDto client, string productId, string apiCall);
    
    }
}