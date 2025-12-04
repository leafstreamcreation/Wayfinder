using System.Linq;
using System.Net;
using System.Web.Http;
using Wayfinder.API.Filters;
using Wayfinder.API.Models;
using Wayfinder.API.Models.DTOs;
using Wayfinder.API.Services;

namespace Wayfinder.API.Controllers
{
    /// <summary>
    /// Controller for task management (JWE protected)
    /// </summary>
    [RoutePrefix("api/tasks")]
    public class TasksController : AuthenticatedApiController
    {
        /// <summary>
        /// Get all tasks for the current user
        /// </summary>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            if (!CurrentUserId.HasValue)
            {
                return Unauthorized();
            }

            using (var context = new OracleDbContext())
            {
                var repository = new TaskRepository(context);
                var tasks = repository.GetByUserId(CurrentUserId.Value)
                    .Select(t => new TaskDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        UserId = t.UserId,
                        LastFinishedDate = t.LastFinishedDate,
                        RefreshInterval = t.RefreshInterval,
                        AlertThresholdPercentage = t.AlertThresholdPercentage,
                        IsActive = t.IsActive,
                        InitialRefreshInterval = t.InitialRefreshInterval,
                        CreatedAt = t.CreatedAt
                    });

                return Ok(tasks);
            }
        }

        /// <summary>
        /// Get task by ID
        /// </summary>
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            if (!CurrentUserId.HasValue)
            {
                return Unauthorized();
            }

            using (var context = new OracleDbContext())
            {
                var repository = new TaskRepository(context);
                var task = repository.GetById(id);

                if (task == null)
                {
                    return NotFound();
                }

                // Check ownership
                if (task.UserId != CurrentUserId.Value)
                {
                    return Content(HttpStatusCode.Forbidden, new { message = "Access denied" });
                }

                var dto = new TaskDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    UserId = task.UserId,
                    LastFinishedDate = task.LastFinishedDate,
                    RefreshInterval = task.RefreshInterval,
                    AlertThresholdPercentage = task.AlertThresholdPercentage,
                    IsActive = task.IsActive,
                    InitialRefreshInterval = task.InitialRefreshInterval,
                    CreatedAt = task.CreatedAt
                };

                return Ok(dto);
            }
        }

        /// <summary>
        /// Create a new task
        /// </summary>
        [HttpPost]
        [Route("")]
        public IHttpActionResult Create([FromBody] CreateTaskRequest request)
        {
            if (!CurrentUserId.HasValue)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (var context = new OracleDbContext())
            {
                var repository = new TaskRepository(context);
                var task = new TaskItem
                {
                    Title = request.Title,
                    UserId = CurrentUserId.Value,
                    RefreshInterval = request.RefreshInterval,
                    AlertThresholdPercentage = request.AlertThresholdPercentage,
                    IsActive = request.IsActive,
                    InitialRefreshInterval = request.InitialRefreshInterval ?? request.RefreshInterval
                };

                task = repository.Create(task);

                var dto = new TaskDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    UserId = task.UserId,
                    LastFinishedDate = task.LastFinishedDate,
                    RefreshInterval = task.RefreshInterval,
                    AlertThresholdPercentage = task.AlertThresholdPercentage,
                    IsActive = task.IsActive,
                    InitialRefreshInterval = task.InitialRefreshInterval,
                    CreatedAt = task.CreatedAt
                };

                return Created($"api/tasks/{task.Id}", dto);
            }
        }

        /// <summary>
        /// Update a task
        /// </summary>
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] UpdateTaskRequest request)
        {
            if (!CurrentUserId.HasValue)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (var context = new OracleDbContext())
            {
                var repository = new TaskRepository(context);
                var task = repository.GetById(id);

                if (task == null)
                {
                    return NotFound();
                }

                // Check ownership
                if (task.UserId != CurrentUserId.Value)
                {
                    return Content(HttpStatusCode.Forbidden, new { message = "Access denied" });
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(request.Title))
                {
                    task.Title = request.Title;
                }

                if (request.RefreshInterval.HasValue)
                {
                    task.RefreshInterval = request.RefreshInterval.Value;
                }

                if (request.AlertThresholdPercentage.HasValue)
                {
                    task.AlertThresholdPercentage = request.AlertThresholdPercentage.Value;
                }

                if (request.IsActive.HasValue)
                {
                    task.IsActive = request.IsActive.Value;
                }

                if (request.InitialRefreshInterval.HasValue)
                {
                    task.InitialRefreshInterval = request.InitialRefreshInterval.Value;
                }

                repository.Update(task);

                var dto = new TaskDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    UserId = task.UserId,
                    LastFinishedDate = task.LastFinishedDate,
                    RefreshInterval = task.RefreshInterval,
                    AlertThresholdPercentage = task.AlertThresholdPercentage,
                    IsActive = task.IsActive,
                    InitialRefreshInterval = task.InitialRefreshInterval,
                    CreatedAt = task.CreatedAt
                };

                return Ok(dto);
            }
        }

        /// <summary>
        /// Delete a task
        /// </summary>
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            if (!CurrentUserId.HasValue)
            {
                return Unauthorized();
            }

            using (var context = new OracleDbContext())
            {
                var repository = new TaskRepository(context);
                var task = repository.GetById(id);

                if (task == null)
                {
                    return NotFound();
                }

                // Check ownership
                if (task.UserId != CurrentUserId.Value)
                {
                    return Content(HttpStatusCode.Forbidden, new { message = "Access denied" });
                }

                repository.Delete(id);

                return StatusCode(HttpStatusCode.NoContent);
            }
        }
    }
}
