using WebApplicationTest.Models;

namespace WebApplicationTest.DTO.UserDtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<Trip> Trips { get; set; }
    }
}
