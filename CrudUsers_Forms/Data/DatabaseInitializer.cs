using Microsoft.Data.SqlClient;

namespace CrudUsers_Forms.Data;

internal static class DatabaseInitializer
{
    public static void EnsureStoredProcedures()
    {
        using var connection = Database.CreateConnection();
        connection.Open();

        EnsureStoredProcedure(connection, "spAuth_Login", @"
CREATE PROCEDURE dbo.spAuth_Login
    @Username NVARCHAR(50),
    @PasswordHash CHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (1)
           Id,
           Username,
           DisplayName,
           Email,
           Role,
           IsBlocked,
           CreatedAt,
           CreatedBy
    FROM dbo.Users
    WHERE Username = @Username
      AND PasswordHash = @PasswordHash;
END");
        EnsureStoredProcedure(connection, "spUsers_List", @"
CREATE PROCEDURE dbo.spUsers_List
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Username, DisplayName, Email, Role, IsBlocked, CreatedAt, CreatedBy
    FROM dbo.Users
    ORDER BY Username;
END");
    }

    private static void EnsureStoredProcedure(SqlConnection connection, string name, string createSql)
    {
        using var command = new SqlCommand("SELECT COUNT(*) FROM sys.procedures WHERE name = @name", connection);
        command.Parameters.AddWithValue("@name", name);
        var exists = (int)command.ExecuteScalar() > 0;
        if (exists)
        {
            return;
        }

        using var create = new SqlCommand(createSql, connection);
        create.ExecuteNonQuery();
    }
}
