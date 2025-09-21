CREATE TABLE [dbo].[LoginHistory] (
    LoginId INT IDENTITY(1,1) PRIMARY KEY,  -- Unique login event ID
    UserId INT NOT NULL,                    -- FK to User table
    LoginTime DATETIME DEFAULT GETDATE(),   -- When the login happened
    IPAddress NVARCHAR(45) NULL,            -- User IP (supports IPv6)
    DeviceInfo NVARCHAR(255) NULL,          -- Browser / device details
    WasSuccessful BIT DEFAULT 1,            -- Whether login succeeded (1) or failed (0)
    CONSTRAINT FK_LoginHistory_User FOREIGN KEY (UserId)
        REFERENCES [dbo].[User](UserId) ON DELETE CASCADE
);
GO
