using System.Text.Json.Serialization;

namespace WebApplicationTest.DTO.ActivityTypeDtos
{
    public class AddActivityTypeRequestDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
