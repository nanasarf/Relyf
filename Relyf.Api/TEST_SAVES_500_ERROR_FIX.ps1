# Test script to verify the 500 error fix for /api/Saves/user/{userId}
# This tests the fixed SaveRepository.ListForUserAsync() method

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing Saved Ideas 500 Error Fix" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$baseUrl = "https://localhost:7139"
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwianRpIjoiYmM1YjU0ZjQtNGM1YS00ZDcwLWI1MWQtZjhiODliZmJmZGFjIiwiaWF0IjoiMTczNjc1NjI5NCIsImV4cCI6MTczNzM2MTA5NCwiaXNzIjoiUmVseWYuQXBpIiwiYXVkIjoiUmVseWYuQ2xpZW50In0.iMFCO0c1g0YU-5101Njk4NGRkYmRhNy1kVGRsYXBsMXlYdTlzZWdOAGhHVnNuVUx5SFg0"
$userId = 4

# Headers
$headers = @{
    "Authorization" = "Bearer $token"
    "Accept" = "application/json"
}

Write-Host "Step 1: Testing GET /api/Saves/user/$userId" -ForegroundColor Yellow
Write-Host "---------------------------------------" -ForegroundColor Gray

try {
    $response = Invoke-WebRequest `
        -Uri "$baseUrl/api/Saves/user/$userId" `
        -Method GET `
        -Headers $headers `
        -SkipCertificateCheck

    $statusCode = $response.StatusCode
    $savedIdeas = $response.Content | ConvertFrom-Json

    Write-Host "? Status Code: $statusCode" -ForegroundColor Green
    Write-Host "? Response received successfully" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "Saved Ideas Count: $($savedIdeas.Count)" -ForegroundColor Cyan
    Write-Host ""

    if ($savedIdeas.Count -gt 0) {
        Write-Host "Sample Saved Idea:" -ForegroundColor Cyan
        $firstIdea = $savedIdeas[0]
        Write-Host "  IdeaId: $($firstIdea.ideaId)" -ForegroundColor White
        Write-Host "  Title: $($firstIdea.title)" -ForegroundColor White
        Write-Host "  Preview: $($firstIdea.preview.Substring(0, [Math]::Min(50, $firstIdea.preview.Length)))..." -ForegroundColor White
        Write-Host "  ImageUrl: $($firstIdea.imageUrl)" -ForegroundColor White
        Write-Host "  Tags: $($firstIdea.tags -join ', ')" -ForegroundColor White
        Write-Host "  SavedAtUtc: $($firstIdea.savedAtUtc)" -ForegroundColor White
        Write-Host ""

        # Verify all required fields are present
        Write-Host "Field Verification:" -ForegroundColor Yellow
        $fieldsOk = $true
        
        foreach ($idea in $savedIdeas) {
            if (-not $idea.ideaId) {
                Write-Host "  ? Missing ideaId in idea: $($idea.title)" -ForegroundColor Red
                $fieldsOk = $false
            }
            if (-not $idea.title) {
                Write-Host "  ? Missing title in idea ID: $($idea.ideaId)" -ForegroundColor Red
                $fieldsOk = $false
            }
            if (-not $idea.preview) {
                Write-Host "  ? Missing preview in idea: $($idea.title)" -ForegroundColor Red
                $fieldsOk = $false
            }
            if ($null -eq $idea.tags) {
                Write-Host "  ? Missing tags array in idea: $($idea.title)" -ForegroundColor Red
                $fieldsOk = $false
            }
            if (-not $idea.savedAtUtc) {
                Write-Host "  ? Missing savedAtUtc in idea: $($idea.title)" -ForegroundColor Red
                $fieldsOk = $false
            }
        }

        if ($fieldsOk) {
            Write-Host "  ? All required fields present in all ideas" -ForegroundColor Green
        }
        Write-Host ""

        # Show ideas with tags
        $ideasWithTags = $savedIdeas | Where-Object { $_.tags.Count -gt 0 }
        Write-Host "Ideas with Tags: $($ideasWithTags.Count)" -ForegroundColor Cyan
        foreach ($idea in $ideasWithTags) {
            Write-Host "  - $($idea.title): $($idea.tags -join ', ')" -ForegroundColor White
        }
        Write-Host ""

        # Show ideas with images
        $ideasWithImages = $savedIdeas | Where-Object { $_.imageUrl }
        Write-Host "Ideas with Images: $($ideasWithImages.Count)" -ForegroundColor Cyan
        foreach ($idea in $ideasWithImages) {
            Write-Host "  - $($idea.title): $($idea.imageUrl)" -ForegroundColor White
        }
    } else {
        Write-Host "? No saved ideas found for user $userId" -ForegroundColor Yellow
        Write-Host "  This could mean:" -ForegroundColor Gray
        Write-Host "  - User hasn't saved any ideas" -ForegroundColor Gray
        Write-Host "  - All saved ideas are deleted (IsDeleted = 1)" -ForegroundColor Gray
    }

} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "? Status Code: $statusCode" -ForegroundColor Red
    
    if ($statusCode -eq 500) {
        Write-Host "? INTERNAL SERVER ERROR - Fix did not work!" -ForegroundColor Red
        Write-Host ""
        Write-Host "Error Details:" -ForegroundColor Yellow
        Write-Host $_.Exception.Message -ForegroundColor Red
        Write-Host ""
        Write-Host "Likely causes:" -ForegroundColor Yellow
        Write-Host "  1. SQL syntax error in tags query" -ForegroundColor Gray
        Write-Host "  2. Dapper can't expand @ideaIds parameter" -ForegroundColor Gray
        Write-Host "  3. SavedIdeaView.Tags property is not settable" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Check backend logs for stack trace" -ForegroundColor Yellow
    } elseif ($statusCode -eq 401) {
        Write-Host "? UNAUTHORIZED - Token expired or invalid" -ForegroundColor Red
        Write-Host "  Update the token in this script" -ForegroundColor Gray
    } else {
        Write-Host "? Unexpected error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Test Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
