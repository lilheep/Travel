using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplicationTest.Data;
using WebApplicationTest.DTO.ActivityDtos;
using WebApplicationTest.Models;
using WebApplicationTest.Services;

namespace WebApplicationTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly ExtractAccessTokenFromHeaderService _extractAccessTokenFromHeaderService;
        public ActivityController(ApplicationDbContext dbContext, JwtService jwtService, IConfiguration configuration, ExtractAccessTokenFromHeaderService extractAccessTokenFromHeaderService)
        {
            _dbContext = dbContext;
            _jwtService = jwtService;
            _configuration = configuration;
            _extractAccessTokenFromHeaderService = extractAccessTokenFromHeaderService;
        }

        [HttpGet("get_activity")]
        [Authorize]
        public async Task<ActionResult<List<GetActivityResponseDto>>> GetActivity()
        {
            var accessToken = _extractAccessTokenFromHeaderService.ExtractAccessTokenFromHeader(Request);
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("Попробуйте авторизироваться снова.");
            }

            var userId = _jwtService.GetUserIdFromToken(accessToken);
            if (userId == null)
            {
                return BadRequest("Недействительный access token.");
            }

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized("Пользователь не найден. Попробуйте авторизоваться заново.");
            }

            List<Activity> activities = new List<Activity>();
            List<GetActivityResponseDto> responseDtos = new List<GetActivityResponseDto>();
            activities.ForEach(activity =>
            {
                GetActivityResponseDto activityResponseDto = new GetActivityResponseDto();
                activityResponseDto.Id = activity.Id;
                activityResponseDto.TripId = activity.TripId;
                activityResponseDto.ActivityTypeId = activity.ActivityTypeId;
                activityResponseDto.Cost = activity.Cost;
                activityResponseDto.Date = activity.Date;
                activityResponseDto.Description = activity.Description;
                responseDtos.Add(activityResponseDto);
            });
            return Ok(responseDtos);
        }
    } 
}
