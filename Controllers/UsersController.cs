using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApplicationTest.Data;
using WebApplicationTest.DTO;
using WebApplicationTest.DTO.AuthDto;
using WebApplicationTest.DTO.RefreshTokenDto;
using WebApplicationTest.DTO.RegisterDto;
using WebApplicationTest.Models;
using WebApplicationTest.Services;

namespace WebApplicationTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;

        public UsersController(ApplicationDbContext dbContext, JwtService jwtService, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        private string? ExtractAccessTokenFromHeader()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader))
                return null;

            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return null;

            return authHeader.Substring("Bearer ".Length).Trim();
        }

        [HttpGet("get_users")]
        [Authorize]
        public async Task<ActionResult<List<UserDto>>> GetUsers()
        {
            List<User> users = await _dbContext.Users.ToListAsync();
            List<UserDto> userDtos = new List<UserDto>();
            users.ForEach(user =>
            {
                UserDto userDto = new UserDto();
                userDto.Id = user.Id;
                userDto.Trips = user.Trips;
                userDto.Email = user.Email;
                userDto.CreatedAt = user.CreatedAt;
                userDto.Password = user.Password;
                userDto.FullName = user.FullName;
                userDtos.Add(userDto);
            }
            );
            return Ok(userDtos);
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponseDto>> RegisterUser([FromBody] RegisterRequestDto registerRequestDto)
        {
            if (!registerRequestDto.Password.Equals(registerRequestDto.PasswordConfrim))
            {
                return BadRequest("Пароли не совпадают!");
            }

            if (_dbContext.Users.FirstOrDefault(user =>
                user.Email.Equals(registerRequestDto.Email)) != null)
            {
                return BadRequest("Пользователь с таким email уже существует!");
            }

            if (registerRequestDto.Password.Length < 8)
            {
                return BadRequest("Длина пароля не должна быть менее 8 символов!");
            }

            User user = new User();
            user.Email = registerRequestDto.Email;
            user.FullName = registerRequestDto.FullName;
            user.Password = HashingPassword.HashPassword(registerRequestDto.Password);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            RegisterResponseDto registerResponseDto = new RegisterResponseDto();
            registerResponseDto.UserId = user.Id;

            return Ok(registerResponseDto);

        }

        [HttpPost("auth")]
        public async Task<ActionResult<AuthResponseDto>> AuthUser([FromBody] AuthRequestDto authRequestDto)
        {
            var user = _dbContext.Users.FirstOrDefault(user =>
                user.Email.Equals(authRequestDto.Email));

            if (user == null)
            {
                return BadRequest("Пользователя с таким email не существует!");
            }
            if (!HashingPassword.VerifyPassword(authRequestDto.Password, user.Password))
            {
                return BadRequest("Введен неверный пароль!");
            }

            var token = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var hashedRefreshToken = HashingRefreshToken.HashRefreshToken(refreshToken);
            var jwtSettings = _configuration.GetSection("Jwt");
            var expiresInMinutes = Convert.ToInt32(jwtSettings["ExpireMinutes"]);

            user.RefreshToken = hashedRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _dbContext.SaveChangesAsync();

            AuthResponseDto authResponseDto = new AuthResponseDto
            {
                Email = user.Email,
                FullName = user.FullName,
                Token = token,
                RefreshToken = refreshToken,
                Expires = expiresInMinutes * 60
            };
            return Ok(authResponseDto);

        }

        [HttpPost("refresh")]
        public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenRequestDto)
        {
            if (string.IsNullOrEmpty(refreshTokenRequestDto.RefreshToken))
            {
                return BadRequest("Refresh token не предоставлен!");
            }

            var accessToken = ExtractAccessTokenFromHeader();
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("Попробуйте авторизироваться снова!");
            }

            var userId = _jwtService.GetUserIdFromToken(accessToken);
            if (userId == null)
            {
                return BadRequest("Недействительный access token.");
            }

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest("Пользователь не найден");
            }

            if (user.RefreshToken == null)
            {
                return BadRequest("Refresh token отсутствует у пользователя.");
            }

            if (!HashingRefreshToken.VerifyRefreshToken(refreshTokenRequestDto.RefreshToken, user.RefreshToken))
            {
                return BadRequest("Недействительный refresh token");
            }

            if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _dbContext.SaveChangesAsync();

                return BadRequest("Срок действия refresh token истек.");
            }

            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var newHashedRefreshToken = HashingRefreshToken.HashRefreshToken(newRefreshToken);
            user.RefreshToken = newHashedRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _dbContext.SaveChangesAsync();

            var jwtSetting = _configuration.GetSection("Jwt");
            var expiresInMinutes = Convert.ToInt32(jwtSetting["ExpireMinutes"]);

            RefreshTokenResponseDto refreshTokenResponseDto = new RefreshTokenResponseDto();
            refreshTokenResponseDto.AccessToken = newAccessToken;
            refreshTokenResponseDto.RefreshToken = newRefreshToken;
            refreshTokenResponseDto.ExpiresIn = expiresInMinutes * 60;
            return Ok(refreshTokenResponseDto);



            //[HttpDelete("delete_user")]
            //public async Task<ActionResult<>> DeleteUser([FromBody] User user)
            //{
            //    User oldItem = await _dbContext.Users.FindAsync(user.Id);
            //    if (oldItem != null)
            //    {
            //        _dbContext.Users.Remove(oldItem);
            //    }
            //    await _dbContext.SaveChangesAsync();
            //    return Ok();
            //}

            //[HttpPost]
            //public async 
        }
    } 
}
