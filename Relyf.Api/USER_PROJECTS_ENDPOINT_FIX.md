# ?? User Projects Endpoint Fix

## Problem Summary

The frontend was displaying incorrect project counts on user profile pages:
- **Issue**: Projects (2) shown in tab, but only 1 project rendered
- **Root Cause**: Frontend was calling `GET /api/projects` which returns the **authenticated user's** projects, not the viewed profile owner's projects
- **Result**: Mixed/incorrect data when viewing another user's profile

## Solution Implemented

### Added New Public Endpoint

**New Route**: `GET /api/projects/user/{userId}`

This endpoint allows fetching **any user's projects** for public profile viewing.

---

## Backend Changes

### 1. Repository Interface (`IProjectRepository.cs`)

Added two new methods:

```csharp
// Public profile view - get any user's projects
Task<IEnumerable<ProjectRecord>> GetUserProjectsAsync(int userId, int skip, int take, CancellationToken ct = default);
Task<int> CountUserProjectsAsync(int userId, CancellationToken ct = default);
```

### 2. Repository Implementation (`ProjectRepository.cs`)

```csharp
public Task<IEnumerable<ProjectRecord>> GetUserProjectsAsync(int userId, int skip, int take, CancellationToken ct = default) =>
    WithConnection(conn => conn.QueryAsync<ProjectRecord>(
        new CommandDefinition(
            @"SELECT p.ProjectId, p.IdeaId, p.AiIdeaId, p.UserId, p.Title, p.Description, p.Status, 
                     p.CreatedAtUtc, p.UpdatedAtUtc, p.IsDeleted,
                     (SELECT TOP 1 i.Url 
                      FROM app.Image i 
                      WHERE i.OwnerType = 'Project' 
                        AND i.OwnerId = p.ProjectId 
                      ORDER BY i.CreatedAtUtc ASC) AS ImageUrl
              FROM app.Project p
              WHERE p.UserId = @userId AND p.IsDeleted = 0
              ORDER BY p.CreatedAtUtc DESC
              OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;",
            new { userId, skip, take },
            cancellationToken: ct)));

public Task<int> CountUserProjectsAsync(int userId, CancellationToken ct = default) =>
    WithConnection(conn => conn.ExecuteScalarAsync<int>(
        new CommandDefinition(
            @"SELECT COUNT(*) FROM app.Project WHERE UserId = @userId AND IsDeleted = 0;",
            new { userId },
            cancellationToken: ct)));
```

**Key Differences from `ListAsync`**:
- ? Filters by `p.UserId = @userId` (any user)
- ? No authentication check (public data)
- ? Still filters `IsDeleted = 0`
- ? Includes image URL
- ? Supports pagination

### 3. Controller Endpoint (`ProjectsController.cs`)

```csharp
// GET api/projects/user/{userId} (any user's public projects)
[HttpGet("user/{userId:int}")]
public async Task<ActionResult<PagedProjectsDto>> GetUserProjects(
    int userId, 
    [FromQuery] int skip = 0, 
    [FromQuery] int take = 20, 
    CancellationToken ct = default)
{
    if (take <= 0) take = 20;
    if (take > 100) take = 100;

    // Verify user exists
    if (!await _lookup.UserExistsAsync(userId, ct))
        return NotFound(new { error = "User not found" });

    var rows = await _projects.GetUserProjectsAsync(userId, skip, take, ct);
    var total = await _projects.CountUserProjectsAsync(userId, ct);
    var list = rows.Select(r => new ProjectDto(
        r.ProjectId, r.IdeaId, r.AiIdeaId, r.UserId, 
        r.Title, r.Description, r.Status, r.ImageUrl))
        .ToList();
    
    return new PagedProjectsDto(list, total, skip, take);
}
```

**Features**:
- ? Requires authentication (`[Authorize]` on class)
- ? Validates user exists before querying
- ? Returns 404 if user not found
- ? Returns paginated results
- ? Includes total count for UI

---

## API Usage

### Endpoint Comparison

| Endpoint | Purpose | Returns |
|----------|---------|---------|
| `GET /api/projects` | Get **my** projects | Authenticated user's projects only |
| `GET /api/projects/user/{userId}` | Get **any user's** projects | Specified user's public projects |

### Example Requests

#### 1. Get Current User's Projects (Existing)
```http
GET https://localhost:7099/api/projects?skip=0&take=20
Authorization: Bearer {token}
```

**Response**:
```json
{
  "results": [
    {
      "projectId": 1,
      "ideaId": null,
      "aiIdeaId": 5,
      "userId": 123,  // Always matches authenticated user
      "title": "My Project",
      "description": "Description",
      "status": "in_progress",
      "imageUrl": "/uploads/images/abc123.jpg"
    }
  ],
  "total": 1,
  "skip": 0,
  "take": 20
}
```

#### 2. Get Another User's Projects (NEW)
```http
GET https://localhost:7099/api/projects/user/456?skip=0&take=20
Authorization: Bearer {token}
```

**Response**:
```json
{
  "results": [
    {
      "projectId": 10,
      "ideaId": null,
      "aiIdeaId": 12,
      "userId": 456,  // Profile owner's ID
      "title": "Their Project",
      "description": "Description",
      "status": "completed",
      "imageUrl": "/uploads/images/xyz789.jpg"
    }
  ],
  "total": 1,
  "skip": 0,
  "take": 20
}
```

#### 3. User Not Found
```http
GET https://localhost:7099/api/projects/user/99999
Authorization: Bearer {token}
```

**Response** (404):
```json
{
  "error": "User not found"
}
```

---

## Frontend Integration

### Update RTK Query Hook

**Before** (Incorrect):
```typescript
// UserProfile.tsx
const { data: userProjectsData } = useGetUserProjectsQuery();
// ? This calls GET /api/projects - returns authenticated user's projects
```

**After** (Correct):
```typescript
// Update API slice (api/apiSlice.ts or similar)
getUserProjects: builder.query<PagedProjectsDto, { userId: number; skip?: number; take?: number }>({
  query: ({ userId, skip = 0, take = 20 }) => 
    `/projects/user/${userId}?skip=${skip}&take=${take}`,
  providesTags: (result, error, { userId }) => 
    [{ type: 'Project', id: `USER_${userId}` }],
}),

// UserProfile.tsx
const { data: userProjectsData } = useGetUserProjectsQuery({ 
  userId: viewedUserId,  // ? Profile owner's ID
  skip: 0, 
  take: 20 
});
```

### Remove Frontend Filtering

Since the backend now correctly filters by user ID, you can **remove the defensive filter** in `UserProfile.tsx`:

**Before**:
```typescript
// Defensive filter (was necessary due to backend bug)
const userProjects = useMemo(() => 
  userProjectsRaw?.results.filter(p => p.userId === viewedUserId) ?? [], 
  [userProjectsRaw, viewedUserId]
);
```

**After**:
```typescript
// No filter needed - backend guarantees correct data
const userProjects = userProjectsData?.results ?? [];
```

---

## Testing

### PowerShell Test Script

Save as `TEST_USER_PROJECTS_ENDPOINT.ps1`:

```powershell
# Test User Projects Endpoint

$baseUrl = "https://localhost:7099"
$token = "your_jwt_token_here"

Write-Host "`n=== Test 1: Get User 1's Projects ===" -ForegroundColor Cyan
$response1 = Invoke-RestMethod `
    -Uri "$baseUrl/api/projects/user/1?skip=0&take=20" `
    -Method GET `
    -Headers @{ "Authorization" = "Bearer $token" } `
    -SkipCertificateCheck

Write-Host "Total Projects: $($response1.total)" -ForegroundColor Green
Write-Host "Projects:" -ForegroundColor Yellow
$response1.results | ForEach-Object {
    Write-Host "  - [$($_.projectId)] $($_.title) (User: $($_.userId))" -ForegroundColor White
}

Write-Host "`n=== Test 2: Get User 2's Projects ===" -ForegroundColor Cyan
$response2 = Invoke-RestMethod `
    -Uri "$baseUrl/api/projects/user/2?skip=0&take=20" `
    -Method GET `
    -Headers @{ "Authorization" = "Bearer $token" } `
    -SkipCertificateCheck

Write-Host "Total Projects: $($response2.total)" -ForegroundColor Green
Write-Host "Projects:" -ForegroundColor Yellow
$response2.results | ForEach-Object {
    Write-Host "  - [$($_.projectId)] $($_.title) (User: $($_.userId))" -ForegroundColor White
}

Write-Host "`n=== Test 3: User Not Found (ID 99999) ===" -ForegroundColor Cyan
try {
    Invoke-RestMethod `
        -Uri "$baseUrl/api/projects/user/99999" `
        -Method GET `
        -Headers @{ "Authorization" = "Bearer $token" } `
        -SkipCertificateCheck
} catch {
    Write-Host "Expected 404: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
    $errorBody = $reader.ReadToEnd()
    Write-Host "Error: $errorBody" -ForegroundColor Yellow
}

Write-Host "`n=== Verification Complete ===" -ForegroundColor Green
```

### Expected Results

```
=== Test 1: Get User 1's Projects ===
Total Projects: 2
Projects:
  - [1] Recycled Planter (User: 1)
  - [2] Upcycled Lamp (User: 1)

=== Test 2: Get User 2's Projects ===
Total Projects: 1
Projects:
  - [3] Garden Bench (User: 2)

=== Test 3: User Not Found (ID 99999) ===
Expected 404: NotFound
Error: {"error":"User not found"}

=== Verification Complete ===
```

### SQL Verification

```sql
-- Verify projects are filtered correctly
SELECT p.ProjectId, p.UserId, p.Title, p.Status, p.IsDeleted
FROM app.Project p
WHERE p.UserId = 1 AND p.IsDeleted = 0
ORDER BY p.CreatedAtUtc DESC;

-- Should match API response for GET /api/projects/user/1
```

---

## Security Notes

### ? What's Protected
- **Authentication Required**: Must have valid JWT token
- **User Validation**: Verifies user exists before querying
- **Soft Deletes**: Only returns `IsDeleted = 0` projects
- **Pagination Limits**: Max 100 items per request

### ?? What's Public
- **Any authenticated user** can view any other user's projects
- This is intentional for public profile pages
- Projects are considered **public data** in this application

### ?? If You Need Privacy

To make projects private, add a privacy flag:

**Migration**:
```sql
ALTER TABLE app.Project 
ADD IsPrivate BIT NOT NULL DEFAULT 0;
```

**Repository Update**:
```csharp
WHERE p.UserId = @userId 
  AND p.IsDeleted = 0 
  AND p.IsPrivate = 0  -- Only public projects
```

**Future Enhancement**: Allow viewing private projects only if:
- Viewer is the owner (`@requestingUserId = p.UserId`)
- Or viewer is a follower/friend

---

## Comparison: Before vs After

### Before (Broken)

```
Frontend (viewing User 2's profile):
  ?
GET /api/projects (returns authenticated user's projects)
  ?
Backend: "WHERE p.UserId = @authUserId" (User 1)
  ?
Returns: User 1's projects
  ?
Frontend: Filters projects with userId === 2
  ?
Result: Empty or wrong count
```

### After (Fixed)

```
Frontend (viewing User 2's profile):
  ?
GET /api/projects/user/2 (explicitly requests User 2's projects)
  ?
Backend: "WHERE p.UserId = @userId" (User 2)
  ?
Returns: User 2's projects only
  ?
Frontend: Displays correctly
  ?
Result: Correct count and data
```

---

## Migration Notes

### Breaking Changes
- ? None - this is a **new endpoint**
- ? Existing `GET /api/projects` unchanged
- ? Backward compatible

### Frontend Changes Required
1. Update API slice to use new endpoint
2. Pass `userId` parameter
3. Remove defensive filtering (optional but recommended)

### Database Changes
- ? None required
- ? Uses existing `app.Project` table
- ? Uses existing indexes

---

## Related Files Modified

| File | Change |
|------|--------|
| `Controllers/ProjectController.cs` | Added `GetUserProjects` endpoint |
| `Relyf.Repository/Dapper/IProjectRepository.cs` | Added interface methods |
| `Relyf.Repository/Dapper/ProjectRepository.cs` | Implemented repository methods |

---

## Next Steps

### For Frontend Team

1. **Update API Hook**:
   ```typescript
   // Add to RTK Query slice
   getUserProjects: builder.query<PagedProjectsDto, GetUserProjectsParams>({
     query: ({ userId, skip = 0, take = 20 }) => 
       `/projects/user/${userId}?skip=${skip}&take=${take}`,
   }),
   ```

2. **Update Component**:
   ```typescript
   // UserProfile.tsx
   const { data } = useGetUserProjectsQuery({ 
     userId: viewedUserId,
     skip: 0,
     take: 20
   });
   ```

3. **Remove Filter**:
   ```typescript
   // Delete this defensive code
   // const filtered = data?.results.filter(p => p.userId === viewedUserId);
   
   // Use data directly
   const projects = data?.results ?? [];
   ```

### Optional Enhancements

1. **Add Privacy Flag**: See "Security Notes" section
2. **Add Sorting Options**: Status, date, title
3. **Add Filtering**: By status, idea type
4. **Add Search**: By project title/description

---

## Status

? **Implementation Complete**  
? **Build Successful**  
? **Awaiting Frontend Integration**

---

**Generated**: 2024-01-15  
**Backend Version**: .NET 8  
**Issue**: Fixed project filtering on user profiles  
**Status**: Ready for Testing
