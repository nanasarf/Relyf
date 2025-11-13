# ?? POST /api/Projects - Quick Test Guide

## ? Fix Applied - Ready to Test

The SQL exception in `POST /api/Projects` has been fixed. Here's how to test it:

---

## ?? Test 1: Basic Project Creation

### Request
```http
POST https://localhost:5001/api/Projects
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "userId": 5,
  "title": "My First Project",
  "description": "Testing project creation"
}
```

### Expected Response (201 Created)
```json
{
  "projectId": 1,
  "ideaId": null,
  "aiIdeaId": null,
  "userId": 5,
  "title": "My First Project",
  "description": "Testing project creation",
  "status": "draft"
}
```

? **Success Criteria**:
- HTTP 201 Created
- `projectId` is a valid integer
- `status` is "draft"
- `aiIdeaId` is null
- Content-Type is `application/json`

---

## ?? Test 2: Create Project from AI Idea

### Prerequisites
First create an AI idea:
```http
POST https://localhost:5001/api/aiideas
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "title": "Upcycled Tote Bag",
  "tools": "Scissors, needle, thread",
  "steps": "1. Cut fabric\n2. Sew edges",
  "safety": "Be careful with scissors"
}
```

Get the `aiIdeaId` from the response (e.g., 1).

### Request
```http
POST https://localhost:5001/api/Projects
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "userId": 5,
  "title": "My Tote Bag Project",
  "description": "Making a tote bag from old t-shirt",
  "aiIdeaId": 1
}
```

### Expected Response (201 Created)
```json
{
  "projectId": 2,
  "ideaId": null,
  "aiIdeaId": 1,
  "userId": 5,
  "title": "My Tote Bag Project",
  "description": "Making a tote bag from old t-shirt",
  "status": "draft"
}
```

? **Success Criteria**:
- HTTP 201 Created
- `aiIdeaId` matches the AI idea you created
- Project is linked to the AI idea

---

## ?? Test 3: Invalid AI Idea (Should Fail Gracefully)

### Request
```http
POST https://localhost:5001/api/Projects
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "userId": 5,
  "title": "Test Project",
  "description": "This should fail",
  "aiIdeaId": 999999
}
```

### Expected Response (400 Bad Request)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "AiIdeaId does not exist or is not owned by the current user."
}
```

? **Success Criteria**:
- HTTP 400 Bad Request
- Returns JSON (not HTML)
- Clear error message

---

## ?? Test 4: Missing Title (Should Fail)

### Request
```http
POST https://localhost:5001/api/Projects
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "userId": 5,
  "description": "No title provided"
}
```

### Expected Response (400 Bad Request)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Title": ["The Title field is required."]
  }
}
```

? **Success Criteria**:
- HTTP 400 Bad Request
- Returns JSON validation error
- Clearly indicates Title is required

---

## ?? Test 5: Verify JSON Error Responses

### Simulate Server Error (if possible)
Try to trigger any unexpected error and verify:

### Expected Behavior
- ? Response is JSON (not HTML)
- ? Content-Type is `application/json`
- ? Contains `error`, `message`, and optionally `details`
- ? HTTP 500 status code

### Example Error Response (Development)
```json
{
  "error": "Internal server error",
  "message": "Cannot insert the value NULL into column 'Title'...",
  "details": "Microsoft.Data.SqlClient.SqlException: ..."
}
```

### Example Error Response (Production)
```json
{
  "error": "Internal server error",
  "message": "An error occurred while processing your request.",
  "details": null
}
```

---

## ?? Using Swagger UI

1. **Navigate to Swagger**:
   ```
   https://localhost:5001/swagger
   ```

2. **Authenticate**:
   - Click "Authorize" button (??)
   - Enter: `Bearer YOUR_JWT_TOKEN`
   - Click "Authorize"

3. **Test POST /api/Projects**:
   - Find "Projects" section
   - Click "POST /api/Projects"
   - Click "Try it out"
   - Enter request body
   - Click "Execute"

4. **Verify Response**:
   - Check status code (201 or 400)
   - Check response body is JSON
   - Check all fields present

---

## ?? Using cURL

### Test 1: Basic Creation
```bash
curl -X POST https://localhost:5001/api/Projects \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 5,
    "title": "My Project",
    "description": "Test description"
  }'
```

### Test 2: With AI Idea
```bash
curl -X POST https://localhost:5001/api/Projects \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 5,
    "title": "AI Project",
    "description": "From AI idea",
    "aiIdeaId": 1
  }'
```

---

## ?? Using Postman

### Setup
1. Create new request
2. Set method to `POST`
3. URL: `https://localhost:5001/api/Projects`
4. Headers:
   - `Authorization: Bearer YOUR_JWT_TOKEN`
   - `Content-Type: application/json`
5. Body (raw JSON):
```json
{
  "userId": 5,
  "title": "Postman Test Project",
  "description": "Testing from Postman",
  "aiIdeaId": null
}
```
6. Click "Send"

### Verify
- ? Status: 201 Created
- ? Response is JSON
- ? Contains `projectId`, `title`, `status`, etc.

---

## ?? Test Results Checklist

### Basic Functionality
- [ ] Can create project with title only
- [ ] Can create project with title + description
- [ ] Default status is "draft"
- [ ] Returns valid `projectId`
- [ ] Returns all expected fields

### AI Idea Integration
- [ ] Can create project from AI idea
- [ ] `aiIdeaId` is returned in response
- [ ] Invalid `aiIdeaId` returns 400 error
- [ ] AI idea ownership validated

### Error Handling
- [ ] Missing title returns 400 Bad Request
- [ ] Invalid JWT returns 401 Unauthorized
- [ ] All errors return JSON (not HTML)
- [ ] Error messages are clear
- [ ] Content-Type is always `application/json`

### Response Format
- [ ] All responses are valid JSON
- [ ] Status codes are correct
- [ ] Headers include Content-Type
- [ ] No HTML error pages

---

## ? Common Issues

### Issue 1: "The Title field is required"
**Solution**: Make sure your request includes `"title": "Some Title"`

### Issue 2: 401 Unauthorized
**Solution**: 
1. Get a valid JWT token from `POST /api/auth/login`
2. Include it in header: `Authorization: Bearer {token}`

### Issue 3: "AiIdeaId does not exist"
**Solution**:
1. First create an AI idea: `POST /api/aiideas`
2. Use the returned `aiIdeaId` in your project request

### Issue 4: Still getting HTML errors
**Solution**:
1. Rebuild the application: `dotnet build`
2. Restart the API server
3. Clear browser cache
4. Verify fix was deployed

---

## ? All Tests Passed?

If all tests pass, the fix is successful! ??

**Next Steps**:
1. Mark the issue as resolved
2. Deploy to staging environment
3. Run tests in staging
4. Deploy to production
5. Monitor logs for any issues

---

## ?? Need Help?

### Check Application Logs
```bash
# Development: Check console output
# Production: Check application logs

# Look for:
# - SQL errors
# - Unhandled exceptions
# - HTTP status codes
```

### Verify Database
```sql
-- Check if AiIdeaId column exists
SELECT COLUMN_NAME, IS_NULLABLE, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'app' AND TABLE_NAME = 'Project' AND COLUMN_NAME = 'AiIdeaId';

-- Check recent projects
SELECT TOP 5 ProjectId, Title, AiIdeaId, CreatedAtUtc
FROM app.Project
ORDER BY CreatedAtUtc DESC;
```

---

**Quick Test Version**: 1.0  
**Last Updated**: 2025  
**Fix Status**: ? Ready
