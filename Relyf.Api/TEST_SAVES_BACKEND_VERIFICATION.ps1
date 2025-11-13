# PowerShell Test Script: Verify Saves Backend
# Tests that the backend save functionality is working correctly

$baseUrl = "https://localhost:7139"
$token = "" # TODO: Insert valid JWT token here

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "SAVES BACKEND VERIFICATION TEST" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

# Step 1: Get a valid user token first
Write-Host "`n1??  LOGIN (Get Token)" -ForegroundColor Yellow
$loginBody = @{
    email = "test@example.com"
    password = "Test123!"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body $loginBody
    
    $token = $loginResponse.token
    Write-Host "? Login successful! Token acquired." -ForegroundColor Green
    Write-Host "   Token: $($token.Substring(0, 20))..." -ForegroundColor Gray
} catch {
    Write-Host "? Login failed: $_" -ForegroundColor Red
    Write-Host "??  Please create a test user first or update credentials" -ForegroundColor Yellow
    exit 1
}

# Step 2: Get current user profile to know our userId
Write-Host "`n2??  GET CURRENT USER PROFILE" -ForegroundColor Yellow
try {
    $headers = @{
        "Authorization" = "Bearer $token"
    }
    
    # Decode JWT to get userId (basic decode)
    $tokenParts = $token.Split('.')
    $payload = $tokenParts[1]
    # Add padding if needed
    while ($payload.Length % 4 -ne 0) {
        $payload += "="
    }
    $decodedBytes = [System.Convert]::FromBase64String($payload)
    $decodedPayload = [System.Text.Encoding]::UTF8.GetString($decodedBytes)
    $payloadObj = $decodedPayload | ConvertFrom-Json
    
    $userId = $payloadObj.sub
    Write-Host "? Current User ID: $userId" -ForegroundColor Green
} catch {
    Write-Host "? Failed to decode token: $_" -ForegroundColor Red
    exit 1
}

# Step 3: Save an idea
Write-Host "`n3??  SAVE AN IDEA" -ForegroundColor Yellow
$saveBody = @{
    ideaId = 1  # Using idea ID 1
} | ConvertTo-Json

try {
    $saveResponse = Invoke-WebRequest -Uri "$baseUrl/api/saves" `
        -Method PUT `
        -Headers $headers `
        -ContentType "application/json" `
        -Body $saveBody
    
    Write-Host "? Save successful!" -ForegroundColor Green
    Write-Host "   Status: $($saveResponse.StatusCode)" -ForegroundColor Gray
    Write-Host "   Response: $($saveResponse.Content)" -ForegroundColor Gray
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "? Save failed with status $statusCode" -ForegroundColor Red
    Write-Host "   Error: $_" -ForegroundColor Red
    
    if ($statusCode -eq 400) {
        Write-Host "   ?? Idea ID 1 might not exist. Try creating an idea first." -ForegroundColor Yellow
    }
}

# Step 4: Get saved ideas for current user
Write-Host "`n4??  GET SAVED IDEAS" -ForegroundColor Yellow
try {
    $savedIdeas = Invoke-RestMethod -Uri "$baseUrl/api/saves/user/$userId" `
        -Method GET `
        -Headers $headers
    
    Write-Host "? Retrieved saved ideas!" -ForegroundColor Green
    Write-Host "   Count: $($savedIdeas.Count)" -ForegroundColor Gray
    
    if ($savedIdeas.Count -gt 0) {
        Write-Host "`n   Saved Ideas:" -ForegroundColor Cyan
        foreach ($idea in $savedIdeas) {
            Write-Host "   - Idea #$($idea.ideaId): $($idea.title)" -ForegroundColor White
            Write-Host "     Preview: $($idea.preview)" -ForegroundColor Gray
            Write-Host "     Saved: $($idea.savedAtUtc)" -ForegroundColor Gray
        }
    } else {
        Write-Host "   ??  No saved ideas found!" -ForegroundColor Yellow
        Write-Host "   This could mean:" -ForegroundColor Yellow
        Write-Host "   - The save didn't work (check step 3)" -ForegroundColor Yellow
        Write-Host "   - The idea was deleted (IsDeleted = 1)" -ForegroundColor Yellow
        Write-Host "   - Database issue" -ForegroundColor Yellow
    }
} catch {
    Write-Host "? Failed to get saved ideas: $_" -ForegroundColor Red
}

# Step 5: Get user profile with save count
Write-Host "`n5??  GET USER PROFILE (Check SaveCount)" -ForegroundColor Yellow
try {
    $profile = Invoke-RestMethod -Uri "$baseUrl/api/users/$userId" `
        -Method GET `
        -Headers $headers
    
    Write-Host "? Profile retrieved!" -ForegroundColor Green
    Write-Host "   User: $($profile.displayName) (@$($profile.userName))" -ForegroundColor Gray
    Write-Host "   SaveCount: $($profile.saveCount)" -ForegroundColor Cyan
    Write-Host "   IdeaCount: $($profile.ideaCount)" -ForegroundColor Gray
    Write-Host "   ProjectCount: $($profile.projectCount)" -ForegroundColor Gray
    
    if ($profile.saveCount -eq 0) {
        Write-Host "   ??  SaveCount is 0! This is the issue!" -ForegroundColor Red
    }
} catch {
    Write-Host "? Failed to get profile: $_" -ForegroundColor Red
}

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "TEST COMPLETE" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

Write-Host "`n?? Summary:" -ForegroundColor Yellow
Write-Host "If saves are working but SaveCount is 0, the issue is in UserRepository.GetProfileAsync" -ForegroundColor White
Write-Host "If saves are failing, check if the idea exists and is not deleted" -ForegroundColor White
Write-Host "If retrieving saves returns empty array, check the JOIN in SaveRepository.ListForUserAsync" -ForegroundColor White
