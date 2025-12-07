namespace WebApplicationTest.Models
{
    /// <summary>
    /// Модель таблицы поездок
    /// </summary>
    public class Trip
    {
        public int Id { get; set; }
        public int UserId { get; set; } // внешний ключ
        public string Name { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal TotalBudget { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User User { get; set; } // навигационное свойство
    }
}
