# ?? Image Upload API - Quick Reference

## ?? Quick Start (Recommended Method)

### Upload File (Multipart/Form-Data)
```bash
curl -X POST http://localhost:5000/api/images/upload \
  -F "file=@image.jpg" \
  -F "ownerType=Project" \
  -F "ownerId=123"
```

### JavaScript/React
```javascript
const formData = new FormData();
formData.append('file', fileInput.files[0]);
formData.append('ownerType', 'Project');
formData.append('ownerId', '123');

const response = await fetch('/api/images/upload', {
  method: 'POST',
  body: formData
});

const { imageId, url } = await response.json();
// url: "/uploads/images/abc-123.jpg"
```

---

## ?? All Methods Comparison

| Method | Use Case | Request Type | Example |
|--------|----------|--------------|---------|
| **File Upload** ? | Upload from device | `multipart/form-data` | See above |
| **Base64** | Inline data/canvas | `application/json` | See below |
| **External URL** | Link to existing image | `application/json` | See below |
| **Cloudinary** | CDN reference | `application/json` | See below |

---

## ?? Method 1: File Upload (RECOMMENDED)

**Endpoint**: `POST /api/images/upload`

```javascript
// React Example
const handleUpload = async (file: File, projectId: number) => {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('ownerType', 'Project');
  formData.append('ownerId', projectId.toString());
  
  const res = await fetch('/api/images/upload', {
    method: 'POST',
    body: formData
  });
  
  return await res.json(); // { imageId, url, fileName }
};
```

**Validation**:
- Max size: 10MB
- Allowed types: JPEG, PNG, GIF, WebP
- Owner must exist

---

## ?? Method 2: Base64 Upload

**Endpoint**: `POST /api/images`

```javascript
// Convert file to base64
const fileToBase64 = (file: File): Promise<string> => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result as string);
    reader.onerror = reject;
    reader.readAsDataURL(file);
  });
};

// Upload
const base64 = await fileToBase64(file);
const response = await fetch('/api/images', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    ownerType: 'Project',
    ownerId: 123,
    source: base64, // "data:image/jpeg;base64,..."
    altText: 'My image'
  })
});
```

---

## ?? Method 3: External URL

**Endpoint**: `POST /api/images`

```javascript
const response = await fetch('/api/images', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    ownerType: 'Project',
    ownerId: 123,
    source: 'url',
    url: 'https://example.com/image.jpg',
    altText: 'External image'
  })
});
```

---

## ?? Method 4: Cloudinary URL

**Endpoint**: `POST /api/images`

```javascript
const response = await fetch('/api/images', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    ownerType: 'Project',
    ownerId: 123,
    source: 'cloudinary',
    url: 'https://res.cloudinary.com/demo/image/upload/sample.jpg',
    altText: 'CDN image'
  })
});
```

---

## ?? Get Images

**Endpoint**: `GET /api/images/{ownerType}/{ownerId}`

```javascript
const response = await fetch('/api/images/Project/123');
const images = await response.json();
// [{ imageId, ownerType, ownerId, source, url, altText, createdAtUtc }, ...]
```

---

## ??? Delete Image

**Endpoint**: `DELETE /api/images/{imageId}`

```javascript
await fetch('/api/images/1', { method: 'DELETE' });
// Returns 204 No Content on success
```

---

## ?? Common Errors

| Error | Fix |
|-------|-----|
| "File is required" | Use `FormData.append('file', file)` not `FormData.append('file', fileInput)` |
| "Invalid file type" | Check `file.type` is `image/jpeg`, `image/png`, etc. |
| "File size exceeds..." | Compress image or reduce resolution |
| "Owner not found" | Ensure Project/Item/Idea exists before upload |
| "Invalid base64..." | Check data URL format: `data:image/jpeg;base64,...` |

---

## ?? Validation Rules

### File Upload
- ? MIME type must be: `image/jpeg`, `image/jpg`, `image/png`, `image/gif`, `image/webp`
- ? Max size: 10MB
- ? Owner (Project/Item/Idea) must exist

### Base64
- ? Must start with `data:image/{type};base64,`
- ? Max decoded size: 10MB
- ? Allowed types: same as file upload

### External URL
- ? Must provide valid URL
- ? Source must be `url` or `cloudinary`

---

## ?? Storage

### Uploaded Files
- **Path**: `{ProjectRoot}/uploads/images/`
- **Filename**: `{GUID}.{extension}`
- **URL**: `/uploads/images/{GUID}.{extension}`
- **Access**: `http://localhost:5000/uploads/images/{GUID}.jpg`

### External URLs
- Stored as-is in database
- Not downloaded to server

---

## ?? Test Endpoints

```bash
# 1. Upload file
curl -X POST http://localhost:5000/api/images/upload \
  -F "file=@test.jpg" \
  -F "ownerType=Project" \
  -F "ownerId=1"

# 2. List images
curl http://localhost:5000/api/images/Project/1

# 3. Delete image
curl -X DELETE http://localhost:5000/api/images/1

# 4. Upload base64
curl -X POST http://localhost:5000/api/images \
  -H "Content-Type: application/json" \
  -d '{
    "ownerType": "Project",
    "ownerId": 1,
    "source": "data:image/png;base64,iVBORw0KGgo..."
  }'

# 5. External URL
curl -X POST http://localhost:5000/api/images \
  -H "Content-Type: application/json" \
  -d '{
    "ownerType": "Project",
    "ownerId": 1,
    "source": "url",
    "url": "https://example.com/image.jpg"
  }'
```

---

## ?? React Component Example

```tsx
import { useState } from 'react';

export function ImageUpload({ projectId }: { projectId: number }) {
  const [url, setUrl] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);

  const handleChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setUploading(true);
    try {
      const formData = new FormData();
      formData.append('file', file);
      formData.append('ownerType', 'Project');
      formData.append('ownerId', projectId.toString());

      const res = await fetch('/api/images/upload', {
        method: 'POST',
        body: formData
      });

      if (!res.ok) throw new Error('Upload failed');

      const data = await res.json();
      setUrl(data.url);
    } finally {
      setUploading(false);
    }
  };

  return (
    <div>
      <input type="file" accept="image/*" onChange={handleChange} />
      {uploading && <p>Uploading...</p>}
      {url && <img src={url} alt="Uploaded" style={{ maxWidth: 300 }} />}
    </div>
  );
}
```

---

## ? Checklist

Before deploying:
- [ ] Test file upload endpoint
- [ ] Test base64 upload
- [ ] Test external URL
- [ ] Verify static file serving works
- [ ] Check file size validation
- [ ] Check file type validation
- [ ] Test owner existence validation
- [ ] Test deletion
- [ ] Configure production storage (Azure Blob, S3, etc.)

---

## ?? Full Documentation

See `IMAGE_UPLOAD_API_DOCUMENTATION.md` for complete details.

---

**Quick Reference Version**: 1.0  
**Last Updated**: 2024  
**Status**: ? Ready for use
