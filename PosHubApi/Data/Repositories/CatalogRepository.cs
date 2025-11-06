using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Expressions;
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

        // CatalogImportEntityDto result = new CatalogImportEntityDto();

        //     try
        //     {
        // var response = await _httpClient.PostAsync(url, new StringContent("", Encoding.UTF8, "application/json"));

        //         if (!response.IsSuccessStatusCode)
        //         {
        // var errorBody = await response.Content.ReadAsStringAsync();
        //             result.ErrorMessage = $"Error sync: {response.StatusCode}, {errorBody}";
        //             return result;
        //         }

        // var responseStream = await response.Content.ReadAsStreamAsync();
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
        // List<ModifierGroupDto> modifierGroups = new List<ModifierGroupDto>();
        // List<ProductDto> products = new List<ProductDto>();

        // ProductDto sampleProduct = new ProductDto
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

        public async Task<(CatalogImportEntityDto, bool)> GetPullCatalogAsync(string apiCall, string accountId, string locationId)
        {
            CatalogImportEntityDto returnData = new CatalogImportEntityDto();
            List<CategoryDto> categories = new List<CategoryDto>();
            List<ModifierDto> modifiers = new List<ModifierDto>();
            List<ModifierGroupDto> modifierGroups = new List<ModifierGroupDto>();
            List<ProductDto> products = new List<ProductDto>();
            LocationHoursDto location = new LocationHoursDto();

            bool IsSuccess = true;

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
                SELECT mg.PosReference,mg.Name,mg.Description,mg.PosVersion,mg.Position,mg.MinPermitted,mg.MaxPermitted,
                    mm.ModifierPosRef,m.Name AS ModifierName,m.PosHubModifierId,m.Price 
                FROM ModifierGroups mg
                LEFT JOIN ModifierGroupModifiers mm ON mm.ModifierGroupPosRef = mg.PosReference
                LEFT JOIN Modifiers m ON m.PosReference = mm.ModifierPosRef
                ORDER BY mg.PosReference, mm.ModifierPosRef;


                /* SELECT mg.PosReference, mg.Name, mg.Description, mg.PosVersion, mg.Position, mg.MinPermitted, mg.MaxPermitted,
                    mm.ModifierPosRef
                FROM ModifierGroups mg
                LEFT JOIN ModifierGroupModifiers mm ON mm.ModifierGroupPosRef = mg.PosReference
                ORDER BY mg.PosReference, mm.ModifierPosRef; */
               

            -- Products
            SELECT p.PosReference, p.Name, p.Description, p.Type, p.PosVersion, p.OriginalImageUrl, p.Price, p.InStorePrice,
            p.TaxRate, p.IsTaxIncluded, p.ContainsAlcohol, p.ContainsTobacco, p.IsBikeFriendly, p.ShowOnline,
            p.Position, p.DietaryRestriction, p.Spiciness,p.FulfillmentTypes,
            STUFF((
                SELECT ',' + pc.CategoryPosRef
                FROM ProductCategories pc
                WHERE pc.ProductPosRef = p.PosReference
                FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS Categories,

            STUFF((
                SELECT ',' + pm.ModifierGroupPosRef
                FROM ProductModifierGroups pm
                WHERE pm.ProductPosRef = p.PosReference
                FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS ModifierGroups,

            sa.Weekday,
            tp.StartDate,
            tp.EndDate,
            tp.StartTime,
            tp.EndTime
                FROM Products p
                LEFT JOIN CategoryServiceAvailability sa ON sa.CategoryPosRef = p.PosReference
                LEFT JOIN ServiceTimePeriods tp ON tp.ServiceAvailabilityId = sa.Id
                GROUP BY p.PosReference, p.Name, p.Description, p.Type, p.PosVersion, p.OriginalImageUrl, p.Price,
                        p.InStorePrice, p.TaxRate, p.IsTaxIncluded, p.ContainsAlcohol, p.ContainsTobacco,
                        p.IsBikeFriendly, p.ShowOnline, p.Position, p.DietaryRestriction, p.Spiciness,p.FulfillmentTypes,
                        sa.Weekday, tp.StartDate, tp.EndDate, tp.StartTime, tp.EndTime;


            -- Location hours
            SELECT 
                AL.LocationId,
                LA.Weekday,
                P.StartDate,
                P.EndDate,
                P.StartTime,
                P.EndTime
            FROM [dbo].[AccountLocation] AL
            LEFT JOIN [dbo].[LocationAvailability] LA ON AL.LocationId = LA.LocationRef
            LEFT JOIN [dbo].[LocationServiceTimePeriods] P ON P.ServiceAvailabilityId = LA.Id
            WHERE AL.LocationId = @LocationId and AL.AccountId=@AccountId
            ORDER BY AL.LocationId, LA.Id, P.StartDate;
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

                        command.Parameters.AddWithValue("@LocationId", locationId);
                        command.Parameters.AddWithValue("@AccountId", accountId);

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
                                        StartDate = DateTime.Parse(reader["StartDate"]?.ToString()).ToString("yyyy-MM-dd"),
                                        EndDate = DateTime.Parse(reader["EndDate"]?.ToString()).ToString("yyyy-MM-dd"),
                                        StartTime = DateTime.Parse(reader["StartTime"]?.ToString()).ToString("HH:mm"),
                                        EndTime = DateTime.Parse(reader["EndTime"]?.ToString()).ToString("HH:mm")
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
                                    ModifierDto modifier = new ModifierDto
                                    {
                                        PosReference = reader["PosReference"].ToString(),
                                        Name = reader["Name"].ToString(),
                                        Description = reader["Description"].ToString(),
                                        PosVersion = reader["PosVersion"].ToString(),
                                        OriginalImageUrl = reader["OriginalImageUrl"].ToString(),
                                        Price = (decimal)reader["Price"] * 100,
                                        InStorePrice = (decimal)reader["InStorePrice"] * 100,
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
                                            Modifiers = new List<ModifierDtoForGroupDto>()
                                        };
                                        modifierGroups.Add(currentGroup);
                                        lastGroupRef = groupRef;
                                    }
                                    if (reader["ModifierPosRef"] != DBNull.Value)
                                    {
                                        var modifierForGroup = new ModifierDtoForGroupDto
                                        {
                                            Id = reader["ModifierPosRef"].ToString(),
                                            Name = reader["ModifierName"].ToString(),
                                            Price = reader["Price"] != DBNull.Value ? Convert.ToDecimal(reader["Price"]) * 100 : 0
                                        };

                                        currentGroup.Modifiers.Add(modifierForGroup);
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
                                    string prodPosRef = reader["PosReference"].ToString();
                                    ProductDto product = products.FirstOrDefault(p => p.PosReference == prodPosRef);
                                    if (product == null)
                                    {
                                        product = new ProductDto
                                        {
                                            PosReference = prodPosRef,
                                            Name = reader["Name"].ToString(),
                                            Description = reader["Description"].ToString(),
                                            Type = reader["Type"].ToString(),
                                            PosVersion = reader["PosVersion"].ToString(),
                                            OriginalImageUrl = reader["OriginalImageUrl"].ToString(),
                                            Price = (decimal)reader["Price"] * 100,
                                            InStorePrice = (decimal)reader["InStorePrice"] * 100,
                                            TaxRate = (decimal)reader["TaxRate"],
                                            IsTaxIncluded = (bool)reader["IsTaxIncluded"],
                                            ContainsAlcohol = (bool)reader["ContainsAlcohol"],
                                            ContainsTobacco = (bool)reader["ContainsTobacco"],
                                            IsBikeFriendly = (bool)reader["IsBikeFriendly"],
                                            ShowOnline = (bool)reader["ShowOnline"],
                                            Position = (int)reader["Position"],

                                            Categories = reader["Categories"] == DBNull.Value
                                                ? new List<string>()
                                                : reader["Categories"].ToString().Split(',').ToList(),

                                            ModifierGroups = reader["ModifierGroups"] == DBNull.Value
                                                ? new List<string>()
                                                : reader["ModifierGroups"].ToString().Split(',').ToList(),

                                            ServiceAvailability = new List<ServiceAvailabilityDto>(),

                                            FulfillmentTypes = reader["FulfillmentTypes"] == DBNull.Value
                                                ? new List<string>()
                                                : reader["FulfillmentTypes"].ToString().Split(',').ToList(),

                                            NutritionalInfo = new NutritionalInfoDto
                                            {
                                                DietaryRestriction = reader["DietaryRestriction"].ToString(),
                                                Spiciness = reader["Spiciness"].ToString(),
                                            }
                                        };
                                        products.Add(product);
                                    }

                                    if (reader["Weekday"] != DBNull.Value)
                                    {
                                        string weekday = reader["Weekday"].ToString();
                                        var serviceAvailability = product.ServiceAvailability
                                            .FirstOrDefault(sa => sa.Weekday == weekday);
                                        if (serviceAvailability == null)
                                        {
                                            serviceAvailability = new ServiceAvailabilityDto
                                            {
                                                Weekday = weekday,
                                                TimePeriods = new List<TimePeriodDto>()
                                            };
                                            product.ServiceAvailability.Add(serviceAvailability);
                                        }

                                        serviceAvailability.TimePeriods.Add(new TimePeriodDto
                                        {
                                            StartDate = DateTime.Parse(reader["StartDate"]?.ToString()).ToString("yyyy-MM-dd"),
                                            EndDate = DateTime.Parse(reader["EndDate"]?.ToString()).ToString("yyyy-MM-dd"),
                                            StartTime = DateTime.Parse(reader["StartTime"]?.ToString()).ToString("HH:mm"),
                                            EndTime = DateTime.Parse(reader["EndTime"]?.ToString()).ToString("HH:mm")
                                        });
                                    }
                                }

                                returnData.Products = products;
                            }
                            #endregion Products

                            #region Location Hours
                            if (await reader.NextResultAsync())
                            {
                                List<ServiceAvailabilityDto> businessHours = new List<ServiceAvailabilityDto>();

                                while (await reader.ReadAsync())
                                {
                                    string weekday = reader["Weekday"]?.ToString();
                                    if (string.IsNullOrEmpty(weekday)) continue;

                                    var availability = businessHours.FirstOrDefault(sa => sa.Weekday == weekday);
                                    if (availability == null)
                                    {
                                        availability = new ServiceAvailabilityDto
                                        {
                                            Weekday = weekday,
                                            TimePeriods = new List<TimePeriodDto>()
                                        };
                                        businessHours.Add(availability);
                                    }

                                    availability.TimePeriods.Add(new TimePeriodDto
                                    {
                                        StartDate = reader["StartDate"] != DBNull.Value
                                            ? DateTime.Parse(reader["StartDate"].ToString()).ToString("yyyy-MM-dd")
                                            : null,
                                        EndDate = reader["EndDate"] != DBNull.Value
                                            ? DateTime.Parse(reader["EndDate"].ToString()).ToString("yyyy-MM-dd")
                                            : null,
                                        StartTime = reader["StartTime"] != DBNull.Value
                                            ? DateTime.Parse(reader["StartTime"].ToString()).ToString("HH:mm")
                                            : null,
                                        EndTime = reader["EndTime"] != DBNull.Value
                                            ? DateTime.Parse(reader["EndTime"].ToString()).ToString("HH:mm")
                                            : null
                                    });
                                }

                                returnData.Location = new LocationHoursDto
                                {
                                    BusinessHours = businessHours
                                };
                            }
                            #endregion Location Hours


                        }
                    }
                    #endregion Execute SQL
                    return (returnData, IsSuccess);
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
                IsSuccess = false;
                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return (new CatalogImportEntityDto(), IsSuccess);
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
                        Event = "SyncCatalogToPosHub",
                        IsSuccess = false,
                        FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}",
                        RequestBody = body,
                        ApplicationId = applicationId
                    });
                    return false;
                }

                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "SyncCatalogToPosHub",
                    IsSuccess = true,
                    FailMessage = "",
                    RequestBody = "",
                    ApplicationId = applicationId
                });
                return true;

            }
            catch (Exception ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "SyncCatalogToPosHub",
                    IsSuccess = false,
                    FailMessage = ex.Message,
                    RequestBody = "",
                    ApplicationId = applicationId
                });
                ApiErrorMessageModel error = new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(SyncCatalogToPosHub),
                    ErrorOccurredDateTime = DateTime.Now,
                    ClientId = applicationId

                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return false;
            }

        }

        // public async Task<CatalogProductsResponseDto> GetCatalogProducts(string applicationId, string limit, string apiCall)
        // {
        //     ClientsDto client = await _posHubAuthDA.GetClientDetailsByClientIdAsync(applicationId, apiCall);
        //     CatalogProductsResponseDto finalResponse = new CatalogProductsResponseDto
        //     {
        //         Data = new List<ProductDto>()
        //     };

        //     string nextPageKey = null;
        //     string lastUrl = string.Empty;

        //     try
        //     {
        //         do
        //         {
        //             string url = $"{_baseUrl}/v1/accounts/{client.AccountId}/locations/{client.LocationId}/catalog/products?limit={limit}";

        //             if (!string.IsNullOrEmpty(nextPageKey))
        //             {
        //                 url += $"&nextPageKey={nextPageKey}";
        //             }
        //             lastUrl = url;

        //             HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
        //             request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", client.AccessToken);

        //             HttpResponseMessage response = await _httpClient.SendAsync(request);
        //             Console.WriteLine("count");
        //             if (!response.IsSuccessStatusCode)
        //             {
        //                 string body = await response.Content.ReadAsStringAsync();

        //                 await _logsDA.InsertLogAsync(new LogModel
        //                 {
        //                     Url = url,
        //                     Event = "GetCatalogProducts",
        //                     IsSuccess = false,
        //                     FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}",
        //                     RequestBody = body,
        //                     ApplicationId = applicationId
        //                 });
        //                 await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
        //                 {
        //                     ErrorMessage = $"API call failed with status {response.StatusCode}. Body: {body}",
        //                     ApiCall = apiCall,
        //                     MethodName = nameof(GetCatalogProducts),
        //                     ErrorOccurredDateTime = DateTime.Now,
        //                     ClientId = applicationId

        //                 });
        //                 return finalResponse;
        //             }

        //             string json = await response.Content.ReadAsStringAsync();

        //             CatalogProductsResponseDto catalogResponse = JsonSerializer.Deserialize<CatalogProductsResponseDto>(json,
        //                 new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //             if (catalogResponse?.Data != null && catalogResponse.Data.Any())
        //             {
        //                 finalResponse.Data.AddRange(catalogResponse.Data);
        //                 await _catalogDA.UpdatePosHubProductIdsAsync(catalogResponse.Data, apiCall);
        //             }
        //             nextPageKey = catalogResponse?.NextPageKey;
        //         }

        //         while (!string.IsNullOrEmpty(nextPageKey));

        //         await _logsDA.InsertLogAsync(new LogModel
        //         {
        //             Url = lastUrl,
        //             Event = "GetCatalogProducts",
        //             IsSuccess = true,
        //             FailMessage = "",
        //             RequestBody = "",
        //             ApplicationId = applicationId
        //         });
        //         return finalResponse;
        //     }
        //     catch (Exception ex)
        //     {
        //         await _logsDA.InsertLogAsync(new LogModel
        //         {
        //             Url = lastUrl,
        //             Event = "GetCatalogProducts",
        //             IsSuccess = false,
        //             FailMessage = "",
        //             RequestBody = "",
        //             ApplicationId = applicationId
        //         });
        //         ApiErrorMessageModel error = new ApiErrorMessageModel
        //         {
        //             ErrorMessage = ex.Message,
        //             ErrorSource = ex.Source,
        //             StackTrace = ex.StackTrace,
        //             InnerErrorMessage = ex.InnerException?.Message ?? "",
        //             ApiCall = apiCall,
        //             MethodName = nameof(GetCatalogProducts),
        //             ErrorOccurredDateTime = DateTime.Now,
        //             ClientId = applicationId
        //         };

        //         await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
        //         return finalResponse;
        //     }
        // }

        public async Task<List<ProductDto>> GetCatalogProducts(string apiCall)
        {
            List<ProductDto> products = await _posHubAuthDA.GetCatalogProductsDetails(apiCall);
            return products;
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
                        Event = "GetCatalogProductByProductId",
                        IsSuccess = false,
                        FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}",
                        RequestBody = body,
                        ApplicationId = applicationId,
                        UniqueId = productId
                    });
                    await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                    {
                        ErrorMessage = $"API call failed with status {response.StatusCode}. Body: {body}",
                        ApiCall = apiCall,
                        MethodName = nameof(GetCatalogProductByProductId),
                        ErrorOccurredDateTime = DateTime.Now,
                        ClientId = applicationId

                    });
                    return new ProductDto();
                }

                string json = await response.Content.ReadAsStringAsync();

                ProductResponse productResponse = JsonSerializer.Deserialize<ProductResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "GetCatalogProductByProductId",
                    IsSuccess = true,
                    FailMessage = "",
                    RequestBody = "",
                    ApplicationId = applicationId,
                    UniqueId = productId
                });
                return productResponse.Data ?? new ProductDto();
            }
            catch (Exception ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "GetCatalogProductByProductId",
                    IsSuccess = false,
                    FailMessage = ex.Message,
                    RequestBody = "",
                    ApplicationId = applicationId,
                    UniqueId = productId
                });
                ApiErrorMessageModel error = new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(GetCatalogProductByProductId),
                    ErrorOccurredDateTime = DateTime.Now,
                    ClientId = applicationId
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
                        Event = "GetCatalogProductByPosRefId",
                        IsSuccess = false,
                        FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}",
                        RequestBody = body,
                        ApplicationId = applicationId,
                        UniqueId = posRefId
                    });

                    await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                    {
                        ErrorMessage = $"API call failed with status {response.StatusCode}. Body: {body}",
                        ApiCall = apiCall,
                        MethodName = nameof(GetCatalogProductByPosRefId),
                        ErrorOccurredDateTime = DateTime.Now,
                        ClientId = applicationId
                    });
                    return new List<ProductDataResponseByPosRefDto>();
                }

                string json = await response.Content.ReadAsStringAsync();

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
                    Event = "GetCatalogProductByPosRefId",
                    IsSuccess = true,
                    FailMessage = "",
                    RequestBody = "",
                    ApplicationId = applicationId,
                    UniqueId = posRefId
                });
                return productResponseByPosRef.Data ?? new List<ProductDataResponseByPosRefDto>();
            }
            catch (Exception ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "GetCatalogProductByPosRefId",
                    IsSuccess = false,
                    FailMessage = ex.Message,
                    RequestBody = "",
                    ApplicationId = applicationId,
                    UniqueId = posRefId
                });
                ApiErrorMessageModel error = new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(GetCatalogProductByPosRefId),
                    ErrorOccurredDateTime = DateTime.Now,
                    ClientId = applicationId
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

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync();

                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = url,
                        Event = "UpdateCatalogProductByProductId",
                        IsSuccess = false,
                        FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}",
                        RequestBody = body,
                        ApplicationId = applicationId,
                        UniqueId = productId
                    });

                    await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                    {
                        ErrorMessage = $"API call failed with status {response.StatusCode}. Body: {body}",
                        ApiCall = apiCall,
                        MethodName = nameof(UpdateCatalogProductByProductId),
                        ErrorOccurredDateTime = DateTime.Now,
                        ClientId = applicationId
                    });
                    return new ProductDto();
                }

                string json = await response.Content.ReadAsStringAsync();

                ProductResponse productResponse = JsonSerializer.Deserialize<ProductResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "UpdateCatalogProductByProductId",
                    IsSuccess = true,
                    FailMessage = "",
                    RequestBody = "",
                    ApplicationId = applicationId,
                    UniqueId = productId
                });

                return productResponse.Data ?? new ProductDto();
            }
            catch (Exception ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "UpdateCatalogProductByProductId",
                    IsSuccess = false,
                    FailMessage = ex.Message,
                    RequestBody = "",
                    ApplicationId = applicationId,
                    UniqueId = productId
                });
                ApiErrorMessageModel error = new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(UpdateCatalogProductByProductId),
                    ErrorOccurredDateTime = DateTime.Now,
                    ClientId = applicationId
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return new ProductDto();
            }
        }

        public async Task<bool> DeleteCatalogProductByPosRefId(string applicationId, string posRefId, string apiCall)
        {
            try
            {
                ClientsDto client = await _posHubAuthDA.GetClientDetailsByClientIdAsync(applicationId, apiCall);
                Console.WriteLine("Client " + client);

                string? posHubProductId = await _catalogDA.GetPosHubProductIdByPosReferenceAsync(posRefId);

                if (string.IsNullOrEmpty(posHubProductId))
                {
                    List<ProductDataResponseByPosRefDto> externalProducts = await GetCatalogProductByPosRefId(applicationId, posRefId, apiCall);

                    if (externalProducts != null && externalProducts.Count > 0)
                    {
                        // Use the first result
                        var product = externalProducts.FirstOrDefault();
                        posHubProductId = product?.Id;
                    }
                }

                if (string.IsNullOrEmpty(posHubProductId))
                {
                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = apiCall,
                        Event = "DeleteCatalogProductByPosRefId",
                        IsSuccess = false,
                        FailMessage = "PosHubProductId not found from DB or API.",
                        RequestBody = "",
                        ApplicationId = applicationId,
                        UniqueId = posRefId
                    });

                    return false;
                }

                string deleteUrl = $"{_baseUrl}/v1/accounts/{client.AccountId}/locations/{client.LocationId}/catalog/products/{posHubProductId}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", client.AccessToken);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync();

                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = deleteUrl,
                        Event = "DeleteCatalogProductByPosRefId",
                        IsSuccess = false,
                        FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}",
                        RequestBody = body,
                        ApplicationId = applicationId,
                        UniqueId = posRefId
                    });

                    await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                    {
                        ErrorMessage = $"API call failed with status {response.StatusCode}. Body: {body}",
                        ApiCall = apiCall,
                        MethodName = nameof(DeleteCatalogProductByPosRefId),
                        ErrorOccurredDateTime = DateTime.Now,
                        ClientId = applicationId
                    });

                    return false;
                }

                await _catalogDA.DeleteProductAndRelationsByPosReferenceAsync(posRefId);

                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = deleteUrl,
                    Event = "DeleteCatalogProductByPosRefId",
                    IsSuccess = true,
                    FailMessage = "",
                    RequestBody = "",
                    ApplicationId = applicationId,
                    UniqueId = posRefId
                });

                return true;
            }
            catch (Exception ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = apiCall,
                    Event = "DeleteCatalogProductByPosRefId",
                    IsSuccess = false,
                    FailMessage = ex.Message,
                    RequestBody = "",
                    ApplicationId = applicationId,
                    UniqueId = posRefId
                });

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(DeleteCatalogProductByPosRefId),
                    ErrorOccurredDateTime = DateTime.Now,
                    ClientId = applicationId
                });

                return false;
            }
        }

        public async Task<List<CategoryDataResponseByPosRefDto>> GetCatalogCategoryByPosRefId(string applicationId, string posRefId, string apiCall)
        {
            ClientsDto client = await _posHubAuthDA.GetClientDetailsByClientIdAsync(applicationId, apiCall);
            Console.WriteLine("Client " + client);
            string url = $"{_baseUrl}/v1/accounts/{client.AccountId}/locations/{client.LocationId}/catalog/categories?posReference={posRefId}";
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
                        Event = "GetCatalogCategoryByPosRefId",
                        IsSuccess = false,
                        FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}",
                        RequestBody = body,
                        ApplicationId = applicationId,
                        UniqueId = posRefId
                    });

                    await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                    {
                        ErrorMessage = $"API call failed with status {response.StatusCode}. Body: {body}",
                        ApiCall = apiCall,
                        MethodName = nameof(GetCatalogCategoryByPosRefId),
                        ErrorOccurredDateTime = DateTime.Now,
                        ClientId = applicationId
                    });
                    return new List<CategoryDataResponseByPosRefDto>();
                }

                string json = await response.Content.ReadAsStringAsync();

                CategoryResponseByPosRefDto categoryResponseByPosRef = JsonSerializer.Deserialize<CategoryResponseByPosRefDto>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (categoryResponseByPosRef?.Data != null && categoryResponseByPosRef.Data.Any())
                {
                    List<CategoryDto> categories = categoryResponseByPosRef.Data
                                            .Select(c => new CategoryDto
                                            {
                                                Id = c.Id,
                                                PosReference = c.PosReference
                                            }).ToList();

                    await _catalogDA.UpdatePosHubCategoryIdsAsync(categories, apiCall);
                }
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "GetCatalogCategoryByPosRefId",
                    IsSuccess = true,
                    FailMessage = "",
                    RequestBody = "",
                    ApplicationId = applicationId,
                    UniqueId = posRefId
                });
                return categoryResponseByPosRef.Data ?? new List<CategoryDataResponseByPosRefDto>();
            }
            catch (Exception ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "GetCatalogCategoryByPosRefId",
                    IsSuccess = false,
                    FailMessage = ex.Message,
                    RequestBody = "",
                    ApplicationId = applicationId,
                    UniqueId = posRefId
                });
                ApiErrorMessageModel error = new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(GetCatalogCategoryByPosRefId),
                    ErrorOccurredDateTime = DateTime.Now,
                    ClientId = applicationId
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return new List<CategoryDataResponseByPosRefDto>();
            }
        }

        public async Task<bool> DeleteCatalogCategoryByPosRefId(string applicationId, string posRefId, string apiCall)
        {
            try
            {
                ClientsDto client = await _posHubAuthDA.GetClientDetailsByClientIdAsync(applicationId, apiCall);
                Console.WriteLine("Client " + client);

                string? posHubCategoryId = await _catalogDA.GetPosHubCategoryIdByPosReferenceAsync(posRefId);

                if (string.IsNullOrEmpty(posHubCategoryId))
                {
                    List<CategoryDataResponseByPosRefDto> externalCategories = await GetCatalogCategoryByPosRefId(applicationId, posRefId, apiCall);

                    if (externalCategories != null && externalCategories.Count > 0)
                    {
                        // Use the first result
                        CategoryDataResponseByPosRefDto category = externalCategories.FirstOrDefault();
                        posHubCategoryId = category?.Id;
                    }
                }

                if (string.IsNullOrEmpty(posHubCategoryId))
                {
                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = apiCall,
                        Event = "DeleteCatalogCategoryByPosRefId",
                        IsSuccess = false,
                        FailMessage = "PosHubCategoryId not found from DB or API.",
                        RequestBody = "",
                        ApplicationId = applicationId,
                        UniqueId = posRefId
                    });

                    return false;
                }

                string deleteUrl = $"{_baseUrl}/v1/accounts/{client.AccountId}/locations/{client.LocationId}/catalog/categories/{posHubCategoryId}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", client.AccessToken);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync();

                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = deleteUrl,
                        Event = "DeleteCatalogCategoryByPosRefId",
                        IsSuccess = false,
                        FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}",
                        RequestBody = body,
                        ApplicationId = applicationId,
                        UniqueId = posRefId
                    });

                    await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                    {
                        ErrorMessage = $"API call failed with status {response.StatusCode}. Body: {body}",
                        ApiCall = apiCall,
                        MethodName = nameof(DeleteCatalogCategoryByPosRefId),
                        ErrorOccurredDateTime = DateTime.Now,
                        ClientId = applicationId
                    });

                    return false;
                }

                await _catalogDA.DeleteCategoryAndRelationsByPosReferenceAsync(posRefId);

                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = deleteUrl,
                    Event = "DeleteCatalogCategoryByPosRefId",
                    IsSuccess = true,
                    FailMessage = "",
                    RequestBody = "",
                    ApplicationId = applicationId,
                    UniqueId = posRefId
                });

                return true;
            }
            catch (Exception ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = apiCall,
                    Event = "DeleteCatalogCategoryByPosRefId",
                    IsSuccess = false,
                    FailMessage = ex.Message,
                    RequestBody = "",
                    ApplicationId = applicationId,
                    UniqueId = posRefId
                });

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(DeleteCatalogCategoryByPosRefId),
                    ErrorOccurredDateTime = DateTime.Now,
                    ClientId = applicationId
                });

                return false;
            }
        }

        public async Task<bool> UpdateProductByPosRefId(string applicationId, ProductDto product, string apiCall)
        {

            ClientsDto client = await _posHubAuthDA.GetClientDetailsByClientIdAsync(applicationId, apiCall);
            string? posHubProductId = await _catalogDA.GetPosHubProductIdByPosReferenceAsync(product.PosReference);
            string url = $"{_baseUrl}/v1/accounts/{client.AccountId}/locations/{client.LocationId}/catalog/products/{posHubProductId}";

            try
            {
                if (string.IsNullOrEmpty(posHubProductId))
                {
                    List<ProductDataResponseByPosRefDto> externalProducts = await GetCatalogProductByPosRefId(applicationId, product.PosReference, apiCall);

                    if (externalProducts != null && externalProducts.Count > 0)
                    {
                        ProductDataResponseByPosRefDto productData = externalProducts.FirstOrDefault();
                        posHubProductId = productData?.Id;
                    }
                }

                if (string.IsNullOrEmpty(posHubProductId))
                {
                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = apiCall,
                        Event = "UpdateProductByPosRefId",
                        IsSuccess = false,
                        FailMessage = "PosHubProductId not found from DB or API.",
                        RequestBody = "",
                        ApplicationId = applicationId,
                        UniqueId = product.PosReference
                    });

                    return false;
                }

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", client.AccessToken);

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                ProductUpdateRequestDto productUpdateRequestDto = new ProductUpdateRequestDto();
                productUpdateRequestDto.NutritionalInfo = product.NutritionalInfo;
                productUpdateRequestDto.ContainsAlcohol = product.ContainsAlcohol;
                productUpdateRequestDto.IsTaxIncluded = product.IsTaxIncluded;
                productUpdateRequestDto.PosVersion = product.PosVersion;
                productUpdateRequestDto.Description = product.Description;
                productUpdateRequestDto.Type = product.Type;
                productUpdateRequestDto.ModifierGroups = product.ModifierGroups;
                // productUpdateRequestDto.ParentId = product.ParentId;
                productUpdateRequestDto.OriginalImageUrl = product.OriginalImageUrl;
                productUpdateRequestDto.TaxRate = product.TaxRate;
                productUpdateRequestDto.IsBikeFriendly = product.IsBikeFriendly;
                productUpdateRequestDto.ShowOnline = product.ShowOnline;
                productUpdateRequestDto.IsBikeFriendly = product.IsBikeFriendly;
                // productUpdateRequestDto.Selections = product.Selections;
                // productUpdateRequestDto.TaxRateIds = product.TaxRateIds;
                productUpdateRequestDto.Price = product.Price;
                productUpdateRequestDto.Name = product.Name;
                productUpdateRequestDto.ContainsTobacco = product.ContainsTobacco;
                productUpdateRequestDto.ServiceAvailability = product.ServiceAvailability;
                productUpdateRequestDto.Categories = product.Categories;
                productUpdateRequestDto.Position = product.Position;
                // productUpdateRequestDto.InStorePrice = product.InStorePrice;

                string jsonContent = JsonSerializer.Serialize(productUpdateRequestDto, options);

                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync();

                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = url,
                        Event = "UpdateProductByPosRefId",
                        IsSuccess = false,
                        FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}",
                        RequestBody = body,
                        ApplicationId = applicationId,
                        UniqueId = posHubProductId
                    });

                    await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                    {
                        ErrorMessage = $"API call failed with status {response.StatusCode}. Body: {body}",
                        ApiCall = apiCall,
                        MethodName = nameof(UpdateProductByPosRefId),
                        ErrorOccurredDateTime = DateTime.Now,
                        ClientId = applicationId
                    });
                    return false;
                }

                bool isUpdated = await _catalogDA.UpdateProductByPosRefId(product, apiCall);

                if (!isUpdated)
                {
                    var error = new ApiErrorMessageModel
                    {
                        ErrorMessage = "UpdateProductByPosRefId failed for PosReference: " + product.PosReference,
                        ApiCall = apiCall,
                        MethodName = nameof(UpdateProductByPosRefId),
                        ErrorOccurredDateTime = DateTime.Now,
                        ClientId = applicationId
                    };

                    await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                    return false;
                }

                // await SyncCatalogToPosHub(applicationId, apiCall);

                return true;
            }

            catch (Exception ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "UpdateProductByPosRefId",
                    IsSuccess = false,
                    FailMessage = ex.Message,
                    RequestBody = "",
                    ApplicationId = applicationId,
                    UniqueId = posHubProductId
                });
                ApiErrorMessageModel error = new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(UpdateProductByPosRefId),
                    ErrorOccurredDateTime = DateTime.Now,
                    ClientId = applicationId
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return false;
            }

        }

        public async Task<CatalogModifiersResponseDto> GetCatalogModifiers(string applicationId, string limit, string apiCall)
        {
            ClientsDto client = await _posHubAuthDA.GetClientDetailsByClientIdAsync(applicationId, apiCall);
            CatalogModifiersResponseDto finalResponse = new CatalogModifiersResponseDto
            {
                Data = new List<ModifierDto>()
            };

            string nextPageKey = null;
            string lastUrl = string.Empty;

            try
            {
                do
                {
                    string url = $"{_baseUrl}/v1/accounts/{client.AccountId}/locations/{client.LocationId}/catalog/modifiers?limit={limit}";

                    if (!string.IsNullOrEmpty(nextPageKey))
                    {
                        url += $"&nextPageKey={nextPageKey}";
                    }
                    lastUrl = url;

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", client.AccessToken);

                    HttpResponseMessage response = await _httpClient.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        string body = await response.Content.ReadAsStringAsync();

                        await _logsDA.InsertLogAsync(new LogModel
                        {
                            Url = url,
                            Event = "GetCatalogModifiers",
                            IsSuccess = false,
                            FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}",
                            RequestBody = body,
                            ApplicationId = applicationId
                        });
                        await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                        {
                            ErrorMessage = $"API call failed with status {response.StatusCode}. Body: {body}",
                            ApiCall = apiCall,
                            MethodName = nameof(GetCatalogModifiers),
                            ErrorOccurredDateTime = DateTime.Now,
                            ClientId = applicationId

                        });
                        return finalResponse;
                    }

                    string json = await response.Content.ReadAsStringAsync();

                    CatalogModifiersResponseDto catalogResponse = JsonSerializer.Deserialize<CatalogModifiersResponseDto>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (catalogResponse?.Data != null && catalogResponse.Data.Any())
                    {
                        finalResponse.Data.AddRange(catalogResponse.Data);
                        await _catalogDA.UpdatePosHubModifierIdsAsync(catalogResponse.Data, apiCall);
                    }
                    nextPageKey = catalogResponse?.NextPageKey;
                }

                while (!string.IsNullOrEmpty(nextPageKey));

                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = lastUrl,
                    Event = "GetCatalogModifiers",
                    IsSuccess = true,
                    FailMessage = "",
                    RequestBody = "",
                    ApplicationId = applicationId
                });
                return finalResponse;
            }
            catch (Exception ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = lastUrl,
                    Event = "GetCatalogModifiers",
                    IsSuccess = false,
                    FailMessage = "",
                    RequestBody = "",
                    ApplicationId = applicationId
                });
                ApiErrorMessageModel error = new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(GetCatalogModifiers),
                    ErrorOccurredDateTime = DateTime.Now,
                    ClientId = applicationId
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return finalResponse;
            }
        }

        public async Task<List<ModifierDataResponseByPosRefDto>> GetCatalogModifierByPosRefId(string applicationId, string posRefId, string apiCall)
        {
            ClientsDto client = await _posHubAuthDA.GetClientDetailsByClientIdAsync(applicationId, apiCall);
            Console.WriteLine("Client " + client);
            string url = $"{_baseUrl}/v1/accounts/{client.AccountId}/locations/{client.LocationId}/catalog/modifiers?posReference={posRefId}";
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
                        Event = "GetCatalogModifierByPosRefId",
                        IsSuccess = false,
                        FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}",
                        RequestBody = body,
                        ApplicationId = applicationId,
                        UniqueId = posRefId
                    });

                    await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                    {
                        ErrorMessage = $"API call failed with status {response.StatusCode}. Body: {body}",
                        ApiCall = apiCall,
                        MethodName = nameof(GetCatalogProductByPosRefId),
                        ErrorOccurredDateTime = DateTime.Now,
                        ClientId = applicationId
                    });
                    return new List<ModifierDataResponseByPosRefDto>();
                }

                string json = await response.Content.ReadAsStringAsync();

                ModifierResponseByPosRefDto modifierResponseByPosRef = JsonSerializer.Deserialize<ModifierResponseByPosRefDto>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (modifierResponseByPosRef?.Data != null && modifierResponseByPosRef.Data.Any())
                {
                    List<ModifierDto> modifiers = modifierResponseByPosRef.Data
                                            .Select(p => new ModifierDto
                                            {
                                                Id = p.Id,
                                                PosReference = p.PosReference
                                            }).ToList();

                    await _catalogDA.UpdatePosHubModifierIdsAsync(modifiers, apiCall);
                }
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "GetCatalogModifierByPosRefId",
                    IsSuccess = true,
                    FailMessage = "",
                    RequestBody = "",
                    ApplicationId = applicationId,
                    UniqueId = posRefId
                });
                return modifierResponseByPosRef.Data ?? new List<ModifierDataResponseByPosRefDto>();
            }
            catch (Exception ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "GetCatalogModifierByPosRefId",
                    IsSuccess = false,
                    FailMessage = ex.Message,
                    RequestBody = "",
                    ApplicationId = applicationId,
                    UniqueId = posRefId
                });
                ApiErrorMessageModel error = new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(GetCatalogProductByPosRefId),
                    ErrorOccurredDateTime = DateTime.Now,
                    ClientId = applicationId
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return new List<ModifierDataResponseByPosRefDto>();
            }
        }

        public async Task<bool> UpdateModifierByPosRefId(string applicationId, ModifierDto modifier, string apiCall)
        {

            ClientsDto client = await _posHubAuthDA.GetClientDetailsByClientIdAsync(applicationId, apiCall);
            string? posHubModifierId = await _catalogDA.GetPosHubModifierIdByPosReferenceAsync(modifier.PosReference);

            if (string.IsNullOrEmpty(posHubModifierId))
            {
                List<ModifierDataResponseByPosRefDto> externalModifiers = await GetCatalogModifierByPosRefId(applicationId, modifier.PosReference, apiCall);

                if (externalModifiers != null && externalModifiers.Count > 0)
                {
                    ModifierDataResponseByPosRefDto modifierData = externalModifiers.FirstOrDefault();
                    posHubModifierId = modifierData?.Id;
                }
            }

            if (string.IsNullOrEmpty(posHubModifierId))
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = apiCall,
                    Event = "UpdateModifierByPosRefId",
                    IsSuccess = false,
                    FailMessage = "PosHubModifierId not found from DB or API.",
                    RequestBody = "",
                    ApplicationId = applicationId,
                    UniqueId = posHubModifierId
                });

                return false;
            }

            string url = $"{_baseUrl}/v1/accounts/{client.AccountId}/locations/{client.LocationId}/catalog/modifiers/{posHubModifierId}";

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", client.AccessToken);

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                ModifierUpdateRequestDto modifierUpdateRequestDto = new ModifierUpdateRequestDto();
                modifierUpdateRequestDto.NutritionalInfo = modifier.NutritionalInfo;
                modifierUpdateRequestDto.ContainsAlcohol = modifier.ContainsAlcohol;
                modifierUpdateRequestDto.IsTaxIncluded = modifier.IsTaxIncluded;
                modifierUpdateRequestDto.PosVersion = modifier.PosVersion;
                modifierUpdateRequestDto.Description = modifier.Description;
                modifierUpdateRequestDto.OriginalImageUrl = modifier.OriginalImageUrl;
                modifierUpdateRequestDto.TaxRate = modifier.TaxRate;
                modifierUpdateRequestDto.IsBikeFriendly = modifier.IsBikeFriendly;
                modifierUpdateRequestDto.ShowOnline = modifier.ShowOnline;
                modifierUpdateRequestDto.IsBikeFriendly = modifier.IsBikeFriendly;
                modifierUpdateRequestDto.TaxRateIds = modifier.TaxRateIds;
                modifierUpdateRequestDto.Price = modifier.Price;
                modifierUpdateRequestDto.Name = modifier.Name;
                modifierUpdateRequestDto.ContainsTobacco = modifier.ContainsTobacco;
                modifierUpdateRequestDto.Position = modifier.Position;
                // modifierUpdateRequestDto.InStorePrice = modifier.InStorePrice;

                string jsonContent = JsonSerializer.Serialize(modifierUpdateRequestDto, options);

                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync();

                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = url,
                        Event = "UpdateModifierByPosRefId",
                        IsSuccess = false,
                        FailMessage = $"Failed with status code {(int)response.StatusCode} - {response.ReasonPhrase}",
                        RequestBody = body,
                        ApplicationId = applicationId,
                        UniqueId = modifier.Id
                    });

                    await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                    {
                        ErrorMessage = $"API call failed with status {response.StatusCode}. Body: {body}",
                        ApiCall = apiCall,
                        MethodName = nameof(UpdateModifierByPosRefId),
                        ErrorOccurredDateTime = DateTime.Now,
                        ClientId = applicationId
                    });
                    return false;
                }

                // modifier.Id = posHubModifierId;

                bool isUpdated = await _catalogDA.UpdateModifierByPosRefId(modifier, apiCall);

                if (!isUpdated)
                {
                    var error = new ApiErrorMessageModel
                    {
                        ErrorMessage = "UpdateModifierByPosRefId failed for PosReference: " + modifier.PosReference,
                        ApiCall = apiCall,
                        MethodName = nameof(UpdateModifierByPosRefId),
                        ErrorOccurredDateTime = DateTime.Now,
                        ClientId = applicationId
                    };

                    await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                    return false;
                }

                // await SyncCatalogToPosHub(applicationId, apiCall);

                return true;
            }

            catch (Exception ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = url,
                    Event = "UpdateModifierByPosRefId",
                    IsSuccess = false,
                    FailMessage = ex.Message,
                    RequestBody = "",
                    ApplicationId = applicationId,
                    UniqueId = modifier.Id
                });
                ApiErrorMessageModel error = new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(UpdateModifierByPosRefId),
                    ErrorOccurredDateTime = DateTime.Now,
                    ClientId = applicationId
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return false;
            }

        }


    }
}