using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplicationTest.DTO.UserDtos.UserChangeDataDtos
{
    public class UserChangeDataRequestDto
    {
        [JsonPropertyName("new_email")]
        [EmailAddress(ErrorMessage = "Адрес электронной почты введен в невалидном формате!")]
        public string? Email { get; set; }
        [JsonPropertyName("new_full_name")]
        public string? FullName { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }

    }
}
