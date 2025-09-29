
namespace PosHubApi.Dtos
{
    public class AccountLocationDto
{
    public int Id { get; set; }
    public string AccountId { get; set; }
    public string LocationId { get; set; }
    public string ApplicationId { get; set; }
    public string ConnectionId { get; set; }
    public string AccessToken { get; set; }
    public string Code { get; set; }
    public string RefreshToken { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool Authorized { get; set; } = false;
}

}