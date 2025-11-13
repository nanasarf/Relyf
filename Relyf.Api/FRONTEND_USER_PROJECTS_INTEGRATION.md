# ?? Frontend Integration - User Projects Fix

## Quick Summary

**Problem**: Projects count mismatch on user profiles  
**Root Cause**: Frontend calling wrong endpoint  
**Solution**: New endpoint `GET /api/projects/user/{userId}`

---

## ? Quick Fix (3 Steps)

### Step 1: Update API Slice

```typescript
// src/store/api/apiSlice.ts (or wherever your RTK Query is defined)

export interface PagedProjectsDto {
  results: ProjectDto[];
  total: number;
  skip: number;
  take: number;
}

export interface GetUserProjectsParams {
  userId: number;
  skip?: number;
  take?: number;
}

// Add this to your API builder
getUserProjects: builder.query<PagedProjectsDto, GetUserProjectsParams>({
  query: ({ userId, skip = 0, take = 20 }) => 
    `/projects/user/${userId}?skip=${skip}&take=${take}`,
  providesTags: (result, error, { userId }) => 
    result
      ? [
          ...result.results.map(({ projectId }) => ({ type: 'Project' as const, id: projectId })),
          { type: 'Project' as const, id: `USER_${userId}` }
        ]
      : [{ type: 'Project' as const, id: `USER_${userId}` }],
}),
```

### Step 2: Update UserProfile Component

```typescript
// src/components/UserProfile.tsx

// BEFORE (Broken):
const { data: userProjectsRaw } = useGetUserProjectsQuery();  // ? Gets auth user's projects
const userProjects = useMemo(
  () => userProjectsRaw?.results.filter(p => p.userId === viewedUserId) ?? [],
  [userProjectsRaw, viewedUserId]
);

// AFTER (Fixed):
const { data: userProjectsData } = useGetUserProjectsQuery({ 
  userId: viewedUserId  // ? Gets profile owner's projects
});
const userProjects = userProjectsData?.results ?? [];
```

### Step 3: Update Tab Count

```typescript
// Use total from API response
<Tabs.Tab value="projects" disabled={!isOwnProfile && (userProjectsData?.total ?? 0) === 0}>
  Projects ({userProjectsData?.total ?? 0})
</Tabs.Tab>
```

---

## ?? Complete Example

```typescript
// UserProfile.tsx - Full working example

import { useGetUserProjectsQuery } from '@/store/api/apiSlice';

interface UserProfileProps {
  userId: number;  // The profile being viewed
}

export function UserProfile({ userId }: UserProfileProps) {
  const currentUser = useAuth();  // Your auth hook
  const isOwnProfile = currentUser.userId === userId;

  // Fetch projects for the profile owner
  const { 
    data: projectsData, 
    isLoading: projectsLoading,
    error: projectsError 
  } = useGetUserProjectsQuery({ 
    userId: userId,  // Profile owner's ID
    skip: 0,
    take: 20
  });

  return (
    <div>
      <Tabs defaultValue="projects">
        <Tabs.List>
          <Tabs.Tab value="projects" disabled={!isOwnProfile && (projectsData?.total ?? 0) === 0}>
            Projects ({projectsData?.total ?? 0})
          </Tabs.Tab>
        </Tabs.List>

        <Tabs.Panel value="projects">
          {projectsLoading && <Loader />}
          
          {projectsError && (
            <Alert color="red">Failed to load projects</Alert>
          )}

          {projectsData?.results.length === 0 && (
            <Text>No projects yet</Text>
          )}

          <Grid>
            {projectsData?.results.map(project => (
              <Grid.Col key={project.projectId} span={4}>
                <ProjectCard project={project} />
              </Grid.Col>
            ))}
          </Grid>
        </Tabs.Panel>
      </Tabs>
    </div>
  );
}
```

---

## ?? Before vs After Comparison

### Before (Broken Flow)

```
User views profile of User B (ID: 2)
  ?
Frontend: useGetUserProjectsQuery()
  ?
Calls: GET /api/projects
  ?
Backend: Returns authenticated user's projects (User A's)
  ?
Frontend: Filters for userId === 2
  ?
Result: Empty or wrong count
```

### After (Fixed Flow)

```
User views profile of User B (ID: 2)
  ?
Frontend: useGetUserProjectsQuery({ userId: 2 })
  ?
Calls: GET /api/projects/user/2
  ?
Backend: Returns User B's projects only
  ?
Frontend: Displays correctly
  ?
Result: Correct count and data
```

---

## ?? Testing Checklist

After implementing the fix, verify:

### ? Own Profile
- [ ] Visit your own profile (`/profile/{yourUserId}`)
- [ ] Check Projects tab shows correct count
- [ ] Verify all displayed projects are yours

### ? Other User's Profile
- [ ] Visit another user's profile (`/profile/{otherUserId}`)
- [ ] Check Projects tab shows **their** count, not yours
- [ ] Verify all displayed projects belong to them

### ? Empty State
- [ ] View a user with 0 projects
- [ ] Verify tab shows "Projects (0)"
- [ ] Verify tab is disabled (if not own profile)

### ? Pagination
- [ ] Create 25+ projects
- [ ] Verify pagination controls work
- [ ] Verify only 20 projects load at once

---

## ?? Common Issues

### Issue 1: Still getting wrong count

**Symptom**: Tab shows "Projects (2)" but only 1 displays  
**Cause**: Still using old endpoint or old filtering logic  
**Fix**: Ensure using `useGetUserProjectsQuery({ userId })`

### Issue 2: "User not found" error

**Symptom**: 404 error when viewing profile  
**Cause**: Invalid userId or user doesn't exist  
**Fix**: Verify userId is valid before making request

```typescript
const { data: projectsData } = useGetUserProjectsQuery(
  { userId },
  { skip: !userId }  // Don't fetch if no userId
);
```

### Issue 3: Authentication error

**Symptom**: 401 Unauthorized  
**Cause**: Missing or invalid token  
**Fix**: Ensure user is logged in and token is valid

```typescript
const { data: projectsData } = useGetUserProjectsQuery(
  { userId },
  { skip: !isAuthenticated }  // Only fetch if logged in
);
```

---

## ?? API Response Format

### Success Response (200)

```json
{
  "results": [
    {
      "projectId": 1,
      "ideaId": null,
      "aiIdeaId": 5,
      "userId": 2,
      "title": "Recycled Planter",
      "description": "Turn old bottles into planters",
      "status": "completed",
      "imageUrl": "/uploads/images/abc123.jpg"
    }
  ],
  "total": 1,
  "skip": 0,
  "take": 20
}
```

### User Not Found (404)

```json
{
  "error": "User not found"
}
```

### Unauthorized (401)

```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

---

## ?? TypeScript Types

```typescript
// Add to your types file

export interface ProjectDto {
  projectId: number;
  ideaId: number | null;
  aiIdeaId: number | null;
  userId: number;
  title: string;
  description: string | null;
  status: 'draft' | 'in_progress' | 'completed';
  imageUrl: string | null;
}

export interface PagedProjectsDto {
  results: ProjectDto[];
  total: number;
  skip: number;
  take: number;
}

export interface GetUserProjectsParams {
  userId: number;
  skip?: number;
  take?: number;
}
```

---

## ?? Performance Tips

### 1. Cache Invalidation

```typescript
// Invalidate cache when user creates/updates/deletes project
invalidatesTags: (result, error, arg) => [
  { type: 'Project', id: `USER_${arg.userId}` }
]
```

### 2. Prefetching

```typescript
// Prefetch projects when hovering over profile link
const [trigger] = usePrefetch('getUserProjects');

<Link 
  to={`/profile/${userId}`}
  onMouseEnter={() => trigger({ userId })}
>
  View Profile
</Link>
```

### 3. Optimistic Updates

```typescript
// Optimistically update count after creating project
onQueryStarted: async (arg, { dispatch, queryFulfilled }) => {
  const patchResult = dispatch(
    apiSlice.util.updateQueryData('getUserProjects', { userId: arg.userId }, (draft) => {
      draft.total += 1;
    })
  );
  try {
    await queryFulfilled;
  } catch {
    patchResult.undo();
  }
}
```

---

## ?? Mobile Considerations

```typescript
// Adjust take parameter for mobile
const isMobile = useMediaQuery('(max-width: 768px)');

const { data } = useGetUserProjectsQuery({ 
  userId,
  take: isMobile ? 10 : 20  // Fewer items on mobile
});
```

---

## ? Final Checklist

- [ ] API slice updated with `getUserProjects` query
- [ ] Component uses new hook with `userId` parameter
- [ ] Removed defensive filtering logic
- [ ] Tab count uses `total` from API response
- [ ] Error handling implemented
- [ ] Loading states handled
- [ ] Tested with own profile
- [ ] Tested with other user's profile
- [ ] Tested with user with 0 projects
- [ ] Tested pagination
- [ ] TypeScript types updated
- [ ] Cache invalidation configured

---

## ?? Need Help?

**Backend Documentation**: See `USER_PROJECTS_ENDPOINT_FIX.md`  
**Test Script**: Run `TEST_USER_PROJECTS_ENDPOINT.ps1`  
**API Docs**: https://localhost:7099/swagger

---

**Status**: ? Backend Ready  
**Next**: Frontend Integration  
**ETA**: 30 minutes
