using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using PosHubApi.Dtos;
using PosHubApi.Models;

namespace PosHubApi.Data.DataAccess
{
    public class CatalogDA
    {
        private readonly string _defaultConnectionString;
        private readonly ApiErrorDA _apiErrorDA;
        public CatalogDA(IConfiguration configuration, ApiErrorDA apiErrorDA)
        {
            _defaultConnectionString = configuration.GetConnectionString("DefaultConnection");
            _apiErrorDA = apiErrorDA;
        }

        //     #region Catalog
        //     public async Task<CatalogImportEntityDto> GetCatalogSync(ClientsDto client, string apiCall)
        //     {
        //         CatalogImportEntityDto returnData = new CatalogImportEntityDto();
        //         List<CategoryDto> categories = new List<CategoryDto>();
        //         List<ModifierDto> modifiers = new List<ModifierDto>();
        // List<ModifierGroupDto> modifierGroups = new List<ModifierGroupDto>();
        // List<ProductDto> products = new List<ProductDto>();

        //         string Sql = @"
        //             -- Categories + service availability
        //             SELECT c.PosReference,c.Name,c.Description,
        //                 c.PosVersion,c.OriginalImageUrl,c.ShowOnline,c.Position,sa.Weekday,tp.StartDate,
        //                 tp.EndDate,tp.StartTime,tp.EndTime
        //             FROM Categories c
        //             LEFT JOIN CategoryServiceAvailability sa ON sa.CategoryPosRef = c.PosReference
        //             LEFT JOIN ServiceTimePeriods tp ON tp.ServiceAvailabilityId = sa.Id
        //             ORDER BY c.PosReference, sa.Id, tp.StartDate;

        //             -- Modifiers
        //             SELECT m.PosReference, m.Name, m.Description, m.PosVersion, m.OriginalImageUrl, m.Price, m.InStorePrice,
        //                 m.TaxRate, m.IsTaxIncluded, m.ContainsAlcohol, m.ContainsTobacco, m.IsBikeFriendly, m.ShowOnline,
        //                 m.Position, m.MinPermitted, m.MaxPermitted, m.DietaryRestriction, m.Spiciness,
        //                 ni.KilojoulesLower, ni.KilojoulesUpper, ni.CaloriesLower, ni.CaloriesUpper,
        //                 ni.ProteinLower, ni.ProteinUpper, ni.CarbohydratesLower, ni.CarbohydratesUpper,
        //                 ni.SugarLower, ni.SugarUpper, ni.SaturatedFatLower, ni.SaturatedFatUpper, ni.SaltLower, ni.SaltUpper,
        //                 STUFF((
        //                     SELECT ',' + da.Additive
        //                     FROM ModifierAdditives da
        //                     WHERE da.ModifierPosRef = m.PosReference
        //                     FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS Additives,

        //                 -- Allergens concatenation
        //                 STUFF((
        //                     SELECT ',' + al.Allergen
        //                     FROM ModifierAllergens al
        //                     WHERE al.ModifierPosRef = m.PosReference
        //                     FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS Allergens
        //             FROM Modifiers m
        //             LEFT JOIN ModifierNutritionalInfo ni ON ni.ModifierPosRef = m.PosReference
        //             LEFT JOIN ModifierAdditives da ON da.ModifierPosRef = m.PosReference
        //             LEFT JOIN ModifierAllergens al ON al.ModifierPosRef = m.PosReference
        //             GROUP BY m.PosReference, m.Name, m.Description, m.PosVersion, m.OriginalImageUrl,
        //                     m.Price, m.InStorePrice, m.TaxRate, m.IsTaxIncluded, m.ContainsAlcohol, m.ContainsTobacco,
        //                     m.IsBikeFriendly, m.ShowOnline, m.Position, m.MinPermitted, m.MaxPermitted,
        //                     m.DietaryRestriction, m.Spiciness,
        //                     ni.KilojoulesLower, ni.KilojoulesUpper, ni.CaloriesLower, ni.CaloriesUpper,
        //                     ni.ProteinLower, ni.ProteinUpper, ni.CarbohydratesLower, ni.CarbohydratesUpper,
        //                     ni.SugarLower, ni.SugarUpper, ni.SaturatedFatLower, ni.SaturatedFatUpper, ni.SaltLower, ni.SaltUpper;

        //             -- ModifierGroups
        //             SELECT mg.PosReference, mg.Name, mg.Description, mg.PosVersion, mg.Position, mg.MinPermitted, mg.MaxPermitted,
        //                 mm.ModifierPosRef
        //             FROM ModifierGroups mg
        //             LEFT JOIN ModifierGroupModifiers mm ON mm.ModifierGroupPosRef = mg.PosReference
        //             ORDER BY mg.PosReference, mm.ModifierPosRef;


        //         -- Products
        //         SELECT p.PosReference, p.Name, p.Description, p.Type, p.PosVersion, p.OriginalImageUrl, p.Price, p.InStorePrice,
        //             p.TaxRate, p.IsTaxIncluded, p.ContainsAlcohol, p.ContainsTobacco, p.IsBikeFriendly, p.ShowOnline,
        //             p.Position, p.DietaryRestriction, p.Spiciness,
        //             STUFF((
        //     SELECT ',' + pc.CategoryPosRef
        //     FROM ProductCategories pc
        //     WHERE pc.ProductPosRef = p.PosReference
        //     FOR XML PATH(''), TYPE
        // ).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS Categories,

        // -- ModifierGroups concatenation
        // STUFF((
        //     SELECT ',' + pm.ModifierGroupPosRef
        //     FROM ProductModifierGroups pm
        //     WHERE pm.ProductPosRef = p.PosReference
        //     FOR XML PATH(''), TYPE
        // ).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS ModifierGroups
        //         FROM Products p
        //         LEFT JOIN ProductCategories pc ON pc.ProductPosRef = p.PosReference
        //         LEFT JOIN ProductModifierGroups pm ON pm.ProductPosRef = p.PosReference
        //         GROUP BY p.PosReference, p.Name, p.Description, p.Type, p.PosVersion, p.OriginalImageUrl, p.Price,
        //                 p.InStorePrice, p.TaxRate, p.IsTaxIncluded, p.ContainsAlcohol, p.ContainsTobacco,
        //                 p.IsBikeFriendly, p.ShowOnline, p.Position, p.DietaryRestriction, p.Spiciness;

        //         ";

        //         #region Execute SQL
        //         using (SqlConnection connection = new SqlConnection(_defaultConnectionString))
        //         {
        //             await connection.OpenAsync();
        //             using (SqlCommand command = new SqlCommand(Sql, connection))
        //             {
        //                 command.CommandTimeout = 60 * 60;

        //                 using (SqlDataReader reader = await command.ExecuteReaderAsync())
        //                 {
        //                     #region Fill Category
        //                     while (await reader.ReadAsync())
        //                     {
        //                         string catPosRef = reader["PosReference"]?.ToString();
        //                         CategoryDto category = categories.FirstOrDefault(c => c.PosReference == catPosRef);
        //                         if (category == null)
        //                         {
        //                             category = new CategoryDto
        //                             {
        //                                 PosReference = catPosRef,
        //                                 Name = reader["Name"]?.ToString(),
        //                                 Description = reader["Description"]?.ToString(),
        //                                 PosVersion = reader["PosVersion"]?.ToString(),
        //                                 OriginalImageUrl = reader["OriginalImageUrl"]?.ToString(),
        //                                 ShowOnline = reader["ShowOnline"] != DBNull.Value && (bool)reader["ShowOnline"],
        //                                 Position = reader["Position"] != DBNull.Value ? Convert.ToInt32(reader["Position"]) : 0,
        //                                 ServiceAvailability = new List<ServiceAvailabilityDto>()
        //                             };
        //                             categories.Add(category);
        //                         }

        //                         if (reader["Weekday"] != DBNull.Value)
        //                         {
        //                             string weekday = reader["Weekday"].ToString();
        //                             ServiceAvailabilityDto serviceAvailability = category.ServiceAvailability.FirstOrDefault(sa => sa.Weekday == weekday);
        //                             if (serviceAvailability == null)
        //                             {
        //                                 serviceAvailability = new ServiceAvailabilityDto
        //                                 {
        //                                     Weekday = weekday,
        //                                     TimePeriods = new List<TimePeriodDto>()
        //                                 };
        //                                 category.ServiceAvailability.Add(serviceAvailability);
        //                             }

        //                             serviceAvailability.TimePeriods.Add(new TimePeriodDto
        //                             {
        //                                 StartDate = reader["StartDate"]?.ToString(),
        //                                 EndDate = reader["EndDate"]?.ToString(),
        //                                 StartTime = reader["StartTime"]?.ToString(),
        //                                 EndTime = reader["EndTime"]?.ToString()
        //                             });
        //                         }
        //                     }
        //                     returnData.Categories = categories;
        //                     #endregion Fill Category

        //                     #region Modifiers
        //                     if (await reader.NextResultAsync())
        //                     {
        //                         while (await reader.ReadAsync())
        //                         {
        // ModifierDto modifier = new ModifierDto
        //                             {
        //                                 PosReference = reader["PosReference"].ToString(),
        //                                 Name = reader["Name"].ToString(),
        //                                 Description = reader["Description"].ToString(),
        //                                 PosVersion = reader["PosVersion"].ToString(),
        //                                 OriginalImageUrl = reader["OriginalImageUrl"].ToString(),
        //                                 Price = (int)reader["Price"],
        //                                 InStorePrice = (int)reader["InStorePrice"],
        //                                 TaxRate = (decimal)reader["TaxRate"],
        //                                 IsTaxIncluded = (bool)reader["IsTaxIncluded"],
        //                                 ContainsAlcohol = (bool)reader["ContainsAlcohol"],
        //                                 ContainsTobacco = (bool)reader["ContainsTobacco"],
        //                                 IsBikeFriendly = (bool)reader["IsBikeFriendly"],
        //                                 ShowOnline = (bool)reader["ShowOnline"],
        //                                 Position = (int)reader["Position"],
        //                                 MinPermitted = (int)reader["MinPermitted"],
        //                                 MaxPermitted = (int)reader["MaxPermitted"],
        //                                 DietaryRestriction = reader["DietaryRestriction"].ToString(),
        //                                 Spiciness = reader["Spiciness"].ToString(),

        //                                 NutritionalInfo = new NutritionalInfoDto
        //                                 {
        //                                     Kilojoules = new RangeDto
        //                                     {
        //                                         LowerRange = (decimal)reader["KilojoulesLower"],
        //                                         UpperRange = (decimal)reader["KilojoulesUpper"]
        //                                     },
        //                                     Calories = new RangeDto
        //                                     {
        //                                         LowerRange = (decimal)reader["CaloriesLower"],
        //                                         UpperRange = (decimal)reader["CaloriesUpper"]
        //                                     },
        //                                     Protein = new RangeDto
        //                                     {
        //                                         LowerRange = (decimal)reader["ProteinLower"],
        //                                         UpperRange = (decimal)reader["ProteinUpper"]
        //                                     },
        //                                     Carbohydrates = new RangeDto
        //                                     {
        //                                         LowerRange = (decimal)reader["CarbohydratesLower"],
        //                                         UpperRange = (decimal)reader["CarbohydratesUpper"]
        //                                     },
        //                                     Sugar = new RangeDto
        //                                     {
        //                                         LowerRange = (decimal)reader["SugarLower"],
        //                                         UpperRange = (decimal)reader["SugarUpper"]
        //                                     },
        //                                     SaturatedFat = new RangeDto
        //                                     {
        //                                         LowerRange = (decimal)reader["SaturatedFatLower"],
        //                                         UpperRange = (decimal)reader["SaturatedFatUpper"]
        //                                     },
        //                                     Salt = new RangeDto
        //                                     {
        //                                         LowerRange = (decimal)reader["SaltLower"],
        //                                         UpperRange = (decimal)reader["SaltUpper"]
        //                                     }
        //                                 },

        //                                 Additives = reader["Additives"] is DBNull
        //                                     ? new List<string>()
        //                                     : reader["Additives"].ToString().Split(',').ToList(),

        //                                 Allergens = reader["Allergens"] is DBNull
        //                                     ? new List<string>()
        //                                     : reader["Allergens"].ToString().Split(',').ToList()
        //                             };

        //                             modifiers.Add(modifier);
        //                         }
        //                         returnData.Modifiers = modifiers;
        //                     }
        //                     #endregion Modifiers

        //                     #region Modifier Groups
        //                     if (await reader.NextResultAsync())
        //                     {
        //                         ModifierGroupDto currentGroup = null;
        //                         string lastGroupRef = null;

        //                         while (await reader.ReadAsync())
        //                         {
        //                             string groupRef = reader["PosReference"].ToString();

        //                             if (lastGroupRef != groupRef)
        //                             {
        //                                 currentGroup = new ModifierGroupDto
        //                                 {
        //                                     PosReference = groupRef,
        //                                     Name = reader["Name"].ToString(),
        //                                     Description = reader["Description"].ToString(),
        //                                     PosVersion = reader["PosVersion"].ToString(),
        //                                     Position = Convert.ToInt32(reader["Position"]),
        //                                     MinPermitted = Convert.ToInt32(reader["MinPermitted"]),
        //                                     MaxPermitted = Convert.ToInt32(reader["MaxPermitted"]),
        //                                     Modifiers = modifiers
        //                                         .Where(m => m.PosReference == reader["ModifierPosRef"].ToString())
        //                                         .ToList()
        //                                 };
        //                                 modifierGroups.Add(currentGroup);
        //                                 lastGroupRef = groupRef;
        //                             }
        //                         }

        //                         returnData.ModifierGroups = modifierGroups;
        //                     }
        //                     #endregion Modifier Groups

        //                     #region Products
        //                     if (await reader.NextResultAsync())
        //                     {
        //                         while (await reader.ReadAsync())
        //                         {
        // ProductDto product = new ProductDto
        //                             {
        //                                 PosReference = reader["PosReference"].ToString(),
        //                                 Name = reader["Name"].ToString(),
        //                                 Description = reader["Description"].ToString(),
        //                                 Type = reader["Type"].ToString(),
        //                                 PosVersion = reader["PosVersion"].ToString(),
        //                                 OriginalImageUrl = reader["OriginalImageUrl"].ToString(),
        //                                 Price = (decimal)reader["Price"],
        //                                 InStorePrice = (decimal)reader["InStorePrice"],
        //                                 TaxRate = (decimal)reader["TaxRate"],
        //                                 IsTaxIncluded = (bool)reader["IsTaxIncluded"],
        //                                 ContainsAlcohol = (bool)reader["ContainsAlcohol"],
        //                                 ContainsTobacco = (bool)reader["ContainsTobacco"],
        //                                 IsBikeFriendly = (bool)reader["IsBikeFriendly"],
        //                                 ShowOnline = (bool)reader["ShowOnline"],
        //                                 Position = (int)reader["Position"],
        //                                 DietaryRestriction = reader["DietaryRestriction"].ToString(),
        //                                 Spiciness = reader["Spiciness"].ToString(),

        //                                 Categories = reader["Categories"] == DBNull.Value 
        //                                     ? new List<string>() 
        //                                     : reader["Categories"].ToString().Split(',').ToList(),

        //                                 ModifierGroups = reader["ModifierGroups"] == DBNull.Value 
        //                                     ? new List<string>() 
        //                                     : reader["ModifierGroups"].ToString().Split(',').ToList()
        //                             };

        //                             products.Add(product);
        //                         }

        //                         returnData.Products = products;
        //                     }
        //                     #endregion Products

        //                 }
        //             }
        //             #endregion Execute SQL
        //             return returnData;
        //         }
        //     }
        //     #endregion Catalog

        public async Task<bool> UpdatePosHubProductIdsAsync(List<ProductDto> products, string apiCall)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_defaultConnectionString))
                {
                    await conn.OpenAsync();
                    foreach (ProductDto product in products)
                    {
                        string sql = @"
                                UPDATE Products
                                SET PosHubProductId = @Id , UpdatedAt = GetDate()
                                WHERE PosReference = @PosReference and (PosHubProductId is NULL or PosHubProductId='');";

                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", product.Id ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PosReference", product.PosReference ?? (object)DBNull.Value);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                return true;
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
                    MethodName = nameof(UpdatePosHubProductIdsAsync),
                    ErrorOccurredDateTime = DateTime.Now

                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return false;
            }
        }

        public async Task<bool> UpdatePosHubCategoryIdsAsync(List<CategoryDto> categories, string apiCall)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_defaultConnectionString))
                {
                    await conn.OpenAsync();
                    foreach (CategoryDto category in categories)
                    {
                        string sql = @"
                                UPDATE Categories
                                SET PosHubCategoryId = @Id , UpdatedAt = GetDate()
                                WHERE PosReference = @PosReference and (PosHubCategoryId is NULL or PosHubCategoryId='');";

                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", category.Id ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PosReference", category.PosReference ?? (object)DBNull.Value);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                return true;
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
                    MethodName = nameof(UpdatePosHubProductIdsAsync),
                    ErrorOccurredDateTime = DateTime.Now

                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return false;
            }
        }

        #region GetPosHubProductIdByPosReferenceAsync
        public async Task<string?> GetPosHubProductIdByPosReferenceAsync(string posReference)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_defaultConnectionString))
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT PosHubProductId 
                        FROM Products 
                        WHERE PosReference = @PosReference;";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@PosReference", posReference ?? (object)DBNull.Value);

                        object result = await cmd.ExecuteScalarAsync();

                        if (result != null && result != DBNull.Value)
                        {
                            return result.ToString();
                        }

                        return null;
                    }
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
                    ApiCall = "GetPosHubProductIdByPosReferenceAsync",
                    MethodName = nameof(GetPosHubProductIdByPosReferenceAsync),
                    ErrorOccurredDateTime = DateTime.Now
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return null;
            }
        }
        #endregion GetPosHubProductIdByPosReferenceAsync

        #region DeleteProductAndRelationsByPosReferenceAsync
        public async Task<bool> DeleteProductAndRelationsByPosReferenceAsync(string posReference)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_defaultConnectionString))
                {
                    await conn.OpenAsync();

                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        string sql = @"
                            DELETE FROM ProductCategories WHERE ProductPosRef = @PosReference;
                            DELETE FROM ProductModifierGroups WHERE ProductPosRef = @PosReference;
                            DELETE FROM Products WHERE PosReference = @PosReference;
                        ";

                        using (SqlCommand cmd = new SqlCommand(sql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@PosReference", posReference);

                            int rowsAffected = await cmd.ExecuteNonQueryAsync();

                            transaction.Commit();

                            return rowsAffected > 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = "DeleteProductAndRelationsByPosReferenceAsync",
                    MethodName = nameof(DeleteProductAndRelationsByPosReferenceAsync),
                    ErrorOccurredDateTime = DateTime.Now
                });

                return false;
            }
        }

        #endregion DeleteProductAndRelationsByPosReferenceAsync

        #region GetPosHubCategoryIdByPosReferenceAsync
        public async Task<string?> GetPosHubCategoryIdByPosReferenceAsync(string posReference)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_defaultConnectionString))
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT PosHubCategoryId 
                        FROM Categories 
                        WHERE PosReference = @PosReference;";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@PosReference", posReference ?? (object)DBNull.Value);

                        object result = await cmd.ExecuteScalarAsync();

                        if (result != null && result != DBNull.Value)
                        {
                            return result.ToString();
                        }

                        return null;
                    }
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
                    ApiCall = "GetPosHubCategoryIdByPosReferenceAsync",
                    MethodName = nameof(GetPosHubCategoryIdByPosReferenceAsync),
                    ErrorOccurredDateTime = DateTime.Now
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return null;
            }
        }
        #endregion GetPosHubCategoryIdByPosReferenceAsync

        #region DeleteCategoryAndRelationsByPosReferenceAsync
        public async Task<bool> DeleteCategoryAndRelationsByPosReferenceAsync(string posReference)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_defaultConnectionString))
                {
                    await conn.OpenAsync();

                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        string sql = @"
                                    DELETE FROM ServiceTimePeriods
                                    WHERE ServiceAvailabilityId IN (
                                        SELECT ServiceAvailabilityId
                                        FROM CategoryServiceAvailability
                                        WHERE CategoryPosRef = @PosReference);
                                    DELETE FROM CategoryServiceAvailability WHERE CategoryPosRef = @PosReference;
                                    DELETE FROM ProductCategories WHERE CategoryPosRef = @PosReference;
                                    DELETE FROM Categories WHERE PosReference = @PosReference;
                                ";

                        using (SqlCommand cmd = new SqlCommand(sql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@PosReference", posReference);

                            int rowsAffected = await cmd.ExecuteNonQueryAsync();

                            transaction.Commit();

                            return rowsAffected > 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _apiErrorDA.InsertOrUpdateApiErrorAsync(new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = "DeleteCategoryAndRelationsByPosReferenceAsync",
                    MethodName = nameof(DeleteCategoryAndRelationsByPosReferenceAsync),
                    ErrorOccurredDateTime = DateTime.Now
                });

                return false;
            }
        }

        #endregion DeleteCategoryAndRelationsByPosReferenceAsync

        public async Task<bool> UpdateProductByPosRefId(ProductDto product, string apiCall)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_defaultConnectionString))
                {
                    await conn.OpenAsync();

                        string sql = @"
                                UPDATE Products
                                SET ShowOnline = @ShowOnline, PosVersion = CAST(CAST(PosVersion AS FLOAT) + 0.1 AS NVARCHAR(10)), UpdatedAt = GetDate()
                                WHERE PosReference = @PosReference ;";

                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@ShowOnline", product.ShowOnline);
                            cmd.Parameters.AddWithValue("@PosReference", product.PosReference ?? (object)DBNull.Value);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                return true;
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
                    MethodName = nameof(UpdateProductByPosRefId),
                    ErrorOccurredDateTime = DateTime.Now

                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return false;
            }
        }
    }
}