using System.Text.Json.Serialization;

namespace WebApplicationTest.DTO.RefreshTokenDto
{
    public class RefreshTokenResponseDto
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
