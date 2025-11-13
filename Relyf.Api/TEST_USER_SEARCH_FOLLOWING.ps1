#!/usr/bin/env pwsh
# Test script to verify that the GET /api/Users/search endpoint returns correct isFollowing field

$baseUrl = "https://localhost:7139"
$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  User Search IsFollowing Field Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Helper function to make API calls
function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Url,
        [object]$Body = $null,
        [string]$Token = $null
    )
    
    $headers = @{
        "Content-Type" = "application/json"
    }
    
    if ($Token) {
        $headers["Authorization"] = "Bearer $Token"
    }
    
    try {
        $params = @{
            Method = $Method
            Uri = $Url
            Headers = $headers
            SkipCertificateCheck = $true
        }
        
        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json)
        }
        
        $response = Invoke-RestMethod @params
        return $response
    }
    catch {
        Write-Host "API Error: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.ErrorDetails.Message) {
            Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
        }
        throw
    }
}

# Step 1: Register three test users
Write-Host "Step 1: Creating test users..." -ForegroundColor Yellow

$timestamp = Get-Date -Format "yyyyMMddHHmmss"

$user1Payload = @{
    email = "searchtest1_$timestamp@test.com"
    password = "Test123!"
    userName = "search_user1_$timestamp"
    displayName = "Search Test User 1"
}

$user2Payload = @{
    email = "searchtest2_$timestamp@test.com"
    password = "Test123!"
    userName = "search_user2_$timestamp"
    displayName = "Search Test User 2"
}

$user3Payload = @{
    email = "searchtest3_$timestamp@test.com"
    password = "Test123!"
    userName = "search_user3_$timestamp"
    displayName = "Search Test User 3"
}

try {
    $user1 = Invoke-ApiRequest -Method POST -Url "$baseUrl/api/Auth/register" -Body $user1Payload
    Write-Host "? User 1 created: $($user1.userName) (ID: $($user1.userId))" -ForegroundColor Green
    
    $user2 = Invoke-ApiRequest -Method POST -Url "$baseUrl/api/Auth/register" -Body $user2Payload
    Write-Host "? User 2 created: $($user2.userName) (ID: $($user2.userId))" -ForegroundColor Green
    
    $user3 = Invoke-ApiRequest -Method POST -Url "$baseUrl/api/Auth/register" -Body $user3Payload
    Write-Host "? User 3 created: $($user3.userName) (ID: $($user3.userId))" -ForegroundColor Green
}
catch {
    Write-Host "? Failed to create test users" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 2: User 1 follows User 2
Write-Host "Step 2: User 1 follows User 2..." -ForegroundColor Yellow

try {
    $followPayload = @{
        followingId = $user2.userId
    }
    
    Invoke-ApiRequest -Method POST -Url "$baseUrl/api/Follow" -Body $followPayload -Token $user1.token
    Write-Host "? User 1 is now following User 2" -ForegroundColor Green
}
catch {
    Write-Host "? Failed to create follow relationship" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 3: Search as User 1 (authenticated) - should show isFollowing for User 2
Write-Host "Step 3: Search as User 1 (authenticated)..." -ForegroundColor Yellow

try {
    $searchResults = Invoke-ApiRequest -Method GET -Url "$baseUrl/api/Users/search?query=Search%20Test%20User" -Token $user1.token
    
    Write-Host "Total results: $($searchResults.total)" -ForegroundColor Cyan
    Write-Host ""
    
    foreach ($user in $searchResults.results) {
        $followStatus = if ($user.isFollowing) { "? Following" } else { "? Not Following" }
        $followedByStatus = if ($user.isFollowedBy) { "? Followed By" } else { "? Not Followed By" }
        
        Write-Host "User: $($user.displayName) (@$($user.userName))" -ForegroundColor White
        Write-Host "  ID: $($user.userId)" -ForegroundColor Gray
        Write-Host "  $followStatus" -ForegroundColor $(if ($user.isFollowing) { "Green" } else { "Gray" })
        Write-Host "  $followedByStatus" -ForegroundColor $(if ($user.isFollowedBy) { "Green" } else { "Gray" })
        Write-Host "  Followers: $($user.followerCount) | Following: $($user.followingCount)" -ForegroundColor Gray
        Write-Host ""
    }
    
    # Verify User 2 shows isFollowing = true
    $user2Result = $searchResults.results | Where-Object { $_.userId -eq $user2.userId }
    if ($user2Result) {
        if ($user2Result.isFollowing -eq $true) {
            Write-Host "? PASS: User 2 correctly shows isFollowing = true" -ForegroundColor Green
        }
        else {
            Write-Host "? FAIL: User 2 shows isFollowing = $($user2Result.isFollowing), expected true" -ForegroundColor Red
            exit 1
        }
    }
    else {
        Write-Host "? FAIL: User 2 not found in search results" -ForegroundColor Red
        exit 1
    }
    
    # Verify User 3 shows isFollowing = false
    $user3Result = $searchResults.results | Where-Object { $_.userId -eq $user3.userId }
    if ($user3Result) {
        if ($user3Result.isFollowing -eq $false) {
            Write-Host "? PASS: User 3 correctly shows isFollowing = false" -ForegroundColor Green
        }
        else {
            Write-Host "? FAIL: User 3 shows isFollowing = $($user3Result.isFollowing), expected false" -ForegroundColor Red
            exit 1
        }
    }
    else {
        Write-Host "? FAIL: User 3 not found in search results" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "? Failed to search users" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 4: User 2 follows User 1 back
Write-Host "Step 4: User 2 follows User 1 back..." -ForegroundColor Yellow

try {
    $followPayload = @{
        followingId = $user1.userId
    }
    
    Invoke-ApiRequest -Method POST -Url "$baseUrl/api/Follow" -Body $followPayload -Token $user2.token
    Write-Host "? User 2 is now following User 1" -ForegroundColor Green
}
catch {
    Write-Host "? Failed to create follow relationship" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 5: Search as User 1 again - should show isFollowedBy for User 2
Write-Host "Step 5: Search as User 1 again (should show mutual follow)..." -ForegroundColor Yellow

try {
    $searchResults = Invoke-ApiRequest -Method GET -Url "$baseUrl/api/Users/search?query=Search%20Test%20User" -Token $user1.token
    
    $user2Result = $searchResults.results | Where-Object { $_.userId -eq $user2.userId }
    if ($user2Result) {
        Write-Host "User 2 Status:" -ForegroundColor White
        Write-Host "  isFollowing: $($user2Result.isFollowing)" -ForegroundColor Cyan
        Write-Host "  isFollowedBy: $($user2Result.isFollowedBy)" -ForegroundColor Cyan
        
        if ($user2Result.isFollowing -eq $true -and $user2Result.isFollowedBy -eq $true) {
            Write-Host "? PASS: User 2 correctly shows mutual follow relationship" -ForegroundColor Green
        }
        else {
            Write-Host "? FAIL: User 2 doesn't show mutual follow. isFollowing=$($user2Result.isFollowing), isFollowedBy=$($user2Result.isFollowedBy)" -ForegroundColor Red
            exit 1
        }
    }
}
catch {
    Write-Host "? Failed to search users" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 6: Search as unauthenticated user - should show isFollowing = false for all
Write-Host "Step 6: Search as unauthenticated user..." -ForegroundColor Yellow

try {
    $searchResults = Invoke-ApiRequest -Method GET -Url "$baseUrl/api/Users/search?query=Search%20Test%20User"
    
    Write-Host "Total results: $($searchResults.total)" -ForegroundColor Cyan
    Write-Host ""
    
    $allFalse = $true
    foreach ($user in $searchResults.results) {
        Write-Host "User: $($user.displayName) - isFollowing: $($user.isFollowing), isFollowedBy: $($user.isFollowedBy)" -ForegroundColor Gray
        
        if ($user.isFollowing -ne $false -or $user.isFollowedBy -ne $false) {
            $allFalse = $false
        }
    }
    
    if ($allFalse) {
        Write-Host "? PASS: All users correctly show isFollowing = false and isFollowedBy = false for unauthenticated request" -ForegroundColor Green
    }
    else {
        Write-Host "? FAIL: Some users show incorrect follow status for unauthenticated request" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "? Failed to search users" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ? All Tests Passed!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "  • isFollowing field works correctly for authenticated users" -ForegroundColor White
Write-Host "  • isFollowedBy field works correctly for authenticated users" -ForegroundColor White
Write-Host "  • Mutual follows are detected properly" -ForegroundColor White
Write-Host "  • Unauthenticated requests return false for all relationship fields" -ForegroundColor White
Write-Host ""
Write-Host "Test Users Created:" -ForegroundColor Yellow
Write-Host "  User 1: $($user1.userName) (ID: $($user1.userId))" -ForegroundColor Gray
Write-Host "  User 2: $($user2.userName) (ID: $($user2.userId))" -ForegroundColor Gray
Write-Host "  User 3: $($user3.userName) (ID: $($user3.userId))" -ForegroundColor Gray
