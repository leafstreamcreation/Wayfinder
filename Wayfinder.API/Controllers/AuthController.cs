using System.Net;
using System.Net.Http;
using System.Web.Http;
using Wayfinder.API.Models.DTOs;
using Wayfinder.API.Services;

namespace Wayfinder.API.Controllers
{
    /// <summary>
    /// Controller for authentication endpoints (login/register)
    /// </summary>
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (var context = new OracleDbContext())
            {
                var userRepository = new UserRepository(context);
                var jwtService = new JwtService();
                var authService = new AuthService(userRepository, jwtService);

                var (user, token, error) = authService.Register(
                    request.Email, 
                    request.Password, 
                    request.Color1, 
                    request.Color2, 
                    request.Color3);

                if (error != null)
                {
                    return Content(HttpStatusCode.Conflict, new { message = error });
                }

                var response = new AuthResponse
                {
                    Token = token,
                    ExpiresAt = jwtService.GetExpirationTime(),
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Color1 = user.Color1,
                        Color2 = user.Color2,
                        Color3 = user.Color3,
                        CreatedAt = user.CreatedAt
                    }
                };

                return Created($"api/users/{user.Id}", response);
            }
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (var context = new OracleDbContext())
            {
                var userRepository = new UserRepository(context);
                var jwtService = new JwtService();
                var authService = new AuthService(userRepository, jwtService);

                var (user, token, error) = authService.Login(request.Email, request.Password);

                if (error != null)
                {
                    return Content(HttpStatusCode.Unauthorized, new { message = error });
                }

                var response = new AuthResponse
                {
                    Token = token,
                    ExpiresAt = jwtService.GetExpirationTime(),
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Color1 = user.Color1,
                        Color2 = user.Color2,
                        Color3 = user.Color3,
                        CreatedAt = user.CreatedAt
                    }
                };

                return Ok(response);
            }
        }
    }
}
