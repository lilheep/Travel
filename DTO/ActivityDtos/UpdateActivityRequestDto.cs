using System.Text.Json.Serialization;

namespace WebApplicationTest.DTO.ActivityDtos
{
    public class UpdateActivityRequestDto
    {
        public int? TripId { get; set; }
        [JsonPropertyName("activity_type_id")]
        public int? ActivityTypeId { get; set; }
        [JsonPropertyName("cost")]
        public decimal? Cost { get; set; }
        [JsonPropertyName("date")]
        public DateTime? Date { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
