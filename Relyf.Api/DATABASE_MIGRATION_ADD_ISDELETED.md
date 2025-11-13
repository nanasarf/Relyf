# Database Migration: Add IsDeleted Columns

## ?? Safe Migration Scripts

These scripts are **idempotent** — they only add the column if it doesn't already exist. Safe to run multiple times.

### Script 1: Add IsDeleted to app.AiIdea

```sql
-- Add IsDeleted column to app.AiIdea if it doesn't exist
IF COL_LENGTH('app.AiIdea', 'IsDeleted') IS NULL
BEGIN
    ALTER TABLE app.AiIdea
    ADD IsDeleted BIT NOT NULL CONSTRAINT DF_AiIdea_IsDeleted DEFAULT (0);
    PRINT 'Added IsDeleted to app.AiIdea';
END
ELSE
BEGIN
    PRINT 'app.AiIdea already has IsDeleted column';
END
GO
```

### Script 2: Add IsDeleted to app.Project

```sql
-- Add IsDeleted column to app.Project if it doesn't exist
IF COL_LENGTH('app.Project', 'IsDeleted') IS NULL
BEGIN
    ALTER TABLE app.Project
    ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Project_IsDeleted DEFAULT (0);
    PRINT 'Added IsDeleted to app.Project';
END
ELSE
BEGIN
    PRINT 'app.Project already has IsDeleted column';
END
GO
```

### Combined Script (Run All at Once)

```sql
-- Migration: Add IsDeleted columns to soft-delete tables
-- Safe to run multiple times (idempotent)
-- Default value: 0 (not deleted)

-- Add IsDeleted to app.AiIdea
IF COL_LENGTH('app.AiIdea', 'IsDeleted') IS NULL
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
IF COL_LENGTH('app.Project', 'IsDeleted') IS NULL
BEGIN
    ALTER TABLE app.Project
    ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Project_IsDeleted DEFAULT (0);
    PRINT 'Added IsDeleted to app.Project';
END
ELSE
BEGIN
    PRINT 'app.Project already has IsDeleted column';
END;

GO
PRINT 'Migration complete: IsDeleted columns added/verified';
```

---

## ?? How to Run the Migration

### Option 1: SQL Server Management Studio (SSMS)

1. **Open SSMS**
2. **Connect** to `(localdb)\ProjectModels`
3. **Select Database**: `Relyf.Database`
4. **New Query**
5. **Paste** the combined script above
6. **Execute** (F5 or Ctrl+Shift+E)

Expected Output:
```
Added IsDeleted to app.AiIdea
Added IsDeleted to app.Project
Migration complete: IsDeleted columns added/verified
```

### Option 2: Azure Data Studio

1. **Connect** to `(localdb)\ProjectModels`
2. **Select Database**: `Relyf.Database`
3. **New Query**
4. **Paste** the combined script
5. **Run** (Ctrl+Shift+E)

### Option 3: PowerShell + sqlcmd

```powershell
# Save migration script to file
@"
-- Add IsDeleted to app.AiIdea
IF COL_LENGTH('app.AiIdea', 'IsDeleted') IS NULL
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
IF COL_LENGTH('app.Project', 'IsDeleted') IS NULL
BEGIN
    ALTER TABLE app.Project
    ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Project_IsDeleted DEFAULT (0);
    PRINT 'Added IsDeleted to app.Project';
END
ELSE
BEGIN
    PRINT 'app.Project already has IsDeleted column';
END;
"@ | Out-File -FilePath "C:\temp\migration.sql" -Encoding UTF8

# Run migration
sqlcmd -S "(localdb)\ProjectModels" -d "Relyf.Database" -E -i "C:\temp\migration.sql"
```

### Option 4: .NET CLI (EF Core)

If you want to create a formal EF migration:

```bash
cd C:\Users\ennxk\COMSCI\SofwareDev\Relyf\Relyf.Api

# Create migration
dotnet ef migrations add AddIsDeletedColumns --project ../Relyf.Repository

# Apply migration
dotnet ef database update
```

---

## ? Verification Steps

### After Running Migration

**Verify columns were added:**

```sql
-- Check app.AiIdea
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AiIdea' AND TABLE_SCHEMA = 'app'
ORDER BY ORDINAL_POSITION;

-- Check app.Project
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Project' AND TABLE_SCHEMA = 'app'
ORDER BY ORDINAL_POSITION;
```

**Should show:**
```
COLUMN_NAME    DATA_TYPE    IS_NULLABLE
??????????????????????????????????????
IdeaId         int          NO
CoherePromptId int          YES
...
IsDeleted      bit          NO          ? NEW!
```

---

## ?? Next: Restart Backend & Test

### Step 1: Restart API (VS)
- **Stop** debugging: Shift+F5
- **Start** debugging: F5

### Step 2: Test Failing Endpoints

Test these in Swagger or Postman:

```
1. GET /api/ideas/search?q=denim
   Expected: 200 OK (not 500)

2. GET /api/Projects
   Expected: 200 OK (not 500)

3. POST /api/ideas/generate
   Expected: 201 Created (not 500)

4. GET /api/admin/logs/summary
   Expected: 200 OK (not 500)
```

### Step 3: Check Backend Logs

Look for:
- ? `Database connected successfully`
- ? No SQL errors in output
- ? No 500 responses

---

## ?? What This Migration Does

```
BEFORE:
Database:  No IsDeleted column
Code:      Tries to query IsDeleted
Result:    SQL Error 207 (Invalid column) ? HTTP 500 ?

AFTER:
Database:  Has IsDeleted column (DEFAULT 0 = not deleted)
Code:      Queries IsDeleted successfully
Result:    Proper soft-delete filtering ? HTTP 200 ?
```

---

## ??? Safety Features

? **Idempotent**: Can run multiple times safely  
? **Non-destructive**: Only adds column, doesn't modify data  
? **Default Value**: Existing rows default to 0 (not deleted)  
? **Constraint Named**: Constraint named for easy identification  
? **Non-nullable**: IsDeleted defaults to 0, no NULLs allowed  

---

## ?? Affected Tables & Columns

| Table | Schema | New Column | Type | Default | Purpose |
|-------|--------|-----------|------|---------|---------|
| AiIdea | app | IsDeleted | bit | 0 | Soft-delete ideas |
| Project | app | IsDeleted | bit | 0 | Soft-delete projects |

---

## ?? Long-Term Considerations

### Option A: Keep Soft-Delete (Recommended)
- Keep the IsDeleted columns
- Continue using in repositories
- Maintains data recovery capability
- Queries filter out deleted records

### Option B: Remove Later (If Desired)
- After migration succeeds and everything works
- Create EF migration to add `[SoftDelete]` attribute
- Refactor repositories to cleaner soft-delete pattern
- Run migration again
- This is safer step-by-step approach

---

## ?? If Migration Fails

**Common Issues & Solutions:**

| Issue | Cause | Solution |
|-------|-------|----------|
| "Invalid object name" | Wrong schema/table name | Check exact names in your DB |
| Permission denied | User lacks ALTER TABLE rights | Use admin/sa account |
| Constraint error | Constraint name already exists | Script is idempotent, won't re-run |
| Column already exists | Migration already applied | Script handles this (prints "already has") |

---

## ? Summary

This migration:
1. ? Adds IsDeleted column to AiIdea table
2. ? Adds IsDeleted column to Project table
3. ? Sets default value to 0 (not deleted)
4. ? Is completely safe to run multiple times
5. ? Requires no code changes
6. ? Enables all failing endpoints to work

**Time to run**: < 1 second  
**Risk level**: Very Low (additive only)  
**Rollback**: Can drop columns if needed (shouldn't be necessary)  

---

**Status**: Ready to Execute ?
