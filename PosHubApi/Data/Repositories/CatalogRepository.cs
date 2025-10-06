using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.SqlClient;
using PosHubApi.Data.DataAccess;
using PosHubApi.Data.Interfaces;
using PosHubApi.Dtos;
using PosHubApi.Models;

namespace PosHubApi.Data.Repositories
{
    public class CatalogRepository : ICatalogRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _connectionString;
        private readonly CatalogDA _catalogDA;
        private readonly ApiErrorDA _apiErrorDA;
        private readonly PosHubAuthDA _posHubAuthDA;
        private readonly LogsDA _logsDA;
         private readonly string _baseUrl;

        public CatalogRepository(HttpClient httpClient, IConfiguration configuration, CatalogDA catalogDA, ApiErrorDA apiErrorDA,
         PosHubAuthDA posHubAuthDA, LogsDA logsDA)
        {
            _httpClient = httpClient;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _catalogDA = catalogDA;
            _apiErrorDA = apiErrorDA;
            _posHubAuthDA = posHubAuthDA;
            _baseUrl = configuration.GetSection("PosHubUrl").Value;
            _logsDA = logsDA;
        }
        // public async Task<CatalogImportEntityDto> GetCatalogAsync(ClientsDto client)
        // {
        //     string url = $"{_baseUrl}/v1/accounts/{client.AccountId}/locations/{client.LocationId}/connections/{client.ConnectionId}/pull";

        //     var result = new CatalogImportEntityDto();

        //     try
        //     {
        //         var response = await _httpClient.PostAsync(url, new StringContent("", Encoding.UTF8, "application/json"));

        //         if (!response.IsSuccessStatusCode)
        //         {
        //             var errorBody = await response.Content.ReadAsStringAsync();
        //             result.ErrorMessage = $"Error sync: {response.StatusCode}, {errorBody}";
        //             return result;
        //         }

        //         var responseStream = await response.Content.ReadAsStreamAsync();
        //         result = await JsonSerializer.DeserializeAsync<CatalogImportEntityDto>(responseStream,
        //             new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        //     }
        //     catch (Exception ex)
        //     {
        //         result.ErrorMessage = $"Token fetched successfully but failed to save in database: {ex.Message}";
        //     }

        //     return result;
        // }

        // #region PullCatalog
        // public async Task<CatalogImportEntityDto> GetPullCatalogAsync(ClientsDto client)
        // {
        //     CatalogImportEntityDto returnData = new CatalogImportEntityDto();
        //     List<CategoryDto> categories = new List<CategoryDto>();
        //     List<ModifierDto> modifiers = new List<ModifierDto>();
        //     var modifierGroups = new List<ModifierGroupDto>();
        //     var products = new List<ProductDto>();

        //     var sampleProduct = new ProductDto
        //     {
        //         Id = "ffaf9d11-024a-4274-842c-fbb3c16e761a",
        //         PosReference = "PROD-001",
        //         Name = "Sandwich MODIFIED",
        //         Description = "Rich espresso topped with steamed milk and foam",
        //         Type = "Beverage",
        //         PosVersion = "1.0",
        //         OriginalImageUrl = "https://example.com/images/cappuccino.jpg",
        //         Price = 3.50m,
        //         InStorePrice = 3.25m,
        //         TaxRate = 0.08m,
        //         IsTaxIncluded = true,
        //         ContainsAlcohol = false,
        //         ContainsTobacco = false,
        //         IsBikeFriendly = true,
        //         ShowOnline = true,
        //         Categories = new List<string> { "Hot Drinks", "Coffee" },
        //         Position = 1,

        //         DietaryRestriction = "Vegetarian",
        //         Spiciness = "None",
        //         Additives = new List<string> { "Vanilla Syrup" },
        //         Allergens = new List<string> { "Milk" },
        //         ServiceAvailability = new List<ServiceAvailabilityDto>
        //     {
        //         new ServiceAvailabilityDto { Weekday = "Monday"},
        //         new ServiceAvailabilityDto { Weekday = "Tuesday" }
        //     },
        //         ModifierGroups = new List<string> { "Milk Options", "Size Options" },

        //     };
        //     products.Add(sampleProduct);
        //     returnData.Products = products;
        //     return returnData;
        // }

        // #endregion PullCatalog

        public async Task<CatalogImportEntityDto> GetPullCatalogAsync(string apiCall)
        {
            CatalogImportEntityDto returnData = new CatalogImportEntityDto();
            List<CategoryDto> categories = new List<CategoryDto>();
            List<ModifierDto> modifiers = new List<ModifierDto>();
            List<ModifierGroupDto> modifierGroups = new List<ModifierGroupDto>();
            List<ProductDto> products = new List<ProductDto>();

            string Sql = @"
                -- Categories + service availability
                SELECT c.PosReference,c.Name,c.Description,
                    c.PosVersion,c.OriginalImageUrl,c.ShowOnline,c.Position,sa.Weekday,tp.StartDate,
                    tp.EndDate,tp.StartTime,tp.EndTime
                FROM Categories c
                LEFT JOIN CategoryServiceAvailability sa ON sa.CategoryPosRef = c.PosReference
                LEFT JOIN ServiceTimePeriods tp ON tp.ServiceAvailabilityId = sa.Id
                ORDER BY c.PosReference, sa.Id, tp.StartDate;
               
                -- Modifiers
                SELECT m.PosReference, m.Name, m.Description, m.PosVersion, m.OriginalImageUrl, m.Price, m.InStorePrice,
                    m.TaxRate, m.IsTaxIncluded, m.ContainsAlcohol, m.ContainsTobacco, m.IsBikeFriendly, m.ShowOnline,
                    m.Position, m.MinPermitted, m.MaxPermitted, m.DietaryRestriction, m.Spiciness,
                    ni.KilojoulesLower, ni.KilojoulesUpper, ni.CaloriesLower, ni.CaloriesUpper,
                    ni.ProteinLower, ni.ProteinUpper, ni.CarbohydratesLower, ni.CarbohydratesUpper,
                    ni.SugarLower, ni.SugarUpper, ni.SaturatedFatLower, ni.SaturatedFatUpper, ni.SaltLower, ni.SaltUpper,
                    STUFF((
                        SELECT ',' + da.Additive
                        FROM ModifierAdditives da
                        WHERE da.ModifierPosRef = m.PosReference
                        FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS Additives,

                    -- Allergens concatenation
                    STUFF((
                        SELECT ',' + al.Allergen
                        FROM ModifierAllergens al
                        WHERE al.ModifierPosRef = m.PosReference
                        FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS Allergens
                FROM Modifiers m
                LEFT JOIN ModifierNutritionalInfo ni ON ni.ModifierPosRef = m.PosReference
                LEFT JOIN ModifierAdditives da ON da.ModifierPosRef = m.PosReference
                LEFT JOIN ModifierAllergens al ON al.ModifierPosRef = m.PosReference
                GROUP BY m.PosReference, m.Name, m.Description, m.PosVersion, m.OriginalImageUrl,
                        m.Price, m.InStorePrice, m.TaxRate, m.IsTaxIncluded, m.ContainsAlcohol, m.ContainsTobacco,
                        m.IsBikeFriendly, m.ShowOnline, m.Position, m.MinPermitted, m.MaxPermitted,
                        m.DietaryRestriction, m.Spiciness,
                        ni.KilojoulesLower, ni.KilojoulesUpper, ni.CaloriesLower, ni.CaloriesUpper,
                        ni.ProteinLower, ni.ProteinUpper, ni.CarbohydratesLower, ni.CarbohydratesUpper,
                        ni.SugarLower, ni.SugarUpper, ni.SaturatedFatLower, ni.SaturatedFatUpper, ni.SaltLower, ni.SaltUpper;
                
                -- ModifierGroups
                SELECT mg.PosReference, mg.Name, mg.Description, mg.PosVersion, mg.Position, mg.MinPermitted, mg.MaxPermitted,
                    mm.ModifierPosRef
                FROM ModifierGroups mg
                LEFT JOIN ModifierGroupModifiers mm ON mm.ModifierGroupPosRef = mg.PosReference
                ORDER BY mg.PosReference, mm.ModifierPosRef;
               

            -- Products
            SELECT p.PosReference, p.Name, p.Description, p.Type, p.PosVersion, p.OriginalImageUrl, p.Price, p.InStorePrice,
                p.TaxRate, p.IsTaxIncluded, p.ContainsAlcohol, p.ContainsTobacco, p.IsBikeFriendly, p.ShowOnline,
                p.Position, p.DietaryRestriction, p.Spiciness,
                STUFF((
                    SELECT ',' + pc.CategoryPosRef
                    FROM ProductCategories pc
                    WHERE pc.ProductPosRef = p.PosReference
                    FOR XML PATH(''), TYPE
                ).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS Categories,

                -- ModifierGroups concatenation
                STUFF((
                    SELECT ',' + pm.ModifierGroupPosRef
                    FROM ProductModifierGroups pm
                    WHERE pm.ProductPosRef = p.PosReference
                    FOR XML PATH(''), TYPE
                ).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS ModifierGroups
            FROM Products p
            LEFT JOIN ProductCategories pc ON pc.ProductPosRef = p.PosReference
            LEFT JOIN ProductModifierGroups pm ON pm.ProductPosRef = p.PosReference
            GROUP BY p.PosReference, p.Name, p.Description, p.Type, p.PosVersion, p.OriginalImageUrl, p.Price,
                    p.InStorePrice, p.TaxRate, p.IsTaxIncluded, p.ContainsAlcohol, p.ContainsTobacco,
                    p.IsBikeFriendly, p.ShowOnline, p.Position, p.DietaryRestriction, p.Spiciness;

            ";
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    #region Execute SQL
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(Sql, connection))
                    {
                        command.CommandTimeout = 60 * 60;

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            #region Fill Category
                            while (await reader.ReadAsync())
                            {
                                string catPosRef = reader["PosReference"]?.ToString();
                                CategoryDto category = categories.FirstOrDefault(c => c.PosReference == catPosRef);
                                if (category == null)
                                {
                                    category = new CategoryDto
                                    {
                                        PosReference = catPosRef,
                                        Name = reader["Name"]?.ToString(),
                                        Description = reader["Description"]?.ToString(),
                                        PosVersion = reader["PosVersion"]?.ToString(),
                                        OriginalImageUrl = reader["OriginalImageUrl"]?.ToString(),
                                        ShowOnline = reader["ShowOnline"] != DBNull.Value && (bool)reader["ShowOnline"],
                                        Position = reader["Position"] != DBNull.Value ? Convert.ToInt32(reader["Position"]) : 0,
                                        ServiceAvailability = new List<ServiceAvailabilityDto>()
                                    };
                                    categories.Add(category);
                                }

                                if (reader["Weekday"] != DBNull.Value)
                                {
                                    string weekday = reader["Weekday"].ToString();
                                    ServiceAvailabilityDto serviceAvailability = category.ServiceAvailability.FirstOrDefault(sa => sa.Weekday == weekday);
                                    if (serviceAvailability == null)
                                    {
                                        serviceAvailability = new ServiceAvailabilityDto
                                        {
                                            Weekday = weekday,
                                            TimePeriods = new List<TimePeriodDto>()
                                        };
                                        category.ServiceAvailability.Add(serviceAvailability);
                                    }

                                    serviceAvailability.TimePeriods.Add(new TimePeriodDto
                                    {
                                        StartDate = reader["StartDate"]?.ToString(),
                                        EndDate = reader["EndDate"]?.ToString(),
                                        StartTime = reader["StartTime"]?.ToString(),
                                        EndTime = reader["EndTime"]?.ToString()
                                    });
                                }
                            }
                            returnData.Categories = categories;
                            #endregion Fill Category

                            #region Modifiers
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var modifier = new ModifierDto
                                    {
                                        PosReference = reader["PosReference"].ToString(),
                                        Name = reader["Name"].ToString(),
                                        Description = reader["Description"].ToString(),
                                        PosVersion = reader["PosVersion"].ToString(),
                                        OriginalImageUrl = reader["OriginalImageUrl"].ToString(),
                                        Price = (int)reader["Price"],
                                        InStorePrice = (int)reader["InStorePrice"],
                                        TaxRate = (decimal)reader["TaxRate"],
                                        IsTaxIncluded = (bool)reader["IsTaxIncluded"],
                                        ContainsAlcohol = (bool)reader["ContainsAlcohol"],
                                        ContainsTobacco = (bool)reader["ContainsTobacco"],
                                        IsBikeFriendly = (bool)reader["IsBikeFriendly"],
                                        ShowOnline = (bool)reader["ShowOnline"],
                                        Position = (int)reader["Position"],
                                        MinPermitted = (int)reader["MinPermitted"],
                                        MaxPermitted = (int)reader["MaxPermitted"],
                                        // DietaryRestriction = reader["DietaryRestriction"].ToString(),
                                        // Spiciness = reader["Spiciness"].ToString(),

                                        NutritionalInfo = new NutritionalInfoDto
                                        {
                                            Kilojoules = new RangeDto
                                            {
                                                LowerRange = (decimal)reader["KilojoulesLower"],
                                                UpperRange = (decimal)reader["KilojoulesUpper"]
                                            },
                                            Calories = new RangeDto
                                            {
                                                LowerRange = (decimal)reader["CaloriesLower"],
                                                UpperRange = (decimal)reader["CaloriesUpper"]
                                            },
                                            Protein = new RangeDto
                                            {
                                                LowerRange = (decimal)reader["ProteinLower"],
                                                UpperRange = (decimal)reader["ProteinUpper"]
                                            },
                                            Carbohydrates = new RangeDto
                                            {
                                                LowerRange = (decimal)reader["CarbohydratesLower"],
                                                UpperRange = (decimal)reader["CarbohydratesUpper"]
                                            },
                                            Sugar = new RangeDto
                                            {
                                                LowerRange = (decimal)reader["SugarLower"],
                                                UpperRange = (decimal)reader["SugarUpper"]
                                            },
                                            SaturatedFat = new RangeDto
                                            {
                                                LowerRange = (decimal)reader["SaturatedFatLower"],
                                                UpperRange = (decimal)reader["SaturatedFatUpper"]
                                            },
                                            Salt = new RangeDto
                                            {
                                                LowerRange = (decimal)reader["SaltLower"],
                                                UpperRange = (decimal)reader["SaltUpper"]
                                            },
                                            DietaryRestriction = reader["DietaryRestriction"].ToString(),
                                            Spiciness = reader["Spiciness"].ToString(),
                                            Additives = reader["Additives"] is DBNull
                                                ? new List<string>()
                                                : reader["Additives"].ToString().Split(',').ToList(),

                                            Allergens = reader["Allergens"] is DBNull
                                                ? new List<string>()
                                                : reader["Allergens"].ToString().Split(',').ToList()
                                            },

                                        // Additives = reader["Additives"] is DBNull
                                        //     ? new List<string>()
                                        //     : reader["Additives"].ToString().Split(',').ToList(),

                                        // Allergens = reader["Allergens"] is DBNull
                                        //     ? new List<string>()
                                        //     : reader["Allergens"].ToString().Split(',').ToList()
                                    };

                                    modifiers.Add(modifier);
                                }
                                returnData.Modifiers = modifiers;
                            }
                            #endregion Modifiers

                            #region Modifier Groups
                            if (await reader.NextResultAsync())
                            {
                                ModifierGroupDto currentGroup = null;
                                string lastGroupRef = null;

                                while (await reader.ReadAsync())
                                {
                                    string groupRef = reader["PosReference"].ToString();

                                    if (lastGroupRef != groupRef)
                                    {
                                        currentGroup = new ModifierGroupDto
                                        {
                                            PosReference = groupRef,
                                            Name = reader["Name"].ToString(),
                                            Description = reader["Description"].ToString(),
                                            PosVersion = reader["PosVersion"].ToString(),
                                            Position = Convert.ToInt32(reader["Position"]),
                                            MinPermitted = Convert.ToInt32(reader["MinPermitted"]),
                                            MaxPermitted = Convert.ToInt32(reader["MaxPermitted"]),
                                            Modifiers = modifiers
                                                .Where(m => m.PosReference == reader["ModifierPosRef"].ToString())
                                                .ToList()
                                        };
                                        modifierGroups.Add(currentGroup);
                                        lastGroupRef = groupRef;
                                    }
                                }

                                returnData.ModifierGroups = modifierGroups;
                            }
                            #endregion Modifier Groups

                            #region Products
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var product = new ProductDto
                                    {
                                        PosReference = reader["PosReference"].ToString(),
                                        Name = reader["Name"].ToString(),
                                        Description = reader["Description"].ToString(),
                                        Type = reader["Type"].ToString(),
                                        PosVersion = reader["PosVersion"].ToString(),
                                        OriginalImageUrl = reader["OriginalImageUrl"].ToString(),
                                        Price = (decimal)reader["Price"],
                                        InStorePrice = (decimal)reader["InStorePrice"],
                                        TaxRate = (decimal)reader["TaxRate"],
                                        IsTaxIncluded = (bool)reader["IsTaxIncluded"],
                                        ContainsAlcohol = (bool)reader["ContainsAlcohol"],
                                        ContainsTobacco = (bool)reader["ContainsTobacco"],
                                        IsBikeFriendly = (bool)reader["IsBikeFriendly"],
                                        ShowOnline = (bool)reader["ShowOnline"],
                                        Position = (int)reader["Position"],
                                        // DietaryRestriction = reader["DietaryRestriction"].ToString(),
                                        // Spiciness = reader["Spiciness"].ToString(),

                                        Categories = reader["Categories"] == DBNull.Value
                                            ? new List<string>()
                                            : reader["Categories"].ToString().Split(',').ToList(),

                                        ModifierGroups = reader["ModifierGroups"] == DBNull.Value
                                            ? new List<string>()
                                            : reader["ModifierGroups"].ToString().Split(',').ToList()
                                    };

                                    products.Add(product);
                                }

                                returnData.Products = products;
                            }
                            #endregion Products

                        }
                    }
                    #endregion Execute SQL
                    return returnData;
                }
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
                    MethodName = nameof(GetPullCatalogAsync),
                    ErrorOccurredDateTime = DateTime.Now
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return new CatalogImportEntityDto();
            }
        }

        public async Task<bool> SyncCatalogToPosHub(string applicationId, string apiCall)
        {
            ClientsDto client = await _posHubAuthDA.GetClientDetailsByClientIdAsync(applicationId, apiCall);
            string url = $"{_baseUrl}/v1/accounts/{client.AccountId}/locations/{client.LocationId}/connections/{client.ConnectionId}/pull";
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", client.AccessToken);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync();
                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = url,
                        Event = "Post",
                        IsSuccess = false,
                        FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}"
                    });
                    return false;
                }

                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "Post",
                    IsSuccess = true,
                    FailMessage = ""
                });
                return true;

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
                    MethodName = nameof(SyncCatalogToPosHub),
                    ErrorOccurredDateTime = DateTime.Now
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return false;
            }

        }

        public async Task<CatalogProductsResponseDto> GetCatalogProducts(string applicationId, string limit, string apiCall)
        {
            ClientsDto client = await _posHubAuthDA.GetClientDetailsByClientIdAsync(applicationId, apiCall);
            var finalResponse = new CatalogProductsResponseDto
            {
                Data = new List<ProductDto>()
            };

            string nextPageKey = null;
            string url = $"{_baseUrl}/v1/accounts/{client.AccountId}/locations/{client.LocationId}/catalog/products?limit={limit}";

            if (!string.IsNullOrEmpty(nextPageKey))
            {
                url += $"&nextPageKey={nextPageKey}";
            }

            try
            {
                do
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", client.AccessToken);

                    HttpResponseMessage response = await _httpClient.SendAsync(request);
                    Console.WriteLine("count");
                    if (!response.IsSuccessStatusCode)
                    {
                        string body = await response.Content.ReadAsStringAsync();
                        
                        await _logsDA.InsertLogAsync(new LogModel
                        {
                            Url = url,
                            Event = "Get",
                            IsSuccess = false,
                            FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}"
                        });
                        await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                        {
                            ErrorMessage = $"API call failed with status {response.StatusCode}. Body: {body}",
                            ApiCall = apiCall,
                            MethodName = nameof(GetCatalogProducts),
                            ErrorOccurredDateTime = DateTime.Now
                        });
                        return finalResponse;
                    }

                    string json = await response.Content.ReadAsStringAsync();

                    CatalogProductsResponseDto catalogResponse = JsonSerializer.Deserialize<CatalogProductsResponseDto>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (catalogResponse?.Data != null && catalogResponse.Data.Any())
                    {
                        finalResponse.Data.AddRange(catalogResponse.Data);
                        await _catalogDA.UpdatePosHubProductIdsAsync(catalogResponse.Data, apiCall);
                    }
                    nextPageKey = catalogResponse?.NextPageKey;
                }
                while (!string.IsNullOrEmpty(nextPageKey));
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "Get",
                    IsSuccess = true,
                    FailMessage = ""
                });
                return finalResponse;
            }
            catch (Exception ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "Get",
                    IsSuccess = false,
                    FailMessage = ""
                });
                ApiErrorMessageModel error = new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(GetCatalogProducts),
                    ErrorOccurredDateTime = DateTime.Now
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return finalResponse;
            }
        }

        public async Task<ProductDto> GetCatalogProductByProductId(string applicationId, string productId, string apiCall)
        {
            ClientsDto client = await _posHubAuthDA.GetClientDetailsByClientIdAsync(applicationId, apiCall);
            
            string url = $"{_baseUrl}/v1/accounts/{client.AccountId}/locations/{client.LocationId}/catalog/products/{productId}";
            try
            {

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", client.AccessToken);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync();

                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = url,
                        Event = "Get",
                        IsSuccess = false,
                        FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}"
                    });
                    await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                    {
                        ErrorMessage = $"API call failed with status {response.StatusCode}. Body: {body}",
                        ApiCall = apiCall,
                        MethodName = nameof(GetCatalogProductByProductId),
                        ErrorOccurredDateTime = DateTime.Now
                    });
                    return new ProductDto();
                }

                string json = await response.Content.ReadAsStringAsync();

                ProductResponse productResponse = JsonSerializer.Deserialize<ProductResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "Get",
                    IsSuccess = true,
                    FailMessage = ""
                });
                return productResponse.Data ?? new ProductDto();
            }
            catch (Exception ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "Get",
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
                    MethodName = nameof(GetCatalogProductByProductId),
                    ErrorOccurredDateTime = DateTime.Now
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return new ProductDto();
            }
        }

        public async Task<List<ProductDataResponseByPosRefDto>> GetCatalogProductByPosRefId(string applicationId, string posRefId, string apiCall)
        {
            ClientsDto client = await _posHubAuthDA.GetClientDetailsByClientIdAsync(applicationId, apiCall);
            Console.WriteLine("Client " + client);
            string url = $"{_baseUrl}/v1/accounts/{client.AccountId}/locations/{client.LocationId}/catalog/products?posReference={posRefId}";
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", client.AccessToken);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync();

                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = url,
                        Event = "Get",
                        IsSuccess = false,
                        FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}"
                    });

                    await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                    {
                        ErrorMessage = $"API call failed with status {response.StatusCode}. Body: {body}",
                        ApiCall = apiCall,
                        MethodName = nameof(GetCatalogProductByPosRefId),
                        ErrorOccurredDateTime = DateTime.Now
                    });
                    return new List<ProductDataResponseByPosRefDto>();
                }

                string json = await response.Content.ReadAsStringAsync() ;

                ProductResponseByPosRefDto productResponseByPosRef = JsonSerializer.Deserialize<ProductResponseByPosRefDto>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (productResponseByPosRef?.Data != null && productResponseByPosRef.Data.Any())
                {
                    List<ProductDto> products = productResponseByPosRef.Data
                                            .Select(p => new ProductDto
                                            {
                                                Id = p.Id,
                                                PosReference = p.PosReference
                                            }).ToList();

                    await _catalogDA.UpdatePosHubProductIdsAsync(products, apiCall);
                }
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "Get",
                    IsSuccess = true,
                    FailMessage = ""
                });
                return productResponseByPosRef.Data ?? new List<ProductDataResponseByPosRefDto>();
            }
            catch (Exception ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "Get",
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
                    MethodName = nameof(GetCatalogProductByPosRefId),
                    ErrorOccurredDateTime = DateTime.Now
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return new List<ProductDataResponseByPosRefDto>();
            }
        }

        public async Task<ProductDto> UpdateCatalogProductByProductId(string applicationId, ProductUpdateRequestDto product, string productId, string apiCall)
        {
            ClientsDto client = await _posHubAuthDA.GetClientDetailsByClientIdAsync(applicationId, apiCall);
            string url = $"{_baseUrl}/v1/accounts/{client.AccountId}/locations/{client.LocationId}/catalog/products/{productId}";
            try
            {

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", client.AccessToken);

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string jsonContent = JsonSerializer.Serialize(product, options);

                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                Console.WriteLine(jsonContent);
                ;
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync();

                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = url,
                        Event = "Patch",
                        IsSuccess = false,
                        FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}"
                    });
                    
                    await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                    {
                        ErrorMessage = $"API call failed with status {response.StatusCode}. Body: {body}",
                        ApiCall = apiCall,
                        MethodName = nameof(UpdateCatalogProductByProductId),
                        ErrorOccurredDateTime = DateTime.Now
                    });
                    return new ProductDto();
                }

                string json = await response.Content.ReadAsStringAsync();

                ProductResponse productResponse = JsonSerializer.Deserialize<ProductResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "Patch",
                    IsSuccess = true,
                    FailMessage = ""
                });
                
                return productResponse.Data ?? new ProductDto();
            }
            catch (Exception ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "Patch",
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
                    MethodName = nameof(UpdateCatalogProductByProductId),
                    ErrorOccurredDateTime = DateTime.Now
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return new ProductDto();
            }
        }

    //     public async Task<ProductDto> CreateCatalogProductByProductId(ClientsDto client, ProductDto product, string apiCall)
    //     {
    //         try
    //         {
    //             string url = $"{_baseUrl}/v1/accounts/{client.AccountId}/locations/{client.LocationId}/catalog/products";

    //             HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
    //             request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", client.AccessToken);

    //             string jsonContent = JsonSerializer.Serialize(product);
    //             request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

    //             HttpResponseMessage response = await _httpClient.SendAsync(request);

    //             if (!response.IsSuccessStatusCode)
    //             {
    //                 string body = await response.Content.ReadAsStringAsync();
    //                 await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
    //                 {
    //                     ErrorMessage = $"API call failed with status {response.StatusCode}. Body: {body}",
    //                     ApiCall = apiCall,
    //                     MethodName = nameof(CreateCatalogProductByProductId),
    //                     ErrorOccurredDateTime = DateTime.Now
    //                 });
    //                 return new ProductDto();
    //             }

    //             string json = await response.Content.ReadAsStringAsync();

    //             ProductDto productResponse = JsonSerializer.Deserialize<ProductDto>(json,
    //                 new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    //             return productResponse;
    //         }
    //         catch (Exception ex)
    //         {
    //             ApiErrorMessageModel error = new ApiErrorMessageModel
    //             {
    //                 ErrorMessage = ex.Message,
    //                 ErrorSource = ex.Source,
    //                 StackTrace = ex.StackTrace,
    //                 InnerErrorMessage = ex.InnerException?.Message ?? "",
    //                 ApiCall = apiCall,
    //                 MethodName = nameof(CreateCatalogProductByProductId),
    //                 ErrorOccurredDateTime = DateTime.Now
    //             };

    //             await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
    //             return new ProductDto();
    //         }
    //     }

    //     public async Task<bool> DeleteCatalogProductByProductId(ClientsDto client, string productId, string apiCall)
    //     {
    //         try
    //         {
    //             string url = $"{_baseUrl}/v1/accounts/{client.AccountId}/locations/{client.LocationId}/catalog/products/{productId}";
        
    //             HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);
    //             request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", client.AccessToken);
                
    //             HttpResponseMessage response = await _httpClient.SendAsync(request);

    //             if (!response.IsSuccessStatusCode)
    //             {
    //                 string body = await response.Content.ReadAsStringAsync();
    //                 await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
    //                 {
    //                     ErrorMessage = $"API call failed with status {response.StatusCode}. Body: {body}",
    //                     ApiCall = apiCall,
    //                     MethodName = nameof(DeleteCatalogProductByProductId),
    //                     ErrorOccurredDateTime = DateTime.Now
    //                 });
    //                 return false;
    //             }

    //             return true;
    //         }
    //         catch (Exception ex)
    //         {
    //             ApiErrorMessageModel error = new ApiErrorMessageModel
    //             {
    //                 ErrorMessage = ex.Message,
    //                 ErrorSource = ex.Source,
    //                 StackTrace = ex.StackTrace,
    //                 InnerErrorMessage = ex.InnerException?.Message ?? "",
    //                 ApiCall = apiCall,
    //                 MethodName = nameof(DeleteCatalogProductByProductId),
    //                 ErrorOccurredDateTime = DateTime.Now
    //             };

    //             await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
    //             return false;
    //         }
    //     }
    
    }
}