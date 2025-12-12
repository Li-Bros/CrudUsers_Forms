using CrudUsers_Forms.Models;
using CrudUsers_Forms.Security;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CrudUsers_Forms.Data;

public class UserRepository
{
    public UserRecord? Authenticate(string username, string password)
    {
        using var connection = Database.CreateConnection();
        using var command = new SqlCommand("[UsersCRUDNET].dbo.spAuth_Login", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@Username", username);
        command.Parameters.AddWithValue("@PasswordHash", PasswordHelper.ComputeHash(password));

        connection.Open();
        using var reader = command.ExecuteReader();
        return reader.Read() ? MapUser(reader) : null;
    }

    public void UpdateLastLogin(int userId)
    {
        using var connection = Database.CreateConnection();
        using var command = new SqlCommand("spUsers_UpdateLastLogin", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@Id", userId);
        connection.Open();
        command.ExecuteNonQuery();
    }

    public List<UserRecord> GetUsers()
    {
        var users = new List<UserRecord>();
        using var connection = Database.CreateConnection();
        using var command = new SqlCommand("spUsers_List", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            users.Add(MapUser(reader));
        }
        return users;
    }

    public int CreateUser(UserRecord user, string password, int? createdBy)
    {
        using var connection = Database.CreateConnection();
        using var command = new SqlCommand("spUsers_Insert", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@Username", user.Username);
        command.Parameters.AddWithValue("@DisplayName", user.DisplayName);
        command.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(user.Email) ? DBNull.Value : user.Email);
        command.Parameters.AddWithValue("@PasswordHash", PasswordHelper.ComputeHash(password));
        command.Parameters.AddWithValue("@Role", (int)user.Role);
        command.Parameters.AddWithValue("@CreatedBy", createdBy.HasValue ? createdBy.Value : DBNull.Value);
        var output = command.Parameters.Add("@NewId", SqlDbType.Int);
        output.Direction = ParameterDirection.Output;

        connection.Open();
        command.ExecuteNonQuery();
        return Convert.ToInt32(output.Value);
    }

    public void UpdateUser(UserRecord user, string? newPassword)
    {
        using var connection = Database.CreateConnection();
        using var command = new SqlCommand("spUsers_Update", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@Id", user.Id);
        command.Parameters.AddWithValue("@DisplayName", user.DisplayName);
        command.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(user.Email) ? DBNull.Value : user.Email);
        command.Parameters.AddWithValue("@Role", (int)user.Role);
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            command.Parameters.AddWithValue("@PasswordHash", DBNull.Value);
        }
        else
        {
            command.Parameters.AddWithValue("@PasswordHash", PasswordHelper.ComputeHash(newPassword));
        }

        connection.Open();
        command.ExecuteNonQuery();
    }

    public void DeleteUser(int id)
    {
        using var connection = Database.CreateConnection();
        using var command = new SqlCommand("spUsers_Delete", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@Id", id);
        connection.Open();
        command.ExecuteNonQuery();
    }

    public void SetBlocked(int id, bool isBlocked)
    {
        using var connection = Database.CreateConnection();
        using var command = new SqlCommand("spUsers_SetBlocked", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@IsBlocked", isBlocked);
        connection.Open();
        command.ExecuteNonQuery();
    }

    private static UserRecord MapUser(SqlDataReader reader)
    {
        var idOrdinal = reader.GetOrdinal("Id");
        var usernameOrdinal = reader.GetOrdinal("Username");
        var displayNameOrdinal = reader.GetOrdinal("DisplayName");
        var emailOrdinal = reader.GetOrdinal("Email");
        var roleOrdinal = reader.GetOrdinal("Role");
        var blockedOrdinal = reader.GetOrdinal("IsBlocked");
        var createdAtOrdinal = reader.GetOrdinal("CreatedAt");
        var createdByOrdinal = reader.GetOrdinal("CreatedBy");

        return new UserRecord
        {
            Id = reader.GetInt32(idOrdinal),
            Username = reader.GetString(usernameOrdinal),
            DisplayName = reader.GetString(displayNameOrdinal),
            Email = reader.IsDBNull(emailOrdinal) ? null : reader.GetString(emailOrdinal),
            Role = (UserRole)reader.GetInt32(roleOrdinal),
            IsBlocked = reader.GetBoolean(blockedOrdinal),
            CreatedAt = reader.IsDBNull(createdAtOrdinal) ? null : reader.GetDateTime(createdAtOrdinal),
            CreatedBy = reader.IsDBNull(createdByOrdinal) ? null : reader.GetInt32(createdByOrdinal)
        };
    }
}
