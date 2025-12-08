using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplicationTest.DTO.UserDtos.UserChangeDataDtos
{
    public class UserChangePasswordRequestDto
    {
        [JsonPropertyName("old_password")]
        public string OldPassword { get; set; }
        [JsonPropertyName("new_password")]
        public string NewPassword { get; set; }
        [MinLength(8, ErrorMessage = "Длина нового пароля должна составлять не менее 8 символов!")]
        [JsonPropertyName("confirm_new_password")]
        public string ConfrirmNewPassword { get; set; }
    }
}
