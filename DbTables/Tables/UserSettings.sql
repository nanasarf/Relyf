CREATE TABLE [dbo].[UserSettings] (
    SettingId INT IDENTITY(1,1) PRIMARY KEY,      -- Unique setting record
    UserId INT NOT NULL,                          -- FK to User
    IsProfilePublic BIT DEFAULT 1,                -- Public (1) or Private (0) profile
    ReceiveEmailNotifications BIT DEFAULT 1,      -- Email notifications on/off
    ReceiveAppNotifications BIT DEFAULT 1,        -- In-app notifications on/off
    Theme NVARCHAR(20) DEFAULT 'Light',           -- Light or Dark mode
    Language NVARCHAR(10) DEFAULT 'en',           -- Language preference (e.g., en, fr, es)
    CreatedAt DATETIME DEFAULT GETDATE(),         -- When settings were created
    UpdatedAt DATETIME NULL,                      -- When last updated
    CONSTRAINT FK_UserSettings_User FOREIGN KEY (UserId)
        REFERENCES [dbo].[User](UserId) ON DELETE CASCADE,
    CONSTRAINT UQ_UserSettings UNIQUE (UserId)    -- Ensure one settings row per user
);
GO
