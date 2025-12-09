using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationTest.Data;
using WebApplicationTest.DTO.ActivityTypeDtos;
using WebApplicationTest.DTO.UserDtos;
using WebApplicationTest.Models;
using WebApplicationTest.Services;

namespace WebApplicationTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityTypeController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly ExtractAccessTokenFromHeaderService _extractAccessTokenFromHeaderService;
        public ActivityTypeController(ApplicationDbContext dbContext, JwtService jwtService, IConfiguration configuration, ExtractAccessTokenFromHeaderService extractAccessTokenFromHeaderService)
        {
            _dbContext = dbContext;
            _jwtService = jwtService;
            _configuration = configuration;
            _extractAccessTokenFromHeaderService = extractAccessTokenFromHeaderService;
        }

        [HttpGet("activity_type_get")]
        [Authorize]
        public async Task<ActionResult<List<GetActivityTypeResponseDto>>> GetActivityType()
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
            List<ActivityType> types = await _dbContext.ActivityTypes.ToListAsync();
            List<GetActivityTypeResponseDto> typeDtos = new List<GetActivityTypeResponseDto>();
            types.ForEach(type =>
            {
                GetActivityTypeResponseDto typeDto = new GetActivityTypeResponseDto();
                typeDto.Id = type.Id;
                typeDto.Name = type.Name;
                typeDtos.Add(typeDto);
            }
            );
            return Ok(typeDtos);
        }

        [HttpGet("activity_type_get/{id}")]
        [Authorize]
        public async Task<ActionResult<GetActivityTypeResponseDto>> GetActivityTypeById(int id)
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

            var activityType = await _dbContext.ActivityTypes
                .FirstOrDefaultAsync(type => type.Id == id);

            if (activityType == null)
            {
                return BadRequest("Тип активности не найден.");
            }

            var responseDto = new GetActivityTypeResponseDto()
            {
                Id = activityType.Id,
                Name = activityType.Name
            };
            return Ok(responseDto);
        }

        [HttpPost("activity_type_add")]
        [Authorize]
        public async Task<ActionResult> CreateActivityType([FromBody] AddActivityTypeRequestDto requestDto)
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

            ActivityType activityType = new ActivityType();
            activityType.Name = requestDto.Name;
            _dbContext.Add(activityType);
            await _dbContext.SaveChangesAsync();
            return Ok("Статус активности успешно создан.");
        } 
    }
}
