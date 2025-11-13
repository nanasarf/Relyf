-- SQL Migration: Make AiIdea.ItemId Nullable
-- File: make_aiidea_itemid_nullable.sql
-- Purpose: Allow ideas to be created without an associated item
-- Issue: FK constraint violation when ItemId is NULL

USE [Relyf.Database];
GO

-- Check current column definition
SELECT 
  TABLE_NAME,
  COLUMN_NAME,
  IS_NULLABLE,
  DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'AiIdea' AND COLUMN_NAME = 'ItemId';

-- Make ItemId nullable
IF EXISTS (
  SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'AiIdea' 
    AND COLUMN_NAME = 'ItemId' AND IS_NULLABLE = 'NO'
)
BEGIN
  ALTER TABLE app.AiIdea
  ALTER COLUMN ItemId INT NULL;
  PRINT 'SUCCESS: Made app.AiIdea.ItemId nullable';
END
ELSE
BEGIN
  PRINT 'INFO: app.AiIdea.ItemId is already nullable or column does not exist';
END;

GO
PRINT '====================================';
PRINT 'Migration Complete - ItemId Nullable';
PRINT '====================================';
