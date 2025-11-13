# Test script to diagnose SavedIdea issues
# Run this against your API to check the save flow

param(
    [string]$BaseUrl = "https://localhost:7139",
    [string]$Token = ""
)

if ([string]::IsNullOrEmpty($Token)) {
    Write-Host "ERROR: Please provide a JWT token" -ForegroundColor Red
    Write-Host "Usage: .\TEST_SAVED_IDEA_DIAGNOSIS.ps1 -Token 'YOUR_JWT_TOKEN'"
    exit 1
}

$Headers = @{
    "Authorization" = "Bearer $Token"
    "Content-Type" = "application/json"
}

Write-Host "==================== SAVED IDEA DIAGNOSIS ====================" -ForegroundColor Cyan

# Step 1: Get current user info
Write-Host "`n[Step 1] Getting current user info..." -ForegroundColor Yellow
try {
    # First, generate an idea to get an IdeaId
    Write-Host "Generating a test idea..." -ForegroundColor Gray
    $generateBody = @{
        PromptText = "Creative ways to upcycle plastic bottles"
        TitleHint = "Plastic Bottle Ideas"
    } | ConvertTo-Json

    $generateResponse = Invoke-RestMethod -Uri "$BaseUrl/api/Ideas/generate" `
        -Method Post -Headers $Headers -Body $generateBody
    
    Write-Host "? Idea generated: IdeaId = $($generateResponse.ideaId)" -ForegroundColor Green
    $testIdeaId = $generateResponse.ideaId
    $userId = $generateResponse.userId
    
} catch {
    Write-Host "? Failed to generate idea: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Save the idea
Write-Host "`n[Step 2] Saving the idea (IdeaId: $testIdeaId)..." -ForegroundColor Yellow
try {
    $saveBody = @{
        IdeaId = $testIdeaId
    } | ConvertTo-Json

    $saveResponse = Invoke-RestMethod -Uri "$BaseUrl/api/Saves" `
        -Method Put -Headers $Headers -Body $saveBody
    
    Write-Host "? Idea saved successfully!" -ForegroundColor Green
    
} catch {
    if ($_.Exception.Response.StatusCode -eq 204) {
        Write-Host "? Idea was already saved (204 No Content)" -ForegroundColor Green
    } else {
        Write-Host "? Failed to save idea: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Response: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
}

# Step 3: Get saved ideas for user
Write-Host "`n[Step 3] Retrieving saved ideas for user $userId..." -ForegroundColor Yellow
try {
    $savedIdeasResponse = Invoke-RestMethod -Uri "$BaseUrl/api/Saves/user/$userId" `
        -Method Get -Headers $Headers
    
    Write-Host "? Retrieved saved ideas" -ForegroundColor Green
    Write-Host "Count: $($savedIdeasResponse.Count)" -ForegroundColor Cyan
    
    if ($savedIdeasResponse.Count -gt 0) {
        Write-Host "`nSaved Ideas:" -ForegroundColor Cyan
        $savedIdeasResponse | Format-Table -Property IdeaId, Title, Preview, SavedAtUtc
    } else {
        Write-Host "? WARNING: No saved ideas found!" -ForegroundColor Red
        Write-Host "Expected to find the idea we just saved (IdeaId: $testIdeaId)" -ForegroundColor Red
    }
    
} catch {
    Write-Host "? Failed to retrieve saved ideas: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.ErrorDetails.Message)" -ForegroundColor Red
}

# Step 4: Get user profile to check SaveCount
Write-Host "`n[Step 4] Checking user profile SaveCount..." -ForegroundColor Yellow
try {
    $profileResponse = Invoke-RestMethod -Uri "$BaseUrl/api/Users/$userId" `
        -Method Get -Headers $Headers
    
    Write-Host "? Retrieved user profile" -ForegroundColor Green
    Write-Host "User: $($profileResponse.displayName) (@$($profileResponse.userName))" -ForegroundColor Cyan
    Write-Host "SaveCount: $($profileResponse.saveCount)" -ForegroundColor Cyan
    Write-Host "FollowerCount: $($profileResponse.followerCount)" -ForegroundColor Gray
    Write-Host "ProjectCount: $($profileResponse.projectCount)" -ForegroundColor Gray
    Write-Host "IdeaCount: $($profileResponse.ideaCount)" -ForegroundColor Gray
    
    if ($profileResponse.saveCount -eq 0) {
        Write-Host "? WARNING: SaveCount is 0!" -ForegroundColor Red
        Write-Host "Expected SaveCount > 0 after saving an idea" -ForegroundColor Red
    } else {
        Write-Host "? SaveCount is correct: $($profileResponse.saveCount)" -ForegroundColor Green
    }
    
} catch {
    Write-Host "? Failed to retrieve user profile: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 5: Verify the idea exists
Write-Host "`n[Step 5] Verifying the idea still exists..." -ForegroundColor Yellow
try {
    $ideaResponse = Invoke-RestMethod -Uri "$BaseUrl/api/Ideas/$testIdeaId" `
        -Method Get -Headers $Headers
    
    Write-Host "? Idea exists: '$($ideaResponse.title)'" -ForegroundColor Green
    Write-Host "IsDeleted: False (idea is active)" -ForegroundColor Gray
    
} catch {
    Write-Host "? Failed to retrieve idea: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "? The idea might have been deleted, which could explain why saves don't show" -ForegroundColor Red
}

Write-Host "`n==================== DIAGNOSIS COMPLETE ====================" -ForegroundColor Cyan

Write-Host "`n?? SUMMARY:" -ForegroundColor Yellow
Write-Host "- IdeaId tested: $testIdeaId"
Write-Host "- UserId: $userId"
Write-Host "`nIf SaveCount is 0 or saved ideas list is empty, there's a bug in:"
Write-Host "1. SaveRepository.ListForUserAsync (check SQL query)"
Write-Host "2. UserRepository.GetProfileAsync (check SaveCount subquery)"
Write-Host "3. Database: app.SavedIdea table might be empty despite successful PUT"
Write-Host "`nRun the diagnose_saved_ideas.sql script on the database to investigate further."
