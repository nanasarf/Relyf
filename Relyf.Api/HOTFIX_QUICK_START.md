# ? QUICK START - 5 Minute Hotfix

## STATUS: Backend Ready ? | Database Pending ?

---

## What's Wrong?

Backend code uses `IsDeleted` column, but database doesn't have it.

**Result:** HTTP 500 - SQL Error 207 "Invalid column name 'IsDeleted'"

---

## What's Fixed in Backend?

? AiIdeaRepository - Already had it  
? ProjectRepository - Just updated  
? Code builds successfully  

---

## What You Need To Do

### STEP 1: Open SSMS (1 min)
```
Server: (localdb)\ProjectModels
DB: Relyf.Database
```

### STEP 2: Run This SQL
```sql
USE [Relyf.Database];
GO
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA='app' AND TABLE_NAME='AiIdea' AND COLUMN_NAME='IsDeleted')
BEGIN ALTER TABLE app.AiIdea ADD IsDeleted BIT NOT NULL DEFAULT (0);
  PRINT 'SUCCESS: Added IsDeleted to app.AiIdea'; END
ELSE PRINT 'AiIdea already has IsDeleted';

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA='app' AND TABLE_NAME='Project' AND COLUMN_NAME='IsDeleted')
BEGIN ALTER TABLE app.Project ADD IsDeleted BIT NOT NULL DEFAULT (0);
  PRINT 'SUCCESS: Added IsDeleted to app.Project'; END
ELSE PRINT 'Project already has IsDeleted';

GO
PRINT 'Migration Complete';
```

Press F5

### STEP 3: Restart Backend (1 min)
- Shift+F5 (stop)
- F5 (start)

### STEP 4: Test (2 min)
```
POST https://localhost:5101/api/ideas/generate
Body: {"promptText": "Ideas for old t-shirts?"}
Expected: 201 Created ?
```

---

## Done! ?

You fixed HTTP 500 error in ~5 minutes.

---

## Detailed Help

See these files for more info:
- `HOTFIX_CHECKLIST.md` - Checklist format
- `HOTFIX_EXECUTION_STEPS.md` - Step-by-step with troubleshooting
- `HOTFIX_COMPLETE_SUMMARY.md` - Full technical overview
