using System.Linq;
using System.Net;
using System.Web.Http;
using Wayfinder.API.Filters;
using Wayfinder.API.Models.DTOs;
using Wayfinder.API.Services;

namespace Wayfinder.API.Controllers
{
    /// <summary>
    /// Controller for user management (JWE protected)
    /// </summary>
    [RoutePrefix("api/users")]
    public class UsersController : AuthenticatedApiController
    {
        /// <summary>
        /// Get all users (admin only - returns current user for now)
        /// </summary>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            using (var context = new OracleDbContext())
            {
                var repository = new UserRepository(context);
                var users = repository.GetAll()
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        Color1 = u.Color1,
                        Color2 = u.Color2,
                        Color3 = u.Color3,
                        CreatedAt = u.CreatedAt
                    });

                return Ok(users);
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            // Users can only access their own data
            if (CurrentUserId != id)
            {
                return Content(HttpStatusCode.Forbidden, new { message = "Access denied" });
            }

            using (var context = new OracleDbContext())
            {
                var repository = new UserRepository(context);
                var user = repository.GetById(id);

                if (user == null)
                {
                    return NotFound();
                }

                var dto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Color1 = user.Color1,
                    Color2 = user.Color2,
                    Color3 = user.Color3,
                    CreatedAt = user.CreatedAt
                };

                return Ok(dto);
            }
        }

        /// <summary>
        /// Get current authenticated user
        /// </summary>
        [HttpGet]
        [Route("me")]
        public IHttpActionResult GetCurrentUser()
        {
            if (!CurrentUserId.HasValue)
            {
                return Unauthorized();
            }

            using (var context = new OracleDbContext())
            {
                var repository = new UserRepository(context);
                var user = repository.GetById(CurrentUserId.Value);

                if (user == null)
                {
                    return NotFound();
                }

                var dto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Color1 = user.Color1,
                    Color2 = user.Color2,
                    Color3 = user.Color3,
                    CreatedAt = user.CreatedAt
                };

                return Ok(dto);
            }
        }

        /// <summary>
        /// Update user
        /// </summary>
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] UpdateUserRequest request)
        {
            // Users can only update their own data
            if (CurrentUserId != id)
            {
                return Content(HttpStatusCode.Forbidden, new { message = "Access denied" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (var context = new OracleDbContext())
            {
                var repository = new UserRepository(context);
                var user = repository.GetById(id);

                if (user == null)
                {
                    return NotFound();
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(request.Email))
                {
                    if (repository.EmailExists(request.Email, id))
                    {
                        return Content(HttpStatusCode.Conflict, new { message = "Email already in use" });
                    }
                    user.Email = request.Email;
                }

                if (!string.IsNullOrEmpty(request.Password))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                }

                if (request.Color1 != null)
                {
                    user.Color1 = request.Color1;
                }

                if (request.Color2 != null)
                {
                    user.Color2 = request.Color2;
                }

                if (request.Color3 != null)
                {
                    user.Color3 = request.Color3;
                }

                repository.Update(user);

                var dto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Color1 = user.Color1,
                    Color2 = user.Color2,
                    Color3 = user.Color3,
                    CreatedAt = user.CreatedAt
                };

                return Ok(dto);
            }
        }

        /// <summary>
        /// Delete user
        /// </summary>
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            // Users can only delete their own account
            if (CurrentUserId != id)
            {
                return Content(HttpStatusCode.Forbidden, new { message = "Access denied" });
            }

            using (var context = new OracleDbContext())
            {
                var repository = new UserRepository(context);
                
                if (!repository.Delete(id))
                {
                    return NotFound();
                }

                return StatusCode(HttpStatusCode.NoContent);
            }
        }
    }
}
