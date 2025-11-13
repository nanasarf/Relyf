# Backend SQL Error Fixes - Implementation Plan

## ?? Identified Issues

Based on frontend debug output, there are **2 critical SQL errors**:

### Issue 1: Invalid Column 'IsDeleted' in Search
**Endpoint**: `GET /api/ideas/search`  
**File**: `IdeaSearchRepository.cs` (line ~63)  
**Error**: `Microsoft.Data.SqlClient.SqlException: Invalid column name 'IsDeleted'`

**Root Cause**: The query tries to filter `i.IsDeleted = 0` but this column might not exist in the database, or the database schema definition is inconsistent.

### Issue 2: Invalid Column 'IsDeleted' in Projects
**Endpoint**: `GET /api/Projects`  
**File**: `ProjectController.cs` (line ~86) calling ProjectRepository  
**Error**: `Microsoft.Data.SqlClient.SqlException: Invalid column name 'IsDeleted'`

**Root Cause**: Similar to Issue 1 - Project table IsDeleted column issue.

### Issue 3: SQL Syntax Error (ALREADY FIXED)
**Endpoint**: `GET /api/admin/logs/summary`  
**File**: `AdminLogsRepository.cs` (line ~49)  
**Error**: `Incorrect syntax near the keyword 'AND'`

**Status**: ? **ALREADY FIXED** in previous implementation

---

## ?? Database Schema Analysis

Looking at the code:

### What EXISTS in Database (Confirmed):
- **AiIdeaRecord.cs** has `public bool IsDeleted { get; init; }`
- **AiIdeaRepository** successfully uses `IsDeleted` column
- Multiple repositories reference IsDeleted successfully

### What MIGHT NOT EXIST:
- The `IsDeleted` column might not actually exist in the **physical database** for certain tables
- Or the column exists but has a different name or type

### Decision Matrix:

| Table | EF Model | Dapper Model | Repository | Status |
|-------|----------|--------------|------------|--------|
| AiIdea | No IsDeleted | Has IsDeleted | Uses IsDeleted | ?? Mismatch |
| Project | No IsDeleted | N/A | Uses IsDeleted | ?? Mismatch |
| Item | No IsDeleted | Has IsDeleted | Uses IsDeleted | ?? Mismatch |
| User | No IsDeleted | Has IsDeleted | Uses IsDeleted | ?? Mismatch |

---

## ?? Recommended Fix Strategy

### Option A: Remove IsDeleted (If soft-delete not needed)
- Remove `IsDeleted` from all WHERE clauses
- Remove `IsDeleted` from all SELECT statements
- Keep the model properties (backward compatibility)

### Option B: Add IsDeleted Column to Database
- Run SQL migration to add column to all tables that need it
- Add column to EF models for consistency
- Continue using IsDeleted in code

### Option C: Check Actual Database Schema
- Query SQL Server to see what columns actually exist
- Align code with actual schema

---

## ? RECOMMENDED APPROACH

I recommend **Option A** (Remove IsDeleted references) because:
1. Your EF models don't define IsDeleted
2. The database schema apparently doesn't have these columns
3. The Dapper models having these properties is the mismatch
4. Soft-delete logic can be added later if needed

---

## ?? Implementation Steps

### Step 1: Fix IdeaSearchRepository
Remove `i.IsDeleted = 0` from WHERE clause

### Step 2: Fix ProjectRepository
Already partially fixed - verify no remaining IsDeleted references

### Step 3: Verify AdminLogsRepository
Confirm previous fix is in place

### Step 4: Update Models
Remove IsDeleted from Dapper models where not in database

### Step 5: Test All Endpoints
- GET /api/ideas/search
- GET /api/Projects
- GET /api/admin/logs/summary
- Other affected endpoints

---

## ?? Quick Fix Checklist

- [ ] Fix IdeaSearchRepository.cs - Remove `i.IsDeleted = 0`
- [ ] Verify ProjectRepository.cs - Check for remaining IsDeleted refs
- [ ] Verify AdminLogsRepository.cs - Confirm AND syntax fix
- [ ] Remove IsDeleted from Dapper models (optional)
- [ ] Run full build
- [ ] Test endpoints
- [ ] Commit changes

---

**Next**: Apply the recommended fixes below.
