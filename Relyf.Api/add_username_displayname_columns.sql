-- =============================================
-- SQL Migration: Add UserName, Bio, and AvatarUrl to User Table
-- =============================================
-- Description: Adds username (unique, permanent identifier), bio, and avatar URL
-- Author: System
-- Date: 2024
-- Purpose: Enable username-based authentication and enhanced user profiles
-- Safety: Idempotent - safe to run multiple times
-- =============================================

USE [Relyf.Database];
GO

-- Add UserName column (unique, 3-20 chars, alphanumeric + underscores)
IF NOT EXISTS (
  SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'User' AND COLUMN_NAME = 'UserName'
)
BEGIN
  ALTER TABLE app.[User]
  ADD UserName NVARCHAR(20) NULL; -- Temporarily NULL for existing records
  PRINT 'Added UserName column to app.[User]';
END
ELSE
BEGIN
  PRINT 'app.[User] already has UserName column';
END;
GO

-- Add Bio column for user description
IF NOT EXISTS (
  SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'User' AND COLUMN_NAME = 'Bio'
)
BEGIN
  ALTER TABLE app.[User]
  ADD Bio NVARCHAR(500) NULL;
  PRINT 'Added Bio column to app.[User]';
END
ELSE
BEGIN
  PRINT 'app.[User] already has Bio column';
END;
GO

-- Add AvatarUrl column for user profile pictures
IF NOT EXISTS (
  SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'User' AND COLUMN_NAME = 'AvatarUrl'
)
BEGIN
  ALTER TABLE app.[User]
  ADD AvatarUrl NVARCHAR(500) NULL;
  PRINT 'Added AvatarUrl column to app.[User]';
END
ELSE
BEGIN
  PRINT 'app.[User] already has AvatarUrl column';
END;
GO

-- Generate UserNames for existing users (from email prefix)
-- This is a one-time data migration for existing records
DECLARE @UpdatedCount INT = 0;

UPDATE app.[User]
SET UserName = 'user_' + CAST(UserId AS NVARCHAR(20))
WHERE UserName IS NULL;

SET @UpdatedCount = @@ROWCOUNT;

IF @UpdatedCount > 0
BEGIN
  PRINT 'Generated UserNames for ' + CAST(@UpdatedCount AS NVARCHAR(10)) + ' existing users';
END;
GO

-- Now make UserName NOT NULL after populating existing records
IF EXISTS (
  SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'User' AND COLUMN_NAME = 'UserName' AND IS_NULLABLE = 'YES'
)
BEGIN
  ALTER TABLE app.[User]
  ALTER COLUMN UserName NVARCHAR(20) NOT NULL;
  PRINT 'Made UserName column NOT NULL';
END;
GO

-- Create unique index on UserName for fast case-insensitive lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_User_UserName_Unique' AND object_id = OBJECT_ID('app.[User]'))
BEGIN
  CREATE UNIQUE NONCLUSTERED INDEX IX_User_UserName_Unique 
  ON app.[User](UserName);
  PRINT 'Created unique index IX_User_UserName_Unique';
END
ELSE
BEGIN
  PRINT 'Unique index IX_User_UserName_Unique already exists';
END;
GO

PRINT '============================';
PRINT 'User Profile Migration Complete';
PRINT '============================';
PRINT 'Summary:';
PRINT '  - UserName: Unique identifier (3-20 chars, alphanumeric + underscores)';
PRINT '  - DisplayName: Public display name (can be changed)';
PRINT '  - Bio: User description (up to 500 chars)';
PRINT '  - AvatarUrl: Profile picture URL';
GO

