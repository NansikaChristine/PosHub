using System.ComponentModel.DataAnnotations;

namespace PosHubApi.Models
{
    public class ApiErrorMessageModel
    {
        [Key]
        public long Id { get; set; }   
        public int Count { get; set; }      
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorSource { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
        public string InnerErrorMessage { get; set; } = string.Empty;
        public string ApiCall { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public DateTime ErrorOccurredDateTime { get; set; } 
    }
}