using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplicationTest.DTO.RegisterDto
{
    public class RegisterResponseDto
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = "Вы успешно зарегистрировались!";
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }
    }
}
