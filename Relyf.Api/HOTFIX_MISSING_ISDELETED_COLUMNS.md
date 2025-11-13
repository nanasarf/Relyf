# HOTFIX: Add Missing IsDeleted Columns - URGENT

## ?? Issue
`POST /api/ideas/generate` returns HTTP 500 with: `Invalid column name 'IsDeleted'`

## ? Solution
Add the missing IsDeleted columns to tables referenced in the failing query.

---

## ?? Run This SQL Immediately

Copy and paste into **SQL Server Management Studio** or **Azure Data Studio**:

```sql
-- HOTFIX: Add IsDeleted to app.AiIdea
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
  PRINT 'app.AiIdea already has IsDeleted';
END;

-- HOTFIX: Add IsDeleted to app.Project
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
  PRINT 'app.Project already has IsDeleted';
END;

-- HOTFIX: Add IsDeleted to app.CoherePrompt (if used)
IF NOT EXISTS (
  SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'CoherePrompt' AND COLUMN_NAME = 'IsDeleted'
)
BEGIN
  ALTER TABLE app.CoherePrompt
  ADD IsDeleted BIT NOT NULL CONSTRAINT DF_CoherePrompt_IsDeleted DEFAULT (0);
  PRINT 'Added IsDeleted to app.CoherePrompt';
END
ELSE
BEGIN
  PRINT 'app.CoherePrompt already has IsDeleted';
END;

GO
PRINT '=== Migration Complete ===';
```

### Steps to Execute:

1. **Open SQL Server Management Studio**
2. **Connect** to: `(localdb)\ProjectModels`
3. **Select Database**: `Relyf.Database`
4. **Right-click** ? **New Query**
5. **Paste** the SQL above
6. **Press F5** to execute

**Expected Output:**
```
Added IsDeleted to app.AiIdea
Added IsDeleted to app.Project
Added IsDeleted to app.CoherePrompt
=== Migration Complete ===
```

---

## ? Verify Columns Were Added

Run this to confirm:

```sql
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME IN ('AiIdea', 'Project', 'CoherePrompt')
ORDER BY TABLE_NAME, ORDINAL_POSITION;
```

Should show `IsDeleted` for all three tables with:
- Data Type: **bit**
- IS_NULLABLE: **NO**
- COLUMN_DEFAULT: **(0)**

---

## ?? After Migration

1. **Restart Backend**
   - Stop debugging: `Shift+F5`
   - Start debugging: `F5`

2. **Test Endpoint**
   - Go to Swagger: `https://localhost:5101/swagger`
   - Try: `POST /api/ideas/generate`
   - Expected: **201 Created** (not 500)

3. **If Still Failing**
   - Check backend console for new errors
   - Verify migration ran successfully
   - Restart API if needed

---

## ?? What This Fixes

| Endpoint | Before | After |
|----------|--------|-------|
| `POST /api/ideas/generate` | ? 500 Error | ? 201 Created |
| `GET /api/ideas/search` | ? 500 Error | ? 200 OK |
| `GET /api/Projects` | ? 500 Error | ? 200 OK |

---

**Status**: Ready to execute immediately  
**Risk**: Very low (additive only)  
**Time**: < 1 minute
