namespace PosHubApi.Dtos
{
    public class TokenRequestDto
    {
        public string Grant_Type { get; set; } 
        public string Client_Id { get; set; }
        public string Client_Secret { get; set; }
        public string Scope { get; set; }
    }
}