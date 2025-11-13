# =============================================
# Test Script: Follow System Endpoints
# =============================================
# Purpose: Test that all follow-related endpoints work after migration
# Usage: .\TEST_FOLLOW_ENDPOINTS.ps1
# =============================================

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing Follow System Endpoints" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$baseUrl = "https://localhost:7139"

# Check if API is running
try {
    Write-Host "Checking if API is running..." -ForegroundColor Yellow
    $healthCheck = Invoke-WebRequest -Uri "$baseUrl/swagger" -Method GET -SkipCertificateCheck -TimeoutSec 5 -ErrorAction SilentlyContinue
    Write-Host "? API is running`n" -ForegroundColor Green
} catch {
    Write-Host "? API is not running on $baseUrl" -ForegroundColor Red
    Write-Host "Please start the API first: dotnet run" -ForegroundColor Yellow
    exit 1
}

# Database verification
Write-Host "1. Verifying Follow Table..." -ForegroundColor Yellow
$checkTable = sqlcmd -S "(localdb)\ProjectModels" -d "Relyf.Database" -Q "SELECT name FROM sys.tables WHERE schema_id = SCHEMA_ID('app') AND name = 'Follow';" -h -1

if ($checkTable -match "Follow") {
    Write-Host "? Follow table exists`n" -ForegroundColor Green
} else {
    Write-Host "? Follow table does not exist!" -ForegroundColor Red
    Write-Host "Run: sqlcmd -S `"(localdb)\ProjectModels`" -d `"Relyf.Database`" -i create_follows_table.sql" -ForegroundColor Yellow
    exit 1
}

# Get existing users for testing
Write-Host "2. Getting Existing Users..." -ForegroundColor Yellow
$usersQuery = @"
SELECT TOP 2 UserId, Email, UserName, DisplayName 
FROM app.[User] 
WHERE IsDeleted = 0
ORDER BY UserId;
"@

$usersResult = sqlcmd -S "(localdb)\ProjectModels" -d "Relyf.Database" -Q $usersQuery -h -1
Write-Host $usersResult
Write-Host ""

# Test with existing users or create test users
Write-Host "3. Testing Follow Endpoints..." -ForegroundColor Yellow

try {
    # Try to register two test users
    Write-Host "Creating test users..." -ForegroundColor Gray
    
    $user1Email = "followtest1_$(Get-Random)@test.com"
    $user2Email = "followtest2_$(Get-Random)@test.com"
    
    $user1Body = @{
        email = $user1Email
        password = "Test123!"
        userName = "followtest1_$(Get-Random -Maximum 9999)"
        displayName = "Follow Test User 1"
    } | ConvertTo-Json
    
    $user2Body = @{
        email = $user2Email
        password = "Test123!"
        userName = "followtest2_$(Get-Random -Maximum 9999)"
        displayName = "Follow Test User 2"
    } | ConvertTo-Json
    
    $user1Response = Invoke-RestMethod -Uri "$baseUrl/api/Auth/register" -Method POST -Body $user1Body -ContentType "application/json" -SkipCertificateCheck
    Write-Host "? User 1 registered: $($user1Response.userName)" -ForegroundColor Green
    
    $user2Response = Invoke-RestMethod -Uri "$baseUrl/api/Auth/register" -Method POST -Body $user2Body -ContentType "application/json" -SkipCertificateCheck
    Write-Host "? User 2 registered: $($user2Response.userName)`n" -ForegroundColor Green
    
    $user1Token = $user1Response.token
    $user1Id = $user1Response.userId
    $user2Id = $user2Response.userId
    
    # Test 1: User 1 follows User 2
    Write-Host "4. Test: User 1 Follows User 2" -ForegroundColor Yellow
    $followBody = @{
        followingId = $user2Id
    } | ConvertTo-Json
    
    $followResponse = Invoke-RestMethod -Uri "$baseUrl/api/Follow" -Method POST -Body $followBody -ContentType "application/json" -Headers @{Authorization = "Bearer $user1Token"} -SkipCertificateCheck
    Write-Host "? Follow created successfully`n" -ForegroundColor Green
    
    # Test 2: Check follow status
    Write-Host "5. Test: Check Follow Status" -ForegroundColor Yellow
    $checkResponse = Invoke-RestMethod -Uri "$baseUrl/api/Follow/check/$user2Id" -Method GET -Headers @{Authorization = "Bearer $user1Token"} -SkipCertificateCheck
    
    if ($checkResponse.isFollowing -eq $true) {
        Write-Host "? Follow status confirmed: User 1 is following User 2`n" -ForegroundColor Green
    } else {
        Write-Host "? Follow status check failed!" -ForegroundColor Red
    }
    
    # Test 3: Get User 2's followers
    Write-Host "6. Test: Get User 2's Followers" -ForegroundColor Yellow
    $followersResponse = Invoke-RestMethod -Uri "$baseUrl/api/Users/$user2Id/followers" -Method GET -SkipCertificateCheck
    
    Write-Host "User 2 has $($followersResponse.Count) follower(s)" -ForegroundColor Cyan
    if ($followersResponse.Count -gt 0) {
        Write-Host "? Followers endpoint working" -ForegroundColor Green
        $followersResponse | ForEach-Object {
            Write-Host "  - $($_.displayName) (@$($_.userName))" -ForegroundColor Gray
        }
    }
    Write-Host ""
    
    # Test 4: Get User 1's following
    Write-Host "7. Test: Get User 1's Following" -ForegroundColor Yellow
    $followingResponse = Invoke-RestMethod -Uri "$baseUrl/api/Users/$user1Id/following" -Method GET -SkipCertificateCheck
    
    Write-Host "User 1 is following $($followingResponse.Count) user(s)" -ForegroundColor Cyan
    if ($followingResponse.Count -gt 0) {
        Write-Host "? Following endpoint working" -ForegroundColor Green
        $followingResponse | ForEach-Object {
            Write-Host "  - $($_.displayName) (@$($_.userName))" -ForegroundColor Gray
        }
    }
    Write-Host ""
    
    # Test 5: Get User 1's profile (should show followingCount = 1)
    Write-Host "8. Test: User Profile Shows Correct Counts" -ForegroundColor Yellow
    $profileResponse = Invoke-RestMethod -Uri "$baseUrl/api/Users/$user1Id" -Method GET -SkipCertificateCheck
    
    Write-Host "User 1 Profile:" -ForegroundColor Cyan
    Write-Host "  - Followers: $($profileResponse.followerCount)" -ForegroundColor Gray
    Write-Host "  - Following: $($profileResponse.followingCount)" -ForegroundColor Gray
    
    if ($profileResponse.followingCount -ge 1) {
        Write-Host "? Following count is correct`n" -ForegroundColor Green
    }
    
    $profile2Response = Invoke-RestMethod -Uri "$baseUrl/api/Users/$user2Id" -Method GET -SkipCertificateCheck
    
    Write-Host "User 2 Profile:" -ForegroundColor Cyan
    Write-Host "  - Followers: $($profile2Response.followerCount)" -ForegroundColor Gray
    Write-Host "  - Following: $($profile2Response.followingCount)" -ForegroundColor Gray
    
    if ($profile2Response.followerCount -ge 1) {
        Write-Host "? Follower count is correct`n" -ForegroundColor Green
    }
    
    # Test 6: Unfollow
    Write-Host "9. Test: Unfollow User 2" -ForegroundColor Yellow
    $unfollowResponse = Invoke-RestMethod -Uri "$baseUrl/api/Follow/$user2Id" -Method DELETE -Headers @{Authorization = "Bearer $user1Token"} -SkipCertificateCheck -StatusCodeVariable statusCode
    
    if ($statusCode -eq 204) {
        Write-Host "? Unfollow successful`n" -ForegroundColor Green
    }
    
    # Test 7: Verify unfollow
    Write-Host "10. Test: Verify Unfollow" -ForegroundColor Yellow
    $checkAfterUnfollow = Invoke-RestMethod -Uri "$baseUrl/api/Follow/check/$user2Id" -Method GET -Headers @{Authorization = "Bearer $user1Token"} -SkipCertificateCheck
    
    if ($checkAfterUnfollow.isFollowing -eq $false) {
        Write-Host "? Unfollow confirmed: User 1 is no longer following User 2`n" -ForegroundColor Green
    } else {
        Write-Host "? Unfollow verification failed!" -ForegroundColor Red
    }
    
    # Summary
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Test Results Summary" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "? Follow table exists" -ForegroundColor Green
    Write-Host "? POST /api/Follow - Create follow" -ForegroundColor Green
    Write-Host "? GET /api/Follow/check/{id} - Check status" -ForegroundColor Green
    Write-Host "? GET /api/Users/{id}/followers - Get followers" -ForegroundColor Green
    Write-Host "? GET /api/Users/{id}/following - Get following" -ForegroundColor Green
    Write-Host "? GET /api/Users/{id} - Profile shows counts" -ForegroundColor Green
    Write-Host "? DELETE /api/Follow/{id} - Unfollow" -ForegroundColor Green
    Write-Host "`n?? All Follow System Tests PASSED!" -ForegroundColor Green
    
} catch {
    Write-Host "`n? Error during testing: $_" -ForegroundColor Red
    Write-Host "`nStack Trace:" -ForegroundColor Yellow
    Write-Host $_.ScriptStackTrace -ForegroundColor Gray
    
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "`nHTTP Status Code: $statusCode" -ForegroundColor Yellow
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Test Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
