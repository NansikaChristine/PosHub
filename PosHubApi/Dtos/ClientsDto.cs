using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PosHubApi.Dtos
{
    public class ClientsDto
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUrl { get; set; }
        public string SyncUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string AccountId { get; set; }
        public string LocationId { get; set; }
        public string ApplicationId { get; set; }
        public string ConnectionId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public bool Authorized { get; set; }
        public string Code { get; set; }
    }
}