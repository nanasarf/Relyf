-- SQL Migration: Create AIIdeas Table
-- File: create_ai_ideas_table.sql
-- Purpose: Create AIIdeas table for storing AI-generated ideas
-- Safety: Idempotent - safe to run multiple times

-- Create AIIdeas table
IF NOT EXISTS (
  SELECT 1 FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'AIIdeas'
)
BEGIN
  CREATE TABLE app.AIIdeas
  (
    AiIdeaId INT PRIMARY KEY IDENTITY(1, 1),
    UserId INT NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    Tools NVARCHAR(MAX),
    Steps NVARCHAR(MAX),
    Safety NVARCHAR(MAX),
    CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_AIIdeas_User FOREIGN KEY (UserId) REFERENCES app.[User](UserId)
  );
  
  PRINT 'Created app.AIIdeas table';
  
  -- Create index on UserId for faster queries
  CREATE INDEX IX_AIIdeas_UserId ON app.AIIdeas(UserId, IsDeleted);
  PRINT 'Created index IX_AIIdeas_UserId';
END
ELSE
BEGIN
  PRINT 'app.AIIdeas table already exists';
END;

-- Add AiIdeaId to Projects table if it doesn't exist
IF NOT EXISTS (
  SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'Project' AND COLUMN_NAME = 'AiIdeaId'
)
BEGIN
  ALTER TABLE app.Project
  ADD AiIdeaId INT NULL;
  
  -- Add foreign key constraint
  ALTER TABLE app.Project
  ADD CONSTRAINT FK_Project_AIIdea FOREIGN KEY (AiIdeaId) REFERENCES app.AIIdeas(AiIdeaId);
  
  PRINT 'Added AiIdeaId column and foreign key to app.Project';
END
ELSE
BEGIN
  PRINT 'app.Project already has AiIdeaId column';
END;

PRINT '============================';
PRINT 'AIIdeas Migration Complete';
PRINT '============================';
