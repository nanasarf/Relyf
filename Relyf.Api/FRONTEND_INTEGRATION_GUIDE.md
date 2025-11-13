# ?? Frontend Integration Guide - Saved Ideas Fix

> **?? IMPORTANT UPDATE (2025-01-13):**  
> A 500 error bug in the backend has been fixed. The endpoint now works correctly.  
> See `SAVES_500_ERROR_FIX.md` for technical details.

## ? Backend Status

**Endpoint:** `GET /api/Saves/user/{userId}`  
**Status:** ? **WORKING** (500 error fixed)  
**Response Format:** ? **CORRECT** (includes preview, imageUrl, tags)

### Recent Fix
The backend had a 500 Internal Server Error caused by:
1. Immutable `Tags` property in `SavedIdeaView`
2. Inefficient object rebuilding

Both issues are now **FIXED**. The endpoint returns the correct response format.

---

## ?? Required Changes in Frontend

### 1. Update TypeScript Interface
**File:** `src/services/api/savesApi.ts` (or wherever SavedIdea is defined)

```typescript
// BEFORE ?
export interface SavedIdea {
  ideaId: number;
  title: string;
  description: string;  // ? Backend sends "preview"
  savedAtUtc: string;
  // ? Missing: imageUrl, tags
}

// AFTER ?
export interface SavedIdea {
  ideaId: number;
  title: string;
  preview: string;       // ? Matches backend response
  imageUrl?: string;     // ? NEW
  tags?: string[];       // ? NEW
  savedAtUtc: string;
}
```

---

### 2. Update Profile Component
**File:** `src/pages/Profile.tsx`

#### Option A: Use `preview` field (Recommended) ?
```tsx
// Line 151-161
{savedItems?.map((item) => (
  <Link to={`/ideas/${item.ideaId}`} key={item.ideaId}>
    <IdeaCard
      title={item.title}
      description={item.preview}  // ? CHANGED: Use preview instead of description
      imageUrl={item.imageUrl}    // ? Now works!
      tags={item.tags || []}      // ? Now works!
    />
  </Link>
))}
```

#### Option B: Add alias in backend (If frontend can't change)
If you absolutely cannot change the frontend, add this to SavedIdeaView:
```csharp
public string Description => Preview;  // Alias for frontend compatibility
```

---

### 3. Update Debug Logging (Optional)
**File:** `src/pages/Profile.tsx` (Line 51-56)

```tsx
console.log("Profile Debug:", {
  currentUserId,
  savedItems,
  savesLoading,
  savesError,
  userSaveCount: user?.saveCount,
  displayedCount: savedItems?.length,  // ? NEW: Compare counts
  itemsHaveImages: savedItems?.some(item => item.imageUrl),  // ? NEW
  itemsHaveTags: savedItems?.some(item => item.tags?.length > 0),  // ? NEW
});
```

---

## ?? Testing Steps

### 1. Verify API Response
Open browser DevTools ? Network tab, then:

```javascript
// In browser console
const response = await fetch('https://localhost:7139/api/saves/user/1', {
  headers: {
    'Authorization': 'Bearer ' + localStorage.getItem('relyf_token')
  }
});
const data = await response.json();
console.log(data);

// Expected output:
[
  {
    "ideaId": 5,
    "title": "Upcycle Bottle Into Vase",
    "preview": "Transform plastic bottles into...",  // ? Check this field
    "imageUrl": "https://...",  // ? Should be present
    "tags": ["Plastic", "DIY"],  // ? Should be present
    "savedAtUtc": "2025-01-12T10:30:00Z"
  }
]
```

### 2. Check Component Rendering
Add temporary logging in Profile.tsx:

```tsx
{savedItems?.map((item) => {
  console.log("Rendering saved idea:", {
    ideaId: item.ideaId,
    title: item.title,
    preview: item.preview,
    description: item.description,  // Should be undefined
    imageUrl: item.imageUrl,
    tags: item.tags
  });
  
  return (
    <Link to={`/ideas/${item.ideaId}`} key={item.ideaId}>
      <IdeaCard
        title={item.title}
        description={item.preview}  // ? Use this
        imageUrl={item.imageUrl}
        tags={item.tags || []}
      />
    </Link>
  );
})}
```

### 3. Visual Verification
After deploying the changes:

1. ? Saved ideas should now display images
2. ? Saved ideas should now display tags
3. ? Deleted ideas should NOT appear in the list
4. ? SaveCount may be higher than displayed count (expected)

---

## ?? Troubleshooting

### Issue: Saved ideas still not showing

**Check 1: Is the idea deleted?**
```sql
-- Run in database
SELECT IdeaId, Title, IsDeleted 
FROM app.AiIdea 
WHERE IdeaId IN (
    SELECT IdeaId FROM app.SavedIdea WHERE UserId = 1
);
```
If `IsDeleted = 1`, the idea won't show (correct behavior).

**Check 2: Is the API returning data?**
```javascript
// Browser console
const response = await fetch('https://localhost:7139/api/saves/user/1', {
  headers: { 'Authorization': 'Bearer ' + localStorage.getItem('relyf_token') }
});
console.log(await response.json());
```
If empty array `[]`, all saved ideas are deleted.

**Check 3: Is frontend using correct field?**
```typescript
// Check if you're using "description" instead of "preview"
<IdeaCard description={item.description} />  // ? Wrong
<IdeaCard description={item.preview} />      // ? Correct
```

---

### Issue: Images not showing

**Check 1: Does ImageUrl exist in response?**
```javascript
const data = await fetch('...').then(r => r.json());
console.log(data[0].imageUrl);  // Should NOT be undefined
```

**Check 2: Is IdeaCard component receiving imageUrl?**
```tsx
<IdeaCard
  title={item.title}
  description={item.preview}
  imageUrl={item.imageUrl}  // ? Make sure this is passed
  tags={item.tags || []}
/>
```

**Check 3: Does AiIdea table have ImageUrl?**
```sql
SELECT IdeaId, Title, ImageUrl 
FROM app.AiIdea 
WHERE IdeaId = 1;
```
If ImageUrl is NULL, images won't show (expected).

---

### Issue: Tags not showing

**Check 1: Does Tags exist in response?**
```javascript
const data = await fetch('...').then(r => r.json());
console.log(data[0].tags);  // Should be array, might be empty []
```

**Check 2: Are tags in database?**
```sql
SELECT it.IdeaId, t.TagName
FROM app.IdeaTag it
JOIN app.Tag t ON t.TagId = it.TagId
WHERE it.IdeaId = 1;
```
If no rows, tags won't show (expected).

---

### Issue: SaveCount ? Displayed Count

**This is EXPECTED behavior** ?

| SaveCount | Displayed Count | Reason |
|-----------|-----------------|--------|
| 5 | 5 | All saved ideas are active |
| 5 | 3 | 2 saved ideas are deleted (IsDeleted = 1) |
| 5 | 0 | All saved ideas are deleted |

**SaveCount counts all saves (including deleted ideas).**
**Displayed Count only shows active ideas (IsDeleted = 0).**

If you want them to match, ask backend to filter IsDeleted in SaveCount query.

---

## ?? Before vs After

### Before Fix ?
```json
{
  "ideaId": 5,
  "title": "Upcycle Bottle Into Vase",
  "preview": "Transform plastic bottles...",
  "savedAtUtc": "2025-01-12T10:30:00Z"
  // ? Missing: imageUrl, tags
  // ? Deleted ideas still showing
}
```

**Frontend Result:**
- No images displayed
- No tags displayed
- Deleted ideas showing (broken)

---

### After Fix ?
```json
{
  "ideaId": 5,
  "title": "Upcycle Bottle Into Vase",
  "preview": "Transform plastic bottles...",
  "imageUrl": "https://storage.example.com/idea-5.jpg",  // ? NEW
  "tags": ["Plastic", "DIY", "Home Decor"],             // ? NEW
  "savedAtUtc": "2025-01-12T10:30:00Z"
}
```

**Frontend Result:**
- ? Images displayed
- ? Tags displayed
- ? Only active ideas showing

---

## ?? Deployment Checklist

### Backend (Already Done ?)
- [x] Add `IsDeleted = 0` filter to SaveRepository
- [x] Add `ImageUrl` field to SavedIdeaView
- [x] Add `Tags` field to SavedIdeaView
- [x] Update SQL query to include ImageUrl
- [x] Load tags for saved ideas
- [x] Build passes

### Frontend (Your Turn ??)
- [ ] Update `SavedIdea` interface to use `preview` instead of `description`
- [ ] Update Profile.tsx to use `item.preview`
- [ ] Add `imageUrl` and `tags` to SavedIdea interface
- [ ] Test locally
- [ ] Deploy to staging
- [ ] Verify images and tags show
- [ ] Deploy to production

---

## ?? Support

If issues persist after making these changes:

1. **Check Network Tab** in browser DevTools
   - Look for `GET /api/saves/user/{userId}` request
   - Verify response has `imageUrl` and `tags` fields

2. **Check Console Logs**
   - Look for any errors when rendering IdeaCard
   - Check if `item.preview` is undefined

3. **Share with Backend Team:**
   - Screenshot of API response (Network tab)
   - Screenshot of console errors
   - Which fields are missing in frontend

---

## ?? Summary

### Changes Required:
1. ? **Update interface:** `description` ? `preview`
2. ? **Update component:** Use `item.preview` instead of `item.description`
3. ? **Add fields:** `imageUrl?`, `tags?`

### Expected Results:
- ? Saved ideas show images
- ? Saved ideas show tags
- ? Deleted ideas don't appear
- ?? SaveCount may be > displayed count (if ideas deleted)

---

**File:** `FRONTEND_INTEGRATION_GUIDE.md`
**Last Updated:** 2025-01-13
**Related:** `SAVED_IDEAS_FIX_COMPLETE.md`
