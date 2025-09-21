CREATE TABLE [dbo].[User] (
    UserId INT IDENTITY(1,1) PRIMARY KEY,  -- Auto-incrementing unique user ID
    Username NVARCHAR(50) NOT NULL UNIQUE, -- Login username
    Email NVARCHAR(255) NOT NULL UNIQUE,   -- User email address
    PasswordHash NVARCHAR(255) NOT NULL,   -- Hashed password
    RoleId INT NOT NULL,                   -- FK to Role table
    CreatedAt DATETIME DEFAULT GETDATE(),  -- When the user registered
    UpdatedAt DATETIME NULL,               -- When profile last updated
    IsActive BIT DEFAULT 1,                -- Whether the account is active
    LastLogin DATETIME NULL,               -- Last login timestamp
    CONSTRAINT FK_User_Role FOREIGN KEY (RoleId)
        REFERENCES [dbo].[Role](RoleId) ON DELETE CASCADE
);
GO
