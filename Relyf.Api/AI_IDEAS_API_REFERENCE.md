# AI Ideas API Endpoints - Quick Reference

## Base URL
```
/api/aiideas
```

## Endpoints

### 1. Create AI Idea
```
POST /api/aiideas
Authorization: Bearer {token}
Content-Type: application/json

Request Body:
{
  "title": "string (required)",
  "tools": "string (optional)",
  "steps": "string (optional)",
  "safety": "string (optional)"
}

Response: 201 Created
{
  "aiIdeaId": 1,
  "userId": 5,
  "title": "Reusable Tote Bag",
  "tools": "Needle, thread, scissors",
  "steps": "1. Cut fabric. 2. Sew sides.",
  "safety": "Use scissors carefully",
  "createdAtUtc": "2024-01-15T10:30:00Z",
  "updatedAtUtc": null
}
```

### 2. Get AI Idea by ID
```
GET /api/aiideas/{id}
Authorization: Bearer {token}

Response: 200 OK
{
  "aiIdeaId": 1,
  "userId": 5,
  "title": "Reusable Tote Bag",
  "tools": "Needle, thread, scissors",
  "steps": "1. Cut fabric. 2. Sew sides.",
  "safety": "Use scissors carefully",
  "createdAtUtc": "2024-01-15T10:30:00Z",
  "updatedAtUtc": null
}

Response: 404 Not Found
"AI idea not found."
```

### 3. List User's AI Ideas
```
GET /api/aiideas/user/{userId}?skip=0&take=20
Authorization: Bearer {token}

Query Parameters:
- skip: int (default: 0) - Number of results to skip
- take: int (default: 20) - Number of results to return (max: 100)

Response: 200 OK
{
  "results": [
    {
      "aiIdeaId": 1,
      "userId": 5,
      "title": "Reusable Tote Bag",
      "tools": "Needle, thread, scissors",
      "steps": "1. Cut fabric. 2. Sew sides.",
      "safety": "Use scissors carefully",
      "createdAtUtc": "2024-01-15T10:30:00Z",
      "updatedAtUtc": null
    },
    ...
  ],
  "total": 15,
  "skip": 0,
  "take": 20
}

Response: 403 Forbidden
"You can only view your own AI ideas."
```

### 4. Update AI Idea
```
PUT /api/aiideas/{id}
Authorization: Bearer {token}
Content-Type: application/json

Request Body:
{
  "title": "string (required)",
  "tools": "string (optional)",
  "steps": "string (optional)",
  "safety": "string (optional)"
}

Response: 204 No Content

Response: 404 Not Found
"AI idea not found."

Response: 400 Bad Request
"Title is required."
```

### 5. Delete AI Idea (Soft Delete)
```
DELETE /api/aiideas/{id}
Authorization: Bearer {token}

Response: 204 No Content

Response: 404 Not Found
"AI idea not found."
```

## Updated Project Endpoints

### Create Project with AI Idea
```
POST /api/projects
Authorization: Bearer {token}
Content-Type: application/json

Request Body:
{
  "userId": 5,
  "ideaId": null,              // Community idea (optional)
  "aiIdeaId": 1,               // AI-generated idea (optional)
  "title": "My Project",
  "description": "Description"
}

Response: 201 Created
{
  "projectId": 10,
  "ideaId": null,
  "aiIdeaId": 1,
  "userId": 5,
  "title": "My Project",
  "description": "Description",
  "status": "draft"
}
```

### Get Project
```
GET /api/projects/{id}
Authorization: Bearer {token}

Response: 200 OK
{
  "projectId": 10,
  "ideaId": null,
  "aiIdeaId": 1,
  "userId": 5,
  "title": "My Project",
  "description": "Description",
  "status": "draft",
  "steps": [
    {
      "projectStepId": 1,
      "stepNumber": 1,
      "instruction": "Cut the fabric"
    }
  ]
}
```

## Error Responses

### 400 Bad Request
```json
{
  "error": "Title is required."
}
```

### 403 Forbidden
```json
{
  "error": "You can only view your own AI ideas."
}
```

### 404 Not Found
```json
{
  "error": "AI idea not found."
}
```

### 500 Internal Server Error
```json
{
  "error": "Failed to retrieve created idea."
}
```

## Status Codes Summary

| Code | Meaning |
|------|---------|
| 200 | OK - Request successful |
| 201 | Created - Resource created successfully |
| 204 | No Content - Request successful, no content returned |
| 400 | Bad Request - Invalid request parameters |
| 403 | Forbidden - Access denied (e.g., not owner of resource) |
| 404 | Not Found - Resource does not exist |
| 500 | Internal Server Error - Server error |

## Authentication

All endpoints require a valid JWT token in the Authorization header:
```
Authorization: Bearer {token}
```

The token must contain the user ID in the `sub` or `NameIdentifier` claim.

## Pagination

The list endpoint supports pagination:
- **skip**: Number of records to skip (default: 0, min: 0)
- **take**: Number of records to return (default: 20, max: 100)

Example:
```
GET /api/aiideas/user/5?skip=20&take=10
```
Returns 10 results starting from the 21st record.

## Notes

- All ideas are returned in descending order by CreatedAtUtc (newest first)
- Deleted ideas (IsDeleted = 1) are never returned
- Users can only access their own AI ideas
- Soft deletes are used (records not physically deleted)
