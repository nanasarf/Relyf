CREATE TABLE [dbo].[ActivityLog] (
    ActivityId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Action NVARCHAR(50) NOT NULL,               -- e.g., 'Created Project', 'Liked Project', 'Commented'
    TargetId INT NULL,                          -- The project/comment/item affected
    TargetType NVARCHAR(50) NULL,               -- 'Project', 'Comment', 'MarketplaceItem'
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_ActivityLog_User FOREIGN KEY (UserId)
        REFERENCES [dbo].[User](UserId) ON DELETE CASCADE
);
GO
