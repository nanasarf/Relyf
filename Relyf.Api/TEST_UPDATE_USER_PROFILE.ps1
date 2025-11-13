# ============================================================================
# TEST: PUT /api/Users/{id} - Update User Profile
# ============================================================================
# Tests the user profile update endpoint with authentication
# ============================================================================

$baseUrl = "https://localhost:7139"

Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "  UPDATE USER PROFILE API TEST" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

# ============================================================================
# STEP 1: Register a new user
# ============================================================================
Write-Host "[STEP 1] Creating test user..." -ForegroundColor Yellow

$registerPayload = @{
    email = "testuser_profile_$(Get-Random)@example.com"
    password = "TestPass123!"
    userName = "testuser_$(Get-Random -Minimum 1000 -Maximum 9999)"
    displayName = "Test User"
    countryCode = "US"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod `
        -Uri "$baseUrl/api/Auth/register" `
        -Method POST `
        -ContentType "application/json" `
        -Body $registerPayload `
        -SkipCertificateCheck

    $userId = $registerResponse.userId
    $token = $registerResponse.token
    $originalUserName = $registerResponse.userName

    Write-Host "? User created successfully" -ForegroundColor Green
    Write-Host "  User ID: $userId" -ForegroundColor Gray
    Write-Host "  Username: $originalUserName" -ForegroundColor Gray
    Write-Host "  Token: $($token.Substring(0, 20))..." -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host "? Failed to create user" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# ============================================================================
# STEP 2: Get initial profile
# ============================================================================
Write-Host "[STEP 2] Fetching initial profile..." -ForegroundColor Yellow

try {
    $initialProfile = Invoke-RestMethod `
        -Uri "$baseUrl/api/Users/$userId" `
        -Method GET `
        -SkipCertificateCheck

    Write-Host "? Initial profile retrieved" -ForegroundColor Green
    Write-Host "  Display Name: $($initialProfile.displayName)" -ForegroundColor Gray
    Write-Host "  Bio: $($initialProfile.bio ?? '(empty)')" -ForegroundColor Gray
    Write-Host "  Avatar: $($initialProfile.avatarUrl ?? '(empty)')" -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host "? Failed to get profile" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# ============================================================================
# STEP 3: Update profile with all fields
# ============================================================================
Write-Host "[STEP 3] Updating profile (all fields)..." -ForegroundColor Yellow

$updatePayload = @{
    userName = "updated_user_$(Get-Random -Minimum 1000 -Maximum 9999)"
    displayName = "Updated Test User"
    bio = "I love upcycling and creating sustainable projects!"
    avatarUrl = "https://example.com/avatar.jpg"
} | ConvertTo-Json

try {
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }

    $updatedProfile = Invoke-RestMethod `
        -Uri "$baseUrl/api/Users/$userId" `
        -Method PUT `
        -Headers $headers `
        -Body $updatePayload `
        -SkipCertificateCheck

    Write-Host "? Profile updated successfully" -ForegroundColor Green
    Write-Host "  New Username: $($updatedProfile.userName)" -ForegroundColor Gray
    Write-Host "  New Display Name: $($updatedProfile.displayName)" -ForegroundColor Gray
    Write-Host "  New Bio: $($updatedProfile.bio)" -ForegroundColor Gray
    Write-Host "  New Avatar: $($updatedProfile.avatarUrl)" -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host "? Failed to update profile" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "  Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
    exit 1
}

# ============================================================================
# STEP 4: Partial update (only bio)
# ============================================================================
Write-Host "[STEP 4] Partial update (bio only)..." -ForegroundColor Yellow

$partialPayload = @{
    bio = "Updated bio with new information about sustainability!"
} | ConvertTo-Json

try {
    $partialProfile = Invoke-RestMethod `
        -Uri "$baseUrl/api/Users/$userId" `
        -Method PUT `
        -Headers $headers `
        -Body $partialPayload `
        -SkipCertificateCheck

    Write-Host "? Partial update successful" -ForegroundColor Green
    Write-Host "  Username (unchanged): $($partialProfile.userName)" -ForegroundColor Gray
    Write-Host "  Bio (updated): $($partialProfile.bio)" -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host "? Failed to do partial update" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# ============================================================================
# STEP 5: Test unauthorized update (create another user)
# ============================================================================
Write-Host "[STEP 5] Testing authorization (should fail)..." -ForegroundColor Yellow

$otherUserPayload = @{
    email = "otheruser_$(Get-Random)@example.com"
    password = "TestPass123!"
    userName = "otheruser_$(Get-Random -Minimum 1000 -Maximum 9999)"
    displayName = "Other User"
} | ConvertTo-Json

try {
    $otherUserResponse = Invoke-RestMethod `
        -Uri "$baseUrl/api/Auth/register" `
        -Method POST `
        -ContentType "application/json" `
        -Body $otherUserPayload `
        -SkipCertificateCheck

    $otherUserId = $otherUserResponse.userId
    Write-Host "  Created second user (ID: $otherUserId)" -ForegroundColor Gray
}
catch {
    Write-Host "? Failed to create second user" -ForegroundColor Red
    exit 1
}

# Try to update the other user's profile with first user's token
$unauthorizedUpdate = @{
    displayName = "Hacked Display Name"
} | ConvertTo-Json

try {
    $hackAttempt = Invoke-RestMethod `
        -Uri "$baseUrl/api/Users/$otherUserId" `
        -Method PUT `
        -Headers $headers `
        -Body $unauthorizedUpdate `
        -SkipCertificateCheck

    Write-Host "? SECURITY ISSUE: Unauthorized update succeeded!" -ForegroundColor Red
    exit 1
}
catch {
    if ($_.Exception.Response.StatusCode -eq 403) {
        Write-Host "? Authorization check working (403 Forbidden)" -ForegroundColor Green
    }
    else {
        Write-Host "? Unexpected error: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        exit 1
    }
}
Write-Host ""

# ============================================================================
# STEP 6: Test duplicate username validation
# ============================================================================
Write-Host "[STEP 6] Testing duplicate username validation..." -ForegroundColor Yellow

$duplicatePayload = @{
    userName = $otherUserResponse.userName
} | ConvertTo-Json

try {
    $duplicateAttempt = Invoke-RestMethod `
        -Uri "$baseUrl/api/Users/$userId" `
        -Method PUT `
        -Headers $headers `
        -Body $duplicatePayload `
        -SkipCertificateCheck

    Write-Host "? Duplicate username validation failed!" -ForegroundColor Red
    exit 1
}
catch {
    if ($_.Exception.Response.StatusCode -eq 409) {
        Write-Host "? Duplicate username correctly rejected (409 Conflict)" -ForegroundColor Green
    }
    else {
        Write-Host "? Unexpected error: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        Write-Host "  Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
        exit 1
    }
}
Write-Host ""

# ============================================================================
# STEP 7: Test invalid username format
# ============================================================================
Write-Host "[STEP 7] Testing invalid username format..." -ForegroundColor Yellow

$invalidPayload = @{
    userName = "invalid user!" # Contains space and special char
} | ConvertTo-Json

try {
    $invalidAttempt = Invoke-RestMethod `
        -Uri "$baseUrl/api/Users/$userId" `
        -Method PUT `
        -Headers $headers `
        -Body $invalidPayload `
        -SkipCertificateCheck

    Write-Host "? Invalid username validation failed!" -ForegroundColor Red
    exit 1
}
catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "? Invalid username format correctly rejected (400 Bad Request)" -ForegroundColor Green
    }
    else {
        Write-Host "? Unexpected error: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        exit 1
    }
}
Write-Host ""

# ============================================================================
# SUMMARY
# ============================================================================
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "  ALL TESTS PASSED! ?" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Test Results:" -ForegroundColor White
Write-Host "  ? Profile update (all fields)" -ForegroundColor Green
Write-Host "  ? Partial update" -ForegroundColor Green
Write-Host "  ? Authorization check" -ForegroundColor Green
Write-Host "  ? Duplicate username validation" -ForegroundColor Green
Write-Host "  ? Invalid username format validation" -ForegroundColor Green
Write-Host ""
