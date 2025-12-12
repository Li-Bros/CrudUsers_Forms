/*
    Script de base de datos para UsersCRUDNET.
    Ejecutar en SQL Server Management Studio antes de correr la app.
*/
IF DB_ID(N'UsersCRUDNET') IS NULL
BEGIN
    EXEC ('CREATE DATABASE UsersCRUDNET');
END
GO

USE UsersCRUDNET;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        DisplayName NVARCHAR(120) NOT NULL,
        Email NVARCHAR(120) NULL,
        PasswordHash CHAR(64) NOT NULL,
        Role INT NOT NULL,
        IsBlocked BIT NOT NULL DEFAULT(0),
        CreatedBy INT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        LastLogin DATETIME2 NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = N'superadmin')
BEGIN
    INSERT INTO dbo.Users (Username, DisplayName, Email, PasswordHash, Role, IsBlocked, CreatedBy)
    VALUES (N'superadmin', N'Super Administrador', N'superadmin@example.com',
            N'4b3d4b1f6eae92e5b017a753e7eee2072ecf1c5c09b4c4a61c59f8cfcf65ead5',
            0, 0, NULL);
END
GO

IF OBJECT_ID(N'dbo.spAuth_Login', N'P') IS NOT NULL
    DROP PROCEDURE dbo.spAuth_Login;
GO
CREATE PROCEDURE dbo.spAuth_Login
    @Username NVARCHAR(50),
    @PasswordHash CHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (1) Id, Username, DisplayName, Email, Role, IsBlocked, CreatedAt, CreatedBy
    FROM dbo.Users
    WHERE Username = @Username AND PasswordHash = @PasswordHash;
END
GO

IF OBJECT_ID(N'dbo.spUsers_List', N'P') IS NOT NULL
    DROP PROCEDURE dbo.spUsers_List;
GO
CREATE PROCEDURE dbo.spUsers_List
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Username, DisplayName, Email, Role, IsBlocked, CreatedAt, CreatedBy
    FROM dbo.Users
    ORDER BY Username;
END
GO

IF OBJECT_ID(N'dbo.spUsers_Insert', N'P') IS NOT NULL
    DROP PROCEDURE dbo.spUsers_Insert;
GO
CREATE PROCEDURE dbo.spUsers_Insert
    @Username NVARCHAR(50),
    @DisplayName NVARCHAR(120),
    @Email NVARCHAR(120),
    @PasswordHash CHAR(64),
    @Role INT,
    @CreatedBy INT = NULL,
    @NewId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Username = @Username)
    BEGIN
        RAISERROR('El usuario ya existe.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.Users (Username, DisplayName, Email, PasswordHash, Role, CreatedBy)
    VALUES (@Username, @DisplayName, @Email, @PasswordHash, @Role, @CreatedBy);

    SET @NewId = SCOPE_IDENTITY();
END
GO

IF OBJECT_ID(N'dbo.spUsers_Update', N'P') IS NOT NULL
    DROP PROCEDURE dbo.spUsers_Update;
GO
CREATE PROCEDURE dbo.spUsers_Update
    @Id INT,
    @DisplayName NVARCHAR(120),
    @Email NVARCHAR(120),
    @Role INT,
    @PasswordHash CHAR(64) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Users
    SET DisplayName = @DisplayName,
        Email = @Email,
        Role = @Role,
        PasswordHash = COALESCE(@PasswordHash, PasswordHash)
    WHERE Id = @Id;
END
GO

IF OBJECT_ID(N'dbo.spUsers_Delete', N'P') IS NOT NULL
    DROP PROCEDURE dbo.spUsers_Delete;
GO
CREATE PROCEDURE dbo.spUsers_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Users WHERE Id = @Id;
END
GO

IF OBJECT_ID(N'dbo.spUsers_SetBlocked', N'P') IS NOT NULL
    DROP PROCEDURE dbo.spUsers_SetBlocked;
GO
CREATE PROCEDURE dbo.spUsers_SetBlocked
    @Id INT,
    @IsBlocked BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Users SET IsBlocked = @IsBlocked WHERE Id = @Id;
END
GO

IF OBJECT_ID(N'dbo.spUsers_UpdateLastLogin', N'P') IS NOT NULL
    DROP PROCEDURE dbo.spUsers_UpdateLastLogin;
GO
CREATE PROCEDURE dbo.spUsers_UpdateLastLogin
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Users SET LastLogin = SYSUTCDATETIME() WHERE Id = @Id;
END
GO
