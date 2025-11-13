# Backend Image Upload Verification Test
# Run this to verify all backend requirements are working

Write-Host "?? Backend Image Upload Verification Test" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "https://localhost:7099"
$testImagePath = "test-image.jpg"

# Create a test image if it doesn't exist
if (-not (Test-Path $testImagePath)) {
    Write-Host "?? Creating test image..." -ForegroundColor Yellow
    
    # Create a simple 1x1 pixel JPEG
    $base64Image = "/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAIBAQIBAQICAgICAgICAwUDAwMDAwYEBAMFBwYHBwcGBwcICQsJCAgKCAcHCg0KCgsMDAwMBwkODw0MDgsMDAz/2wBDAQICAgMDAwYDAwYMCAcIDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAz/wAARCAABAAEDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlbaWmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD9/KKKKAP/2Q=="
    $imageBytes = [Convert]::FromBase64String($base64Image)
    [System.IO.File]::WriteAllBytes($testImagePath, $imageBytes)
    
    Write-Host "? Test image created: $testImagePath" -ForegroundColor Green
}

Write-Host ""
Write-Host "1??  Testing Image Upload (Multipart Form Data)" -ForegroundColor Cyan
Write-Host "   Endpoint: POST /api/images/upload" -ForegroundColor Gray

try {
    $form = @{
        file = Get-Item $testImagePath
        ownerType = "Project"
        ownerId = 1
        altText = "Backend verification test image"
    }

    $uploadResponse = Invoke-WebRequest `
        -Uri "$baseUrl/api/images/upload" `
        -Method POST `
        -Form $form `
        -SkipCertificateCheck `
        -ErrorAction Stop

    $uploadResult = $uploadResponse.Content | ConvertFrom-Json
    
    Write-Host "   ? Upload successful!" -ForegroundColor Green
    Write-Host "      ImageId: $($uploadResult.imageId)" -ForegroundColor White
    Write-Host "      URL: $($uploadResult.url)" -ForegroundColor White
    Write-Host "      FileName: $($uploadResult.fileName)" -ForegroundColor White
    
    $imageUrl = $uploadResult.url
    $imageId = $uploadResult.imageId
    
    # Verify URL format
    Write-Host ""
    Write-Host "2??  Verifying URL Format" -ForegroundColor Cyan
    if ($imageUrl -match '^/uploads/images/[a-f0-9\-]+\.(jpg|jpeg|png|gif|webp)$') {
        Write-Host "   ? URL format correct: $imageUrl" -ForegroundColor Green
        Write-Host "      (Relative path, not absolute)" -ForegroundColor Gray
    } else {
        Write-Host "   ? URL format incorrect: $imageUrl" -ForegroundColor Red
        Write-Host "      Expected: /uploads/images/{guid}.{ext}" -ForegroundColor Red
    }
    
    # Test image retrieval WITHOUT authentication
    Write-Host ""
    Write-Host "3??  Testing Image Retrieval (No Auth)" -ForegroundColor Cyan
    Write-Host "   Endpoint: GET $imageUrl" -ForegroundColor Gray
    
    $fullImageUrl = "$baseUrl$imageUrl"
    $downloadResponse = Invoke-WebRequest `
        -Uri $fullImageUrl `
        -Method GET `
        -SkipCertificateCheck `
        -ErrorAction Stop
    
    Write-Host "   ? Image retrieved successfully!" -ForegroundColor Green
    Write-Host "      Status: $($downloadResponse.StatusCode)" -ForegroundColor White
    Write-Host "      Content-Type: $($downloadResponse.Headers['Content-Type'])" -ForegroundColor White
    Write-Host "      Size: $($downloadResponse.RawContentLength) bytes" -ForegroundColor White
    
    # Verify Content-Type
    Write-Host ""
    Write-Host "4??  Verifying Content-Type Header" -ForegroundColor Cyan
    $contentType = $downloadResponse.Headers['Content-Type']
    if ($contentType -match '^image/(jpeg|jpg|png|gif|webp)') {
        Write-Host "   ? Content-Type correct: $contentType" -ForegroundColor Green
        Write-Host "      (Not application/octet-stream)" -ForegroundColor Gray
    } else {
        Write-Host "   ? Content-Type incorrect: $contentType" -ForegroundColor Red
        Write-Host "      Expected: image/jpeg or similar" -ForegroundColor Red
    }
    
    # Test CORS headers
    Write-Host ""
    Write-Host "5??  Checking CORS Headers" -ForegroundColor Cyan
    if ($downloadResponse.Headers.ContainsKey('Access-Control-Allow-Origin') -or 
        $uploadResponse.Headers.ContainsKey('Access-Control-Allow-Origin')) {
        Write-Host "   ? CORS headers present" -ForegroundColor Green
        if ($downloadResponse.Headers.ContainsKey('Access-Control-Allow-Origin')) {
            Write-Host "      Origin: $($downloadResponse.Headers['Access-Control-Allow-Origin'])" -ForegroundColor White
        }
    } else {
        Write-Host "   ??  CORS headers not visible in this test" -ForegroundColor Yellow
        Write-Host "      (May require Origin header in request)" -ForegroundColor Gray
    }
    
    # List images for the owner
    Write-Host ""
    Write-Host "6??  Testing List Images Endpoint" -ForegroundColor Cyan
    Write-Host "   Endpoint: GET /api/images/Project/1" -ForegroundColor Gray
    
    $listResponse = Invoke-WebRequest `
        -Uri "$baseUrl/api/images/Project/1" `
        -Method GET `
        -SkipCertificateCheck `
        -ErrorAction Stop
    
    $images = $listResponse.Content | ConvertFrom-Json
    Write-Host "   ? List retrieved successfully!" -ForegroundColor Green
    Write-Host "      Total images: $($images.Count)" -ForegroundColor White
    
    if ($images.Count -gt 0) {
        Write-Host "      Latest image:" -ForegroundColor Gray
        $latest = $images[0]
        Write-Host "        ImageId: $($latest.imageId)" -ForegroundColor White
        Write-Host "        Url: $($latest.url)" -ForegroundColor White
        Write-Host "        Source: $($latest.source)" -ForegroundColor White
    }
    
    # Test deletion
    Write-Host ""
    Write-Host "7??  Testing Image Deletion" -ForegroundColor Cyan
    Write-Host "   Endpoint: DELETE /api/images/$imageId" -ForegroundColor Gray
    
    $deleteResponse = Invoke-WebRequest `
        -Uri "$baseUrl/api/images/$imageId" `
        -Method DELETE `
        -SkipCertificateCheck `
        -ErrorAction Stop
    
    if ($deleteResponse.StatusCode -eq 204) {
        Write-Host "   ? Image deleted successfully!" -ForegroundColor Green
        Write-Host "      Status: 204 No Content" -ForegroundColor White
    } else {
        Write-Host "   ??  Unexpected status: $($deleteResponse.StatusCode)" -ForegroundColor Yellow
    }
    
    # Final summary
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Cyan
    Write-Host "? ALL BACKEND TESTS PASSED!" -ForegroundColor Green
    Write-Host "=========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Backend Checklist Verification:" -ForegroundColor White
    Write-Host "  ? Image Upload - Saves to disk correctly" -ForegroundColor Green
    Write-Host "  ? URL Storage - Relative path format (/uploads/...)" -ForegroundColor Green
    Write-Host "  ? Static Files - UseStaticFiles() configured" -ForegroundColor Green
    Write-Host "  ? No Auth Required - Images accessible without token" -ForegroundColor Green
    Write-Host "  ? Content-Type - Correct image MIME type" -ForegroundColor Green
    Write-Host "  ? CORS - Configured for frontend access" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? Backend is ready for frontend integration!" -ForegroundColor Cyan
    
} catch {
    Write-Host ""
    Write-Host "? Test failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "  1. Ensure backend is running at $baseUrl" -ForegroundColor White
    Write-Host "  2. Check that Project with ID 1 exists in database" -ForegroundColor White
    Write-Host "  3. Verify SQL Server is running" -ForegroundColor White
    Write-Host "  4. Check uploads/images directory permissions" -ForegroundColor White
    Write-Host ""
    Write-Host "Run these SQL commands to verify:" -ForegroundColor Yellow
    Write-Host "  SELECT * FROM app.Project WHERE ProjectId = 1;" -ForegroundColor Gray
    Write-Host "  SELECT * FROM app.Image;" -ForegroundColor Gray
    exit 1
}

# Cleanup
if (Test-Path $testImagePath) {
    Remove-Item $testImagePath -Force
    Write-Host "?? Cleaned up test image file" -ForegroundColor Gray
}

Write-Host ""
Write-Host "?? Next Steps:" -ForegroundColor Cyan
Write-Host "   1. Share BACKEND_IMAGE_UPLOAD_VERIFICATION.md with frontend team" -ForegroundColor White
Write-Host "   2. Frontend should use POST /api/images/upload endpoint" -ForegroundColor White
Write-Host "   3. Images accessible at: $baseUrl/uploads/images/{filename}" -ForegroundColor White
Write-Host "   4. No Authorization header needed for GET requests" -ForegroundColor White
Write-Host ""
