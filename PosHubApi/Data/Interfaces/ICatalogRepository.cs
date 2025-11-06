using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PosHubApi.Dtos;

namespace PosHubApi.Data.Interfaces
{
    public interface ICatalogRepository
    {
        Task<(CatalogImportEntityDto, bool)> GetPullCatalogAsync(string apiCall, string accountId, string locationId);
        Task<bool> SyncCatalogToPosHub(string applicationId, string apiCall);
        Task<List<ProductDto>> GetCatalogProducts(string apiCall);
        Task<ProductDto> GetCatalogProductByProductId(string applicationId, string productId, string apiCall);
        Task<ProductDto> UpdateCatalogProductByProductId(string applicationId, ProductUpdateRequestDto product,string productId, string apiCall);
        Task<bool> DeleteCatalogProductByPosRefId(string applicationId, string productId, string apiCall);
        Task<bool> DeleteCatalogCategoryByPosRefId(string applicationId, string productId, string apiCall);
        Task<List<ProductDataResponseByPosRefDto>> GetCatalogProductByPosRefId(string applicationId, string posRefId, string apiCall);
        Task<bool> UpdateProductByPosRefId(string applicationId, ProductDto product, string apiCall);
        Task<CatalogModifiersResponseDto> GetCatalogModifiers(string applicationId, string limit, string apiCall);
        Task<bool> UpdateModifierByPosRefId(string applicationId, ModifierDto modifier, string apiCall);

    }
}