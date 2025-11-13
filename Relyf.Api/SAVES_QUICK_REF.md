# ?? Saves Endpoint - Quick Reference (Updated)

## Endpoint
```
GET /api/Saves/user/{userId}
```

## Authentication
?? **REQUIRED** - Must provide valid JWT token

## Privacy Rules

| Scenario | Access | Status Code |
|----------|--------|-------------|
| View own saves | ? Allowed | 200 OK |
| View followed user's saves | ? Allowed | 200 OK |
| View non-followed user's saves | ? Denied | 403 Forbidden |

## Quick Examples

### View Your Own Saves
```bash
curl -X GET "https://localhost:7139/api/Saves/user/1" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### View Followed User's Saves
```bash
# First, follow the user
curl -X POST "https://localhost:7139/api/Follow" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"followingId": 4}'

# Then view their saves
curl -X GET "https://localhost:7139/api/Saves/user/4" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Response Format
```json
[
  {
    "ideaId": 123,
    "userId": 4,
    "ideaTitle": "Upcycled Lamp from Bottles",
    "ideaText": "Transform glass bottles into...",
    "createdAt": "2025-01-12T10:00:00Z",
    "savedAt": "2025-01-12T15:30:00Z"
  }
]
```

## Frontend Integration

### React Hook
```javascript
import { useState } from 'react';

function useUserSaves(userId) {
  const [saves, setSaves] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  
  const fetchSaves = async () => {
    setLoading(true);
    setError(null);
    
    try {
      const token = localStorage.getItem('authToken');
      const response = await fetch(
        `https://localhost:7139/api/Saves/user/${userId}`,
        { headers: { 'Authorization': `Bearer ${token}` }}
      );
      
      if (response.status === 403) {
        setError('You must follow this user to view their saves');
        return;
      }
      
      if (!response.ok) throw new Error('Failed to load saves');
      
      const data = await response.json();
      setSaves(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };
  
  return { saves, loading, error, fetchSaves };
}
```

### Conditional Button Display
```javascript
function SavesButton({ userId, currentUserId, isFollowing }) {
  const { saves, loading, error, fetchSaves } = useUserSaves(userId);
  
  // Own profile - always show
  if (userId === currentUserId) {
    return (
      <button onClick={fetchSaves}>
        ?? View My Saves ({saves.length})
      </button>
    );
  }
  
  // Following user - show saves button
  if (isFollowing) {
    return (
      <button onClick={fetchSaves}>
        ?? View Saves
      </button>
    );
  }
  
  // Not following - show follow prompt
  return (
    <button onClick={() => followUser(userId)}>
      ? Follow to View Saves
    </button>
  );
}
```

## Common Errors

| Error | Cause | Solution |
|-------|-------|----------|
| 401 Unauthorized | No/invalid JWT token | Include valid Bearer token |
| 403 Forbidden | Not following user | Follow the user first via POST /api/Follow |
| 404 Not Found | User doesn't exist | Verify userId is valid |

## Related Endpoints

- `POST /api/Follow` - Follow a user (enables save viewing)
- `DELETE /api/Follow/{userId}` - Unfollow (disables save viewing)
- `PUT /api/Saves` - Save an idea
- `DELETE /api/Saves` - Unsave an idea

## Testing

Run the test script:
```powershell
.\TEST_SAVES_FOLLOWING.ps1
```

## Privacy & Security

? **Protects User Privacy**
- Random users cannot view your saves
- Only followers can see what you've saved

? **Enables Social Discovery**
- Follow interesting users
- See what they're saving
- Discover new ideas

? **IDOR Protection**
- Cannot access saves from non-followed users
- Token validation required
- User ID verification enforced

## Frontend URL Format

? **WRONG** (causes 403 error):
```javascript
const url = `/api/Saves/user/${userId}:1`;  // Don't add :1
```

? **CORRECT**:
```javascript
const url = `/api/Saves/user/${userId}`;
```

## Notes

- ?? Saves are private by default
- ?? Following creates a trust relationship
- ?? Security maintained via follow system
- ? No database changes required
- ? Works with existing follow system

---

**See full documentation:** [SAVES_PRIVACY_FIX_SUMMARY.md](SAVES_PRIVACY_FIX_SUMMARY.md)
