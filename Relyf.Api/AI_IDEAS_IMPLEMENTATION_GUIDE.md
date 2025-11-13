# AI-Generated Ideas Feature Implementation

## Overview
This document outlines the complete implementation of AI-Generated Ideas support for the Relyf backend, allowing projects to link to both community ideas and AI-generated ideas.

## Changes Made

### 1. Database Schema
**File**: `create_ai_ideas_table.sql`

Created a new `app.AIIdeas` table with the following structure:
```sql
CREATE TABLE app.AIIdeas (
    AiIdeaId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    Tools NVARCHAR(MAX),
    Steps NVARCHAR(MAX),
    Safety NVARCHAR(MAX),
    CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_AIIdeas_User FOREIGN KEY (UserId) REFERENCES app.[User](UserId)
);

CREATE INDEX IX_AIIdeas_UserId ON app.AIIdeas(UserId, IsDeleted);
```

Updated `app.Project` table:
- Added `AiIdeaId INT NULL` column
- Added foreign key: `FK_Project_AIIdea` ? `app.AIIdeas(AiIdeaId)`

### 2. Data Models

#### API Models
- **`Models/SavedAIIdea.cs`**: New model for API contracts
  - `AiIdeaId`: Primary key
  - `UserId`: Owner of the idea
  - `Title`: Idea title
  - `Tools`: Optional tools information
  - `Steps`: Optional steps/instructions
  - `Safety`: Optional safety considerations
  - Timestamps and soft delete flag

- **`Models/Project.cs`**: Updated to support both idea types
  - `IdeaId`: For community ideas (existing)
  - `AiIdeaId`: For AI-generated ideas (new)

#### Repository Models
- **`Relyf.Repository/Dapper/Models/SavedAIIdeaRecord.cs`**: Dapper mapping for AIIdeas table
- **`Relyf.Repository/Dapper/Models/ProjectRecord.cs`**: Updated to include `AiIdeaId` field

### 3. Data Access Layer

#### Repository Interface
**File**: `Relyf.Repository/Dapper/ISavedAIIdeaRepository.cs`

```csharp
public interface ISavedAIIdeaRepository
{
    Task<SavedAIIdeaRecord?> GetByIdAsync(int aiIdeaId, int authUserId);
    Task<(IReadOnlyList<SavedAIIdeaRecord> Rows, int Total)> ListByUserAsync(int authUserId, int skip, int take);
    Task<int> CreateAsync(int userId, string title, string? tools, string? steps, string? safety);
    Task<int> UpdateAsync(int aiIdeaId, int authUserId, string title, string? tools, string? steps, string? safety);
    Task<int> SoftDeleteAsync(int aiIdeaId, int authUserId);
}
```

#### Repository Implementation
**File**: `Relyf.Repository/Dapper/SavedAIIdeaRepository.cs`

Provides full CRUD operations with:
- User ownership validation
- Soft delete support
- Pagination for list operations
- Index-optimized queries

### 4. API Controllers

#### New Endpoint
**File**: `Controllers/AIIdeasController.cs`

Implements the following RESTful endpoints:

```
POST   /api/aiideas                    - Create a new AI idea
GET    /api/aiideas/{id}              - Get a specific AI idea
GET    /api/aiideas/user/{userId}     - List user's AI ideas (paginated)
PUT    /api/aiideas/{id}              - Update an AI idea
DELETE /api/aiideas/{id}              - Delete (soft) an AI idea
```

All endpoints require JWT authentication (`[Authorize]`).

**Request/Response DTOs**:
```csharp
public record CreateAIIdeaRequest(string Title, string? Tools, string? Steps, string? Safety);
public record UpdateAIIdeaRequest(string Title, string? Tools, string? Steps, string? Safety);
public record AIIdeaDto(
    int AiIdeaId,
    int UserId,
    string Title,
    string? Tools,
    string? Steps,
    string? Safety,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);
public record PagedAIIdeasDto(List<AIIdeaDto> Results, int Total, int Skip, int Take);
```

#### Updated ProjectsController
**File**: `Controllers/ProjectController.cs`

Enhanced to support AI ideas:
- Updated DTOs: `CreateProjectRequest`, `ProjectDto`, `ProjectWithStepsDto` now include `AiIdeaId`
- Added validation in `Create()` to verify AI idea exists and belongs to user
- Projects can now link to either `IdeaId` (community) or `AiIdeaId` (AI-generated)

### 5. Dependency Injection
**File**: `Program.cs`

Registered new service:
```csharp
builder.Services.AddScoped<ISavedAIIdeaRepository, SavedAIIdeaRepository>();
```

### 6. Updated Repository Models
**File**: `Relyf.Repository/Dapper/ProjectRepository.cs`

All queries updated to include `AiIdeaId` field in SELECT statements and support the new column.

## Key Features

### Security
- ? JWT authentication required for all endpoints
- ? User ownership validation on all operations
- ? Scoped data access (users can only see their own ideas)
- ? Cannot update/delete ideas owned by other users

### Data Integrity
- ? Soft deletes (IsDeleted flag instead of hard deletes)
- ? Proper foreign key constraints
- ? Audit timestamps (CreatedAtUtc, UpdatedAtUtc)
- ? Index on UserId for efficient queries

### API Design
- ? RESTful endpoints following conventions
- ? Proper HTTP status codes (201 Created, 204 No Content, 404 Not Found, 403 Forbidden)
- ? Pagination support on list endpoints (skip/take)
- ? Comprehensive error messages

## Usage Examples

### Create an AI Idea
```bash
curl -X POST http://localhost:5000/api/aiideas \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d {
    "title": "Reusable Tote Bag",
    "tools": "Needle, thread, scissors",
    "steps": "1. Cut fabric. 2. Sew sides. 3. Attach handles.",
    "safety": "Use sharp scissors carefully"
  }
```

### Get User's AI Ideas
```bash
curl -X GET "http://localhost:5000/api/aiideas/user/1?skip=0&take=20" \
  -H "Authorization: Bearer {token}"
```

### Create a Project from AI Idea
```bash
curl -X POST http://localhost:5000/api/projects \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d {
    "title": "My Upcycling Project",
    "description": "Creating a tote bag",
    "aiIdeaId": 5,
    "ideaId": null
  }
```

### Delete an AI Idea
```bash
curl -X DELETE http://localhost:5000/api/aiideas/5 \
  -H "Authorization: Bearer {token}"
```

## Database Migration

**Important**: Execute `create_ai_ideas_table.sql` against your database:

```sql
-- From SQL Server Management Studio or dotnet-ef
sqlcmd -S {server} -d {database} -i create_ai_ideas_table.sql
```

The migration is idempotent and safe to run multiple times.

## Testing Checklist

- [ ] Create an AI idea
- [ ] Retrieve a specific AI idea
- [ ] List user's AI ideas with pagination
- [ ] Update an AI idea
- [ ] Delete an AI idea
- [ ] Create a project linked to an AI idea
- [ ] Verify project returns AiIdeaId in response
- [ ] Verify authorization (users can't access other users' ideas)
- [ ] Verify soft delete works (ideas with IsDeleted=1 not returned)
- [ ] Verify pagination (skip/take work correctly)

## Notes

- The `AiIdea` model (in `Models/AiIdea.cs`) is different from `SavedAIIdea` - the former is for Cohere API-generated ideas, the latter is user-saved ideas
- Projects can link to EITHER community ideas (IdeaId) OR AI-generated ideas (AiIdeaId), not both
- All timestamps are in UTC (CreatedAtUtc, UpdatedAtUtc, SYSUTCDATETIME())
- Soft deletes are enforced at the data layer (IsDeleted = 0 in WHERE clauses)
