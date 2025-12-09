using System.Text.Json.Serialization;

namespace WebApplicationTest.DTO.ActivityTypeDtos
{
    public class GetActivityTypeResponseDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
