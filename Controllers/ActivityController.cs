using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet("get_activities")]
        [Authorize]
        public async Task<ActionResult<List<GetActivityResponseDto>>> GetActivities()
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

            var activities = await _dbContext.Activities
                .Where(a => a.UserId == userId)
                .ToListAsync();


            List<GetActivityResponseDto> responseDtos = new List<GetActivityResponseDto>();

            foreach (var activity in activities)
            {
                GetActivityResponseDto activityResponseDto = new GetActivityResponseDto
                {
                    Id = activity.Id,
                    UserId = activity.UserId,
                    TripId = activity.TripId,
                    ActivityTypeId = activity.ActivityTypeId,
                    Cost = activity.Cost,
                    Date = activity.Date,
                    Description = activity.Description
                };
                responseDtos.Add(activityResponseDto);
            }

            return Ok(responseDtos);
        }

        [HttpGet("get_activity/{id}")]
        [Authorize]
        public async Task<ActionResult<GetActivityResponseDto>> GetActivityById(int id)
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

            var activity = await _dbContext.Activities
                .Include(a => a.Trip)
                .FirstOrDefaultAsync(a => a.Id == id && a.Trip.UserId == userId);

            if (activity == null)
            {
                return NotFound("Активность не найдена или у вас нет доступа к данной активности.");
            }

            var responseDto = new GetActivityResponseDto
            {
                Id = activity.Id,
                UserId = activity.UserId,
                TripId = activity.TripId,
                ActivityTypeId = activity.ActivityTypeId,
                Cost = activity.Cost,
                Date = activity.Date,
                Description = activity.Description
            };

            return Ok(responseDto);
        }

        [HttpPut("update_activity/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateActivity([FromBody] UpdateActivityRequestDto requestDto, int id)
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

            var activity = await _dbContext.Activities
                .Include(a => a.Trip)
                .FirstOrDefaultAsync(a => a.Id == id && a.Trip.UserId == userId);

            if (activity == null)
            {
                return NotFound("Активность не найдена или у вас нет доступа к данной активности.");
            }

            if (requestDto.TripId.HasValue && requestDto.TripId.Value != activity.TripId)
            {
                var newTrip = await _dbContext.Trips
                    .FirstOrDefaultAsync(t => t.Id == requestDto.TripId && t.UserId == userId);

                if (newTrip == null)
                {
                    return BadRequest("Новая поездка не найдена или не принадлежит вам.");
                }

                activity.TripId = requestDto.TripId.Value;
            }

            if (requestDto.ActivityTypeId.HasValue && requestDto.ActivityTypeId.Value != activity.ActivityTypeId)
            {
                var activityType = await _dbContext.ActivityTypes
                    .FirstOrDefaultAsync(at => at.Id == requestDto.ActivityTypeId);

                if (activityType == null)
                {
                    return BadRequest("Указанный тип активности не существует.");
                }

                activity.ActivityTypeId = requestDto.ActivityTypeId.Value;
            }

            if (requestDto.Cost.HasValue)
            {
                activity.Cost = requestDto.Cost.Value;
            }

            if (requestDto.Date.HasValue)
            {
                activity.Date = requestDto.Date.Value;
            }
            if (requestDto.Description != null)
            {
                activity.Description = requestDto.Description;
            }
            _dbContext.Entry(activity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return Ok("Активность успешно обновлена.");
        }

        [HttpPost("add_activity")]
        [Authorize]
        public async Task<ActionResult> CreateActivity([FromBody] AddActivityRequestDto requestDto)
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

            var trip = await _dbContext.Trips
                .FirstOrDefaultAsync(t => t.Id == requestDto.TripId && t.UserId == userId);

            if (trip == null)
            {
                return BadRequest("Поездка не найдена или не принадлежит вам.");
            }

            var activityType = await _dbContext.ActivityTypes
                .FirstOrDefaultAsync(at => at.Id == requestDto.ActivityTypeId);

            if (activityType == null)
            {
                return BadRequest("Указанный тип активности не существует.");
            }

            DateTime date;
            if (!DateTime.TryParse(requestDto.DateString, out date))
            {
                return BadRequest("Неверный формат даты. Используйте формат: dd.MM.yyyy или yyyy-MM-dd");
            }

            Activity activity = new Activity
            {
                UserId = (int)userId,
                User = user,
                TripId = requestDto.TripId,
                Trip = trip,
                ActivityType = activityType,
                ActivityTypeId = requestDto.ActivityTypeId,
                Cost = requestDto.Cost,
                Date = date,
                Description = requestDto.Description
            };

            _dbContext.Activities.Add(activity);
            await _dbContext.SaveChangesAsync();

            return Ok("Активность успешно добавлена");
        }

        [HttpDelete("delete_activity/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteActivity(int id)
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

            var activity = await _dbContext.Activities
                .Include(a => a.Trip)
                .FirstOrDefaultAsync(a => a.Id == id && a.Trip.UserId == userId);

            if (activity == null)
            {
                return NotFound("Активность не найдена или у вас нет доступа к данной активности.");
            }


            _dbContext.Activities.Remove(activity);
            await _dbContext.SaveChangesAsync();

            return Ok("Активность успешно удалена");
        }
    } 
}
