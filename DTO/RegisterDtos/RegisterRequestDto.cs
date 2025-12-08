using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplicationTest.DTO.RegisterDto
{
    public class RegisterRequestDto
    {
        [EmailAddress(ErrorMessage = "Адрес электронной почты введен в невалидном формате!")]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; }
        [MinLength(8, ErrorMessage = "Пароль должен состоять минимум из 8 символов.")]
        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("password_confirm")]
        public string PasswordConfrim { get; set; }
    }
}
