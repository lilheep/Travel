using Microsoft.AspNetCore.Http;

namespace WebApplicationTest.Services
{
    public class ExtractAccessTokenFromHeaderService
    {
        public string? ExtractAccessTokenFromHeader(HttpRequest request)
        {
            var authHeader = request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader))
                return null;

            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return null;

            return authHeader.Substring("Bearer ".Length).Trim();
        }
    }
}
