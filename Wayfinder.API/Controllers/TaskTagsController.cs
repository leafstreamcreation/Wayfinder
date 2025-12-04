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
    /// Controller for TaskTag management (JWE protected) - many-to-many relationship between tasks and tags
    /// </summary>
    [RoutePrefix("api/tasktags")]
    public class TaskTagsController : AuthenticatedApiController
    {
        /// <summary>
        /// Get all task-tag associations for the current user
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
                var repository = new TaskTagRepository(context);
                var taskTags = repository.GetByUserId(CurrentUserId.Value)
                    .Select(tt => new TaskTagDto
                    {
                        Id = tt.Id,
                        TaskId = tt.TaskId,
                        TagId = tt.TagId
                    });

                return Ok(taskTags);
            }
        }

        /// <summary>
        /// Get task-tag association by ID
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
                var repository = new TaskTagRepository(context);
                
                // Check ownership
                var ownerId = repository.GetTaskOwner(id);
                if (ownerId != CurrentUserId.Value)
                {
                    return Content(HttpStatusCode.Forbidden, new { message = "Access denied" });
                }

                var taskTag = repository.GetById(id);

                if (taskTag == null)
                {
                    return NotFound();
                }

                var dto = new TaskTagDto
                {
                    Id = taskTag.Id,
                    TaskId = taskTag.TaskId,
                    TagId = taskTag.TagId
                };

                return Ok(dto);
            }
        }

        /// <summary>
        /// Get task-tag associations by task ID
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

                var repository = new TaskTagRepository(context);
                var taskTags = repository.GetByTaskId(taskId)
                    .Select(tt => new TaskTagDto
                    {
                        Id = tt.Id,
                        TaskId = tt.TaskId,
                        TagId = tt.TagId
                    });

                return Ok(taskTags);
            }
        }

        /// <summary>
        /// Get task-tag associations by tag ID
        /// </summary>
        [HttpGet]
        [Route("tag/{tagId:int}")]
        public IHttpActionResult GetByTagId(int tagId)
        {
            if (!CurrentUserId.HasValue)
            {
                return Unauthorized();
            }

            using (var context = new OracleDbContext())
            {
                var tagRepository = new TagRepository(context);
                
                // Check ownership
                var ownerId = tagRepository.GetTaskOwner(tagId);
                if (ownerId != CurrentUserId.Value)
                {
                    return Content(HttpStatusCode.Forbidden, new { message = "Access denied" });
                }

                var repository = new TaskTagRepository(context);
                var taskTags = repository.GetByTagId(tagId)
                    .Select(tt => new TaskTagDto
                    {
                        Id = tt.Id,
                        TaskId = tt.TaskId,
                        TagId = tt.TagId
                    });

                return Ok(taskTags);
            }
        }

        /// <summary>
        /// Create a new task-tag association
        /// </summary>
        [HttpPost]
        [Route("")]
        public IHttpActionResult Create([FromBody] CreateTaskTagRequest request)
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
                // Verify task ownership
                var taskRepository = new TaskRepository(context);
                var task = taskRepository.GetById(request.TaskId);
                
                if (task == null)
                {
                    return Content(HttpStatusCode.BadRequest, new { message = "Task not found" });
                }

                if (task.UserId != CurrentUserId.Value)
                {
                    return Content(HttpStatusCode.Forbidden, new { message = "Access denied to task" });
                }

                // Verify tag ownership
                var tagRepository = new TagRepository(context);
                var tagOwnerId = tagRepository.GetTaskOwner(request.TagId);
                
                if (tagOwnerId == null)
                {
                    return Content(HttpStatusCode.BadRequest, new { message = "Tag not found" });
                }

                if (tagOwnerId != CurrentUserId.Value)
                {
                    return Content(HttpStatusCode.Forbidden, new { message = "Access denied to tag" });
                }

                var repository = new TaskTagRepository(context);

                // Check if association already exists
                if (repository.Exists(request.TaskId, request.TagId))
                {
                    return Content(HttpStatusCode.Conflict, new { message = "Task-tag association already exists" });
                }

                var taskTag = new TaskTag
                {
                    TaskId = request.TaskId,
                    TagId = request.TagId
                };

                taskTag = repository.Create(taskTag);

                var dto = new TaskTagDto
                {
                    Id = taskTag.Id,
                    TaskId = taskTag.TaskId,
                    TagId = taskTag.TagId
                };

                return Created($"api/tasktags/{taskTag.Id}", dto);
            }
        }

        /// <summary>
        /// Delete a task-tag association
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
                var repository = new TaskTagRepository(context);
                
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
