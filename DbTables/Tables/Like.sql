CREATE TABLE [dbo].[Like] (
    LikeId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ProjectId INT NULL,                         -- Either a project or a comment
    CommentId INT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Like_User FOREIGN KEY (UserId)
        REFERENCES [dbo].[User](UserId) ON DELETE CASCADE,
    CONSTRAINT FK_Like_Project FOREIGN KEY (ProjectId)
        REFERENCES [dbo].[Project](ProjectId) ON DELETE CASCADE,
    CONSTRAINT FK_Like_Comment FOREIGN KEY (CommentId)
        REFERENCES [dbo].[Comment](CommentId) ON DELETE CASCADE,
    CONSTRAINT CK_Like_Target CHECK (ProjectId IS NOT NULL OR CommentId IS NOT NULL),
    CONSTRAINT UQ_Like UNIQUE (UserId, ProjectId, CommentId) -- Prevent duplicate likes
);
GO
