# SaveCount Table Name Fix - Summary

## Issue
The backend was throwing an error when fetching user profiles:
```
Invalid object name 'app.Save'
```

## Root Cause
The `UserRepository.cs` SQL queries were referencing a table called `app.Save`, but the actual table name in the database is `app.SavedIdea`.

This mismatch was causing SQL errors when:
- Fetching user profiles via `GET /api/Users/{id}`
- Searching users via `GET /api/Users/search`

## Fix Applied

### Changed Files
1. **`Relyf.Repository/Dapper/UserRepository.cs`**
   - Updated `GetProfileAsync` method
   - Updated `SearchAsync` method

### SQL Query Changes

#### Before (? Incorrect)
```sql
(SELECT COUNT(*) FROM app.[Save] WHERE UserId = u.UserId) as SaveCount
```

#### After (? Correct)
```sql
(SELECT COUNT(*) FROM app.[SavedIdea] WHERE UserId = u.UserId) as SaveCount
```

## Verification

### Database Table Structure
The correct table name is confirmed by checking `SaveRepository.cs`, which uses:
```sql
INSERT INTO app.SavedIdea (UserId, IdeaId, SavedAtUtc)
DELETE FROM app.SavedIdea WHERE UserId=@userId AND IdeaId=@ideaId
```

### Build Status
- ? Build successful
- ? No compilation errors
- ? All references updated

## Impact

### Fixed Endpoints
1. **`GET /api/Users/{id}`** - User profile retrieval
2. **`GET /api/Users/search`** - User search

### Expected Behavior
Both endpoints now correctly return `saveCount` for users:
```json
{
  "userId": 4,
  "userName": "john_doe",
  "displayName": "John Doe",
  "followerCount": 10,
  "followingCount": 5,
  "projectCount": 8,
  "ideaCount": 12,
  "saveCount": 3  // ? Now works correctly
}
```

## Testing

### Manual Test
```bash
# Get user profile (replace {userId} and {token})
curl -X GET "https://localhost:7139/api/Users/{userId}" \
  -H "Authorization: Bearer {token}"

# Search users
curl -X GET "https://localhost:7139/api/Users/search?query=john" \
  -H "Authorization: Bearer {token}"
```

### Expected Result
- No SQL errors
- `saveCount` field populated with correct count
- Frontend can now display save count on user profiles

## Related Files
- `UserRepository.cs` - Database queries
- `UserProfileDto.cs` - Data model
- `SaveRepository.cs` - Reference for correct table name
- `USER_PROFILE_SAVE_COUNT_FIX.md` - Original feature documentation (updated)

## Summary
| Aspect | Status |
|--------|--------|
| Issue Identified | ? Complete |
| Root Cause Found | ? Complete |
| Code Fixed | ? Complete |
| Build Passing | ? Complete |
| Documentation Updated | ? Complete |
| Ready for Testing | ? Yes |

---

**Status**: ? **FIXED AND READY FOR DEPLOYMENT**

**Key Change**: Changed `app.[Save]` to `app.[SavedIdea]` in SQL queries

---

*Fixed: 2025-01-12*
*Issue: Invalid table name in SaveCount queries*
