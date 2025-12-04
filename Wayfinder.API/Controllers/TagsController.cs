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
    /// Controller for tag management (JWE protected)
    /// </summary>
    [RoutePrefix("api/tags")]
    public class TagsController : AuthenticatedApiController
    {
        /// <summary>
        /// Get all tags for the current user
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
                var repository = new TagRepository(context);
                var tags = repository.GetByUserId(CurrentUserId.Value)
                    .Select(t => new TagDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        TaskId = t.TaskId
                    });

                return Ok(tags);
            }
        }

        /// <summary>
        /// Get tag by ID
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
                var repository = new TagRepository(context);
                
                // Check ownership
                var ownerId = repository.GetTaskOwner(id);
                if (ownerId != CurrentUserId.Value)
                {
                    return Content(HttpStatusCode.Forbidden, new { message = "Access denied" });
                }

                var tag = repository.GetById(id);

                if (tag == null)
                {
                    return NotFound();
                }

                var dto = new TagDto
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    TaskId = tag.TaskId
                };

                return Ok(dto);
            }
        }

        /// <summary>
        /// Get tags by task ID
        /// </summary>
        [HttpGet]
        [Route("task/{taskId:int}")]
        public IHttpActionResult GetByTaskId(int taskId)
        {
            if (!CurrentUserId.HasValue)
            {
                return Unauthorized();
            }

            using (var context = new OracleDbContext())
            {
                var taskRepository = new TaskRepository(context);
                var task = taskRepository.GetById(taskId);
                
                if (task == null)
                {
                    return NotFound();
                }

                // Check ownership
                if (task.UserId != CurrentUserId.Value)
                {
                    return Content(HttpStatusCode.Forbidden, new { message = "Access denied" });
                }

                var repository = new TagRepository(context);
                var tags = repository.GetByTaskId(taskId)
                    .Select(t => new TagDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        TaskId = t.TaskId
                    });

                return Ok(tags);
            }
        }

        /// <summary>
        /// Create a new tag
        /// </summary>
        [HttpPost]
        [Route("")]
        public IHttpActionResult Create([FromBody] CreateTagRequest request)
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
                var taskRepository = new TaskRepository(context);
                var task = taskRepository.GetById(request.TaskId);
                
                if (task == null)
                {
                    return Content(HttpStatusCode.BadRequest, new { message = "Task not found" });
                }

                // Check ownership
                if (task.UserId != CurrentUserId.Value)
                {
                    return Content(HttpStatusCode.Forbidden, new { message = "Access denied" });
                }

                var repository = new TagRepository(context);
                var tag = new Tag
                {
                    Name = request.Name,
                    TaskId = request.TaskId
                };

                tag = repository.Create(tag);

                var dto = new TagDto
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    TaskId = tag.TaskId
                };

                return Created($"api/tags/{tag.Id}", dto);
            }
        }

        /// <summary>
        /// Update a tag
        /// </summary>
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] UpdateTagRequest request)
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
                var repository = new TagRepository(context);
                
                // Check ownership
                var ownerId = repository.GetTaskOwner(id);
                if (ownerId != CurrentUserId.Value)
                {
                    return Content(HttpStatusCode.Forbidden, new { message = "Access denied" });
                }

                var tag = repository.GetById(id);

                if (tag == null)
                {
                    return NotFound();
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(request.Name))
                {
                    tag.Name = request.Name;
                }

                if (request.TaskId.HasValue)
                {
                    var taskRepository = new TaskRepository(context);
                    var task = taskRepository.GetById(request.TaskId.Value);
                    
                    if (task == null)
                    {
                        return Content(HttpStatusCode.BadRequest, new { message = "Task not found" });
                    }

                    // Check ownership of the new task
                    if (task.UserId != CurrentUserId.Value)
                    {
                        return Content(HttpStatusCode.Forbidden, new { message = "Access denied to target task" });
                    }

                    tag.TaskId = request.TaskId.Value;
                }

                repository.Update(tag);

                var dto = new TagDto
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    TaskId = tag.TaskId
                };

                return Ok(dto);
            }
        }

        /// <summary>
        /// Delete a tag
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
                var repository = new TagRepository(context);
                
                // Check ownership
                var ownerId = repository.GetTaskOwner(id);
                if (ownerId != CurrentUserId.Value)
                {
                    return Content(HttpStatusCode.Forbidden, new { message = "Access denied" });
                }

                if (!repository.Delete(id))
                {
                    return NotFound();
                }

                return StatusCode(HttpStatusCode.NoContent);
            }
        }
    }
}
