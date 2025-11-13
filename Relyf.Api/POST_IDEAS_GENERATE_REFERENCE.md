# POST /api/ideas/generate - Complete Reference

## ?? One-Line Summary

**Generates AI-powered upcycling ideas for items using Cohere AI and saves everything to the database.**

---

## ?? Quick Reference Card

```
ENDPOINT:        POST /api/ideas/generate
AUTHENTICATION:  ? JWT Bearer Token Required
STATUS CODE:     201 Created (success) | 4xx/5xx (error)
CONTENT-TYPE:    application/json

REQUEST BODY:
{
  "promptText": "...",      // REQUIRED: Your question
  "itemId": 5,              // Optional: Link to item
  "titleHint": "...",       // Optional: Custom title
  "model": "command-r-plus", // Optional: AI model
  "temperature": 0.2,       // Optional: 0-2 (randomness)
  "topP": 0.9               // Optional: 0-1 (diversity)
}

RESPONSE:
{
  "ideaId": 42,
  "title": "...",
  "ideaText": "...",
  "coherePromptId": 15,
  "itemId": 5,
  "userId": 4
}
```

---

## ?? Understanding Each Component

### 1. **IdeasController.Generate()**
The endpoint handler that:
- Validates the JWT token
- Checks user exists
- Verifies item ownership (if itemId provided)
- Orchestrates the entire flow

### 2. **CoherePromptRepository**
Saves the prompt configuration to `app.CoherePrompt` table:
- Which AI model was used
- Temperature and TopP settings
- The exact prompt text
- Timestamp

### 3. **UpcycleIdeaService**
Calls the Cohere API with:
- System message: "You are an expert, safety-first upcycling assistant..."
- User message: Your promptText
- Model parameters
- Returns: Generated ideas text

### 4. **ApiRequestLogRepository**
Logs metrics to `app.ApiRequestLog` table:
- Provider: "cohere"
- Endpoint: "/v2/chat"
- Response time (duration)
- Status code (200 or error)
- Prompt hash (SHA256) for analytics

### 5. **AiIdeaRepository**
Saves the final result to `app.AiIdea` table:
- Generated title
- Generated idea text
- Links back to CoherePrompt
- Links to Item (if provided)
- Links to User
- Timestamp

---

## ?? Field Descriptions

### **promptText** (Required)
The question or request sent to Cohere AI.

**Examples:**
- "What are creative ways to upcycle old jeans?"
- "I have a broken microwave. Can I reuse parts?"
- "How can I repurpose old wine bottles?"

**Rules:**
- Must not be empty
- Must not be whitespace only
- No length limit (but practical limit ~2000 chars)

### **itemId** (Optional)
The ID of an item you own in the system.

**Usage:**
- Links the generated idea to a specific item
- Server verifies you own this item
- Omit if generating ideas without an item

**Rules:**
- Must be a valid item ID
- Must be owned by the current user
- If invalid, returns 400 Bad Request

### **titleHint** (Optional)
A suggested title for the generated idea.

**Examples:**
- "Jean Upcycling Ideas"
- "Microwave Part Reuse"
- "Wine Bottle Crafts"

**Rules:**
- If not provided, defaults to "Upcycling Idea"
- Can be any string
- Is trimmed (whitespace removed)

### **model** (Optional)
Which Cohere AI model to use.

**Options:**
- "command-r-plus" (default, most capable)
- "command-light" (faster, cheaper)
- "command-r" (balanced)

**Rules:**
- If not provided, uses appsettings value
- Is saved in CoherePrompt for reproducibility

### **temperature** (Optional)
Controls randomness/creativity of responses.

**Range:** 0.0 - 2.0
- 0.0 = Deterministic (same input, same output)
- 0.5 = Balanced
- 1.0 = Creative
- 2.0 = Very random

**Default:** 0.2 (from appsettings.json - deterministic)

### **topP** (Optional)
Controls diversity of vocabulary.

**Range:** 0.0 - 1.0
- 0.0 = Limited vocabulary
- 0.5 = Moderate diversity
- 1.0 = Full vocabulary (default)

**Rules:**
- Usually paired with temperature
- Lower = more focused responses

---

## ?? Authentication Details

### How to Get JWT Token

```powershell
# 1. Register (if new user)
POST /api/auth/register
{
  "email": "user@example.com",
  "displayName": "User Name",
  "password": "SecurePassword123!"
}

# 2. Login
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}

# Response includes:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 4,
  "displayName": "User Name"
}
```

### How to Use Token

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Token Claims:**
- `sub`: User ID (e.g., "4")
- `email`: User email
- `name`: User display name
- `nbf`: Not before timestamp
- `exp`: Expiration timestamp
- `iss`: Issuer ("Relyf")
- `aud`: Audience ("Relyf.Client")

---

## ?? Database Records Created

When you call this endpoint, 3 records are created:

### Record 1: CoherePrompt
```sql
INSERT INTO app.CoherePrompt (
  UserId,           -- Your user ID
  ItemId,           -- Optional: the item ID
  Model,            -- The AI model used
  Temperature,      -- Randomness setting
  TopP,             -- Diversity setting
  PromptText,       -- The full prompt (includes system message)
  CreatedAtUtc      -- Timestamp
)
```

### Record 2: AiIdea
```sql
INSERT INTO app.AiIdea (
  UserId,           -- Your user ID
  ItemId,           -- Optional: the item ID
  CoherePromptId,   -- Link to the prompt config
  Title,            -- Generated or hinted title
  IdeaText,         -- The AI-generated ideas
  CreatedAtUtc,     -- Timestamp
  IsDeleted         -- Always 0 (not deleted)
)
```

### Record 3: ApiRequestLog
```sql
INSERT INTO app.ApiRequestLog (
  UserId,           -- Your user ID
  Provider,         -- Always "cohere"
  Endpoint,         -- Always "/v2/chat"
  Model,            -- The model used
  PromptHash,       -- SHA256 hash of prompt (for privacy)
  StatusCode,       -- 200 (success) or 500 (error)
  DurationMs,       -- Response time in milliseconds
  CreatedAtUtc      -- Timestamp
)
```

---

## ?? Testing Examples

### Test 1: Basic Generation

**Request:**
```bash
curl -X POST https://localhost:5101/api/ideas/generate \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "promptText": "How to upcycle old t-shirts?"
  }'
```

**Expected Response (201):**
```json
{
  "ideaId": 42,
  "title": "Upcycling Idea",
  "ideaText": "1. T-Shirt Tote Bag\n2. Pillow Stuffing\n3. Headbands",
  "coherePromptId": 15,
  "itemId": null,
  "userId": 4
}
```

---

### Test 2: With Item Link

**Request:**
```bash
curl -X POST https://localhost:5101/api/ideas/generate \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "itemId": 5,
    "promptText": "Old blue jeans - creative reuse ideas?",
    "titleHint": "Blue Jeans Project"
  }'
```

**Expected Response (201):**
```json
{
  "ideaId": 42,
  "title": "Blue Jeans Project",
  "ideaText": "1. Denim Tote Bag\n...",
  "coherePromptId": 15,
  "itemId": 5,
  "userId": 4
}
```

---

### Test 3: Custom AI Parameters

**Request:**
```bash
curl -X POST https://localhost:5101/api/ideas/generate \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "promptText": "Generate very creative ideas for old CDs",
    "model": "command-r-plus",
    "temperature": 1.5,
    "topP": 0.95,
    "titleHint": "Creative CD Reuse"
  }'
```

**Expected Response (201):**
```json
{
  "ideaId": 42,
  "title": "Creative CD Reuse",
  "ideaText": "1. ... (more creative ideas)\n2. ...\n3. ...",
  "coherePromptId": 15,
  "itemId": null,
  "userId": 4
}
```

---

## ?? Error Responses

### 400: Missing PromptText
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "PromptText is required."
}
```

### 400: Item Not Owned
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Item does not exist or is not owned by the current user."
}
```

### 401: Invalid Token
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authorization header is missing or invalid."
}
```

### 500: Cohere API Error
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An error occurred while processing your request."
}
```

---

## ?? Common Use Cases

### Scenario 1: User uploads item, then generates ideas

```
User uploads: "Old Jeans"
  ? app.Item created (id: 5)

User calls: POST /api/ideas/generate
  {
    "itemId": 5,
    "promptText": "Ideas for old jeans?"
  }

Result: Idea linked to item, saved in database
```

### Scenario 2: Just generate ideas, no specific item

```
User calls: POST /api/ideas/generate
  {
    "promptText": "How to reuse old books?"
  }

Result: Idea saved (itemId is NULL)
```

### Scenario 3: Generate multiple ideas for same item

```
Call 1: POST /api/ideas/generate with itemId=5, prompt A
Call 2: POST /api/ideas/generate with itemId=5, prompt B
Call 3: POST /api/ideas/generate with itemId=5, prompt C

Result: Item now has 3 different ideas in database
```

---

## ?? Performance Notes

- **Fast responses:** ~50ms (validation + database saves)
- **Slow part:** Cohere API call (~800-2000ms depending on network)
- **Total time:** Usually 1-2 seconds per request

---

## ?? Related Endpoints

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/api/ideas` | Get preview ideas (NO auth) |
| GET | `/api/ideas/{id}` | Fetch saved idea (auth required) |
| POST | `/api/ideas/generate` | Generate & save (auth required) |

---

## ?? Documentation Files

- **API_IDEA_GENERATE_DOCUMENTATION.md** - Full detailed documentation
- **IDEA_GENERATE_QUICK_SUMMARY.md** - Quick one-page reference
- **IDEA_GENERATE_VISUAL_WORKFLOW.md** - Visual diagrams and flows
- **POST_IDEAS_GENERATE_REFERENCE.md** - This file

---

**Last Updated**: 2024-01-20  
**Status**: Production Ready ?  
**Auth**: JWT Bearer Required ?  
**Cost**: Cohere API credits per call ??
