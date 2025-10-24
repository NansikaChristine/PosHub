using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PosHubApi.Data.DataAccess;
using PosHubApi.Data.Interfaces;
using PosHubApi.Dtos;
using PosHubApi.Errors;
using PosHubApi.Models;

namespace PosHubApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookEventController : ControllerBase
    {
        private readonly IWebhookEventRepository _webhookEventRrepository;
        private readonly ApiErrorDA _apiErrorDA;
        private readonly LogsDA _logsDA;

        public WebhookEventController(IWebhookEventRepository webhookEventRrepository, ApiErrorDA apiErrorDA,LogsDA logsDA)
        {
            _webhookEventRrepository = webhookEventRrepository;
            _apiErrorDA = apiErrorDA;
            _logsDA = logsDA;

        }

        [HttpPost("orderWebhookEvent_")]
        public async Task<ActionResult> OrderWebhookEvent()
        {
            Console.WriteLine("Order webhook event trigger");
            // using StreamReader reader = new StreamReader(Request.Body);
            // string body = await reader.ReadToEndAsync();
            Request.EnableBuffering();

            string body;
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                Request.Body.Position = 0;
            }

            string xWebhookSignature = Request.Headers["X-Webhook-Signature"];

            OrderWebhookEventRequestDto xWebhookRequest = JsonSerializer.Deserialize<OrderWebhookEventRequestDto>(body);

            string apiCall = $"WebhookEvent/orderWebhookEvent_";

            try
            {
                bool valid = await _webhookEventRrepository.ValidateSignature(xWebhookSignature, body, apiCall);

                if (!valid)
                {
                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = "http://localhost:5091/api/WebhookEvent/orderWebhookEvent_",
                        Event = "OrderWebhookEvent",
                        IsSuccess = false,
                        FailMessage = "Invalid signature",
                        RequestBody = body,
                        ApplicationId = xWebhookRequest.ClientId,
                        UniqueId = xWebhookRequest.EventId
                    });
                    return StatusCode(401, "Unauthorized");
                }

                if (xWebhookRequest == null)
                {
                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = "http://localhost:5091/api/WebhookEvent/orderWebhookEvent_",
                        Event = "OrderWebhookEvent",
                        IsSuccess = false,
                        FailMessage = "Request body is missing or invalid.",
                        ApplicationId = xWebhookRequest.ClientId,
                        RequestBody = body,
                        UniqueId = xWebhookRequest.EventId
                    });

                    return BadRequest(new
                    {
                        statusCode = 400,
                        message = "Request body is missing or invalid."
                    });
                }
                Console.WriteLine("xWebhookRequest");
                Console.WriteLine(xWebhookRequest);

                string json = JsonSerializer.Serialize(xWebhookRequest);

                Console.WriteLine(json);

                bool success = await _webhookEventRrepository.OrderWebhookEvent(xWebhookRequest, apiCall);

                if (success)
                {
                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = "http://localhost:5091/api/WebhookEvent/orderWebhookEvent_",
                        Event = "OrderWebhookEvent",
                        IsSuccess = true,
                        FailMessage = "",
                        RequestBody = "",
                        ApplicationId = xWebhookRequest.ClientId,
                        UniqueId = xWebhookRequest.EventId
                    });
                    return Ok();
                }
                else
                {
                    await _logsDA.InsertLogAsync(new LogModel
                    {
                        Url = "http://localhost:5091/api/WebhookEvent/orderWebhookEvent_",
                        Event = "OrderWebhookEvent",
                        IsSuccess = false,
                        FailMessage = "",
                        RequestBody = json,
                        ApplicationId = xWebhookRequest.ClientId,
                        UniqueId = xWebhookRequest.EventId
                    });
                    return StatusCode(500, "");
                }

            }
            catch (RateLimitExceededException ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = "http://localhost:5091/api/WebhookEvent/orderWebhookEvent_",
                    Event = "OrderWebhookEvent",
                    IsSuccess = false,
                    FailMessage = ex.Message,
                    ApplicationId = xWebhookRequest.ClientId,
                    RequestBody = body,
                    UniqueId = xWebhookRequest.EventId
                });
                ApiErrorMessageModel error = new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(OrderWebhookEvent),
                    ErrorOccurredDateTime = DateTime.Now,
                    ClientId = xWebhookRequest.ClientId
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return StatusCode(429, new
                {
                    statusCode = 429,
                    message = ex.Message
                });
            }
            catch (BadGatewayException ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = "http://localhost:5091/api/WebhookEvent/orderWebhookEvent_",
                    Event = "OrderWebhookEvent",
                    IsSuccess = false,
                    FailMessage = ex.Message,
                    ApplicationId = xWebhookRequest.ClientId,
                    RequestBody = body,
                    UniqueId = xWebhookRequest.EventId
                });
                ApiErrorMessageModel error = new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(OrderWebhookEvent),
                    ErrorOccurredDateTime = DateTime.Now,
                    ClientId = xWebhookRequest.ClientId
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return StatusCode(502, new
                {
                    statusCode = 502,
                    message = ex.Message
                });
            }
            catch (ServiceUnavailableException ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = "http://localhost:5091/api/WebhookEvent/orderWebhookEvent_",
                    Event = "OrderWebhookEvent",
                    IsSuccess = false,
                    FailMessage = ex.Message,
                    ApplicationId = xWebhookRequest.ClientId,
                    RequestBody = body,
                    UniqueId = xWebhookRequest.EventId
                });
                ApiErrorMessageModel error = new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(OrderWebhookEvent),
                    ErrorOccurredDateTime = DateTime.Now,
                    ClientId = xWebhookRequest.ClientId
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return StatusCode(503, new
                {
                    statusCode = 503,
                    message = ex.Message
                });
            }
            catch (GatewayTimeoutException ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = "http://localhost:5091/api/WebhookEvent/orderWebhookEvent_",
                    Event = "OrderWebhookEvent",
                    IsSuccess = false,
                    FailMessage = ex.Message,
                    ApplicationId = xWebhookRequest.ClientId,
                    RequestBody = body,
                    UniqueId = xWebhookRequest.EventId
                });
                ApiErrorMessageModel error = new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(OrderWebhookEvent),
                    ErrorOccurredDateTime = DateTime.Now,
                    ClientId = xWebhookRequest.ClientId
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return StatusCode(504, new
                {
                    statusCode = 504,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                await _logsDA.InsertLogAsync(new LogModel
                {
                    Url = "http://localhost:5091/api/WebhookEvent/orderWebhookEvent_",
                    Event = "OrderWebhookEvent",
                    IsSuccess = false,
                    FailMessage = ex.Message,
                    ApplicationId = xWebhookRequest.ClientId,
                    RequestBody = body,
                    UniqueId = xWebhookRequest.EventId
                });
                ApiErrorMessageModel error = new ApiErrorMessageModel
                {
                    ErrorMessage = ex.Message,
                    ErrorSource = ex.Source,
                    StackTrace = ex.StackTrace,
                    InnerErrorMessage = ex.InnerException?.Message ?? "",
                    ApiCall = apiCall,
                    MethodName = nameof(OrderWebhookEvent),
                    ErrorOccurredDateTime = DateTime.Now,
                    ClientId = xWebhookRequest.ClientId
                };

                await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);
                return StatusCode(500, new
                {
                    statusCode = 500,
                    message = ex.Message
                });
            }
        }
    
    }
}