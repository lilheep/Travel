using BCrypt.Net;

namespace WebApplicationTest.Services

{
    public class HashingPasswordService
    {
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Пароль не может быть пустым!");
            }
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
            {
                return false; 
            }
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
