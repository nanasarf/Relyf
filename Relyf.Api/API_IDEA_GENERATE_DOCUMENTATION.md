# API Endpoint: POST /api/ideas/generate

## ?? Overview

The **`/api/ideas/generate`** endpoint generates AI-powered upcycling ideas for items using the Cohere AI API. It creates persistent records in your database and returns the generated idea along with metadata.

---

## ?? Authentication

**Required**: ? **JWT Bearer Token**

You must include a valid JWT token in the Authorization header:
```
Authorization: Bearer YOUR_JWT_TOKEN
```

---

## ?? Request Body

```json
{
  "itemId": 5,
  "promptText": "I have an old pair of jeans that I want to upcycle. What are some creative ways to reuse them?",
  "model": "command-r-plus",
  "temperature": 0.2,
  "topP": 0.9,
  "titleHint": "Jean Upcycling Ideas"
}
```

### Request Parameters

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `promptText` | string | ? Yes | The prompt to send to Cohere AI. This is the question or request for upcycling ideas. Must not be empty. |
| `itemId` | int? | ? No | ID of an item you own. If provided, the idea will be associated with this item. |
| `model` | string? | ? No | Cohere model to use (default: "command-r-plus"). Example: "command-r-plus", "command-light" |
| `temperature` | decimal? | ? No | Controls randomness (0.0-2.0). Lower = more deterministic. Default: 0.2 (from appsettings) |
| `topP` | decimal? | ? No | Controls diversity (0.0-1.0). Default: varies by model |
| `titleHint` | string? | ? No | Suggested title for the idea. If not provided, defaults to "Upcycling Idea" |

---

## ?? Response

### Success Response (201 Created)

```json
{
  "ideaId": 42,
  "title": "Jean Upcycling Ideas",
  "ideaText": "1. **Denim Tote Bag**: Cut and sew legs into a sturdy bag with interior pockets. Tools: scissors, needle, thread. Steps: Cut legs, sew sides, add handles. Safety: Use sharp scissors carefully.\n\n2. **Jean Jacket Vest**: Cut sleeves off, add decorative patches. Tools: scissors, fabric paint. Steps: Remove sleeves, paint designs, sew patches. Safety: Good ventilation if using fabric paint.\n\n3. **Denim Rug**: Cut into strips, braid, and sew into circular rug. Tools: scissors, needle, thread. Steps: Cut strips, braid three at a time, coil and sew. Safety: Sharp scissors, watch your fingers.",
  "coherePromptId": 15,
  "itemId": 5,
  "userId": 4
}
```

### Error Responses

**400 Bad Request** - Missing required field:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "PromptText is required."
}
```

**400 Bad Request** - Item not owned by user:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Item does not exist or is not owned by the current user."
}
```

**401 Unauthorized** - Invalid or missing JWT token:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authorization header is missing or invalid."
}
```

**500 Internal Server Error** - Cohere API failure:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "Failed to call Cohere API."
}
```

---

## ?? What Happens Behind the Scenes

```
1. Request Validation
   ?? Check JWT token is valid
   ?? Extract userId from token
   ?? Verify promptText is not empty

2. Ownership Checks (Dapper)
   ?? Verify user exists in database
   ?? If itemId provided, verify user owns that item

3. Save Prompt Configuration
   ?? Create CoherePrompt record with:
      - Model
      - Temperature
      - TopP
      - PromptText
      - UserId
      - ItemId (if provided)

4. Call Cohere AI
   ?? Send prompt to Cohere API
   ?? Measure response time
   ?? Handle errors

5. Log API Request
   ?? Create ApiRequestLog record with:
      - Provider: "cohere"
      - Endpoint: "/v2/chat"
      - Status code (200 or 500)
      - Duration in milliseconds
      - Prompt hash (SHA256)

6. Store Generated Idea
   ?? Create AiIdea record with:
      - Title (from titleHint or "Upcycling Idea")
      - IdeaText (from Cohere)
      - CoherePromptId
      - ItemId (if provided)
      - UserId

7. Return Response (201 Created)
   ?? Return the created idea with metadata
```

---

## ?? Database Records Created

### 1. CoherePrompt (in `app.CoherePrompt`)
```sql
INSERT INTO app.CoherePrompt (UserId, ItemId, Model, Temperature, TopP, PromptText, CreatedAtUtc)
VALUES (4, 5, 'command-r-plus', 0.2, 0.9, '...', SYSUTCDATETIME())
```

### 2. AiIdea (in `app.AiIdea`)
```sql
INSERT INTO app.AiIdea (UserId, ItemId, CoherePromptId, Title, IdeaText, CreatedAtUtc, IsDeleted)
VALUES (4, 5, 15, 'Jean Upcycling Ideas', '...', SYSUTCDATETIME(), 0)
```

### 3. ApiRequestLog (in `app.ApiRequestLog`)
```sql
INSERT INTO app.ApiRequestLog (UserId, Provider, Endpoint, Model, PromptHash, StatusCode, DurationMs, CreatedAtUtc)
VALUES (4, 'cohere', '/v2/chat', 'command-r-plus', 0x..., 200, 1245, SYSUTCDATETIME())
```

---

## ?? Example Usage

### cURL
```bash
curl -X POST "https://localhost:5101/api/ideas/generate" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "itemId": 5,
    "promptText": "I have old blue jeans. What are creative upcycling ideas?",
    "model": "command-r-plus",
    "temperature": 0.2,
    "topP": 0.9,
    "titleHint": "Jean Upcycling Ideas"
  }'
```

### PowerShell
```powershell
$headers = @{
    "Authorization" = "Bearer YOUR_JWT_TOKEN"
    "Content-Type" = "application/json"
}

$body = @{
    itemId = 5
    promptText = "I have old blue jeans. What are creative upcycling ideas?"
    model = "command-r-plus"
    temperature = 0.2
    topP = 0.9
    titleHint = "Jean Upcycling Ideas"
} | ConvertTo-Json

Invoke-WebRequest -Uri "https://localhost:5101/api/ideas/generate" `
    -Method POST `
    -Headers $headers `
    -Body $body `
    -SkipCertificateCheck
```

### JavaScript/Fetch
```javascript
const token = "YOUR_JWT_TOKEN";

const response = await fetch('https://localhost:5101/api/ideas/generate', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    itemId: 5,
    promptText: 'I have old blue jeans. What are creative upcycling ideas?',
    model: 'command-r-plus',
    temperature: 0.2,
    topP: 0.9,
    titleHint: 'Jean Upcycling Ideas'
  })
});

const idea = await response.json();
console.log(idea);
```

---

## ?? Use Cases

### Use Case 1: Generate Ideas for a Specific Item
User uploads old jeans to the app, then generates ideas specifically for those jeans.

```json
{
  "itemId": 5,
  "promptText": "I have old blue jeans. What are creative upcycling ideas?",
  "titleHint": "Blue Jeans Upcycling"
}
```

### Use Case 2: Generic Idea Generation
User doesn't have a specific item but wants general upcycling inspiration.

```json
{
  "promptText": "What are some creative ways to upcycle old t-shirts?",
  "titleHint": "T-Shirt Upcycling Ideas"
}
```

### Use Case 3: Custom AI Parameters
User wants to customize the Cohere model settings for more creative or deterministic results.

```json
{
  "itemId": 5,
  "promptText": "Generate very creative and unusual ideas for upcycling old jeans.",
  "model": "command-r-plus",
  "temperature": 0.8,  // Higher = more creative
  "topP": 0.95,        // Higher = more diverse
  "titleHint": "Creative Jean Upcycling"
}
```

---

## ?? Configuration

The endpoint uses settings from `appsettings.json`:

```json
{
  "Cohere": {
    "ApiKey": "USE-USER-SECRETS",
    "BaseUrl": "https://api.cohere.com",
    "Model": "command-r-plus",
    "MaxTokens": 400,
    "Temperature": 0.2
  }
}
```

---

## ?? Key Features

1. **Persistent Storage**: All prompts, ideas, and API calls are logged to the database
2. **Ownership Enforcement**: Users can only create ideas linked to their own items
3. **Performance Tracking**: Response time and API status are logged for analytics
4. **Flexible Prompts**: Accept any prompt, not just pre-defined ones
5. **Cohere Integration**: Uses the Cohere `/v2/chat` endpoint
6. **AI Logging**: Prompt hash (SHA256) stored for analytics without exposing sensitive content

---

## ?? Related Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/ideas` | Get preview ideas (no auth required) |
| GET | `/api/ideas/{id}` | Fetch a stored idea (auth required) |
| POST | `/api/ideas/generate` | Generate and save an idea (auth required) |

---

## ?? Important Notes

1. **JWT Required**: This endpoint requires authentication. You must include a valid JWT token.
2. **Item Ownership**: If you specify an `itemId`, you must own that item.
3. **PromptText Required**: The `promptText` field is mandatory.
4. **Rate Limiting**: Subject to Cohere API rate limits (configured in your account).
5. **Cost**: Each call to this endpoint incurs a cost based on Cohere's pricing.
6. **User Logging**: All API calls are logged with user ID for analytics.

---

## ?? Common Issues & Solutions

### Issue: "User id not found in token"
**Cause**: Invalid JWT token or missing `sub` claim
**Solution**: Get a fresh token by logging in via `/api/auth/login`

### Issue: "Item does not exist or is not owned by the current user"
**Cause**: The itemId doesn't exist or belongs to a different user
**Solution**: Omit `itemId` or use a valid item ID you own

### Issue: "PromptText is required"
**Cause**: You didn't include the `promptText` field
**Solution**: Add `promptText` to your request body

### Issue: No ideas generated / Empty response
**Cause**: Cohere API returned empty or the service parsed it incorrectly
**Solution**: Try a different prompt or check your Cohere API key in user-secrets

---

**Last Updated**: 2024-01-20
**Status**: Production Ready ?
