# POST /api/ideas/generate - Quick Summary

## ?? What It Does

Generates AI-powered **upcycling ideas** for items using Cohere AI and saves them to your database.

---

## ?? Request

```
POST https://localhost:5101/api/ideas/generate
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "itemId": 5,                    // Optional: Item to link idea to
  "promptText": "Old jeans?",     // Required: What to generate ideas for
  "model": "command-r-plus",      // Optional: AI model
  "temperature": 0.2,             // Optional: Randomness level
  "topP": 0.9,                    // Optional: Diversity level
  "titleHint": "Jean Ideas"       // Optional: Custom title
}
```

---

## ?? Response (201 Created)

```json
{
  "ideaId": 42,
  "title": "Jean Ideas",
  "ideaText": "1. Denim Tote Bag: ...\n2. Jean Jacket Vest: ...\n3. Denim Rug: ...",
  "coherePromptId": 15,
  "itemId": 5,
  "userId": 4
}
```

---

## ?? Process Flow

```
Request Received
    ?
Validate JWT Token
    ?
Check Item Ownership (if itemId provided)
    ?
Save CoherePrompt to DB
    ?
Call Cohere AI API
    ?
Log API Request to DB
    ?
Save Generated Idea to DB
    ?
Return Response (201 Created)
```

---

## ?? Database Changes

Creates 3 new records:

1. **CoherePrompt** - Saves the prompt configuration
2. **AiIdea** - Stores the generated idea text and title
3. **ApiRequestLog** - Logs the API call for analytics

---

## ?? Key Parameters

| Parameter | Type | Required | Purpose |
|-----------|------|----------|---------|
| `promptText` | string | ? | The question/request for AI |
| `itemId` | int? | ? | Link idea to your item |
| `titleHint` | string? | ? | Custom idea title |
| `model` | string? | ? | Which AI model to use |
| `temperature` | decimal? | ? | How creative (0-2) |
| `topP` | decimal? | ? | How diverse (0-1) |

---

## ? Example: Generate Ideas for Old Jeans

### Request
```json
{
  "itemId": 5,
  "promptText": "I have old blue jeans. What are creative ways to upcycle them?",
  "titleHint": "Blue Jeans Upcycling Ideas"
}
```

### Response
```json
{
  "ideaId": 42,
  "title": "Blue Jeans Upcycling Ideas",
  "ideaText": "1. Denim Tote Bag\n2. Jean Jacket Vest\n3. Denim Rug",
  "coherePromptId": 15,
  "itemId": 5,
  "userId": 4
}
```

---

## ?? Security

- **Authentication**: JWT Bearer token required
- **Ownership Enforcement**: Can only link to your own items
- **User Isolation**: All ideas are associated with your user ID

---

## ? Common Errors

| Error | Cause | Solution |
|-------|-------|----------|
| 401 Unauthorized | No/invalid JWT token | Login to get token |
| 400 Bad Request: PromptText required | Missing promptText | Add promptText to body |
| 400 Bad Request: Item not owned | ItemId doesn't belong to you | Use valid itemId or omit |
| 500 Internal Server Error | Cohere API failed | Check Cohere API key |

---

## ?? Related Endpoints

| Endpoint | Purpose |
|----------|---------|
| `GET /api/ideas` | Get preview ideas (no auth) |
| `GET /api/ideas/{id}` | Fetch saved idea (auth) |
| `POST /api/ideas/generate` | Generate & save idea (auth) |

---

## ?? Full Documentation

See: `API_IDEA_GENERATE_DOCUMENTATION.md` for complete details including:
- All error responses
- Usage examples (cURL, PowerShell, JavaScript)
- Database schema details
- Configuration options
- Troubleshooting guide

---

**Auth Required**: ? JWT Bearer Token  
**Cost**: Uses Cohere API credits  
**Rate Limit**: Subject to Cohere limits  
**Response Status**: 201 Created (success) or 4xx/5xx (error)
