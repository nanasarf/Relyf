# Username & Display Name Implementation Guide

## ?? Overview

This implementation adds **username-based authentication** and **enhanced user profiles** to the Relyf API, including:

- **UserName**: Unique, permanent identifier (3-20 chars, alphanumeric + underscores) - **Used for social features, NOT login**
- **DisplayName**: Public display name (can be changed)
- **Bio**: User description (up to 500 chars)
- **AvatarUrl**: Profile picture URL

**?? IMPORTANT: Login uses Email + Password, NOT UserName!**

---

## ?? Authentication Clarification

### Login Flow (Email-based):
```json
POST /api/Auth/login
{
  "email": "john@example.com",     // ? Email for authentication
  "password": "SecurePass123!"
  // NO userName field - not used for login
}
```

**Response includes UserName for display:**
```json
{
  "userId": 123,
  "email": "john@example.com",
  "userName": "john_doe",           // ? Returned for social features
  "displayName": "John Doe",
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

### What UserName IS Used For:
? Social features (profile display, mentions)  
? Profile URLs (`relyf.com/@john_doe`)  
? Search functionality  
? Unique identifier for the user  

### What UserName is NOT Used For:
? Login authentication  
? Password reset  
? WHERE clause in login queries  

**See [LOGIN_AUTHENTICATION_FLOW.md](LOGIN_AUTHENTICATION_FLOW.md) for detailed explanation.**

---

## ?? Database Migration

### SQL Migration File
**File**: `add_username_displayname_columns.sql`

**What it does**:
1. Adds `UserName` column (NVARCHAR(20), NOT NULL, UNIQUE)
2. Adds `Bio` column (NVARCHAR(500), NULL)
3. Adds `AvatarUrl` column (NVARCHAR(500), NULL)
4. Generates usernames for existing users (`user_<UserId>`)
5. Creates unique index `IX_User_UserName_Unique` for fast lookups

**How to run**:
```sql
-- Run this in your SQL Server Management Studio or Azure Data Studio
-- Connected to your RelyfDb database
:r add_username_displayname_columns.sql
```

Or via command line:
```bash
sqlcmd -S <server> -d RelyfDb -i add_username_displayname_columns.sql
```

**Safety**: The script is **idempotent** - safe to run multiple times.

---

## ?? API Endpoints

### 1. Check Username Availability
**Endpoint**: `GET /api/Users/check-username/{userName}`

**Purpose**: Check if a username is available before registration

**Response**:
```json
{
  "available": true,
  "message": null
}
```

Or if taken:
```json
{
  "available": false,
  "message": "Username is already taken"
}
```

**Validation Rules**:
- 3-20 characters
- Alphanumeric + underscores only (`^[a-zA-Z0-9_]+$`)
- Case-insensitive check

**Example Usage**:
```bash
# PowerShell
$response = Invoke-RestMethod -Uri "https://localhost:7139/api/Users/check-username/john_doe" -Method Get
Write-Host "Available: $($response.available)"

# cURL
curl -X GET "https://localhost:7139/api/Users/check-username/john_doe"
```

---

### 2. Register (Updated)
**Endpoint**: `POST /api/Auth/register`

**Request Body**:
```json
{
  "email": "john@example.com",
  "password": "SecurePassword123!",
  "userName": "john_doe",        // NEW - Required, unique
  "displayName": "John Doe",     // Can be changed later
  "countryCode": "US"
}
```

**Response**:
```json
{
  "userId": 123,
  "email": "john@example.com",
  "userName": "john_doe",         // NEW
  "displayName": "John Doe",      // NEW
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

**Validation**:
- `userName`: Required, 3-20 chars, alphanumeric + underscores, must be unique
- `displayName`: Required, can contain any characters
- `email`: Required, valid email format
- `password`: Required

**Error Responses**:

Username already taken:
```json
409 Conflict
"Username is already taken."
```

Invalid format:
```json
400 Bad Request
"Username can only contain letters, numbers, and underscores."
```

**Example Usage**:
```powershell
# PowerShell
$body = @{
    email = "john@example.com"
    password = "SecurePass123!"
    userName = "john_doe"
    displayName = "John Doe"
    countryCode = "US"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:7139/api/Auth/register" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"

Write-Host "User created: $($response.userName)"
Write-Host "Token: $($response.token)"
```

```bash
# cURL
curl -X POST "https://localhost:7139/api/Auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "SecurePass123!",
    "userName": "john_doe",
    "displayName": "John Doe",
    "countryCode": "US"
  }'
```

---

### 3. Login (Updated)
**Endpoint**: `POST /api/Auth/login`

**Request Body**:
```json
{
  "email": "john@example.com",
  "password": "SecurePassword123!"
}
```

**Response** (now includes userName):
```json
{
  "userId": 123,
  "email": "john@example.com",
  "userName": "john_doe",         // NEW
  "displayName": "John Doe",
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

---

### 4. Get User Profile (Updated)
**Endpoint**: `GET /api/Users/{id}`

**Response** (now includes userName, bio, avatarUrl):
```json
{
  "userId": 123,
  "email": "john@example.com",
  "userName": "john_doe",              // NEW
  "displayName": "John Doe",
  "bio": "Passionate upcycler and DIY enthusiast",  // NEW
  "avatarUrl": "https://...",          // NEW
  "countryCode": "US",
  "createdAtUtc": "2024-01-15T10:30:00Z",
  "updatedAtUtc": null,
  "followerCount": 42,
  "followingCount": 18,
  "projectCount": 7,
  "ideaCount": 23,
  "isFollowing": false,
  "isFollowedBy": false
}
```

---

### 5. Search Users (Updated)
**Endpoint**: `GET /api/Users/search?query={query}&skip={skip}&take={take}`

**What changed**: Search now includes **userName** in the search criteria

**Searches**: DisplayName, UserName, Email

**Example**:
```bash
GET /api/Users/search?query=john&skip=0&take=20
```

**Response**:
```json
{
  "results": [
    {
      "userId": 123,
      "userName": "john_doe",
      "displayName": "John Doe",
      "bio": "Passionate upcycler",
      "avatarUrl": "https://...",
      // ...other fields
    }
  ],
  "total": 1,
  "skip": 0,
  "take": 20
}
```

---

## ??? Code Changes Summary

### Database Layer

#### Models Updated:
1. **UserRecord.cs** - Added `UserName`, `Bio`, `AvatarUrl`
2. **UserAuthRecord.cs** - Added `UserName`
3. **UserProfileDto.cs** - Already had fields (no changes needed)

#### Repository Interfaces:
1. **IUserRepository.cs**
   - Added `GetByUserNameAsync(string userName)`
   - Added `UserNameExistsAsync(string userName)`
   - Updated `CreateAsync()` to include `userName` parameter

2. **IAuthRepository.cs**
   - Added `UserNameExistsAsync(string userName)`
   - Updated `CreateUserWithCredentialAsync()` to include `userName`

#### Repository Implementations:
1. **UserRepository.cs**
   - Implemented `GetByUserNameAsync()`
   - Implemented `UserNameExistsAsync()`
   - Updated all SQL queries to include `UserName`, `Bio`, `AvatarUrl`

2. **AuthRepository.cs**
   - Implemented `UserNameExistsAsync()`
   - Updated `CreateUserWithCredentialAsync()` to insert `userName`
   - Updated `GetUserByEmailAsync()` to select `userName`

### API Layer

#### Controllers Updated:
1. **AuthController.cs**
   - Added username validation (3-20 chars, alphanumeric + underscores)
   - Added username uniqueness check
   - Updated `RegisterRequest` to include `UserName`
   - Updated `AuthResponse` to include `UserName`

2. **UsersController.cs**
   - Added `CheckUsername` endpoint (`GET /api/Users/check-username/{userName}`)
   - Updated `CreateUserDto` to include `UserName`
   - Added username validation in `Create` method

---

## ?? Security & Validation

### Username Rules:
- **Length**: 3-20 characters
- **Format**: Alphanumeric characters and underscores only (`^[a-zA-Z0-9_]+$`)
- **Uniqueness**: Case-insensitive unique constraint
- **Permanence**: Cannot be changed after registration

### DisplayName Rules:
- **Required** at registration
- Can contain any characters
- Can be changed later (via profile update)
- Shows prominently on profile

### Database Constraints:
```sql
-- Unique constraint on UserName
CREATE UNIQUE NONCLUSTERED INDEX IX_User_UserName_Unique 
ON app.[User](UserName);

-- NOT NULL constraint
ALTER COLUMN UserName NVARCHAR(20) NOT NULL;
```

---

## ?? Data Model

### User Table Schema (After Migration):
```sql
CREATE TABLE app.[User] (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(320) NOT NULL,
    UserName NVARCHAR(20) NOT NULL,      -- NEW
    DisplayName NVARCHAR(100) NOT NULL,
    Bio NVARCHAR(500) NULL,              -- NEW
    AvatarUrl NVARCHAR(500) NULL,        -- NEW
    CountryCode NVARCHAR(10) NULL,
    CreatedAtUtc DATETIME NOT NULL,
    UpdatedAtUtc DATETIME NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    CONSTRAINT UQ_User_Email UNIQUE (Email),
    CONSTRAINT IX_User_UserName_Unique UNIQUE (UserName)
);
```

---

## ?? Testing Guide

### Test 1: Check Username Availability
```powershell
# Check available username
Invoke-RestMethod -Uri "https://localhost:7139/api/Users/check-username/testuser123" -Method Get

# Expected: { available: true, message: null }

# Check taken username (after registration)
Invoke-RestMethod -Uri "https://localhost:7139/api/Users/check-username/testuser123" -Method Get

# Expected: { available: false, message: "Username is already taken" }
```

### Test 2: Register with Username
```powershell
$registerBody = @{
    email = "test@example.com"
    password = "TestPass123!"
    userName = "testuser123"
    displayName = "Test User"
    countryCode = "US"
} | ConvertTo-Json

$registerResponse = Invoke-RestMethod `
    -Uri "https://localhost:7139/api/Auth/register" `
    -Method Post `
    -Body $registerBody `
    -ContentType "application/json"

Write-Host "Registration successful!"
Write-Host "UserName: $($registerResponse.userName)"
Write-Host "DisplayName: $($registerResponse.displayName)"
$token = $registerResponse.token
```

### Test 3: Login and Get Profile
```powershell
# Login
$loginBody = @{
    email = "test@example.com"
    password = "TestPass123!"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod `
    -Uri "https://localhost:7139/api/Auth/login" `
    -Method Post `
    -Body $loginBody `
    -ContentType "application/json"

$token = $loginResponse.token
$userId = $loginResponse.userId

# Get profile
$headers = @{ Authorization = "Bearer $token" }
$profile = Invoke-RestMethod `
    -Uri "https://localhost:7139/api/Users/$userId" `
    -Method Get `
    -Headers $headers

Write-Host "Profile:"
Write-Host "  UserName: $($profile.userName)"
Write-Host "  DisplayName: $($profile.displayName)"
Write-Host "  Bio: $($profile.bio)"
```

### Test 4: Search Users by Username
```powershell
$searchResults = Invoke-RestMethod `
    -Uri "https://localhost:7139/api/Users/search?query=testuser&skip=0&take=10" `
    -Method Get `
    -Headers $headers

Write-Host "Found $($searchResults.total) users"
$searchResults.results | ForEach-Object {
    Write-Host "  - $($_.userName) ($($_.displayName))"
}
```

### Test 5: Validation Errors
```powershell
# Test invalid username format
$invalidBody = @{
    email = "test2@example.com"
    password = "TestPass123!"
    userName = "test user!"  # Spaces and special chars not allowed
    displayName = "Test User 2"
} | ConvertTo-Json

try {
    Invoke-RestMethod `
        -Uri "https://localhost:7139/api/Auth/register" `
        -Method Post `
        -Body $invalidBody `
        -ContentType "application/json"
} catch {
    Write-Host "Expected error: $($_.Exception.Response.StatusCode)"
    # Should be 400 Bad Request: "Username can only contain letters, numbers, and underscores."
}

# Test duplicate username
$duplicateBody = @{
    email = "test3@example.com"
    password = "TestPass123!"
    userName = "testuser123"  # Already taken
    displayName = "Test User 3"
} | ConvertTo-Json

try {
    Invoke-RestMethod `
        -Uri "https://localhost:7139/api/Auth/register" `
        -Method Post `
        -Body $duplicateBody `
        -ContentType "application/json"
} catch {
    Write-Host "Expected error: $($_.Exception.Response.StatusCode)"
    # Should be 409 Conflict: "Username is already taken."
}
```

---

## ?? Frontend Integration Notes

### Registration Flow:
```javascript
// 1. Check username availability as user types
const checkUsername = async (userName) => {
  const response = await fetch(`/api/Users/check-username/${userName}`);
  const data = await response.json();
  return data; // { available: true/false, message: "..." }
};

// 2. Show real-time feedback
<input 
  type="text" 
  onChange={(e) => {
    const result = await checkUsername(e.target.value);
    setUsernameAvailable(result.available);
    setUsernameMessage(result.message);
  }}
/>

// 3. Register with username
const register = async (formData) => {
  const response = await fetch('/api/Auth/register', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      email: formData.email,
      password: formData.password,
      userName: formData.userName,      // Required
      displayName: formData.displayName, // Required
      countryCode: formData.countryCode
    })
  });
  return response.json(); // { userId, email, userName, displayName, token }
};
```

### Profile Display Logic:
```javascript
// Username: Permanent identifier (shown as @userName)
// DisplayName: What appears prominently on profile

<div className="user-profile">
  <h1>{profile.displayName}</h1>
  <p className="username">@{profile.userName}</p>
  <p className="bio">{profile.bio}</p>
  <img src={profile.avatarUrl} alt="Avatar" />
</div>
```

---

## ? Migration Checklist

- [x] Create SQL migration file (`add_username_displayname_columns.sql`)
- [x] Update database models (`UserRecord`, `UserAuthRecord`)
- [x] Update repository interfaces (`IUserRepository`, `IAuthRepository`)
- [x] Update repository implementations (`UserRepository`, `AuthRepository`)
- [x] Update AuthController (registration with username)
- [x] Add check-username endpoint to UsersController
- [x] Add username validation (3-20 chars, alphanumeric + underscores)
- [x] Build verification (successful ?)

### Next Steps (Manual):
1. **Run SQL migration** on your database
2. **Test endpoints** using the testing guide above
3. **Update frontend** to use new username fields
4. **(Optional) Add profile update endpoint** to allow changing DisplayName, Bio, AvatarUrl

---

## ?? Key Differences: UserName vs DisplayName

| Feature | UserName | DisplayName |
|---------|----------|-------------|
| **Purpose** | Unique identifier, like Twitter/Instagram handle | Public-facing name |
| **Format** | Alphanumeric + underscores only | Any characters |
| **Length** | 3-20 characters | Up to 120 characters |
| **Uniqueness** | Must be unique (case-insensitive) | Can be duplicates |
| **Changeability** | **Cannot be changed** after registration | Can be changed anytime |
| **Display** | Shown as `@userName` | Prominently displayed |
| **Example** | `john_doe_1990` | `John Doe ?` |

---

## ?? Example User Flow

1. **User visits registration page**
2. **User types username** ? Frontend calls `GET /api/Users/check-username/john_doe`
3. **System responds** ? `{ available: true }` or `{ available: false, message: "Username is already taken" }`
4. **User fills form**:
   - Email: `john@example.com`
   - Password: `SecurePass123!`
   - Username: `john_doe` (unique, permanent)
   - Display Name: `John Doe` (can change later)
5. **User submits** ? `POST /api/Auth/register`
6. **System validates**:
   - Username format (alphanumeric + underscores)
   - Username uniqueness
   - Email uniqueness
7. **User created** ? Returns token + user info
8. **Profile shows**:
   - Header: `John Doe` (DisplayName)
   - Subheader: `@john_doe` (UserName)

---

## ?? Additional Resources

### Related Endpoints:
- `GET /api/Users/{id}` - Get user profile
- `GET /api/Users/search?query={query}` - Search users
- `GET /api/Users/{id}/followers` - Get followers
- `GET /api/Users/{id}/following` - Get following

### Database Indexes:
- `IX_User_UserName_Unique` - Unique index on UserName (case-insensitive)
- `IX_Follow_FollowerId` - Follow queries by follower
- `IX_Follow_FollowingId` - Follow queries by following

---

**?? Implementation Complete!**

All backend changes are ready. Run the SQL migration and start testing!
