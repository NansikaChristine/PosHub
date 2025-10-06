namespace PosHubApi.Dtos
{
    public class RefreshAccessTokenRequestDto
    {
        public string Grant_Type { get; set; }
        public string Refresh_Token { get; set; } 
        public string Client_Id { get; set; } 
        public string Client_Secret { get; set; } 
    }
}