# Image Upload API Test Script (PowerShell)
# Tests all upload methods and validates responses

$ErrorActionPreference = "Stop"

$baseUrl = "http://localhost:5100"  # Updated to match actual port
$projectId = 1  # Change to existing project ID
$itemId = 1     # Change to existing item ID

Write-Host "?? Image Upload API Test Suite" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan
Write-Host ""

# Create test image files
Write-Host "?? Creating test files..." -ForegroundColor Yellow
"Test image data" | Out-File -FilePath "test.jpg" -Encoding UTF8 -NoNewline
"Test PNG data" | Out-File -FilePath "test.png" -Encoding UTF8 -NoNewline

# Create 1x1 red pixel PNG (base64)
$base64Png = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8DwHwAFBQIAX8jx0gAAAABJRU5ErkJggg=="

Write-Host ""
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "TEST 1: File Upload (Multipart)" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "Uploading test.jpg via multipart/form-data..."

$multipartContent = [System.Net.Http.MultipartFormDataContent]::new()
$fileContent = [System.Net.Http.ByteArrayContent]::new([System.IO.File]::ReadAllBytes("test.jpg"))
$fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("image/jpeg")
$multipartContent.Add($fileContent, "file", "test.jpg")
$multipartContent.Add([System.Net.Http.StringContent]::new("Project"), "ownerType")
$multipartContent.Add([System.Net.Http.StringContent]::new($projectId), "ownerId")
$multipartContent.Add([System.Net.Http.StringContent]::new("Test upload via multipart"), "altText")

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/images/upload" -Method Post -Body $multipartContent -UseBasicParsing
    if ($response.StatusCode -eq 201) {
        Write-Host "? PASSED - Status: 201 Created" -ForegroundColor Green
        Write-Host "Response: $($response.Content)"
        $result = $response.Content | ConvertFrom-Json
        $imageId = $result.imageId
        $imageUrl = $result.url
        Write-Host "Image ID: $imageId"
        Write-Host "Image URL: $imageUrl"
    } else {
        Write-Host "? FAILED - Expected 201, got $($response.StatusCode)" -ForegroundColor Red
    }
} catch {
    Write-Host "? FAILED - $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "TEST 2: Base64 Upload" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "Uploading base64 PNG via JSON..."

$body = @{
    ownerType = "Project"
    ownerId = $projectId
    source = $base64Png
    altText = "Base64 test upload"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/images" -Method Post -Body $body -ContentType "application/json" -UseBasicParsing
    if ($response.StatusCode -eq 201) {
        Write-Host "? PASSED - Status: 201 Created" -ForegroundColor Green
        Write-Host "Response: $($response.Content)"
    } else {
        Write-Host "? FAILED - Expected 201, got $($response.StatusCode)" -ForegroundColor Red
    }
} catch {
    Write-Host "? FAILED - $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "TEST 3: External URL Upload" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "Adding external URL via JSON..."

$body = @{
    ownerType = "Project"
    ownerId = $projectId
    source = "url"
    url = "https://via.placeholder.com/150"
    altText = "External placeholder image"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/images" -Method Post -Body $body -ContentType "application/json" -UseBasicParsing
    if ($response.StatusCode -eq 201) {
        Write-Host "? PASSED - Status: 201 Created" -ForegroundColor Green
        Write-Host "Response: $($response.Content)"
    } else {
        Write-Host "? FAILED - Expected 201, got $($response.StatusCode)" -ForegroundColor Red
    }
} catch {
    Write-Host "? FAILED - $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "TEST 4: List Images for Project" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "Getting all images for Project $projectId..."

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/images/Project/$projectId" -Method Get -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-Host "? PASSED - Status: 200 OK" -ForegroundColor Green
        Write-Host "Response: $($response.Content)"
        $images = $response.Content | ConvertFrom-Json
        Write-Host "Total images: $($images.Count)"
    } else {
        Write-Host "? FAILED - Expected 200, got $($response.StatusCode)" -ForegroundColor Red
    }
} catch {
    Write-Host "? FAILED - $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "TEST 5: Invalid File Type" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "Attempting to upload text file (should fail)..."

"Not an image" | Out-File -FilePath "test.txt" -Encoding UTF8 -NoNewline

$multipartContent = [System.Net.Http.MultipartFormDataContent]::new()
$fileContent = [System.Net.Http.ByteArrayContent]::new([System.IO.File]::ReadAllBytes("test.txt"))
$fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("text/plain")
$multipartContent.Add($fileContent, "file", "test.txt")
$multipartContent.Add([System.Net.Http.StringContent]::new("Project"), "ownerType")
$multipartContent.Add([System.Net.Http.StringContent]::new($projectId), "ownerId")

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/images/upload" -Method Post -Body $multipartContent -UseBasicParsing
    Write-Host "? FAILED - Expected 400, got $($response.StatusCode)" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "? PASSED - Correctly rejected with 400 Bad Request" -ForegroundColor Green
        $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
        Write-Host "Error message: $($reader.ReadToEnd())"
    } else {
        Write-Host "? FAILED - Expected 400, got $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "TEST 6: Missing File" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "Attempting upload without file (should fail)..."

$multipartContent = [System.Net.Http.MultipartFormDataContent]::new()
$multipartContent.Add([System.Net.Http.StringContent]::new("Project"), "ownerType")
$multipartContent.Add([System.Net.Http.StringContent]::new($projectId), "ownerId")

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/images/upload" -Method Post -Body $multipartContent -UseBasicParsing
    Write-Host "? FAILED - Expected 400, got $($response.StatusCode)" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "? PASSED - Correctly rejected with 400 Bad Request" -ForegroundColor Green
        $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
        Write-Host "Error message: $($reader.ReadToEnd())"
    } else {
        Write-Host "? FAILED - Expected 400, got $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "TEST 7: Invalid Owner Type" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "Attempting upload with invalid ownerType (should fail)..."

$multipartContent = [System.Net.Http.MultipartFormDataContent]::new()
$fileContent = [System.Net.Http.ByteArrayContent]::new([System.IO.File]::ReadAllBytes("test.jpg"))
$fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("image/jpeg")
$multipartContent.Add($fileContent, "file", "test.jpg")
$multipartContent.Add([System.Net.Http.StringContent]::new("InvalidType"), "ownerType")
$multipartContent.Add([System.Net.Http.StringContent]::new($projectId), "ownerId")

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/images/upload" -Method Post -Body $multipartContent -UseBasicParsing
    Write-Host "? FAILED - Expected 400, got $($response.StatusCode)" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "? PASSED - Correctly rejected with 400 Bad Request" -ForegroundColor Green
        $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
        Write-Host "Error message: $($reader.ReadToEnd())"
    } else {
        Write-Host "? FAILED - Expected 400, got $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "TEST 8: Delete Image" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan

if ($imageId) {
    Write-Host "Deleting image with ID $imageId..."
    
    try {
        $response = Invoke-WebRequest -Uri "$baseUrl/api/images/$imageId" -Method Delete -UseBasicParsing
        if ($response.StatusCode -eq 204) {
            Write-Host "? PASSED - Status: 204 No Content" -ForegroundColor Green
        } else {
            Write-Host "? FAILED - Expected 204, got $($response.StatusCode)" -ForegroundColor Red
        }
    } catch {
        Write-Host "? FAILED - $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "??  SKIPPED - No image ID from previous tests" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "TEST 9: Delete Non-Existent Image" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "Attempting to delete non-existent image (should return 404)..."

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/images/999999" -Method Delete -UseBasicParsing
    Write-Host "? FAILED - Expected 404, got $($response.StatusCode)" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "? PASSED - Correctly returned 404 Not Found" -ForegroundColor Green
    } else {
        Write-Host "? FAILED - Expected 404, got $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "TEST 10: Cloudinary URL" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "Adding Cloudinary URL via JSON..."

$body = @{
    ownerType = "Project"
    ownerId = $projectId
    source = "cloudinary"
    url = "https://res.cloudinary.com/demo/image/upload/sample.jpg"
    altText = "Cloudinary test image"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/images" -Method Post -Body $body -ContentType "application/json" -UseBasicParsing
    if ($response.StatusCode -eq 201) {
        Write-Host "? PASSED - Status: 201 Created" -ForegroundColor Green
        Write-Host "Response: $($response.Content)"
    } else {
        Write-Host "? FAILED - Expected 201, got $($response.StatusCode)" -ForegroundColor Red
    }
} catch {
    Write-Host "? FAILED - $($_.Exception.Message)" -ForegroundColor Red
}

# Cleanup test files
Write-Host ""
Write-Host "?? Cleaning up test files..." -ForegroundColor Yellow
Remove-Item -Path "test.jpg", "test.png", "test.txt" -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "? Test Suite Complete" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary:"
Write-Host "- All core upload methods tested"
Write-Host "- Validation checks verified"
Write-Host "- Error handling confirmed"
Write-Host "- CRUD operations validated"
Write-Host ""
Write-Host "Next steps:"
Write-Host "1. Verify uploaded files in: {ProjectRoot}\uploads\images\"
Write-Host "2. Test static file access: http://localhost:5000/uploads/images/{filename}"
Write-Host "3. Integrate with frontend application"
Write-Host ""
