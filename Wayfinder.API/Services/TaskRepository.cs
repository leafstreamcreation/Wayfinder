using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using Wayfinder.API.Models;

namespace Wayfinder.API.Services
{
    /// <summary>
    /// Repository for TaskItem entity operations
    /// </summary>
    public class TaskRepository
    {
        private readonly OracleDbContext _context;

        public TaskRepository(OracleDbContext context)
        {
            _context = context;
        }

        public TaskItem GetById(int id)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                @"SELECT ID, TITLE, USER_ID, LAST_FINISHED_DATE, REFRESH_INTERVAL, 
                         ALERT_THRESHOLD_PERCENTAGE, IS_ACTIVE, INITIAL_REFRESH_INTERVAL, 
                         CREATED_AT, UPDATED_AT 
                  FROM TASKS WHERE ID = :id", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("id", id));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapTask(reader);
                    }
                }
            }
            return null;
        }

        public IEnumerable<TaskItem> GetAll()
        {
            var tasks = new List<TaskItem>();
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                @"SELECT ID, TITLE, USER_ID, LAST_FINISHED_DATE, REFRESH_INTERVAL, 
                         ALERT_THRESHOLD_PERCENTAGE, IS_ACTIVE, INITIAL_REFRESH_INTERVAL, 
                         CREATED_AT, UPDATED_AT 
                  FROM TASKS ORDER BY ID", 
                connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tasks.Add(MapTask(reader));
                    }
                }
            }
            return tasks;
        }

        public IEnumerable<TaskItem> GetByUserId(int userId)
        {
            var tasks = new List<TaskItem>();
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                @"SELECT ID, TITLE, USER_ID, LAST_FINISHED_DATE, REFRESH_INTERVAL, 
                         ALERT_THRESHOLD_PERCENTAGE, IS_ACTIVE, INITIAL_REFRESH_INTERVAL, 
                         CREATED_AT, UPDATED_AT 
                  FROM TASKS WHERE USER_ID = :userId ORDER BY ID", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("userId", userId));
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tasks.Add(MapTask(reader));
                    }
                }
            }
            return tasks;
        }

        public TaskItem Create(TaskItem task)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                @"INSERT INTO TASKS (TITLE, USER_ID, LAST_FINISHED_DATE, REFRESH_INTERVAL, 
                                     ALERT_THRESHOLD_PERCENTAGE, IS_ACTIVE, INITIAL_REFRESH_INTERVAL, CREATED_AT) 
                  VALUES (:title, :userId, :lastFinishedDate, :refreshInterval, 
                          :alertThresholdPercentage, :isActive, :initialRefreshInterval, CURRENT_TIMESTAMP)
                  RETURNING ID INTO :id", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("title", task.Title));
                command.Parameters.Add(new OracleParameter("userId", task.UserId));
                command.Parameters.Add(new OracleParameter("lastFinishedDate", 
                    task.LastFinishedDate.HasValue ? (object)task.LastFinishedDate.Value : DBNull.Value));
                command.Parameters.Add(new OracleParameter("refreshInterval", task.RefreshInterval));
                command.Parameters.Add(new OracleParameter("alertThresholdPercentage", task.AlertThresholdPercentage));
                command.Parameters.Add(new OracleParameter("isActive", task.IsActive ? 1 : 0));
                command.Parameters.Add(new OracleParameter("initialRefreshInterval", task.InitialRefreshInterval));
                
                var idParam = new OracleParameter("id", OracleDbType.Decimal, System.Data.ParameterDirection.Output);
                command.Parameters.Add(idParam);
                
                command.ExecuteNonQuery();
                task.Id = Convert.ToInt32(((Oracle.ManagedDataAccess.Types.OracleDecimal)idParam.Value).Value);
                task.CreatedAt = DateTime.UtcNow;
            }
            return task;
        }

        public TaskItem Update(TaskItem task)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                @"UPDATE TASKS SET 
                    TITLE = :title, 
                    LAST_FINISHED_DATE = :lastFinishedDate, 
                    REFRESH_INTERVAL = :refreshInterval, 
                    ALERT_THRESHOLD_PERCENTAGE = :alertThresholdPercentage, 
                    IS_ACTIVE = :isActive, 
                    INITIAL_REFRESH_INTERVAL = :initialRefreshInterval,
                    UPDATED_AT = CURRENT_TIMESTAMP 
                  WHERE ID = :id", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("title", task.Title));
                command.Parameters.Add(new OracleParameter("lastFinishedDate", 
                    task.LastFinishedDate.HasValue ? (object)task.LastFinishedDate.Value : DBNull.Value));
                command.Parameters.Add(new OracleParameter("refreshInterval", task.RefreshInterval));
                command.Parameters.Add(new OracleParameter("alertThresholdPercentage", task.AlertThresholdPercentage));
                command.Parameters.Add(new OracleParameter("isActive", task.IsActive ? 1 : 0));
                command.Parameters.Add(new OracleParameter("initialRefreshInterval", task.InitialRefreshInterval));
                command.Parameters.Add(new OracleParameter("id", task.Id));
                
                command.ExecuteNonQuery();
                task.UpdatedAt = DateTime.UtcNow;
            }
            return task;
        }

        public bool Delete(int id)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand("DELETE FROM TASKS WHERE ID = :id", connection))
            {
                command.Parameters.Add(new OracleParameter("id", id));
                return command.ExecuteNonQuery() > 0;
            }
        }

        public bool UserExists(int userId)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                "SELECT COUNT(*) FROM USERS WHERE ID = :userId", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("userId", userId));
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private TaskItem MapTask(OracleDataReader reader)
        {
            return new TaskItem
            {
                Id = Convert.ToInt32(reader["ID"]),
                Title = reader["TITLE"].ToString(),
                UserId = Convert.ToInt32(reader["USER_ID"]),
                LastFinishedDate = reader["LAST_FINISHED_DATE"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["LAST_FINISHED_DATE"]),
                RefreshInterval = Convert.ToInt32(reader["REFRESH_INTERVAL"]),
                AlertThresholdPercentage = Convert.ToInt32(reader["ALERT_THRESHOLD_PERCENTAGE"]),
                IsActive = Convert.ToInt32(reader["IS_ACTIVE"]) == 1,
                InitialRefreshInterval = Convert.ToInt32(reader["INITIAL_REFRESH_INTERVAL"]),
                CreatedAt = reader["CREATED_AT"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["CREATED_AT"]),
                UpdatedAt = reader["UPDATED_AT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["UPDATED_AT"])
            };
        }
    }
}
