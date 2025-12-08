using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplicationTest.DTO.AuthDto
{
    public class AuthResponseDto
    {
        [EmailAddress(ErrorMessage = "Адрес электронной почты введен в невалидном формате!")]
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("full_name")]
        public string FullName { get; set; }
        [JsonPropertyName("access_token")]
        public string Token { get; set; }
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonPropertyName("expires")]
        public int Expires { get; set; }
    }
}
