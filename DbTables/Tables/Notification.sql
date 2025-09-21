CREATE TABLE [dbo].[Notification] (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,                        -- Recipient of the notification
    Type NVARCHAR(50) NOT NULL,                 -- e.g., 'Comment', 'Follow', 'Sale'
    Message NVARCHAR(255) NOT NULL,             -- Short notification text
    IsRead BIT DEFAULT 0,                       -- Mark as read/unread
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Notification_User FOREIGN KEY (UserId)
        REFERENCES [dbo].[User](UserId) ON DELETE CASCADE
);
GO
