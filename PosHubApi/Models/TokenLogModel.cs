using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PosHubApi.Models
{
    public class TokenLogModel
    {
        public long Id { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
        public string GrantType { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}