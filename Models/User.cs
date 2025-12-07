using Microsoft.AspNetCore.Identity;
using System.Data;
using WebApplicationTest.Models;

namespace WebApplicationTest.Models
{
    /// <summary>
    /// Модель таблицы пользователей
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public List<Trip> Trips { get; set; }
    }
}
