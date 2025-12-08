using System.Text.Json.Serialization;

namespace WebApplicationTest.DTO.DeleteUserDtos
{
    public class DeleteUserRequestDto
    {
        [JsonPropertyName("type_delete")]
        public bool TypeDelete { get; set; } = true;
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
