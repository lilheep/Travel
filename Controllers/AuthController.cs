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
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly ExtractAccessTokenFromHeaderService _extractAccessTokenFromHeaderService;
        public AuthController(ApplicationDbContext dbContext, JwtService jwtService, IConfiguration configuration, ExtractAccessTokenFromHeaderService extractAccessTokenFromHeaderService)
        {
            _dbContext = dbContext;
            _jwtService = jwtService;
            _configuration = configuration;
            _extractAccessTokenFromHeaderService = extractAccessTokenFromHeaderService;
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

            User user = new User();
            user.Email = registerRequestDto.Email;
            user.FullName = registerRequestDto.FullName;
            user.Password = HashingPasswordService.HashPassword(registerRequestDto.Password);
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
            if (!HashingPasswordService.VerifyPassword(authRequestDto.Password, user.Password))
            {
                return BadRequest("Введен неверный пароль!");
            }

            var token = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var hashedRefreshToken = HashingRefreshTokenService.HashRefreshToken(refreshToken);
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

            var accessToken = _extractAccessTokenFromHeaderService.ExtractAccessTokenFromHeader(Request);
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized("Попробуйте авторизироваться снова!");
            }

            var userId = _jwtService.GetUserIdFromToken(accessToken);
            if (userId == null)
            {
                return Unauthorized("Недействительный access token.");
            }

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest("Пользователь не найден");
            }

            if (user.RefreshToken == null)
            {
                return Unauthorized("Refresh token отсутствует у пользователя.");
            }

            if (!HashingRefreshTokenService.VerifyRefreshToken(refreshTokenRequestDto.RefreshToken, user.RefreshToken))
            {
                return Unauthorized("Недействительный refresh token");
            }

            if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _dbContext.SaveChangesAsync();

                return Unauthorized("Срок действия refresh token истек.");
            }

            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var newHashedRefreshToken = HashingRefreshTokenService.HashRefreshToken(newRefreshToken);
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
        }
    }
}

