using BCrypt.Net;

namespace WebApplicationTest.Services
{
    public class HashingRefreshToken
    {
        public static string HashRefreshToken(string token)
        {
            return BCrypt.Net.BCrypt.HashPassword(token, 4);
        }

        public static bool VerifyRefreshToken(string refreshToken, string hashRefreshToken)
        {
            return BCrypt.Net.BCrypt.Verify(refreshToken, hashRefreshToken);
        }
    }
}
