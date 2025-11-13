# Alternative: PowerShell Execution Method

If you prefer not to use SQL Server Management Studio, you can run the migration via PowerShell.

## ?? PowerShell Method

### Step 1: Open PowerShell as Administrator

1. Press **Windows Key**
2. Type `PowerShell`
3. Right-click **Windows PowerShell**
4. Select **Run as administrator**

### Step 2: Run This Command

Copy and paste the entire command below into PowerShell:

```powershell
$connectionString = "Server=(localdb)\ProjectModels;Database=Relyf.Database;Integrated Security=True;"
$sqlScript = @"
-- Add IsDeleted to app.AiIdea
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'AiIdea' AND COLUMN_NAME = 'IsDeleted')
BEGIN
  ALTER TABLE app.AiIdea ADD IsDeleted BIT NOT NULL CONSTRAINT DF_AiIdea_IsDeleted DEFAULT (0);
  PRINT 'SUCCESS: Added IsDeleted to app.AiIdea';
END
ELSE
BEGIN
  PRINT 'INFO: app.AiIdea already has IsDeleted column';
END;

-- Add IsDeleted to app.Project
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'Project' AND COLUMN_NAME = 'IsDeleted')
BEGIN
  ALTER TABLE app.Project ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Project_IsDeleted DEFAULT (0);
  PRINT 'SUCCESS: Added IsDeleted to app.Project';
END
ELSE
BEGIN
  PRINT 'INFO: app.Project already has IsDeleted column';
END;

-- Add IsDeleted to app.CoherePrompt
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'CoherePrompt' AND COLUMN_NAME = 'IsDeleted')
BEGIN
  ALTER TABLE app.CoherePrompt ADD IsDeleted BIT NOT NULL CONSTRAINT DF_CoherePrompt_IsDeleted DEFAULT (0);
  PRINT 'SUCCESS: Added IsDeleted to app.CoherePrompt';
END
ELSE
BEGIN
  PRINT 'INFO: app.CoherePrompt already has IsDeleted column';
END;

PRINT '====================================';
PRINT 'MIGRATION COMPLETE';
PRINT '====================================';
"@

$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString
$connection.Open()

$command = $connection.CreateCommand()
$command.CommandText = $sqlScript
$command.CommandTimeout = 30

try {
    Write-Host "Executing migration..." -ForegroundColor Yellow
    $reader = $command.ExecuteReader()
    
    while ($reader.Read()) {
        Write-Host $reader[0] -ForegroundColor Green
    }
    
    $reader.Close()
    Write-Host "Migration completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    $connection.Close()
    $command.Dispose()
    $connection.Dispose()
}
```

### Step 3: Press Enter

Wait for the script to complete. You should see:

```
Executing migration...
SUCCESS: Added IsDeleted to app.AiIdea
SUCCESS: Added IsDeleted to app.Project
SUCCESS: Added IsDeleted to app.CoherePrompt
====================================
MIGRATION COMPLETE
====================================
Migration completed successfully!
```

---

## Alternative: Using sqlcmd.exe

If PowerShell script above doesn't work, try `sqlcmd` directly:

```powershell
# Save migration to file
$sqlPath = "$env:TEMP\migration.sql"

@"
-- Add IsDeleted columns
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'AiIdea' AND COLUMN_NAME = 'IsDeleted')
ALTER TABLE app.AiIdea ADD IsDeleted BIT NOT NULL CONSTRAINT DF_AiIdea_IsDeleted DEFAULT (0);

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'Project' AND COLUMN_NAME = 'IsDeleted')
ALTER TABLE app.Project ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Project_IsDeleted DEFAULT (0);

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'CoherePrompt' AND COLUMN_NAME = 'IsDeleted')
ALTER TABLE app.CoherePrompt ADD IsDeleted BIT NOT NULL CONSTRAINT DF_CoherePrompt_IsDeleted DEFAULT (0);

PRINT 'Migration Complete';
"@ | Out-File -FilePath $sqlPath -Encoding UTF8

# Execute
sqlcmd -S "(localdb)\ProjectModels" -d "Relyf.Database" -E -i $sqlPath

# Cleanup
Remove-Item $sqlPath
```

---

## Verification via PowerShell

Verify the columns were added:

```powershell
$connectionString = "Server=(localdb)\ProjectModels;Database=Relyf.Database;Integrated Security=True;"
$query = "SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME IN ('AiIdea', 'Project', 'CoherePrompt') AND COLUMN_NAME = 'IsDeleted' ORDER BY TABLE_NAME;"

$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString
$connection.Open()

$command = $connection.CreateCommand()
$command.CommandText = $query

$reader = $command.ExecuteReader()

Write-Host "Verification Results:" -ForegroundColor Yellow
Write-Host "===================" -ForegroundColor Yellow

$found = 0
while ($reader.Read()) {
    Write-Host "$($reader['TABLE_NAME']).IsDeleted - EXISTS ?" -ForegroundColor Green
    $found++
}

$reader.Close()
$connection.Close()

if ($found -eq 0) {
    Write-Host "No IsDeleted columns found! Migration may have failed." -ForegroundColor Red
} elseif ($found -eq 3) {
    Write-Host "All 3 IsDeleted columns created successfully!" -ForegroundColor Green
} else {
    Write-Host "Partial: Only $found/3 columns found." -ForegroundColor Yellow
}
```

---

## Which Method to Use?

| Method | When to Use | Pros | Cons |
|--------|-----------|------|------|
| **SSMS** (Recommended) | You have it installed | Visual feedback, easy to use | Requires SSMS |
| **PowerShell Script** | Quick, no UI needed | Automated, no GUI | Error messages less clear |
| **sqlcmd** | CLI preference | Works everywhere | More complex |

---

**Recommendation**: Use SSMS (easiest and most reliable)

All methods are safe and idempotent - you can run them multiple times without harm.
