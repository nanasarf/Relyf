CREATE TABLE [dbo].[UserFollow] (
    FollowId INT IDENTITY(1,1) PRIMARY KEY,  -- Unique follow record ID
    FollowerId INT NOT NULL,                 -- The user who follows
    FollowingId INT NOT NULL,                -- The user being followed
    CreatedAt DATETIME DEFAULT GETDATE(),    -- When the follow happened
    CONSTRAINT FK_UserFollow_Follower FOREIGN KEY (FollowerId)
        REFERENCES [dbo].[User](UserId) ON DELETE CASCADE,
    CONSTRAINT FK_UserFollow_Following FOREIGN KEY (FollowingId)
        REFERENCES [dbo].[User](UserId) ON DELETE CASCADE,
    CONSTRAINT UQ_UserFollow UNIQUE (FollowerId, FollowingId) -- Prevent duplicate follows
);
GO
