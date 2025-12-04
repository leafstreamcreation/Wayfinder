using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using Wayfinder.API.Models;

namespace Wayfinder.API.Services
{
    /// <summary>
    /// Repository for TaskTag entity operations (many-to-many relationship)
    /// </summary>
    public class TaskTagRepository
    {
        private readonly OracleDbContext _context;

        public TaskTagRepository(OracleDbContext context)
        {
            _context = context;
        }

        public TaskTag GetById(int id)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                "SELECT ID, TASK_ID, TAG_ID FROM TASK_TAGS WHERE ID = :id", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("id", id));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapTaskTag(reader);
                    }
                }
            }
            return null;
        }

        public IEnumerable<TaskTag> GetAll()
        {
            var taskTags = new List<TaskTag>();
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                "SELECT ID, TASK_ID, TAG_ID FROM TASK_TAGS ORDER BY ID", 
                connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        taskTags.Add(MapTaskTag(reader));
                    }
                }
            }
            return taskTags;
        }

        public IEnumerable<TaskTag> GetByTaskId(int taskId)
        {
            var taskTags = new List<TaskTag>();
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                "SELECT ID, TASK_ID, TAG_ID FROM TASK_TAGS WHERE TASK_ID = :taskId ORDER BY ID", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("taskId", taskId));
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        taskTags.Add(MapTaskTag(reader));
                    }
                }
            }
            return taskTags;
        }

        public IEnumerable<TaskTag> GetByTagId(int tagId)
        {
            var taskTags = new List<TaskTag>();
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                "SELECT ID, TASK_ID, TAG_ID FROM TASK_TAGS WHERE TAG_ID = :tagId ORDER BY ID", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("tagId", tagId));
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        taskTags.Add(MapTaskTag(reader));
                    }
                }
            }
            return taskTags;
        }

        public IEnumerable<TaskTag> GetByUserId(int userId)
        {
            var taskTags = new List<TaskTag>();
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                @"SELECT TT.ID, TT.TASK_ID, TT.TAG_ID 
                  FROM TASK_TAGS TT 
                  INNER JOIN TASKS T ON TT.TASK_ID = T.ID 
                  WHERE T.USER_ID = :userId 
                  ORDER BY TT.ID", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("userId", userId));
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        taskTags.Add(MapTaskTag(reader));
                    }
                }
            }
            return taskTags;
        }

        public TaskTag Create(TaskTag taskTag)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                @"INSERT INTO TASK_TAGS (TASK_ID, TAG_ID) 
                  VALUES (:taskId, :tagId)
                  RETURNING ID INTO :id", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("taskId", taskTag.TaskId));
                command.Parameters.Add(new OracleParameter("tagId", taskTag.TagId));
                
                var idParam = new OracleParameter("id", OracleDbType.Decimal, System.Data.ParameterDirection.Output);
                command.Parameters.Add(idParam);
                
                command.ExecuteNonQuery();
                taskTag.Id = Convert.ToInt32(((Oracle.ManagedDataAccess.Types.OracleDecimal)idParam.Value).Value);
            }
            return taskTag;
        }

        public bool Delete(int id)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand("DELETE FROM TASK_TAGS WHERE ID = :id", connection))
            {
                command.Parameters.Add(new OracleParameter("id", id));
                return command.ExecuteNonQuery() > 0;
            }
        }

        public bool DeleteByTaskAndTag(int taskId, int tagId)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                "DELETE FROM TASK_TAGS WHERE TASK_ID = :taskId AND TAG_ID = :tagId", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("taskId", taskId));
                command.Parameters.Add(new OracleParameter("tagId", tagId));
                return command.ExecuteNonQuery() > 0;
            }
        }

        public bool Exists(int taskId, int tagId)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                "SELECT COUNT(*) FROM TASK_TAGS WHERE TASK_ID = :taskId AND TAG_ID = :tagId", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("taskId", taskId));
                command.Parameters.Add(new OracleParameter("tagId", tagId));
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public int? GetTaskOwner(int id)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                @"SELECT T.USER_ID 
                  FROM TASK_TAGS TT 
                  INNER JOIN TASKS T ON TT.TASK_ID = T.ID 
                  WHERE TT.ID = :id", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("id", id));
                var result = command.ExecuteScalar();
                return result == null || result == DBNull.Value ? (int?)null : Convert.ToInt32(result);
            }
        }

        public int? GetTaskOwnerByTaskId(int taskId)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                "SELECT USER_ID FROM TASKS WHERE ID = :taskId", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("taskId", taskId));
                var result = command.ExecuteScalar();
                return result == null || result == DBNull.Value ? (int?)null : Convert.ToInt32(result);
            }
        }

        public bool TaskExists(int taskId)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                "SELECT COUNT(*) FROM TASKS WHERE ID = :taskId", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("taskId", taskId));
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public bool TagExists(int tagId)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                "SELECT COUNT(*) FROM TAGS WHERE ID = :tagId", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("tagId", tagId));
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private TaskTag MapTaskTag(OracleDataReader reader)
        {
            return new TaskTag
            {
                Id = Convert.ToInt32(reader["ID"]),
                TaskId = Convert.ToInt32(reader["TASK_ID"]),
                TagId = Convert.ToInt32(reader["TAG_ID"])
            };
        }
    }
}
