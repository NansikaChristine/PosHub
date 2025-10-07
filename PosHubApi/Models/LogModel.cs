using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PosHubApi.Models
{
    public class LogModel
    {
        public string Url { get; set; }
        public string Event { get; set; }
        public bool IsSuccess { get; set; }
        public string FailMessage { get; set; }
        public string RequestBody { get; set; }
        public string ApplicationId { get; set; }
    }

}