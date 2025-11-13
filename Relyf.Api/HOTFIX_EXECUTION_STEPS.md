# ?? HOTFIX EXECUTION - SQL Error 500 Fix

## Status: READY TO EXECUTE ?

All backend code has been updated. Now you need to execute the database migration.

---

## What Was Fixed in Backend Code

### ? ProjectRepository Updated
- Added `IsDeleted` column to all SELECT statements
- Added `IsDeleted = 0` filter to all WHERE clauses
- Added `IsDeleted` default (0) to INSERT statements

### ? SQL Migration File Updated
- Simplified to only add IsDeleted to **AiIdea** and **Project** tables
- Removed CoherePrompt (doesn't need soft delete)
- Made idempotent (safe to run multiple times)

### ? Code Builds Successfully
- No compilation errors
- All repository methods are consistent

---

## Step 1: Execute Database Migration

### Open SQL Server Management Studio

1. **Search Windows** for "SQL Server Management Studio"
2. **Open** SSMS
3. **Connection Settings:**
   - Server: `(localdb)\ProjectModels`
   - Authentication: Windows Authentication
   - Click **Connect**

### Create New Query

1. **Right-click** `Relyf.Database` in Object Explorer (left panel)
2. Select **New Query**

### Copy & Execute Migration SQL

Copy and paste this SQL into the query editor:

```sql
-- HOTFIX: Add Missing IsDeleted Columns
-- Purpose: Fix SQL Error 207 "Invalid column name 'IsDeleted'"
-- Safety: Idempotent (safe to run multiple times)

USE [Relyf.Database];
GO

-- Add IsDeleted to app.AiIdea
IF NOT EXISTS (
  SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'AiIdea' AND COLUMN_NAME = 'IsDeleted'
)
BEGIN
  ALTER TABLE app.AiIdea
  ADD IsDeleted BIT NOT NULL CONSTRAINT DF_AiIdea_IsDeleted DEFAULT (0);
  PRINT 'SUCCESS: Added IsDeleted to app.AiIdea';
END
ELSE
BEGIN
  PRINT 'INFO: app.AiIdea already has IsDeleted column';
END;

-- Add IsDeleted to app.Project
IF NOT EXISTS (
  SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'Project' AND COLUMN_NAME = 'IsDeleted'
)
BEGIN
  ALTER TABLE app.Project
  ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Project_IsDeleted DEFAULT (0);
  PRINT 'SUCCESS: Added IsDeleted to app.Project';
END
ELSE
BEGIN
  PRINT 'INFO: app.Project already has IsDeleted column';
END;

GO
PRINT '====================================';
PRINT 'MIGRATION COMPLETE';
PRINT '====================================';
```

### Execute Migration

- **Press F5** (or click Execute button)
- **Wait for completion**
- **Check the Messages tab** at the bottom

**Expected Output:**
```
SUCCESS: Added IsDeleted to app.AiIdea
SUCCESS: Added IsDeleted to app.Project
====================================
MIGRATION COMPLETE
====================================
```

---

## Step 2: Verify Migration

Run this verification query in SSMS:

```sql
-- Verify IsDeleted columns exist
SELECT 
  TABLE_NAME,
  COLUMN_NAME,
  DATA_TYPE,
  IS_NULLABLE,
  COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME IN ('AiIdea', 'Project')
  AND COLUMN_NAME = 'IsDeleted'
ORDER BY TABLE_NAME;
```

**Expected Result:**
```
TABLE_NAME       COLUMN_NAME  DATA_TYPE  IS_NULLABLE  COLUMN_DEFAULT
????????????????????????????????????????????????????????????
AiIdea           IsDeleted    bit        NO           (0)
Project          IsDeleted    bit        NO           (0)
```

---

## Step 3: Restart Backend API

### Option A: Visual Studio
1. **Press Shift+F5** (Stop debugging)
2. **Press F5** (Start debugging)

### Option B: Command Line
```powershell
cd C:\Users\ennxk\COMSCI\SofwareDev\Relyf\Relyf.Api
dotnet run
```

---

## Step 4: Test the Endpoint

### Using Swagger UI

1. **Open:** `https://localhost:5101/swagger`
2. **Find:** `POST /api/ideas/generate`
3. **Click:** "Try it out"
4. **Enter Request Body:**
```json
{
  "promptText": "What are creative ways to upcycle old t-shirts?"
}
```
5. **Click:** Execute
6. **Expected Response:** 
   - Status: **201 Created** ?
   - Response has: `ideaId`, `title`, `ideaText`

### Test Other Endpoints

```
GET  /api/Health                    ? 200 OK
GET  /api/ideas/search?q=test       ? 200 OK (empty array is OK)
GET  /api/projects                  ? 200 OK (empty array is OK)
POST /api/ideas/generate            ? 201 Created
```

---

## Step 5: Verify with Different Scenarios

### Test 1: Generate Idea Without ItemId
```json
{
  "promptText": "How can I upcycle old jeans?",
  "titleHint": "Jeans Upcycling"
}
```
Expected: 201 Created

### Test 2: Generate Idea With ItemId (if you have one)
```json
{
  "promptText": "How can I upcycle old jeans?",
  "itemId": 1,
  "titleHint": "Jeans Upcycling"
}
```
Expected: 201 Created

---

## If Migration Fails

### Symptom: "Invalid column name 'IsDeleted'"
**Solution:**
1. Verify you're connected to `Relyf.Database` (top of query editor shows this)
2. Check that the table schema is `app` (not `dbo`)
3. Re-run the migration

### Symptom: "Constraint already exists"
**Solution:**
- This is OK! It means the column was already added
- Run the migration again (it's idempotent)
- Verify with the verification query

### Symptom: "The specified schema name 'app' either does not exist or you do not have permission"
**Solution:**
1. Verify database `Relyf.Database` exists
2. Verify schema `app` exists
3. Check SQL Server Object Explorer for the correct database/schema

---

## If Endpoint Still Returns 500

### Check Backend Logs
1. Look at Visual Studio Output window
2. Search for error messages
3. Verify database connection string in `appsettings.json`

### Verify Database Changes
Run this query to check all IsDeleted columns:
```sql
SELECT TABLE_NAME, COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'IsDeleted'
ORDER BY TABLE_NAME;
```

Expected:
- Item (should already exist)
- User (should already exist)
- AiIdea (just added)
- Project (just added)

### Backend Cache Issues
1. **Close Visual Studio completely**
2. **Delete bin/obj folders:**
   ```powershell
   cd C:\Users\ennxk\COMSCI\SofwareDev\Relyf\Relyf.Api
   dotnet clean
   ```
3. **Rebuild:**
   ```powershell
   dotnet build
   dotnet run
   ```

---

## Summary

| Step | Action | Time |
|------|--------|------|
| 1 | Open SSMS | 1 min |
| 2 | Copy & execute migration | 1 min |
| 3 | Verify migration | 1 min |
| 4 | Restart backend | 1 min |
| 5 | Test endpoint | 2 min |

**Total Time: ~6 minutes**

---

## Success Criteria ?

When you're done:
- [ ] Migration executed successfully (check Messages tab)
- [ ] Verification query returns 2 rows (AiIdea, Project)
- [ ] Backend restarted
- [ ] POST /api/ideas/generate returns 201 Created (not 500)
- [ ] Response includes ideaId, title, ideaText
- [ ] GET /api/Health returns 200 OK
- [ ] GET /api/ideas/search returns 200 OK

---

## Questions?

**If you get stuck:**
1. Check the error message carefully - it usually tells you what's wrong
2. Run the verification query to check database state
3. Review the backend logs for SQL errors
4. Restart SSMS and try again

**Common mistakes to avoid:**
- ? Running migration in wrong database (make sure you select Relyf.Database first)
- ? Typos in table names (use `app` schema, not `dbo`)
- ? Not restarting backend after migration
- ? Testing before migration completes
