-- SQL Migration: Add Missing IsDeleted Columns
-- File: add_isdeleted_columns.sql
-- Purpose: Add IsDeleted columns to tables that reference them in code
-- Safety: Idempotent - safe to run multiple times

-- Add IsDeleted to app.AiIdea
IF NOT EXISTS (
  SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'AiIdea' AND COLUMN_NAME = 'IsDeleted'
)
BEGIN
  ALTER TABLE app.AiIdea
  ADD IsDeleted BIT NOT NULL CONSTRAINT DF_AiIdea_IsDeleted DEFAULT (0);
  PRINT 'Added IsDeleted to app.AiIdea';
END
ELSE
BEGIN
  PRINT 'app.AiIdea already has IsDeleted column';
END;

-- Add IsDeleted to app.Project
IF NOT EXISTS (
  SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'Project' AND COLUMN_NAME = 'IsDeleted'
)
BEGIN
  ALTER TABLE app.Project
  ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Project_IsDeleted DEFAULT (0);
  PRINT 'Added IsDeleted to app.Project';
END
ELSE
BEGIN
  PRINT 'app.Project already has IsDeleted column';
END;

PRINT '============================';
PRINT 'Migration Complete';
PRINT '============================';
