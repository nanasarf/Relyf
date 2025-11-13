# MVP Backend - Quick Execution Guide

## ? Backend Ready

All code changes applied and tested. Now just execute the database migration.

---

## ?? 3 Steps to MVP Ready

### Step 1: Apply IsDeleted Migration (1 min)

**Open SQL Server Management Studio:**
1. Connect: `(localdb)\ProjectModels`
2. Database: `Relyf.Database`
3. Right-click ? **New Query**
4. Copy this:

```sql
-- Add IsDeleted columns (Idempotent - safe to run multiple times)
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
END;

-- Add IsDeleted to app.CoherePrompt
IF NOT EXISTS (
  SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'CoherePrompt' AND COLUMN_NAME = 'IsDeleted'
)
BEGIN
  ALTER TABLE app.CoherePrompt
  ADD IsDeleted BIT NOT NULL CONSTRAINT DF_CoherePrompt_IsDeleted DEFAULT (0);
  PRINT 'SUCCESS: Added IsDeleted to app.CoherePrompt';
END;

GO
PRINT '====================================';
PRINT 'MIGRATION COMPLETE';
PRINT '====================================';
```

5. Press **F5**
6. Verify success messages

**Expected Output:**
```
SUCCESS: Added IsDeleted to app.AiIdea
SUCCESS: Added IsDeleted to app.Project
SUCCESS: Added IsDeleted to app.CoherePrompt
====================================
MIGRATION COMPLETE
====================================
```

### Step 2: Restart API (1 min)

In Visual Studio:
- **Shift+F5** (Stop)
- **F5** (Start)

Or command line:
```bash
cd C:\Users\ennxk\COMSCI\SofwareDev\Relyf\Relyf.Api
dotnet run
```

### Step 3: Test POST /api/ideas/generate (2 min)

In Swagger (`https://localhost:5101/swagger`):

1. Click **POST /api/ideas/generate**
2. Click **Try it out**
3. Enter:
```json
{
  "promptText": "What are creative ways to upcycle old t-shirts?"
}
```
4. Click **Execute**

**Expected Result:**
- Status: ? **201 Created**
- Response includes: `ideaId`, `title`, `ideaText`

---

## ? Verification Checklist

After step 3 above:

- [ ] POST /api/ideas/generate returns 201 Created
- [ ] GET /api/ideas/search returns 200 OK
- [ ] GET /api/Projects returns 200 OK
- [ ] GET /api/Health returns 200 OK
- [ ] No SQL errors in console

---

## ?? You're Done!

Backend is ready for MVP. Commit and move forward:

```bash
git add -A
git commit -m "Apply IsDeleted migration for soft-delete support"
git push origin feature/week8-dapper
```

---

## ?? References

- **MVP_BACKEND_FINAL_STATUS.md** - Full status document
- **HOTFIX_EXECUTE_NOW.md** - Detailed migration guide
- **add_isdeleted_columns.sql** - Raw SQL migration

---

**Time to Complete**: ~5 minutes  
**Risk**: Very Low  
**Status**: Ready ?
