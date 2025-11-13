# =============================================
# Test Script: Verify UserName Migration
# =============================================
# Purpose: Verify that UserName, Bio, and AvatarUrl columns were added correctly
# Usage: .\TEST_USERNAME_MIGRATION.ps1
# =============================================

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing UserName Migration" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Database connection settings (from appsettings.json)
$ServerInstance = "(localdb)\ProjectModels"
$Database = "Relyf.Database"

# SQL Query to check columns
$checkColumnsQuery = @"
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'app' 
  AND TABLE_NAME = 'User'
  AND COLUMN_NAME IN ('UserName', 'Bio', 'AvatarUrl', 'DisplayName')
ORDER BY COLUMN_NAME;
"@

# SQL Query to check index
$checkIndexQuery = @"
SELECT 
    i.name AS IndexName,
    i.is_unique AS IsUnique,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE i.object_id = OBJECT_ID('app.[User]')
  AND i.name LIKE '%UserName%';
"@

# SQL Query to check sample data
$checkDataQuery = @"
SELECT TOP 5
    UserId,
    Email,
    UserName,
    DisplayName,
    Bio,
    AvatarUrl
FROM app.[User]
ORDER BY UserId;
"@

try {
    Write-Host "Connecting to: $ServerInstance" -ForegroundColor Gray
    Write-Host "Database: $Database`n" -ForegroundColor Gray

    # Check columns
    Write-Host "1. Checking User Table Columns..." -ForegroundColor Yellow
    
    $columnsResult = sqlcmd -S $ServerInstance -d $Database -Q $checkColumnsQuery -h -1
    Write-Host $columnsResult
    Write-Host ""

    # Check index
    Write-Host "2. Checking UserName Index..." -ForegroundColor Yellow
    
    $indexResult = sqlcmd -S $ServerInstance -d $Database -Q $checkIndexQuery -h -1
    Write-Host $indexResult
    Write-Host ""

    # Check sample data
    Write-Host "3. Checking Sample User Data..." -ForegroundColor Yellow
    
    $dataResult = sqlcmd -S $ServerInstance -d $Database -Q $checkDataQuery -h -1
    Write-Host $dataResult
    Write-Host ""

    # Verify expected columns exist
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Verification Results" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    
    $hasUserName = $columnsResult -match "UserName"
    $hasBio = $columnsResult -match "Bio"
    $hasAvatarUrl = $columnsResult -match "AvatarUrl"
    $hasIndex = $indexResult -match "IX_User_UserName"

    Write-Host "? UserName column exists: $(if($hasUserName){'YES'}else{'NO'})" -ForegroundColor $(if($hasUserName){'Green'}else{'Red'})
    Write-Host "? Bio column exists: $(if($hasBio){'YES'}else{'NO'})" -ForegroundColor $(if($hasBio){'Green'}else{'Red'})
    Write-Host "? AvatarUrl column exists: $(if($hasAvatarUrl){'YES'}else{'NO'})" -ForegroundColor $(if($hasAvatarUrl){'Green'}else{'Red'})
    Write-Host "? Unique index exists: $(if($hasIndex){'YES'}else{'NO'})" -ForegroundColor $(if($hasIndex){'Green'}else{'Red'})
    
    Write-Host ""
    if ($hasUserName -and $hasBio -and $hasAvatarUrl -and $hasIndex) {
        Write-Host "? Migration Successful! All columns and indexes created." -ForegroundColor Green
    } else {
        Write-Host "? Migration incomplete. Please run add_username_displayname_columns.sql" -ForegroundColor Red
        Write-Host "`nTo execute migration:" -ForegroundColor Yellow
        Write-Host "sqlcmd -S `"$ServerInstance`" -d `"$Database`" -i add_username_displayname_columns.sql" -ForegroundColor Cyan
    }

} catch {
    Write-Host "? Error: $_" -ForegroundColor Red
    Write-Host "`nPlease ensure:" -ForegroundColor Yellow
    Write-Host "  1. SQL Server LocalDB is running" -ForegroundColor Yellow
    Write-Host "  2. Database '$Database' exists" -ForegroundColor Yellow
    Write-Host "  3. You have permission to query the database" -ForegroundColor Yellow
    Write-Host "`nTo start LocalDB:" -ForegroundColor Yellow
    Write-Host "  sqllocaldb start ProjectModels" -ForegroundColor Cyan
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Test Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
