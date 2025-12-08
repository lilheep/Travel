using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApplicationTest.Data;
using WebApplicationTest.DTO;
using WebApplicationTest.DTO.AuthDto;
using WebApplicationTest.DTO.DeleteUserDtos;
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
        private readonly ExtractAccessTokenFromHeaderService _extractAccessTokenFromHeaderService;

        public UsersController(ApplicationDbContext dbContext, JwtService jwtService, IConfiguration configuration, ExtractAccessTokenFromHeaderService extractAccessTokenFromHeaderService)
        {
            _dbContext = dbContext;
            _jwtService = jwtService;
            _configuration = configuration;
            _extractAccessTokenFromHeaderService = extractAccessTokenFromHeaderService;
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
            user.Password = HashingPasswordService.HashPassword(registerRequestDto.Password);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            RegisterResponseDto registerResponseDto = new RegisterResponseDto();
            registerResponseDto.UserId = user.Id;

            return Ok(registerResponseDto);

        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> LogoutUser()
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

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = DateTime.MinValue;
            await _dbContext.SaveChangesAsync();
            return Ok("Вы успешно вышли из аккаунта.");

        }

        [HttpDelete("delete_profile")]
        [Authorize]
        public async Task<ActionResult> DeleteUser([FromBody] DeleteUserRequestDto deleteUserRequest)
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

            if (HashingPasswordService.VerifyPassword(user.Password, deleteUserRequest.Password))
            {
                return BadRequest("Введен неверный пароль.");
            }

            if (deleteUserRequest.TypeDelete)
            {
                user.IsDeleted = true;
                user.DeletedDate = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

            }
            _dbContext.Users.Attach(user);
            _dbContext.Users.Remove(user);

            await _dbContext.SaveChangesAsync();
            return Ok("Аккаунт успешно удален безвозвратно.");
            
        }
    } 
}
