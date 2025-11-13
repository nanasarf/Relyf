# ? HOTFIX CHECKLIST - SQL Error 500

## Backend Code Status: ? COMPLETE

- [x] AiIdeaRepository - Already had IsDeleted (no changes needed)
- [x] ProjectRepository - Updated to select IsDeleted
- [x] Code compiles successfully
- [x] SQL migration file updated (only AiIdea + Project)

---

## Your Next Steps: DATABASE & TESTING

### ?? REQUIRED: Execute SQL Migration

**Location:** Open SQL Server Management Studio

```
SERVER: (localdb)\ProjectModels
AUTH: Windows Authentication
DATABASE: Relyf.Database
```

**Copy & Paste the SQL:**
```sql
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
PRINT 'MIGRATION COMPLETE';
```

**Press F5 to execute**

---

### ?? VERIFY: Check Database

Run this query:

```sql
SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'IsDeleted'
ORDER BY TABLE_NAME;
```

**Should show 4 rows:**
- Item (should already exist)
- User (should already exist)  
- AiIdea ? just added
- Project ? just added

---

### ?? RESTART: Backend API

**Visual Studio:**
- Press Shift+F5 (Stop)
- Press F5 (Start)

**OR Command Line:**
```powershell
cd C:\Users\ennxk\COMSCI\SofwareDev\Relyf\Relyf.Api
dotnet run
```

---

### ?? TEST: Swagger Endpoint

**URL:** https://localhost:5101/swagger

**Endpoint:** POST /api/ideas/generate

**Request:**
```json
{
  "promptText": "What are creative ways to upcycle old t-shirts?"
}
```

**Expected Response:**
- ? Status: **201 Created** (not 500!)
- ? Body includes: `ideaId`, `title`, `ideaText`

---

### ?? SMOKE TESTS: Verify Other Endpoints

```
? GET  /api/Health                    ? 200 OK
? GET  /api/ideas/search?q=test       ? 200 OK  
? GET  /api/projects                  ? 200 OK
? POST /api/ideas/generate            ? 201 Created
```

---

## Troubleshooting

| Problem | Check | Solution |
|---------|-------|----------|
| Migration fails | Database name | Use `Relyf.Database` not master |
| "Invalid column" | Schema | Must be `app` not `dbo` |
| Still getting 500 | Backend logs | Check Visual Studio Output window |
| GetAsync returns null | IsDeleted in SELECT | ProjectRepository should select it (already done) |
| Connection error | appsettings.json | Verify DB connection string |

---

## Files Modified

**Backend Code:**
- ? `Relyf.Repository/Dapper/ProjectRepository.cs` - Added IsDeleted to all queries

**Database Migration:**
- ? `add_isdeleted_columns.sql` - Cleaned up, only AiIdea + Project

**Guides:**
- ? `HOTFIX_EXECUTION_STEPS.md` - Detailed step-by-step
- ? `HOTFIX_CHECKLIST.md` - This file

---

## Timeline

```
Before Migration:
  Backend: ? Updated
  Database: ? Missing columns
  Result: HTTP 500 ?

After Migration (YOUR NEXT STEP):
  Backend: ? Updated  
  Database: ? Has columns
  Result: HTTP 201 ?
```

---

## GO / NO-GO

**READY TO EXECUTE?**
- [x] Code is ready (builds successfully)
- [x] Migration file is ready
- [x] Backend code is updated
- [ ] Migration executed (YOUR TURN)
- [ ] Backend restarted (YOUR TURN)
- [ ] Endpoint tested (YOUR TURN)

**? Execute the migration in SSMS now!**
