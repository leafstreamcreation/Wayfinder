using System;
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
    /// Controller for record management (JWE protected)
    /// </summary>
    [RoutePrefix("api/records")]
    public class RecordsController : AuthenticatedApiController
    {
        /// <summary>
        /// Get all records for the current user
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
                var repository = new RecordRepository(context);
                var records = repository.GetByUserId(CurrentUserId.Value)
                    .Select(r => new RecordDto
                    {
                        Id = r.Id,
                        TaskId = r.TaskId,
                        FinishedDate = r.FinishedDate,
                        Status = r.Status
                    });

                return Ok(records);
            }
        }

        /// <summary>
        /// Get record by ID
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
                var repository = new RecordRepository(context);
                
                // Check ownership
                var ownerId = repository.GetTaskOwner(id);
                if (ownerId != CurrentUserId.Value)
                {
                    return Content(HttpStatusCode.Forbidden, new { message = "Access denied" });
                }

                var record = repository.GetById(id);

                if (record == null)
                {
                    return NotFound();
                }

                var dto = new RecordDto
                {
                    Id = record.Id,
                    TaskId = record.TaskId,
                    FinishedDate = record.FinishedDate,
                    Status = record.Status
                };

                return Ok(dto);
            }
        }

        /// <summary>
        /// Get records by task ID
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

                var repository = new RecordRepository(context);
                var records = repository.GetByTaskId(taskId)
                    .Select(r => new RecordDto
                    {
                        Id = r.Id,
                        TaskId = r.TaskId,
                        FinishedDate = r.FinishedDate,
                        Status = r.Status
                    });

                return Ok(records);
            }
        }

        /// <summary>
        /// Create a new record
        /// </summary>
        [HttpPost]
        [Route("")]
        public IHttpActionResult Create([FromBody] CreateRecordRequest request)
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

                var repository = new RecordRepository(context);
                var record = new Record
                {
                    TaskId = request.TaskId,
                    FinishedDate = request.FinishedDate ?? DateTime.UtcNow,
                    Status = request.Status
                };

                record = repository.Create(record);

                var dto = new RecordDto
                {
                    Id = record.Id,
                    TaskId = record.TaskId,
                    FinishedDate = record.FinishedDate,
                    Status = record.Status
                };

                return Created($"api/records/{record.Id}", dto);
            }
        }

        /// <summary>
        /// Update a record
        /// </summary>
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] UpdateRecordRequest request)
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
                var repository = new RecordRepository(context);
                
                // Check ownership
                var ownerId = repository.GetTaskOwner(id);
                if (ownerId != CurrentUserId.Value)
                {
                    return Content(HttpStatusCode.Forbidden, new { message = "Access denied" });
                }

                var record = repository.GetById(id);

                if (record == null)
                {
                    return NotFound();
                }

                // Update fields if provided
                if (request.FinishedDate.HasValue)
                {
                    record.FinishedDate = request.FinishedDate.Value;
                }

                if (!string.IsNullOrEmpty(request.Status))
                {
                    record.Status = request.Status;
                }

                repository.Update(record);

                var dto = new RecordDto
                {
                    Id = record.Id,
                    TaskId = record.TaskId,
                    FinishedDate = record.FinishedDate,
                    Status = record.Status
                };

                return Ok(dto);
            }
        }

        /// <summary>
        /// Delete a record
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
                var repository = new RecordRepository(context);
                
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
