using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using Wayfinder.API.Models;

namespace Wayfinder.API.Services
{
    /// <summary>
    /// Repository for User entity operations
    /// </summary>
    public class UserRepository
    {
        private readonly OracleDbContext _context;

        public UserRepository(OracleDbContext context)
        {
            _context = context;
        }

        public User GetById(int id)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                "SELECT ID, EMAIL, PASSWORD_HASH, COLOR1, COLOR2, COLOR3, CREATED_AT, UPDATED_AT FROM USERS WHERE ID = :id", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("id", id));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapUser(reader);
                    }
                }
            }
            return null;
        }

        public User GetByEmail(string email)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                "SELECT ID, EMAIL, PASSWORD_HASH, COLOR1, COLOR2, COLOR3, CREATED_AT, UPDATED_AT FROM USERS WHERE LOWER(EMAIL) = LOWER(:email)", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("email", email));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapUser(reader);
                    }
                }
            }
            return null;
        }

        public IEnumerable<User> GetAll()
        {
            var users = new List<User>();
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                "SELECT ID, EMAIL, PASSWORD_HASH, COLOR1, COLOR2, COLOR3, CREATED_AT, UPDATED_AT FROM USERS ORDER BY ID", 
                connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(MapUser(reader));
                    }
                }
            }
            return users;
        }

        public User Create(User user)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                @"INSERT INTO USERS (EMAIL, PASSWORD_HASH, COLOR1, COLOR2, COLOR3, CREATED_AT) 
                  VALUES (:email, :passwordHash, :color1, :color2, :color3, CURRENT_TIMESTAMP)
                  RETURNING ID INTO :id", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("email", user.Email));
                command.Parameters.Add(new OracleParameter("passwordHash", user.PasswordHash));
                command.Parameters.Add(new OracleParameter("color1", (object)user.Color1 ?? DBNull.Value));
                command.Parameters.Add(new OracleParameter("color2", (object)user.Color2 ?? DBNull.Value));
                command.Parameters.Add(new OracleParameter("color3", (object)user.Color3 ?? DBNull.Value));
                
                var idParam = new OracleParameter("id", OracleDbType.Decimal, System.Data.ParameterDirection.Output);
                command.Parameters.Add(idParam);
                
                command.ExecuteNonQuery();
                user.Id = Convert.ToInt32(((Oracle.ManagedDataAccess.Types.OracleDecimal)idParam.Value).Value);
                user.CreatedAt = DateTime.UtcNow;
            }
            return user;
        }

        public User Update(User user)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand(
                @"UPDATE USERS SET 
                    EMAIL = :email, 
                    PASSWORD_HASH = :passwordHash, 
                    COLOR1 = :color1, 
                    COLOR2 = :color2, 
                    COLOR3 = :color3, 
                    UPDATED_AT = CURRENT_TIMESTAMP 
                  WHERE ID = :id", 
                connection))
            {
                command.Parameters.Add(new OracleParameter("email", user.Email));
                command.Parameters.Add(new OracleParameter("passwordHash", user.PasswordHash));
                command.Parameters.Add(new OracleParameter("color1", (object)user.Color1 ?? DBNull.Value));
                command.Parameters.Add(new OracleParameter("color2", (object)user.Color2 ?? DBNull.Value));
                command.Parameters.Add(new OracleParameter("color3", (object)user.Color3 ?? DBNull.Value));
                command.Parameters.Add(new OracleParameter("id", user.Id));
                
                command.ExecuteNonQuery();
                user.UpdatedAt = DateTime.UtcNow;
            }
            return user;
        }

        public bool Delete(int id)
        {
            var connection = _context.GetConnection();
            using (var command = new OracleCommand("DELETE FROM USERS WHERE ID = :id", connection))
            {
                command.Parameters.Add(new OracleParameter("id", id));
                return command.ExecuteNonQuery() > 0;
            }
        }

        public bool EmailExists(string email, int? excludeId = null)
        {
            var connection = _context.GetConnection();
            var sql = excludeId.HasValue
                ? "SELECT COUNT(*) FROM USERS WHERE LOWER(EMAIL) = LOWER(:email) AND ID != :excludeId"
                : "SELECT COUNT(*) FROM USERS WHERE LOWER(EMAIL) = LOWER(:email)";
            
            using (var command = new OracleCommand(sql, connection))
            {
                command.Parameters.Add(new OracleParameter("email", email));
                if (excludeId.HasValue)
                {
                    command.Parameters.Add(new OracleParameter("excludeId", excludeId.Value));
                }
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private User MapUser(OracleDataReader reader)
        {
            return new User
            {
                Id = Convert.ToInt32(reader["ID"]),
                Email = reader["EMAIL"].ToString(),
                PasswordHash = reader["PASSWORD_HASH"].ToString(),
                Color1 = reader["COLOR1"] == DBNull.Value ? null : reader["COLOR1"].ToString(),
                Color2 = reader["COLOR2"] == DBNull.Value ? null : reader["COLOR2"].ToString(),
                Color3 = reader["COLOR3"] == DBNull.Value ? null : reader["COLOR3"].ToString(),
                CreatedAt = reader["CREATED_AT"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["CREATED_AT"]),
                UpdatedAt = reader["UPDATED_AT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["UPDATED_AT"])
            };
        }
    }
}
