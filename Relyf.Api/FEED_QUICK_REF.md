# ?? Feed Endpoint - Quick Reference

## Endpoint
```
GET /api/Feed?skip={offset}&take={limit}
```

## Authentication
? **REQUIRED** - Must provide valid JWT token

## Quick Example

```bash
curl -X GET "https://localhost:7139/api/Feed?skip=0&take=20" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Response
```json
{
  "items": [
    {
      "itemType": "project",
      "itemId": 123,
      "userId": 456,
      "userName": "john_doe",
      "displayName": "John Doe",
      "avatarUrl": "https://...",
      "title": "Upcycled Lamp",
      "description": "A beautiful lamp...",
      "createdAtUtc": "2025-01-12T10:30:00Z",
      "status": "completed",
      "reactionCount": 0,
      "commentCount": 0,
      "saveCount": 0,
      "hasUserReacted": false,
      "hasUserSaved": false
    }
  ],
  "total": 150,
  "skip": 0,
  "take": 20
}
```

## Item Types
- `"project"` - User-created project
- `"idea"` - AI-generated idea

## Parameters

| Parameter | Type | Default | Max | Required |
|-----------|------|---------|-----|----------|
| skip | int | 0 | - | No |
| take | int | 20 | 100 | No |

## Frontend Integration

### React Hook
```javascript
import { useState, useEffect } from 'react';

function useFeed() {
  const [feed, setFeed] = useState([]);
  const [loading, setLoading] = useState(true);
  const [skip, setSkip] = useState(0);
  
  const loadMore = async () => {
    const token = localStorage.getItem('authToken');
    const response = await fetch(
      `https://localhost:7139/api/Feed?skip=${skip}&take=20`,
      { headers: { 'Authorization': `Bearer ${token}` }}
    );
    const data = await response.json();
    
    setFeed(prev => [...prev, ...data.items]);
    setSkip(prev => prev + 20);
  };
  
  useEffect(() => { loadMore(); }, []);
  
  return { feed, loadMore, loading };
}
```

### Display Feed Item
```javascript
function FeedItem({ item }) {
  const icon = item.itemType === 'project' ? '??' : '??';
  
  return (
    <div className="feed-item">
      <div className="header">
        <img src={item.avatarUrl || '/default.png'} />
        <div>
          <h3>{item.displayName}</h3>
          <span>@{item.userName}</span>
        </div>
        <span className="type">{icon} {item.itemType}</span>
      </div>
      
      <h2>{item.title}</h2>
      
      {item.itemType === 'project' && item.description && (
        <p>{item.description}</p>
      )}
      
      {item.itemType === 'idea' && item.ideaText && (
        <p>{item.ideaText}</p>
      )}
      
      {item.status && (
        <span className="status">{item.status}</span>
      )}
      
      <div className="stats">
        <span>?? {item.reactionCount}</span>
        <span>?? {item.commentCount}</span>
        <span>?? {item.saveCount}</span>
      </div>
      
      <small>{new Date(item.createdAtUtc).toLocaleString()}</small>
    </div>
  );
}
```

## Testing

```powershell
.\TEST_FEED_ENDPOINT.ps1
```

## Common Errors

| Error | Cause | Solution |
|-------|-------|----------|
| 401 Unauthorized | No/invalid JWT token | Include valid Bearer token in Authorization header |
| 500 Internal Server Error | Database/query issue | Check logs, verify Follow table exists |
| Empty feed | User follows nobody | Follow users to see their content |

## Related Endpoints
- `POST /api/Follow` - Follow a user
- `GET /api/Users/search` - Find users to follow
- `POST /api/Projects` - Create a project
- `POST /api/AIIdeas` - Create an AI idea

## Notes
- ? Feed only shows content from users you follow
- ? Sorted by creation date (newest first)
- ? Includes both projects and AI ideas
- ? Engagement metrics ready but currently return 0 (tables not yet implemented)
- ? User interaction status ready but currently return false

## Current Status
- ? Endpoint working
- ? Authentication required
- ? Pagination working
- ? Returns mixed content (projects + ideas)
- ? Engagement metrics (pending Reaction/Comment/Save tables)

---

**See full documentation:** [FEED_ENDPOINT_COMPLETE.md](FEED_ENDPOINT_COMPLETE.md)
