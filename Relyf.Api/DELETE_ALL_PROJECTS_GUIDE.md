# ??? Delete All Projects - Quick Guide

## ? Quick Start

### Step 1: Open the SQL Script
Open `delete_all_projects.sql` in **SQL Server Management Studio (SSMS)** or **Azure Data Studio**

### Step 2: Execute (Safe Mode)
Run the script as-is. It will show what would be deleted but **won't actually delete** anything (uses ROLLBACK by default).

### Step 3: Review Output
Check the messages to see:
- How many projects will be deleted
- How many related records will be deleted
- Verification that counts = 0 after deletion

### Step 4: Commit Changes
If you're happy with the preview:
1. Find this line in the script:
   ```sql
   -- COMMIT;   -- Uncomment this to SAVE changes
   ```
2. Remove the `--` to make it:
   ```sql
   COMMIT;   -- Uncomment this to SAVE changes
   ```
3. Comment out the ROLLBACK line:
   ```sql
   -- ROLLBACK;    -- This will UNDO all changes (default safe option)
   ```
4. Run the script again

---

## ?? What Gets Deleted

This script deletes **ALL projects** and their related data:

? **Projects** - All records in `app.Project`  
? **Project Steps** - All records in `app.ProjectStep`  
? **Project Materials** - All records in `app.ProjectMaterial`  
? **Reactions** - Only reactions on projects (`SourceType = 'project'`)  
? **Comments** - Only comments on projects (`SourceType = 'project'`)  
? **Images** - Only images for projects (`EntityType = 'project'`)  

---

## ?? What's NOT Deleted

These stay intact:

? Users  
? Items  
? Materials  
? Tags  
? AI Ideas  
? Saved Ideas  
? Reactions/Comments on non-projects  
? Table structure  

---

## ?? Safety Features

### 1. Transaction-Based
Uses `BEGIN TRANSACTION` / `ROLLBACK` - if anything fails, nothing is deleted.

### 2. Default is Rollback
By default, the script does **NOT** delete anything - you must uncomment `COMMIT`.

### 3. Detailed Logging
Shows exactly what will be deleted before you commit:
```
Found 15 projects
Found 45 project steps
Found 30 project materials
...
Verification - Remaining Records:
Projects: 0
Project Steps: 0
...
```

### 4. Verification Step
After deletion (but before commit), you can see the results and decide to commit or rollback.

---

## ?? Example Output

```
========================================
Starting Project Data Deletion
========================================

Step 1: Deleting Project Steps...
  - Found 45 project steps
  - Deleted all project steps

Step 2: Deleting Project Materials...
  - Found 30 project materials
  - Deleted all project materials

Step 3: Deleting Project Reactions (Likes)...
  - Found 8 project reactions
  - Deleted all project reactions

Step 4: Deleting Project Comments...
  - Found 12 project comments
  - Deleted all project comments

Step 5: Deleting Project Images...
  - Found 15 project images
  - Deleted all project images

Step 6: Deleting Projects...
  - Found 15 projects
  - Deleted all projects

========================================
Verification - Remaining Records:
========================================
Projects: 0
Project Steps: 0
Project Materials: 0
Project Reactions: 0
Project Comments: 0
Project Images: 0

========================================
Summary:
========================================
Deleted 15 projects
Deleted 45 project steps
Deleted 30 project materials
Deleted 8 reactions
Deleted 12 comments
Deleted 15 images

Transaction is ready to commit
Review the verification above
Uncomment COMMIT to apply changes
```

---

## ? Alternative: Quick Delete (No Preview)

If you want to delete immediately without preview:

```sql
-- Quick delete - No transaction, immediate deletion
DELETE FROM app.ProjectStep;
DELETE FROM app.ProjectMaterial;
DELETE FROM app.Reaction WHERE TargetType = 'Project';
DELETE FROM app.Comment WHERE TargetType = 'Project';
DELETE FROM app.Image WHERE OwnerType = 'Project';
DELETE FROM app.Project;

-- Verify
SELECT 'Projects' AS Table_Name, COUNT(*) AS Count FROM app.Project
UNION ALL SELECT 'Steps', COUNT(*) FROM app.ProjectStep
UNION ALL SELECT 'Materials', COUNT(*) FROM app.ProjectMaterial;
```

?? **Warning:** This deletes immediately with no undo!

**Or use the quick script:** `quick_delete_all_projects.sql`

---

## ?? Pre-Delete Verification

Before running the script, check what you have:

```sql
-- Check project counts by user
SELECT 
    u.UserId,
    u.Username,
    COUNT(p.ProjectId) AS ProjectCount
FROM app.[User] u
LEFT JOIN app.Project p ON p.UserId = u.UserId
GROUP BY u.UserId, u.Username
ORDER BY ProjectCount DESC;

-- Check project details
SELECT 
    ProjectId,
    UserId,
    Title,
    Status,
    CreatedAtUtc,
    IsDeleted
FROM app.Project
ORDER BY CreatedAtUtc DESC;
```

---

## ?? Common Use Cases

### Use Case 1: Clean Up Test Data
You created test projects for multiple users and want to reset:
```sql
-- Run delete_all_projects.sql
-- All test projects deleted
-- Database ready for fresh testing
```

### Use Case 2: Start Fresh After Development
You've been testing the projects feature and want to clear everything:
```sql
-- Run delete_all_projects.sql
-- All development data cleared
-- Ready for production data
```

### Use Case 3: Delete Specific User's Projects Only
If you only want to delete projects for specific users:
```sql
-- Delete projects for user 4 and 5 only
BEGIN TRANSACTION;

DELETE FROM app.ProjectStep WHERE ProjectId IN (
    SELECT ProjectId FROM app.Project WHERE UserId IN (4, 5)
);
DELETE FROM app.ProjectMaterial WHERE ProjectId IN (
    SELECT ProjectId FROM app.Project WHERE UserId IN (4, 5)
);
DELETE FROM app.Reaction WHERE TargetType = 'Project' AND TargetId IN (
    SELECT ProjectId FROM app.Project WHERE UserId IN (4, 5)
);
DELETE FROM app.Comment WHERE TargetType = 'Project' AND TargetId IN (
    SELECT ProjectId FROM app.Project WHERE UserId IN (4, 5)
);
DELETE FROM app.Image WHERE OwnerType = 'Project' AND OwnerId IN (
    SELECT ProjectId FROM app.Project WHERE UserId IN (4, 5)
);
DELETE FROM app.Project WHERE UserId IN (4, 5);

-- Uncomment to commit:
-- COMMIT;
ROLLBACK;
```

---

## ??? Troubleshooting

### Error: Foreign Key Constraint Violation
**Cause:** Deleting in wrong order  
**Solution:** Use the provided script - it deletes in the correct order

### Error: Cannot Truncate Referenced Table
**Cause:** Trying to use `TRUNCATE TABLE`  
**Solution:** Use `DELETE FROM` instead (as in the script)

### Projects Not Deleting
**Check:** Are there other tables referencing Project?
```sql
-- Find all foreign keys to Project table
SELECT 
    OBJECT_NAME(f.parent_object_id) AS ReferencingTable,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ReferencingColumn
FROM sys.foreign_keys AS f
INNER JOIN sys.foreign_key_columns AS fc 
    ON f.object_id = fc.constraint_object_id
WHERE OBJECT_NAME(f.referenced_object_id) = 'Project';
```

---

## ? Checklist

Before running:
- [ ] I understand ALL projects will be deleted
- [ ] I have a backup (or don't need one)
- [ ] I'm connected to the correct database
- [ ] I've reviewed the script

When running:
- [ ] Execute with default ROLLBACK first
- [ ] Review the output messages
- [ ] Check verification counts
- [ ] Uncomment COMMIT if satisfied
- [ ] Execute again to apply changes

After running:
- [ ] Verify all counts are 0
- [ ] Test the application
- [ ] Projects functionality still works (with empty data)

---

## ?? Files

- **`delete_all_projects.sql`** - Main deletion script (safe, with transaction)
- **`DELETE_ALL_PROJECTS_GUIDE.md`** - This guide

---

## ?? Summary

**What it does:**  
Deletes ALL projects and related data (steps, materials, reactions, comments, images)

**Safety:**  
Uses transactions, defaults to ROLLBACK, shows detailed preview

**How to use:**  
1. Run script (previews deletion)  
2. Review output  
3. Uncomment COMMIT and run again  

**Result:**  
Clean database with no projects, ready for fresh data

---

**Created:** 2025-01-13  
**Purpose:** Clean up test project data  
**Status:** ? Ready to use
