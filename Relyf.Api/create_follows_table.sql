-- =============================================
-- Create Follows Table
-- =============================================
-- Description: Creates the follow/follower relationship table
-- Author: System
-- Date: 2024
-- =============================================

USE [Relyf.Database];
GO

-- Create Follows table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Follow' AND schema_id = SCHEMA_ID('app'))
BEGIN
    CREATE TABLE app.[Follow] (
        FollowId INT PRIMARY KEY IDENTITY(1,1),
        FollowerId INT NOT NULL,
        FollowingId INT NOT NULL,
        CreatedAtUtc DATETIME NOT NULL DEFAULT (SYSUTCDATETIME()),
        
        -- Foreign keys
        CONSTRAINT FK_Follow_Follower FOREIGN KEY (FollowerId) REFERENCES app.[User](UserId),
        CONSTRAINT FK_Follow_Following FOREIGN KEY (FollowingId) REFERENCES app.[User](UserId),
        
        -- Unique constraint to prevent duplicate follows
        CONSTRAINT UQ_Follow_Follower_Following UNIQUE(FollowerId, FollowingId),
        
        -- Check constraint to prevent self-following
        CONSTRAINT CK_Follow_NoSelfFollow CHECK (FollowerId != FollowingId)
    );

    PRINT 'Table app.[Follow] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table app.[Follow] already exists.';
END
GO

-- Create indexes for better query performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Follow_FollowerId' AND object_id = OBJECT_ID('app.[Follow]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Follow_FollowerId 
    ON app.[Follow](FollowerId) 
    INCLUDE (FollowingId, CreatedAtUtc);
    
    PRINT 'Index IX_Follow_FollowerId created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Follow_FollowingId' AND object_id = OBJECT_ID('app.[Follow]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Follow_FollowingId 
    ON app.[Follow](FollowingId) 
    INCLUDE (FollowerId, CreatedAtUtc);
    
    PRINT 'Index IX_Follow_FollowingId created successfully.';
END
GO

PRINT 'Follow table and indexes setup complete.';
GO
