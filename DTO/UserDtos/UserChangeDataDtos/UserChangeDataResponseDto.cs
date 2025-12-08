using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplicationTest.DTO.UserDtos.UserChangeDataDtos
{
    public class UserChangeDataResponseDto
    {
        [EmailAddress]
        [JsonPropertyName("new_email")]
        public string? Email { get; set; }
        [JsonPropertyName("new_full_name")]
        public string? FullName { get; set; }
    }
}
