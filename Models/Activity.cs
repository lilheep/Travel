using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace WebApplicationTest.Models
{
    /// <summary>
    /// Модель таблицы активностей
    /// </summary>
    public class Activity
    {
        public int Id { get; set; }
        public int UserId {  get; set; }
        public int TripId { get; set; } 
        public int ActivityTypeId { get; set; }
        public decimal Cost { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public User User { get; set; }
        public Trip Trip { get; set; }
        public ActivityType ActivityType { get; set; }
    }
}
