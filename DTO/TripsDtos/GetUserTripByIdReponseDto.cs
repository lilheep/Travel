using System.Text.Json.Serialization;

namespace WebApplicationTest.DTO.TripsDtos
{
    public class GetUserTripByIdResponseDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name_trip")]
        public string Name { get; set; }
        [JsonPropertyName("start_date")]
        public DateOnly StartDate { get; set; }
        [JsonPropertyName("end_date")]
        public DateOnly EndDate { get; set; }
        [JsonPropertyName("budget")]
        public decimal? TotalBudget { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}