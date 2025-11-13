# Follow/Follower System API Documentation

## Overview
Complete implementation of user follow/follower functionality for the Relyf platform.

---

## ?? Table of Contents
1. [Database Setup](#database-setup)
2. [API Endpoints](#api-endpoints)
3. [Authentication](#authentication)
4. [Response Models](#response-models)
5. [Testing](#testing)
6. [Implementation Details](#implementation-details)

---

## ??? Database Setup

### Run the Migration Script

Execute the following SQL script to create the Follow table:

```bash
# From the Relyf.Api directory
sqlcmd -S localhost -d RelyfDb -i create_follows_table.sql
```

Or run it directly in SQL Server Management Studio.

**Script Location:** `create_follows_table.sql`

**What it creates:**
- `app.Follow` table with FollowId, FollowerId, FollowingId, CreatedAtUtc
- Foreign key constraints to User table
- Unique constraint to prevent duplicate follows
- Check constraint to prevent self-following
- Indexes for optimized queries

---

## ?? API Endpoints

### 1. **Search Users**

Search for users by display name or email.

**Endpoint:** `GET /api/Users/search`

**Query Parameters:**
- `query` (string, optional): Search term (searches DisplayName and Email)
- `skip` (int, default: 0): Pagination offset
- `take` (int, default: 20, max: 100): Items per page

**Authentication:** Optional (returns relationship status if authenticated)

**Example Request:**
```http
GET /api/Users/search?query=john&skip=0&take=10
Authorization: Bearer {token}
```

**Example Response:**
```json
{
  "results": [
    {
      "userId": 5,
      "email": "john@example.com",
      "displayName": "John Doe",
      "userName": null,
      "bio": null,
      "avatarUrl": null,
      "countryCode": "US",
      "createdAtUtc": "2024-01-15T10:30:00Z",
      "updatedAtUtc": null,
      "followerCount": 42,
      "followingCount": 28,
      "projectCount": 15,
      "ideaCount": 33,
      "isFollowing": false,
      "isFollowedBy": true
    }
  ],
  "total": 1,
  "skip": 0,
  "take": 10
}
```

---

### 2. **Get User Profile**

Get detailed user profile with counts and relationship status.

**Endpoint:** `GET /api/Users/{id}`

**Path Parameters:**
- `id` (int): User ID

**Authentication:** Optional (returns relationship status if authenticated)

**Example Request:**
```http
GET /api/Users/5
Authorization: Bearer {token}
```

**Example Response:**
```json
{
  "userId": 5,
  "email": "john@example.com",
  "displayName": "John Doe",
  "userName": null,
  "bio": null,
  "avatarUrl": null,
  "countryCode": "US",
  "createdAtUtc": "2024-01-15T10:30:00Z",
  "updatedAtUtc": null,
  "followerCount": 42,
  "followingCount": 28,
  "projectCount": 15,
  "ideaCount": 33,
  "isFollowing": false,
  "isFollowedBy": true
}
```

**Status Codes:**
- `200 OK`: Success
- `404 Not Found`: User not found

---

### 3. **Get User Followers**

Get list of users who follow a specific user.

**Endpoint:** `GET /api/Users/{id}/followers`

**Path Parameters:**
- `id` (int): User ID

**Authentication:** Optional

**Example Request:**
```http
GET /api/Users/5/followers
Authorization: Bearer {token}
```

**Example Response:**
```json
[
  {
    "userId": 10,
    "email": "jane@example.com",
    "displayName": "Jane Smith",
    "userName": null,
    "bio": null,
    "avatarUrl": null,
    "countryCode": "CA",
    "createdAtUtc": "2024-01-10T08:00:00Z",
    "updatedAtUtc": null,
    "followerCount": 15,
    "followingCount": 20,
    "projectCount": 8,
    "ideaCount": 12,
    "isFollowing": true,
    "isFollowedBy": false
  }
]
```

---

### 4. **Get Following**

Get list of users that a specific user is following.

**Endpoint:** `GET /api/Users/{id}/following`

**Path Parameters:**
- `id` (int): User ID

**Authentication:** Optional

**Example Request:**
```http
GET /api/Users/5/following
```

**Example Response:**
```json
[
  {
    "userId": 12,
    "email": "mike@example.com",
    "displayName": "Mike Johnson",
    "userName": null,
    "bio": null,
    "avatarUrl": null,
    "countryCode": "UK",
    "createdAtUtc": "2024-01-05T12:00:00Z",
    "updatedAtUtc": null,
    "followerCount": 50,
    "followingCount": 35,
    "projectCount": 22,
    "ideaCount": 45,
    "isFollowing": false,
    "isFollowedBy": false
  }
]
```

---

### 5. **Follow User**

Follow another user.

**Endpoint:** `POST /api/Follow`

**Authentication:** **Required**

**Request Body:**
```json
{
  "followingId": 5
}
```

**Example Request:**
```http
POST /api/Follow
Authorization: Bearer {token}
Content-Type: application/json

{
  "followingId": 5
}
```

**Example Response:**
```json
{
  "followId": 123,
  "followerId": 10,
  "followingId": 5,
  "createdAtUtc": "2024-01-20T15:30:00Z"
}
```

**Status Codes:**
- `201 Created`: Successfully followed
- `400 Bad Request`: Cannot follow yourself
- `401 Unauthorized`: Not authenticated
- `409 Conflict`: Already following this user

---

### 6. **Unfollow User**

Unfollow a user.

**Endpoint:** `DELETE /api/Follow/{followingId}`

**Path Parameters:**
- `followingId` (int): ID of user to unfollow

**Authentication:** **Required**

**Example Request:**
```http
DELETE /api/Follow/5
Authorization: Bearer {token}
```

**Status Codes:**
- `204 No Content`: Successfully unfollowed
- `401 Unauthorized`: Not authenticated
- `404 Not Found`: Follow relationship not found

---

### 7. **Check Follow Status**

Check if current user is following a specific user.

**Endpoint:** `GET /api/Follow/check/{followingId}`

**Path Parameters:**
- `followingId` (int): User ID to check

**Authentication:** **Required**

**Example Request:**
```http
GET /api/Follow/check/5
Authorization: Bearer {token}
```

**Example Response:**
```json
{
  "isFollowing": true
}
```

**Status Codes:**
- `200 OK`: Success
- `401 Unauthorized`: Not authenticated

---

## ?? Authentication

All endpoints marked as **Required** need JWT authentication.

### How to Authenticate

1. **Login** to get JWT token (use existing auth endpoint)
2. **Include token** in Authorization header:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Token Claims

The JWT token contains:
- `sub`: User ID (used to identify the current user)
- `email`: User email
- `name`: Display name

---

## ?? Response Models

### UserProfileDto

```typescript
{
  userId: number;
  email: string;
  displayName: string;
  userName?: string;          // Currently null, future enhancement
  bio?: string;              // Currently null, future enhancement
  avatarUrl?: string;        // Currently null, future enhancement
  countryCode?: string;
  createdAtUtc: string;      // ISO 8601 format
  updatedAtUtc?: string;     // ISO 8601 format
  
  // Counts
  followerCount: number;     // Number of followers
  followingCount: number;    // Number of users this user follows
  projectCount: number;      // Number of projects created
  ideaCount: number;         // Number of AI ideas created
  
  // Relationship status (from current user's perspective)
  isFollowing: boolean;      // Does current user follow this user?
  isFollowedBy: boolean;     // Does this user follow current user?
}
```

### UserSearchResult

```typescript
{
  results: UserProfileDto[];
  total: number;             // Total matching users
  skip: number;              // Current offset
  take: number;              // Items per page
}
```

### FollowRecord

```typescript
{
  followId: number;
  followerId: number;        // User who is following
  followingId: number;       // User being followed
  createdAtUtc: string;      // ISO 8601 format
}
```

---

## ?? Testing

### PowerShell Test Script

Use the provided PowerShell script to test all endpoints:

```bash
# From Relyf.Api directory
.\TEST_FOLLOW_API.ps1
```

### Manual Testing with cURL

#### Search Users
```bash
curl -X GET "http://localhost:5100/api/Users/search?query=john&skip=0&take=10" \
  -H "Authorization: Bearer {token}"
```

#### Get Profile
```bash
curl -X GET "http://localhost:5100/api/Users/5" \
  -H "Authorization: Bearer {token}"
```

#### Follow User
```bash
curl -X POST "http://localhost:5100/api/Follow" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"followingId": 5}'
```

#### Unfollow User
```bash
curl -X DELETE "http://localhost:5100/api/Follow/5" \
  -H "Authorization: Bearer {token}"
```

#### Check Follow Status
```bash
curl -X GET "http://localhost:5100/api/Follow/check/5" \
  -H "Authorization: Bearer {token}"
```

---

## ??? Implementation Details

### Architecture

```
Controllers/
  ??? UsersController.cs       # User search, profile, followers/following
  ??? FollowController.cs      # Follow/unfollow operations

Repository/
  ??? IUserRepository.cs       # User data interface
  ??? UserRepository.cs        # User data implementation
  ??? IFollowRepository.cs     # Follow data interface
  ??? FollowRepository.cs      # Follow data implementation

Models/
  ??? UserRecord.cs            # Basic user entity
  ??? UserProfileDto.cs        # Extended user with counts
  ??? UserSearchResult.cs      # Search response
  ??? FollowRecord.cs          # Follow relationship entity
```

### Database Schema

```sql
app.[Follow]
  ??? FollowId (PK, IDENTITY)
  ??? FollowerId (FK ? User.UserId)
  ??? FollowingId (FK ? User.UserId)
  ??? CreatedAtUtc

Constraints:
  - UQ_Follow_Follower_Following: Prevents duplicate follows
  - CK_Follow_NoSelfFollow: Prevents self-following
  
Indexes:
  - IX_Follow_FollowerId: Optimizes "who am I following?" queries
  - IX_Follow_FollowingId: Optimizes "who follows me?" queries
```

### Key Features

? **Prevent Self-Following**: Database constraint + API validation
? **Prevent Duplicate Follows**: Unique constraint on (FollowerId, FollowingId)
? **Optimized Queries**: Indexes on both directions of follow relationship
? **Relationship Status**: Each profile shows if authenticated user follows/is followed by that user
? **Pagination**: Search supports skip/take for large result sets
? **Soft Delete Support**: Only shows non-deleted users
? **Authentication**: JWT-based authentication with claims

---

## ?? Frontend Integration Guide

### Using with React/TypeScript

```typescript
// types.ts
export interface UserProfile {
  userId: number;
  email: string;
  displayName: string;
  userName?: string;
  bio?: string;
  avatarUrl?: string;
  countryCode?: string;
  createdAtUtc: string;
  updatedAtUtc?: string;
  followerCount: number;
  followingCount: number;
  projectCount: number;
  ideaCount: number;
  isFollowing: boolean;
  isFollowedBy: boolean;
}

export interface UserSearchResult {
  results: UserProfile[];
  total: number;
  skip: number;
  take: number;
}

// api.ts
const API_BASE = 'http://localhost:5100/api';

export async function searchUsers(
  query: string, 
  skip: number = 0, 
  take: number = 20,
  token?: string
): Promise<UserSearchResult> {
  const headers: HeadersInit = {};
  if (token) headers['Authorization'] = `Bearer ${token}`;
  
  const response = await fetch(
    `${API_BASE}/Users/search?query=${encodeURIComponent(query)}&skip=${skip}&take=${take}`,
    { headers }
  );
  
  if (!response.ok) throw new Error('Search failed');
  return response.json();
}

export async function getUserProfile(
  userId: number,
  token?: string
): Promise<UserProfile> {
  const headers: HeadersInit = {};
  if (token) headers['Authorization'] = `Bearer ${token}`;
  
  const response = await fetch(
    `${API_BASE}/Users/${userId}`,
    { headers }
  );
  
  if (!response.ok) throw new Error('Failed to fetch profile');
  return response.json();
}

export async function followUser(
  followingId: number,
  token: string
): Promise<void> {
  const response = await fetch(`${API_BASE}/Follow`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ followingId })
  });
  
  if (!response.ok) throw new Error('Follow failed');
}

export async function unfollowUser(
  followingId: number,
  token: string
): Promise<void> {
  const response = await fetch(`${API_BASE}/Follow/${followingId}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  if (!response.ok) throw new Error('Unfollow failed');
}

export async function checkFollowStatus(
  followingId: number,
  token: string
): Promise<boolean> {
  const response = await fetch(
    `${API_BASE}/Follow/check/${followingId}`,
    {
      headers: { 'Authorization': `Bearer ${token}` }
    }
  );
  
  if (!response.ok) throw new Error('Status check failed');
  const data = await response.json();
  return data.isFollowing;
}

export async function getFollowers(
  userId: number,
  token?: string
): Promise<UserProfile[]> {
  const headers: HeadersInit = {};
  if (token) headers['Authorization'] = `Bearer ${token}`;
  
  const response = await fetch(
    `${API_BASE}/Users/${userId}/followers`,
    { headers }
  );
  
  if (!response.ok) throw new Error('Failed to fetch followers');
  return response.json();
}

export async function getFollowing(
  userId: number,
  token?: string
): Promise<UserProfile[]> {
  const headers: HeadersInit = {};
  if (token) headers['Authorization'] = `Bearer ${token}`;
  
  const response = await fetch(
    `${API_BASE}/Users/${userId}/following`,
    { headers }
  );
  
  if (!response.ok) throw new Error('Failed to fetch following');
  return response.json();
}
```

---

## ?? Notes

### Future Enhancements

The following fields are currently `null` but can be added later:
- `userName`: Unique handle (e.g., @johndoe)
- `bio`: User biography/description
- `avatarUrl`: Profile picture URL

To add these, you'll need to:
1. Add columns to `app.User` table
2. Update `UserRecord` model
3. Update repository queries
4. Update API DTOs

### Performance Considerations

- Follower/following counts are calculated on-demand
- For very active users, consider caching counts
- Indexes are optimized for read-heavy operations
- Consider pagination for followers/following lists if user has thousands

### Security Notes

- All follow operations require authentication
- Users cannot follow themselves (validated at both DB and API level)
- Follow relationships are unique (cannot follow same user twice)
- Soft-deleted users are filtered from all queries

---

## ? Checklist

Before deploying to production:

- [ ] Run database migration script (`create_follows_table.sql`)
- [ ] Test all endpoints with the PowerShell test script
- [ ] Verify authentication is working correctly
- [ ] Test pagination with large datasets
- [ ] Verify CORS settings for your frontend domain
- [ ] Update frontend to use new endpoints
- [ ] Add monitoring for follow/unfollow operations
- [ ] Consider rate limiting for follow operations

---

## ?? Troubleshooting

### "Table already exists" error
The migration script is idempotent - safe to run multiple times.

### 401 Unauthorized errors
- Verify JWT token is valid and not expired
- Check that Authorization header is formatted as `Bearer {token}`
- Ensure JWT configuration in `appsettings.json` is correct

### 409 Conflict on follow
User is already following the target user. Check with `/api/Follow/check/{id}` first.

### Empty relationship status
If `isFollowing` and `isFollowedBy` are always false, ensure you're passing the JWT token in the request.

---

## ?? Support

For issues or questions:
1. Check the test script output for detailed error messages
2. Review Swagger UI at `http://localhost:5100/swagger`
3. Check application logs for detailed error information

---

**Last Updated:** 2024
**API Version:** 1.0
**Backend Framework:** .NET 8
