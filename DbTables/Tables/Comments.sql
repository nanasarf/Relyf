CREATE TABLE [dbo].[Comment] (
    CommentId INT IDENTITY(1,1) PRIMARY KEY,   -- Unique comment ID
    ProjectId INT NOT NULL,                    -- FK to Project table
    UserId INT NOT NULL,                        -- FK to User table (who made the comment)
    Content NVARCHAR(MAX) NOT NULL,             -- Comment text
    CreatedAt DATETIME DEFAULT GETDATE(),       -- When the comment was posted
    UpdatedAt DATETIME NULL,                    -- If the comment was edited
    ParentCommentId INT NULL,                   -- For threaded replies (optional)
    CONSTRAINT FK_Comment_Project FOREIGN KEY (ProjectId)
        REFERENCES [dbo].[Project](ProjectId) ON DELETE CASCADE,
    CONSTRAINT FK_Comment_User FOREIGN KEY (UserId)
        REFERENCES [dbo].[User](UserId) ON DELETE CASCADE,
    CONSTRAINT FK_Comment_Parent FOREIGN KEY (ParentCommentId)
        REFERENCES [dbo].[Comment](CommentId) ON DELETE CASCADE
);
GO
