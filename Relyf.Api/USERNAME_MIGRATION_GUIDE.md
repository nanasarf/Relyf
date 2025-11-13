# UserName Column Migration - Quick Guide

## ? What This Does

Adds the following columns to the `app.[User]` table:
- **UserName** (NVARCHAR(20), NOT NULL, UNIQUE) - Permanent user identifier
- **DisplayName** (already exists) - Public display name (can be changed)
- **Bio** (NVARCHAR(500), NULL) - User biography
- **AvatarUrl** (NVARCHAR(500), NULL) - Profile picture URL

## ?? Execute Migration

### Step 1: Ensure LocalDB is Running

```powershell
# Start LocalDB instance
sqllocaldb start ProjectModels
```

### Step 2: Run the Migration Script

```powershell
# Navigate to Relyf.Api directory
cd C:\Users\ennxk\COMSCI\SofwareDev\Relyf\Relyf.Api

# Execute the migration
sqlcmd -S "(localdb)\ProjectModels" -d "Relyf.Database" -i add_username_displayname_columns.sql
```

### Step 3: Verify Migration

```powershell
# Run the verification test
.\TEST_USERNAME_MIGRATION.ps1
```

## ?? What the Migration Does

1. ? **Adds UserName column** (temporarily NULL)
2. ? **Adds Bio column** (500 char limit)
3. ? **Adds AvatarUrl column** (500 char limit)
4. ? **Generates usernames** for existing users (`user_123` format)
5. ? **Makes UserName NOT NULL** after data population
6. ? **Creates unique index** on UserName for fast lookups

## ?? Expected Output

```
Added UserName column to app.[User]
Added Bio column to app.[User]
Added AvatarUrl column to app.[User]
Generated UserNames for X existing users
Made UserName column NOT NULL
Created unique index IX_User_UserName_Unique
============================
User Profile Migration Complete
============================
```

## ?? Important Notes

- **Database**: Uses LocalDB `(localdb)\ProjectModels`
- **Database Name**: `Relyf.Database`
- **Idempotent**: Safe to run multiple times
- **Existing users**: Auto-generated usernames (`user_123`)
- **New users**: Must provide UserName during registration
- **Username rules**: 3-20 characters, alphanumeric + underscores only
- **Unique constraint**: No duplicate usernames allowed

## ?? Test the API After Migration

After running the migration, test with:

```powershell
.\TEST_UPDATE_USER_PROFILE.ps1
.\TEST_USERNAME_FEATURE.ps1
```

## ?? Related Files

- Migration script: `add_username_displayname_columns.sql`
- Verification: `TEST_USERNAME_MIGRATION.ps1`
- API Documentation: `USERNAME_DISPLAYNAME_IMPLEMENTATION.md`
- Swagger spec: `swagger.json`

## ?? Troubleshooting

### If LocalDB is not running:

```powershell
# List LocalDB instances
sqllocaldb info

# Start ProjectModels instance
sqllocaldb start ProjectModels

# Check instance info
sqllocaldb info ProjectModels
```

### If migration fails:

1. **Check database exists:**
   ```powershell
   sqlcmd -S "(localdb)\ProjectModels" -Q "SELECT name FROM sys.databases WHERE name = 'Relyf.Database';"
   ```

2. **Check User table exists:**
   ```sql
   SELECT * FROM INFORMATION_SCHEMA.TABLES 
   WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'User';
   ```

3. **Check current columns:**
   ```sql
   SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'User';
   ```

### Manual rollback (if needed):

```sql
-- Remove unique index
DROP INDEX IF EXISTS IX_User_UserName_Unique ON app.[User];

-- Remove columns
ALTER TABLE app.[User] DROP COLUMN IF EXISTS UserName;
ALTER TABLE app.[User] DROP COLUMN IF EXISTS Bio;
ALTER TABLE app.[User] DROP COLUMN IF EXISTS AvatarUrl;
```

## ? Success Criteria

After migration, you should be able to:
- ? Register new users with username
- ? Login with email (username stored separately)
- ? Update user profile (username, displayName, bio, avatarUrl)
- ? Check username availability
- ? View user profiles with all fields

---

**Next Steps After Migration:**
1. Run verification test
2. Test user registration endpoint
3. Test profile update endpoint
4. Verify frontend integration works
