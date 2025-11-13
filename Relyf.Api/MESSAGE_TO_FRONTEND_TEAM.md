# ?? Message to Frontend Team - Saved Ideas Fix

## ?? TL;DR

**Issue:** Saved ideas not showing in profile, even though save count > 0.

**Root Causes:**
1. ? Backend wasn't filtering out deleted ideas (`IsDeleted = 0`)
2. ? Backend response was missing `imageUrl` and `tags` fields
3. ? Frontend was looking for `description` field, but backend sends `preview`

**Status:** ? **Backend fix deployed** - Frontend changes needed

---

## ? What Backend Fixed

### 1. Added Missing Fields to API Response
**Endpoint:** `GET /api/saves/user/{userId}`

**Before:**
```json
{
  "ideaId": 5,
  "title": "Upcycle Bottle",
  "preview": "Transform plastic...",
  "savedAtUtc": "2025-01-12T10:30:00Z"
}
```

**After:**
```json
{
  "ideaId": 5,
  "title": "Upcycle Bottle",
  "preview": "Transform plastic...",
  "imageUrl": "https://storage.example.com/idea-5.jpg",  // ? NEW
  "tags": ["Plastic", "DIY", "Home Decor"],             // ? NEW
  "savedAtUtc": "2025-01-12T10:30:00Z"
}
```

### 2. Filter Out Deleted Ideas
- Now excludes ideas with `IsDeleted = 1`
- Only returns active ideas in the saved list

---

## ?? What Frontend Needs to Change

### Change 1: Update TypeScript Interface
**File:** `src/services/api/savesApi.ts` (or similar)

```typescript
export interface SavedIdea {
  ideaId: number;
  title: string;
  preview: string;       // ? Use "preview", not "description"
  imageUrl?: string;     // ? Add this
  tags?: string[];       // ? Add this
  savedAtUtc: string;
}
```

### Change 2: Update Profile Component
**File:** `src/pages/Profile.tsx` (Line ~156)

```tsx
// BEFORE ?
<IdeaCard
  title={item.title}
  description={item.description}  // ? undefined!
  imageUrl={item.imageUrl}
  tags={item.tags || []}
/>

// AFTER ?
<IdeaCard
  title={item.title}
  description={item.preview}  // ? Use "preview"
  imageUrl={item.imageUrl}    // ? Now works
  tags={item.tags || []}      // ? Now works
/>
```

---

## ?? How to Test

### 1. Quick Browser Console Test
```javascript
// Run in browser after logging in
const token = localStorage.getItem('relyf_token');
const userId = JSON.parse(localStorage.getItem('relyf_user')).id;

const response = await fetch(`https://localhost:7139/api/saves/user/${userId}`, {
  headers: { 'Authorization': `Bearer ${token}` }
});

const data = await response.json();
console.log(data);

// ? Check that each item has:
// - ideaId
// - title
// - preview
// - imageUrl (might be null)
// - tags (might be empty array [])
// - savedAtUtc
```

### 2. Visual Test
After making the frontend changes:

1. Save a few ideas (click save button)
2. Go to your profile
3. Check "Saved Ideas" section

**Expected:**
- ? Ideas should appear
- ? Images should display (if idea has image)
- ? Tags should display (if idea has tags)
- ? Deleted ideas should NOT appear

---

## ?? Expected Behavior

### SaveCount vs Displayed Count

**This is NORMAL:**
- Profile shows `SaveCount: 5`
- But only 3 ideas are displayed

**Why?**
- SaveCount = Total times you clicked "Save" (includes deleted ideas)
- Displayed = Only active ideas (IsDeleted = 0)

If 2 ideas were deleted after you saved them, you'll see:
- SaveCount: 5 ?
- Displayed: 3 ?

---

## ?? Troubleshooting Guide

### Problem: Saved ideas still empty

**Check 1: Are all your saved ideas deleted?**
```javascript
// Browser console
const profile = await fetch(`https://localhost:7139/api/users/${userId}`, {
  headers: { 'Authorization': `Bearer ${token}` }
}).then(r => r.json());

console.log("SaveCount:", profile.saveCount);
// If saveCount > 0 but list is empty, all ideas are deleted
```

**Check 2: Is the API returning data?**
```javascript
const saves = await fetch(`https://localhost:7139/api/saves/user/${userId}`, {
  headers: { 'Authorization': `Bearer ${token}` }
}).then(r => r.json());

console.log("Saved ideas:", saves);
// Should be array with items, not empty []
```

**Check 3: Are you using the correct field?**
```tsx
// ? WRONG - "description" is undefined
<IdeaCard description={item.description} />

// ? CORRECT - "preview" exists
<IdeaCard description={item.preview} />
```

---

### Problem: Images not showing

**Check 1: Does API return imageUrl?**
```javascript
const saves = await fetch(...).then(r => r.json());
console.log(saves[0].imageUrl);  
// Should be a URL or null, NOT undefined
```

**Check 2: Is imageUrl passed to component?**
```tsx
<IdeaCard imageUrl={item.imageUrl} />  // ? Make sure this exists
```

**If imageUrl is null, it's expected** - not all ideas have images.

---

### Problem: Tags not showing

**Check 1: Does API return tags?**
```javascript
const saves = await fetch(...).then(r => r.json());
console.log(saves[0].tags);  
// Should be array [] (might be empty), NOT undefined
```

**Check 2: Are tags passed to component?**
```tsx
<IdeaCard tags={item.tags || []} />  // ? Make sure this exists
```

**If tags is empty array [], it's expected** - not all ideas have tags.

---

## ?? API Response Format Reference

### GET /api/saves/user/{userId}

**Request:**
```http
GET /api/saves/user/1
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
[
  {
    "ideaId": 5,
    "title": "Upcycle Plastic Bottle Into Garden Planter",
    "preview": "Transform empty plastic bottles into beautiful hanging planters for your garden. Simple DIY project...",
    "imageUrl": "https://relyfapi.blob.core.windows.net/ideas/idea-5.jpg",
    "tags": ["Plastic", "Garden", "DIY", "Sustainable"],
    "savedAtUtc": "2025-01-12T10:30:00.000Z"
  },
  {
    "ideaId": 12,
    "title": "T-Shirt Tote Bag",
    "preview": "Turn old t-shirts into reusable shopping bags in 5 minutes. No sewing required!",
    "imageUrl": null,
    "tags": ["Fabric", "Fashion", "Zero Waste"],
    "savedAtUtc": "2025-01-11T14:20:00.000Z"
  }
]
```

**Response (Empty - 200 OK):**
```json
[]
```

**Response (403 Forbidden):**
Trying to view saves of a user you don't follow.

---

## ?? Deployment Checklist

### Backend ?
- [x] Add IsDeleted filter
- [x] Add imageUrl field
- [x] Add tags field
- [x] Deploy to API

### Frontend ??
- [ ] Update SavedIdea interface
- [ ] Change `item.description` to `item.preview`
- [ ] Test locally
- [ ] Deploy to staging
- [ ] Deploy to production

---

## ?? Need Help?

If you're still seeing issues after making these changes, please share:

1. **Network tab screenshot** showing the API response
2. **Console logs** showing any errors
3. **Which field is undefined** (imageUrl, tags, or preview?)

We can help debug further!

---

## ?? Summary

### What Changed:
- ? Backend now returns `imageUrl` and `tags`
- ? Backend filters out deleted ideas
- ?? Frontend needs to use `preview` instead of `description`

### What You Need to Do:
1. Update TypeScript interface (add imageUrl, tags, rename description ? preview)
2. Update Profile.tsx to use `item.preview`
3. Test and deploy

### Expected Result:
- ? Saved ideas will show in profile
- ? Images will display (if available)
- ? Tags will display (if available)
- ? No more empty saved ideas list!

---

**Questions?** Drop a message in the team chat!

**Related Docs:**
- `SAVED_IDEAS_FIX_COMPLETE.md` - Full technical details
- `FRONTEND_INTEGRATION_GUIDE.md` - Detailed integration guide
- `TEST_SAVED_IDEAS_FIX.ps1` - Backend test script

---

*Backend Team*
*Date: 2025-01-13*
