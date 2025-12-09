using System.Text.Json.Serialization;

namespace WebApplicationTest.DTO.TripsDtos
{
    public class GetUserTripByIdRequestDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }}
}