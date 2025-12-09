using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationTest.Data;
using WebApplicationTest.DTO.TripsDtos;
using WebApplicationTest.Models;
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

        [HttpGet("get_user_trips")]
        [Authorize]
        public async Task<ActionResult<List<GetUserTripResponseDto>>> GetUserTrips()
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

            List<Trip> trips = await _dbContext.Trips.Where(u => u.UserId == userId).ToListAsync();
           
            List<GetUserTripResponseDto> response = new List<GetUserTripResponseDto>();
            trips.ForEach(trip =>
            {
                GetUserTripResponseDto responseDto = new GetUserTripResponseDto();
                responseDto.Id = trip.Id;
                responseDto.Name = trip.Name;
                responseDto.StartDate = trip.StartDate;
                responseDto.EndDate = trip.EndDate;
                responseDto.TotalBudget = trip.TotalBudget;
                responseDto.Description = trip.Description;
                response.Add(responseDto);
            });

            return Ok(response);
        }

        [HttpGet("get_user_travel/{id}")]
        [Authorize]
        public async Task<ActionResult<GetUserTripByIdResponseDto>> GetTripById(int id)
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
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (trip == null)
            {
                return NotFound("Поездка не найдена или у вас нет доступа к данной поездке.");
            }

            var responseDto = new GetUserTripByIdResponseDto
            {
                Id = trip.Id,
                Name = trip.Name,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                TotalBudget = trip.TotalBudget,
                Description = trip.Description
            };

            return Ok(responseDto);
        }

        [HttpDelete("delete_trip/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteTrip(int id)
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
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (trip == null)
            {
                return NotFound("Поездка не найдена или у вас нет доступа к данной поездке.");
            }

            _dbContext.Trips.Remove(trip);
            await _dbContext.SaveChangesAsync();

            return Ok("Поездка успешно удалена.");
        }

        [HttpPut("update_trip/{id}")]
        [Authorize]
        public async Task<ActionResult<GetUserTripByIdResponseDto>> UpdateTrip(int id, [FromBody] UpdateTripRequestDto updateTripRequest)
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

            if (updateTripRequest.Name == null &&
                updateTripRequest.StartDate == null &&
                updateTripRequest.EndDate == null &&
                updateTripRequest.TotalBudget == null &&
                updateTripRequest.Description == null)
            {
                return BadRequest("Нужно заполнить минимум одно из полей для обновления.");
            }

            var trip = await _dbContext.Trips
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (trip == null)
            {
                return NotFound("Поездка не найдена или у вас нет доступа к данной поездке.");
            }

            bool hasChanges = false;

            if (updateTripRequest.Name != null)
            {
                if (updateTripRequest.Name != trip.Name)
                {
                    trip.Name = updateTripRequest.Name;
                    hasChanges = true;
                }
            }

            if (updateTripRequest.StartDate.HasValue)
            {
                if (updateTripRequest.StartDate.Value != trip.StartDate)
                {
                    trip.StartDate = updateTripRequest.StartDate.Value;
                    hasChanges = true;
                }
            }

            if (updateTripRequest.EndDate.HasValue)
            {
                if (updateTripRequest.EndDate.Value != trip.EndDate)
                {
                    trip.EndDate = updateTripRequest.EndDate.Value;
                    hasChanges = true;
                }
            }

            if (updateTripRequest.TotalBudget.HasValue)
            {
                if (updateTripRequest.TotalBudget.Value != trip.TotalBudget)
                {
                    trip.TotalBudget = updateTripRequest.TotalBudget.Value;
                    hasChanges = true;
                }
            }

            if (updateTripRequest.Description != null)
            {
                if (updateTripRequest.Description != trip.Description)
                {
                    trip.Description = updateTripRequest.Description;
                    hasChanges = true;
                }
            }

            if (trip.EndDate < trip.StartDate)
            {
                return BadRequest("Дата окончания поездки не может быть раньше даты начала.");
            }

            if (hasChanges)
            {
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                return Ok("Данные не были изменены (новые значения совпадают с текущими).");
            }

            return Ok("Данные о поездке успешно изменены!");
        }

        [HttpPost("create_trip")]
        [Authorize]
        public async Task<ActionResult> CreateTrip([FromBody] CreateTripRequestDto createTripRequestDto)
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

                Trip trip = new Trip();
                trip.UserId = (int)userId;
                trip.User = user;
                trip.Name = createTripRequestDto.Name;
                trip.StartDate = createTripRequestDto.StartDate;
                trip.EndDate = createTripRequestDto.EndDate;
                trip.TotalBudget = (decimal)createTripRequestDto.TotalBudget;
                trip.Description = createTripRequestDto.Description;
                _dbContext.Add(trip);
                await _dbContext.SaveChangesAsync();
                return Ok("Поездка успешно создана!");
            }

   
    }
}
