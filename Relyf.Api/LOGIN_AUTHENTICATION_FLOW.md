# ?? Login Authentication Flow - Clarification

## ? Current Implementation is CORRECT

The login system **does NOT query by UserName** for authentication. It only uses **Email and Password**.

---

## ?? Login Flow Step-by-Step

### 1. **User Submits Login**
```json
POST /api/Auth/login
{
  "email": "john@example.com",
  "password": "SecurePass123!"
}
```

### 2. **Backend Authenticates (Email + Password Only)**

```csharp
// Step 1: Get user by EMAIL only
var user = await _auth.GetUserByEmailAsync(email, ct);
```

**SQL Query:**
```sql
SELECT UserId, Email, UserName, DisplayName, CountryCode
FROM app.[User]
WHERE Email = @email AND IsDeleted = 0;
```

**Key Points:**
- ? **WHERE clause uses Email** (not UserName)
- ? Email is the primary authentication identifier
- ?? UserName is only **selected** for the response, not used in WHERE

### 3. **Verify Password**

```csharp
// Step 2: Get credentials by UserId (from step 1)
var cred = await _auth.GetCredentialAsync(user.UserId, ct);

// Step 3: Verify password hash
if (!PasswordHasher.Verify(req.Password, cred.PasswordSalt, cred.PasswordHash))
    return Unauthorized("Invalid credentials.");
```

**SQL Query:**
```sql
SELECT UserId, PasswordHash, PasswordSalt
FROM app.UserCredential
WHERE UserId = @userId;
```

### 4. **Generate Token & Return User Info**

```csharp
var token = _tokens.Create(user.UserId, user.Email, user.DisplayName);
return Ok(new AuthResponse(
    user.UserId, 
    user.Email, 
    user.UserName,      // Included in response (not used for auth)
    user.DisplayName, 
    token
));
```

**Response:**
```json
{
  "userId": 123,
  "email": "john@example.com",
  "userName": "john_doe",        // Returned for display, not auth
  "displayName": "John Doe",
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

---

## ?? Authentication vs Response Data

### Authentication (WHERE clause):
| Field | Used for Login? | Purpose |
|-------|----------------|---------|
| **Email** | ? YES | Primary login identifier |
| **Password** | ? YES | Authentication credential |
| **UserName** | ? NO | Not used for authentication |

### Response Data (SELECT clause):
| Field | Returned? | Purpose |
|-------|-----------|---------|
| **UserId** | ? YES | User identifier |
| **Email** | ? YES | User's email |
| **UserName** | ? YES | For display on frontend |
| **DisplayName** | ? YES | For display on frontend |
| **Token** | ? YES | JWT for subsequent requests |

---

## ?? Complete Authentication Queries

### Registration Flow
```sql
-- Check email exists
SELECT COUNT(1) FROM app.[User] WHERE Email = @email;

-- Check username exists
SELECT COUNT(1) FROM app.[User] WHERE UserName = @userName;

-- Create user (if checks pass)
INSERT INTO app.[User] (Email, UserName, DisplayName, CountryCode, IsDeleted)
VALUES (@email, @userName, @displayName, @countryCode, 0);

-- Create credentials
INSERT INTO app.UserCredential (UserId, PasswordHash, PasswordSalt)
VALUES (@userId, @passwordHash, @passwordSalt);
```

### Login Flow
```sql
-- Get user by EMAIL (not UserName)
SELECT UserId, Email, UserName, DisplayName, CountryCode
FROM app.[User]
WHERE Email = @email AND IsDeleted = 0;

-- Get credentials by UserId
SELECT UserId, PasswordHash, PasswordSalt
FROM app.UserCredential
WHERE UserId = @userId;

-- Password verification happens in C# code, not SQL
```

---

## ?? Why Email for Login?

1. **Standard Practice**: Most applications use email for login
2. **User Familiarity**: Users expect to login with email
3. **Password Reset**: Email is needed for password recovery anyway
4. **Uniqueness**: Email is already unique and verified
5. **Professional**: Business standard for authentication

**UserName is for:**
- Social features (mentions, tagging)
- Profile URLs (`relyf.com/@username`)
- Display purposes
- Search functionality

---

## ?? Security Summary

### ? Correct Implementation:
```csharp
// LOGIN - Uses Email
var user = await _auth.GetUserByEmailAsync(email, ct);

// Not this (UserName is NOT used for login):
// var user = await _auth.GetUserByUserNameAsync(userName, ct); ?
```

### ? Authentication Flow:
1. **Input**: Email + Password
2. **Lookup**: By Email (in User table)
3. **Verify**: Password hash (in UserCredential table)
4. **Return**: UserId, Email, **UserName**, DisplayName, Token

### ? SQL Queries:
```sql
-- Authentication query (uses Email in WHERE)
SELECT UserId, Email, UserName, DisplayName, CountryCode
FROM app.[User]
WHERE Email = @email AND IsDeleted = 0;  -- ? Email in WHERE clause
```

---

## ?? Summary

| Component | Uses UserName? | Purpose |
|-----------|---------------|---------|
| **Login Request** | ? NO | Only Email + Password |
| **Login SQL Query** | ? NO (WHERE) | Searches by Email |
| **Login SQL Query** | ? YES (SELECT) | Returns UserName in result |
| **Login Response** | ? YES | Included for frontend display |
| **Registration** | ? YES | Required field, must be unique |
| **Profile Display** | ? YES | Shows as @userName |
| **User Search** | ? YES | Searchable field |

---

## ?? Frontend Usage

### Login Form (Email-based):
```javascript
const login = async (email, password) => {
  const response = await fetch('/api/Auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      email: email,        // ? Email for login
      password: password
      // NO userName field needed here
    })
  });
  
  const data = await response.json();
  // data.userName is available in response for display
  console.log(`Welcome, ${data.displayName} (@${data.userName})`);
};
```

### Profile Display (UserName for social features):
```javascript
<div className="user-profile">
  <h1>{user.displayName}</h1>
  <p className="username">@{user.userName}</p>  {/* Social handle */}
  <p className="email">{user.email}</p>          {/* Login credential */}
</div>
```

---

## ? Implementation Checklist

- [x] Login authenticates with **Email + Password** only
- [x] UserName is **NOT** in login request body
- [x] SQL WHERE clause uses **Email**, not UserName
- [x] UserName is **returned** in login response
- [x] UserName is used for **social features** (profile, search)
- [x] Registration **requires** unique UserName
- [x] Username **cannot be changed** after registration
- [x] DisplayName **can be changed** later

---

**? CONFIRMED: Login authentication is correctly implemented using Email + Password.**

**UserName is only used for display and social features, not for authentication.**
