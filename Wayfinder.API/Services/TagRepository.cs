using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using Wayfinder.API.Models;

namespace Wayfinder.API.Services
{
    /// <summary>
    /// Repository for Tag entity operations
    /// </summary>
    public class TagRepository
    {
        private readonly OracleDbContext _context;

        public TagRepository(OracleDbContext context)
        {
            _context = context;
        }

        public Tag GetById(int id)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                "SELECT ID, NAME, TASK_ID FROM TAGS WHERE ID = :id", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("id", id));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapTag(reader);
                    }
                }
            }
            return null;
        }

        public IEnumerable<Tag> GetAll()
        {
            var tags = new List<Tag>();
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                "SELECT ID, NAME, TASK_ID FROM TAGS ORDER BY ID", 
                connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tags.Add(MapTag(reader));
                    }
                }
            }
            return tags;
        }

        public IEnumerable<Tag> GetByTaskId(int taskId)
        {
            var tags = new List<Tag>();
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                "SELECT ID, NAME, TASK_ID FROM TAGS WHERE TASK_ID = :taskId ORDER BY ID", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("taskId", taskId));
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tags.Add(MapTag(reader));
                    }
                }
            }
            return tags;
        }

        public IEnumerable<Tag> GetByUserId(int userId)
        {
            var tags = new List<Tag>();
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                @"SELECT T.ID, T.NAME, T.TASK_ID 
                  FROM TAGS T 
                  INNER JOIN TASKS TA ON T.TASK_ID = TA.ID 
                  WHERE TA.USER_ID = :userId 
                  ORDER BY T.ID", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("userId", userId));
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tags.Add(MapTag(reader));
                    }
                }
            }
            return tags;
        }

        public Tag Create(Tag tag)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                @"INSERT INTO TAGS (NAME, TASK_ID) 
                  VALUES (:name, :taskId)
                  RETURNING ID INTO :id", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("name", tag.Name));
                command.Parameters.Add(new OracleParameter("taskId", tag.TaskId));
                
                var idParam = new OracleParameter("id", OracleDbType.Decimal, System.Data.ParameterDirection.Output);
                command.Parameters.Add(idParam);
                
                command.ExecuteNonQuery();
                tag.Id = Convert.ToInt32(((Oracle.ManagedDataAccess.Types.OracleDecimal)idParam.Value).Value);
            }
            return tag;
        }

        public Tag Update(Tag tag)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                @"UPDATE TAGS SET NAME = :name, TASK_ID = :taskId WHERE ID = :id", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("name", tag.Name));
                command.Parameters.Add(new OracleParameter("taskId", tag.TaskId));
                command.Parameters.Add(new OracleParameter("id", tag.Id));
                
                command.ExecuteNonQuery();
            }
            return tag;
        }

        public bool Delete(int id)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand("DELETE FROM TAGS WHERE ID = :id", connection))
            {
                command.Parameters.Add(new OracleParameter("id", id));
                return command.ExecuteNonQuery() > 0;
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

        public int? GetTaskOwner(int tagId)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                @"SELECT TA.USER_ID 
                  FROM TAGS T 
                  INNER JOIN TASKS TA ON T.TASK_ID = TA.ID 
                  WHERE T.ID = :tagId", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("tagId", tagId));
                var result = command.ExecuteScalar();
                return result == null || result == DBNull.Value ? (int?)null : Convert.ToInt32(result);
            }
        }

        private Tag MapTag(OracleDataReader reader)
        {
            return new Tag
            {
                Id = Convert.ToInt32(reader["ID"]),
                Name = reader["NAME"].ToString(),
                TaskId = Convert.ToInt32(reader["TASK_ID"])
            };
        }
    }
}
