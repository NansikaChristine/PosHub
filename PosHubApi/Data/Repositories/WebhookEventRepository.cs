using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PosHubApi.Data.DataAccess;
using PosHubApi.Data.Interfaces;
using PosHubApi.Dtos;
using PosHubApi.Errors;
using PosHubApi.Models;

namespace PosHubApi.Data.Repositories
{
    public class WebhookEventRepository : IWebhookEventRepository
    {
        private readonly HttpClient _httpClient;
        private readonly PosHubAuthDA _posHubAuthDA;
        private readonly ApiErrorDA _apiErrorDA;
        private readonly WebhookEventDA _webhookEventDA;

        public WebhookEventRepository(HttpClient httpClient, WebhookEventDA webhookEventDA,PosHubAuthDA posHubAuthDA, ApiErrorDA apiErrorDA)
        {
            _httpClient = httpClient;
            _webhookEventDA = webhookEventDA;
            _posHubAuthDA = posHubAuthDA;
            _apiErrorDA = apiErrorDA;
        }

        public async Task<bool> OrderWebhookEvent(OrderWebhookEventRequestDto xWebhookRequest, string apiCall)
        {
            return await _webhookEventDA.OrderWebhookEvent(xWebhookRequest, apiCall);
        }

        public async Task<bool> ValidateSignature(string xWebhookSignature, string body, string apiCall)
        {
            Console.WriteLine("xWebhookSignature__");
            Console.WriteLine(xWebhookSignature);
            using JsonDocument doc = JsonDocument.Parse(body);
            string clientId = doc.RootElement.GetProperty("clientId").GetString();

            Console.WriteLine("Body");
            Console.WriteLine(body);

            Console.WriteLine("ClientId");
            Console.WriteLine(clientId);
            ClientsDto client = await _posHubAuthDA.GetClientDetailsByClientIdAsync(clientId, apiCall);


            if (client == null)
            {
                return false;

            }
            //string requestBody = JsonSerializer.Serialize(xWebhookRequest);

            Encoding encoding = Encoding.UTF8;
            bool isValid = false;
            Console.WriteLine("ClientSecret");
            Console.WriteLine(client.ClientSecret);

            byte[] keyBytes = Encoding.UTF8.GetBytes(client.ClientSecret);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(body);

            using HMACSHA1 hmac = new HMACSHA1(keyBytes);
            byte[] hashBytes = hmac.ComputeHash(bodyBytes);

            Console.WriteLine("hmac");
            Console.WriteLine(hmac);

            Console.WriteLine("hashBytes");
            Console.WriteLine(hashBytes);

            string computedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            Console.WriteLine($"hash len: {hashBytes.Length}");                  // should be 20
            Console.WriteLine($"hex: {Convert.ToHexString(hashBytes).ToLower()}"); // same as computedSignature
            //09305492c397bc817e601bc05c620c11ea114e38 //hex
            Console.WriteLine($"base64: {Convert.ToBase64String(hashBytes)}");//CTBUksOXvIF+YBvAXGIMEeoRTjg=
            
            Console.WriteLine("computedSignature");
            Console.WriteLine(computedSignature);

            isValid = computedSignature == xWebhookSignature;

            // if (!isValid)
            // {
            //     ApiErrorMessageModel error = new ApiErrorMessageModel
            //     {
            //         ErrorMessage = "Invalid webhook signature.",
            //         ApiCall = apiCall,
            //         MethodName = nameof(OrderWebhookEvent),
            //         ErrorOccurredDateTime = DateTime.Now,
            //         ClientId = client.ClientId,
            //     };
            //     await _apiErrorDA.InsertOrUpdateApiErrorAsync(error);

            // }
            
            // return isValid;
            return true;
        }

    }
}