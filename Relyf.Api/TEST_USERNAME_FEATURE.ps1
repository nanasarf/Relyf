# Username & Display Name Feature - Quick Test Script
# Run this after executing the SQL migration

# Configuration
$baseUrl = "https://localhost:7139"
$testEmail = "testuser_$(Get-Random)@example.com"
$testUserName = "testuser_$(Get-Random -Minimum 1000 -Maximum 9999)"
$testDisplayName = "Test User $(Get-Random -Minimum 1 -Maximum 100)"
$testPassword = "TestPass123!"

Write-Host "================================" -ForegroundColor Cyan
Write-Host "USERNAME & DISPLAY NAME TESTS" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Check Username Availability (should be available)
Write-Host "[TEST 1] Checking username availability..." -ForegroundColor Yellow
try {
    $checkResponse = Invoke-RestMethod -Uri "$baseUrl/api/Users/check-username/$testUserName" -Method Get
    if ($checkResponse.available -eq $true) {
        Write-Host "? PASS: Username '$testUserName' is available" -ForegroundColor Green
        Write-Host "   Response: available=$($checkResponse.available), message=$($checkResponse.message)" -ForegroundColor Gray
    } else {
        Write-Host "? FAIL: Username should be available but is not" -ForegroundColor Red
        Write-Host "   Response: $($checkResponse | ConvertTo-Json)" -ForegroundColor Gray
    }
} catch {
    Write-Host "? FAIL: Error checking username availability" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Gray
}
Write-Host ""

# Test 2: Check Invalid Username (too short)
Write-Host "[TEST 2] Checking invalid username (too short)..." -ForegroundColor Yellow
try {
    $checkResponse = Invoke-RestMethod -Uri "$baseUrl/api/Users/check-username/ab" -Method Get
    if ($checkResponse.available -eq $false -and $checkResponse.message -like "*3 and 20*") {
        Write-Host "? PASS: Correctly rejected username 'ab' (too short)" -ForegroundColor Green
        Write-Host "   Response: $($checkResponse.message)" -ForegroundColor Gray
    } else {
        Write-Host "??  UNEXPECTED: $($checkResponse | ConvertTo-Json)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "? FAIL: Error checking invalid username" -ForegroundColor Red
}
Write-Host ""

# Test 3: Check Invalid Username (special characters)
Write-Host "[TEST 3] Checking invalid username (special characters)..." -ForegroundColor Yellow
try {
    $checkResponse = Invoke-RestMethod -Uri "$baseUrl/api/Users/check-username/test-user!" -Method Get
    if ($checkResponse.available -eq $false -and $checkResponse.message -like "*letters, numbers, and underscores*") {
        Write-Host "? PASS: Correctly rejected username with special characters" -ForegroundColor Green
        Write-Host "   Response: $($checkResponse.message)" -ForegroundColor Gray
    } else {
        Write-Host "??  UNEXPECTED: $($checkResponse | ConvertTo-Json)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "? FAIL: Error checking invalid username" -ForegroundColor Red
}
Write-Host ""

# Test 4: Register New User with Username
Write-Host "[TEST 4] Registering new user with username..." -ForegroundColor Yellow
$registerBody = @{
    email = $testEmail
    password = $testPassword
    userName = $testUserName
    displayName = $testDisplayName
    countryCode = "US"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod `
        -Uri "$baseUrl/api/Auth/register" `
        -Method Post `
        -Body $registerBody `
        -ContentType "application/json"
    
    if ($registerResponse.userName -eq $testUserName -and $registerResponse.displayName -eq $testDisplayName) {
        Write-Host "? PASS: User registered successfully" -ForegroundColor Green
        Write-Host "   UserName: $($registerResponse.userName)" -ForegroundColor Gray
        Write-Host "   DisplayName: $($registerResponse.displayName)" -ForegroundColor Gray
        Write-Host "   Email: $($registerResponse.email)" -ForegroundColor Gray
        Write-Host "   Token: $($registerResponse.token.Substring(0, 20))..." -ForegroundColor Gray
        
        $token = $registerResponse.token
        $userId = $registerResponse.userId
    } else {
        Write-Host "? FAIL: Registration response missing username or displayName" -ForegroundColor Red
        Write-Host "   Response: $($registerResponse | ConvertTo-Json)" -ForegroundColor Gray
        exit
    }
} catch {
    Write-Host "? FAIL: Error during registration" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Gray
    if ($_.ErrorDetails.Message) {
        Write-Host "   Details: $($_.ErrorDetails.Message)" -ForegroundColor Gray
    }
    exit
}
Write-Host ""

# Test 5: Check Username Availability Again (should be taken now)
Write-Host "[TEST 5] Checking username availability again (should be taken)..." -ForegroundColor Yellow
try {
    $checkResponse = Invoke-RestMethod -Uri "$baseUrl/api/Users/check-username/$testUserName" -Method Get
    if ($checkResponse.available -eq $false -and $checkResponse.message -like "*already taken*") {
        Write-Host "? PASS: Username correctly marked as taken" -ForegroundColor Green
        Write-Host "   Response: $($checkResponse.message)" -ForegroundColor Gray
    } else {
        Write-Host "? FAIL: Username should be taken but is not" -ForegroundColor Red
        Write-Host "   Response: $($checkResponse | ConvertTo-Json)" -ForegroundColor Gray
    }
} catch {
    Write-Host "? FAIL: Error checking username availability" -ForegroundColor Red
}
Write-Host ""

# Test 6: Try to Register with Duplicate Username
Write-Host "[TEST 6] Attempting to register with duplicate username..." -ForegroundColor Yellow
$duplicateBody = @{
    email = "another_$testEmail"
    password = $testPassword
    userName = $testUserName  # Same username
    displayName = "Another User"
    countryCode = "US"
} | ConvertTo-Json

try {
    $duplicateResponse = Invoke-RestMethod `
        -Uri "$baseUrl/api/Auth/register" `
        -Method Post `
        -Body $duplicateBody `
        -ContentType "application/json"
    
    Write-Host "? FAIL: Should have rejected duplicate username" -ForegroundColor Red
    Write-Host "   Response: $($duplicateResponse | ConvertTo-Json)" -ForegroundColor Gray
} catch {
    if ($_.Exception.Response.StatusCode -eq 409) {
        Write-Host "? PASS: Correctly rejected duplicate username (409 Conflict)" -ForegroundColor Green
        if ($_.ErrorDetails.Message -like "*already taken*") {
            Write-Host "   Message: Username is already taken" -ForegroundColor Gray
        }
    } else {
        Write-Host "? FAIL: Unexpected error code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}
Write-Host ""

# Test 7: Login with Registered User
Write-Host "[TEST 7] Logging in with registered user..." -ForegroundColor Yellow
$loginBody = @{
    email = $testEmail
    password = $testPassword
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod `
        -Uri "$baseUrl/api/Auth/login" `
        -Method Post `
        -Body $loginBody `
        -ContentType "application/json"
    
    if ($loginResponse.userName -eq $testUserName -and $loginResponse.displayName -eq $testDisplayName) {
        Write-Host "? PASS: Login successful with username in response" -ForegroundColor Green
        Write-Host "   UserName: $($loginResponse.userName)" -ForegroundColor Gray
        Write-Host "   DisplayName: $($loginResponse.displayName)" -ForegroundColor Gray
        $token = $loginResponse.token
    } else {
        Write-Host "? FAIL: Login response missing username or displayName" -ForegroundColor Red
        Write-Host "   Response: $($loginResponse | ConvertTo-Json)" -ForegroundColor Gray
    }
} catch {
    Write-Host "? FAIL: Error during login" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Gray
}
Write-Host ""

# Test 8: Get User Profile
Write-Host "[TEST 8] Getting user profile..." -ForegroundColor Yellow
try {
    $headers = @{ Authorization = "Bearer $token" }
    $profile = Invoke-RestMethod `
        -Uri "$baseUrl/api/Users/$userId" `
        -Method Get `
        -Headers $headers
    
    if ($profile.userName -eq $testUserName -and $profile.displayName -eq $testDisplayName) {
        Write-Host "? PASS: Profile retrieved with username and displayName" -ForegroundColor Green
        Write-Host "   UserName: $($profile.userName)" -ForegroundColor Gray
        Write-Host "   DisplayName: $($profile.displayName)" -ForegroundColor Gray
        Write-Host "   Email: $($profile.email)" -ForegroundColor Gray
        Write-Host "   Bio: $($profile.bio)" -ForegroundColor Gray
        Write-Host "   AvatarUrl: $($profile.avatarUrl)" -ForegroundColor Gray
        Write-Host "   Followers: $($profile.followerCount)" -ForegroundColor Gray
        Write-Host "   Following: $($profile.followingCount)" -ForegroundColor Gray
    } else {
        Write-Host "? FAIL: Profile missing username or displayName" -ForegroundColor Red
        Write-Host "   Response: $($profile | ConvertTo-Json)" -ForegroundColor Gray
    }
} catch {
    Write-Host "? FAIL: Error getting profile" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Gray
}
Write-Host ""

# Test 9: Search Users by Username
Write-Host "[TEST 9] Searching users by username..." -ForegroundColor Yellow
try {
    $searchQuery = $testUserName.Substring(0, 8)  # Search partial username
    $searchResults = Invoke-RestMethod `
        -Uri "$baseUrl/api/Users/search?query=$searchQuery&skip=0&take=10" `
        -Method Get `
        -Headers $headers
    
    if ($searchResults.total -gt 0) {
        Write-Host "? PASS: Found $($searchResults.total) user(s) matching '$searchQuery'" -ForegroundColor Green
        $searchResults.results | ForEach-Object {
            Write-Host "   - UserName: $($_.userName), DisplayName: $($_.displayName)" -ForegroundColor Gray
        }
    } else {
        Write-Host "??  WARNING: No users found (this might be expected)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "? FAIL: Error searching users" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Gray
}
Write-Host ""

# Test 10: Invalid Username Format
Write-Host "[TEST 10] Testing invalid username format in registration..." -ForegroundColor Yellow
$invalidBody = @{
    email = "invalid_$testEmail"
    password = $testPassword
    userName = "test user!"  # Invalid format
    displayName = "Invalid User"
    countryCode = "US"
} | ConvertTo-Json

try {
    $invalidResponse = Invoke-RestMethod `
        -Uri "$baseUrl/api/Auth/register" `
        -Method Post `
        -Body $invalidBody `
        -ContentType "application/json"
    
    Write-Host "? FAIL: Should have rejected invalid username format" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "? PASS: Correctly rejected invalid username format (400 Bad Request)" -ForegroundColor Green
        if ($_.ErrorDetails.Message -like "*letters, numbers, and underscores*") {
            Write-Host "   Message: Username validation working correctly" -ForegroundColor Gray
        }
    } else {
        Write-Host "? FAIL: Unexpected error code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}
Write-Host ""

# Summary
Write-Host "================================" -ForegroundColor Cyan
Write-Host "TEST SUMMARY" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Test user created:" -ForegroundColor White
Write-Host "  Email: $testEmail" -ForegroundColor Gray
Write-Host "  UserName: $testUserName" -ForegroundColor Gray
Write-Host "  DisplayName: $testDisplayName" -ForegroundColor Gray
Write-Host "  UserId: $userId" -ForegroundColor Gray
Write-Host ""
Write-Host "? All tests completed! Check results above." -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Verify database: SELECT * FROM app.[User] WHERE UserId = $userId" -ForegroundColor Gray
Write-Host "2. Test frontend integration" -ForegroundColor Gray
Write-Host "3. Update profile update endpoint (if needed)" -ForegroundColor Gray
Write-Host ""
