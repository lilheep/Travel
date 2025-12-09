using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplicationTest.Data;
using WebApplicationTest.Services;

namespace WebApplicationTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly ExtractAccessTokenFromHeaderService _extractAccessTokenFromHeaderService;
        public TripsController(ApplicationDbContext dbContext, JwtService jwtService, IConfiguration configuration, ExtractAccessTokenFromHeaderService extractAccessTokenFromHeaderService)
        {
            _dbContext = dbContext;
            _jwtService = jwtService;
            _configuration = configuration;
            _extractAccessTokenFromHeaderService = extractAccessTokenFromHeaderService;
        }

        
    }
}
