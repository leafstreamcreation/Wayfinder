using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using Wayfinder.API.Models;

namespace Wayfinder.API.Services
{
    /// <summary>
    /// Repository for Record entity operations
    /// </summary>
    public class RecordRepository
    {
        private readonly OracleDbContext _context;

        public RecordRepository(OracleDbContext context)
        {
            _context = context;
        }

        public Record GetById(int id)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                "SELECT ID, TASK_ID, FINISHED_DATE, STATUS FROM RECORDS WHERE ID = :id", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("id", id));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapRecord(reader);
                    }
                }
            }
            return null;
        }

        public IEnumerable<Record> GetAll()
        {
            var records = new List<Record>();
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                "SELECT ID, TASK_ID, FINISHED_DATE, STATUS FROM RECORDS ORDER BY ID", 
                connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        records.Add(MapRecord(reader));
                    }
                }
            }
            return records;
        }

        public IEnumerable<Record> GetByTaskId(int taskId)
        {
            var records = new List<Record>();
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                "SELECT ID, TASK_ID, FINISHED_DATE, STATUS FROM RECORDS WHERE TASK_ID = :taskId ORDER BY FINISHED_DATE DESC", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("taskId", taskId));
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        records.Add(MapRecord(reader));
                    }
                }
            }
            return records;
        }

        public IEnumerable<Record> GetByUserId(int userId)
        {
            var records = new List<Record>();
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                @"SELECT R.ID, R.TASK_ID, R.FINISHED_DATE, R.STATUS 
                  FROM RECORDS R 
                  INNER JOIN TASKS T ON R.TASK_ID = T.ID 
                  WHERE T.USER_ID = :userId 
                  ORDER BY R.FINISHED_DATE DESC", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("userId", userId));
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        records.Add(MapRecord(reader));
                    }
                }
            }
            return records;
        }

        public Record Create(Record record)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                @"INSERT INTO RECORDS (TASK_ID, FINISHED_DATE, STATUS) 
                  VALUES (:taskId, :finishedDate, :status)
                  RETURNING ID INTO :id", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("taskId", record.TaskId));
                command.Parameters.Add(new OracleParameter("finishedDate", record.FinishedDate));
                command.Parameters.Add(new OracleParameter("status", record.Status));
                
                var idParam = new OracleParameter("id", OracleDbType.Decimal, System.Data.ParameterDirection.Output);
                command.Parameters.Add(idParam);
                
                command.ExecuteNonQuery();
                record.Id = Convert.ToInt32(((Oracle.ManagedDataAccess.Types.OracleDecimal)idParam.Value).Value);
            }

            // Update the task's last finished date
            UpdateTaskLastFinishedDate(record.TaskId, record.FinishedDate);

            return record;
        }

        public Record Update(Record record)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                @"UPDATE RECORDS SET 
                    TASK_ID = :taskId, 
                    FINISHED_DATE = :finishedDate, 
                    STATUS = :status 
                  WHERE ID = :id", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("taskId", record.TaskId));
                command.Parameters.Add(new OracleParameter("finishedDate", record.FinishedDate));
                command.Parameters.Add(new OracleParameter("status", record.Status));
                command.Parameters.Add(new OracleParameter("id", record.Id));
                
                command.ExecuteNonQuery();
            }
            return record;
        }

        public bool Delete(int id)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand("DELETE FROM RECORDS WHERE ID = :id", connection))
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

        public int? GetTaskOwner(int recordId)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                @"SELECT T.USER_ID 
                  FROM RECORDS R 
                  INNER JOIN TASKS T ON R.TASK_ID = T.ID 
                  WHERE R.ID = :recordId", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("recordId", recordId));
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

        private void UpdateTaskLastFinishedDate(int taskId, DateTime finishedDate)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                @"UPDATE TASKS SET LAST_FINISHED_DATE = :finishedDate, UPDATED_AT = CURRENT_TIMESTAMP 
                  WHERE ID = :taskId AND (LAST_FINISHED_DATE IS NULL OR LAST_FINISHED_DATE < :finishedDate)", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("finishedDate", finishedDate));
                command.Parameters.Add(new OracleParameter("taskId", taskId));
                command.ExecuteNonQuery();
            }
        }

        private Record MapRecord(OracleDataReader reader)
        {
            return new Record
            {
                Id = Convert.ToInt32(reader["ID"]),
                TaskId = Convert.ToInt32(reader["TASK_ID"]),
                FinishedDate = Convert.ToDateTime(reader["FINISHED_DATE"]),
                Status = reader["STATUS"].ToString()
            };
        }
    }
}
