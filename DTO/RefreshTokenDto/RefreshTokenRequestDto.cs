using System.Text.Json.Serialization;

namespace WebApplicationTest.DTO.RefreshTokenDto
{
    public class RefreshTokenRequestDto
    {
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        
    }
}
