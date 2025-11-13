-- ========================================
-- DELETE ALL PROJECTS AND RELATED DATA
-- ========================================
-- Purpose: Remove all projects and their related data for testing cleanup
-- Safety: Uses transactions - will rollback if any errors occur
-- 
-- WARNING: This will delete ALL project data!
-- Make sure you have a backup before running!
--
-- Usage:
-- 1. Review the script
-- 2. Uncomment the COMMIT at the bottom when ready
-- 3. Execute the script
-- ========================================

BEGIN TRANSACTION;

PRINT '========================================';
PRINT 'Starting Project Data Deletion';
PRINT '========================================';
PRINT '';

-- ========================================
-- Step 1: Delete Project Steps
-- ========================================
PRINT 'Step 1: Deleting Project Steps...';
DECLARE @stepCount INT;
SELECT @stepCount = COUNT(*) FROM app.ProjectStep;
PRINT '  - Found ' + CAST(@stepCount AS NVARCHAR(10)) + ' project steps';

DELETE FROM app.ProjectStep;

PRINT '  - Deleted all project steps';
PRINT '';

-- ========================================
-- Step 2: Delete Project Materials
-- ========================================
PRINT 'Step 2: Deleting Project Materials...';
DECLARE @projectMaterialCount INT;
SELECT @projectMaterialCount = COUNT(*) FROM app.ProjectMaterial;
PRINT '  - Found ' + CAST(@projectMaterialCount AS NVARCHAR(10)) + ' project materials';

DELETE FROM app.ProjectMaterial;

PRINT '  - Deleted all project materials';
PRINT '';

-- ========================================
-- Step 3: Delete Reactions (Likes) on Projects
-- ========================================
PRINT 'Step 3: Deleting Project Reactions (Likes)...';
DECLARE @reactionCount INT;
SELECT @reactionCount = COUNT(*) 
FROM app.Reaction 
WHERE TargetType = 'Project';

PRINT '  - Found ' + CAST(@reactionCount AS NVARCHAR(10)) + ' project reactions';

DELETE FROM app.Reaction 
WHERE TargetType = 'Project';

PRINT '  - Deleted all project reactions';
PRINT '';

-- ========================================
-- Step 4: Delete Comments on Projects
-- ========================================
PRINT 'Step 4: Deleting Project Comments...';
DECLARE @commentCount INT;
SELECT @commentCount = COUNT(*) 
FROM app.Comment 
WHERE TargetType = 'Project';

PRINT '  - Found ' + CAST(@commentCount AS NVARCHAR(10)) + ' project comments';

DELETE FROM app.Comment 
WHERE TargetType = 'Project';

PRINT '  - Deleted all project comments';
PRINT '';

-- ========================================
-- Step 5: Delete Images Associated with Projects
-- ========================================
PRINT 'Step 5: Deleting Project Images...';
DECLARE @imageCount INT;
SELECT @imageCount = COUNT(*) 
FROM app.Image 
WHERE OwnerType = 'Project';

PRINT '  - Found ' + CAST(@imageCount AS NVARCHAR(10)) + ' project images';

DELETE FROM app.Image 
WHERE OwnerType = 'Project';

PRINT '  - Deleted all project images';
PRINT '';

-- ========================================
-- Step 6: Delete Projects
-- ========================================
PRINT 'Step 6: Deleting Projects...';
DECLARE @projectCount INT;
SELECT @projectCount = COUNT(*) FROM app.Project;
PRINT '  - Found ' + CAST(@projectCount AS NVARCHAR(10)) + ' projects';

DELETE FROM app.Project;

PRINT '  - Deleted all projects';
PRINT '';

-- ========================================
-- Verification
-- ========================================
PRINT '========================================';
PRINT 'Verification - Remaining Records:';
PRINT '========================================';
PRINT 'Projects: ' + CAST((SELECT COUNT(*) FROM app.Project) AS NVARCHAR(10));
PRINT 'Project Steps: ' + CAST((SELECT COUNT(*) FROM app.ProjectStep) AS NVARCHAR(10));
PRINT 'Project Materials: ' + CAST((SELECT COUNT(*) FROM app.ProjectMaterial) AS NVARCHAR(10));
PRINT 'Project Reactions: ' + CAST((SELECT COUNT(*) FROM app.Reaction WHERE TargetType = ''Project'') AS NVARCHAR(10));
PRINT 'Project Comments: ' + CAST((SELECT COUNT(*) FROM app.Comment WHERE TargetType = ''Project'') AS NVARCHAR(10));
PRINT 'Project Images: ' + CAST((SELECT COUNT(*) FROM app.Image WHERE OwnerType = ''Project'') AS NVARCHAR(10));
PRINT '';

-- ========================================
-- Summary
-- ========================================
PRINT '========================================';
PRINT 'Summary:';
PRINT '========================================';
PRINT 'Deleted ' + CAST(@projectCount AS NVARCHAR(10)) + ' projects';
PRINT 'Deleted ' + CAST(@stepCount AS NVARCHAR(10)) + ' project steps';
PRINT 'Deleted ' + CAST(@projectMaterialCount AS NVARCHAR(10)) + ' project materials';
PRINT 'Deleted ' + CAST(@reactionCount AS NVARCHAR(10)) + ' reactions';
PRINT 'Deleted ' + CAST(@commentCount AS NVARCHAR(10)) + ' comments';
PRINT 'Deleted ' + CAST(@imageCount AS NVARCHAR(10)) + ' images';
PRINT '';
PRINT 'Transaction is ready to commit';
PRINT 'Review the verification above';
PRINT 'Uncomment COMMIT to apply changes';
PRINT '';

-- ========================================
-- COMMIT or ROLLBACK
-- ========================================
-- Uncomment ONE of the following lines:

-- COMMIT;   -- Uncomment this to SAVE changes
ROLLBACK;    -- This will UNDO all changes (default safe option)

PRINT '';
PRINT '========================================';
PRINT 'Transaction Complete';
PRINT '========================================';
