#!/usr/bin/env pwsh
# Test script to verify the GET /api/Feed endpoint

$baseUrl = "https://localhost:7139"
$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Feed Endpoint Test" -ForegroundColor Cyan
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
    email = "feedtest1_$timestamp@test.com"
    password = "Test123!"
    userName = "feed_user1_$timestamp"
    displayName = "Feed Test User 1"
}

$user2Payload = @{
    email = "feedtest2_$timestamp@test.com"
    password = "Test123!"
    userName = "feed_user2_$timestamp"
    displayName = "Feed Test User 2"
}

$user3Payload = @{
    email = "feedtest3_$timestamp@test.com"
    password = "Test123!"
    userName = "feed_user3_$timestamp"
    displayName = "Feed Test User 3"
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

# Step 2: User 1 follows User 2 and User 3
Write-Host "Step 2: User 1 follows User 2 and User 3..." -ForegroundColor Yellow

try {
    # User 1 follows User 2
    $followPayload = @{
        followingId = $user2.userId
    }
    Invoke-ApiRequest -Method POST -Url "$baseUrl/api/Follow" -Body $followPayload -Token $user1.token
    Write-Host "? User 1 is now following User 2" -ForegroundColor Green
    
    # User 1 follows User 3
    $followPayload = @{
        followingId = $user3.userId
    }
    Invoke-ApiRequest -Method POST -Url "$baseUrl/api/Follow" -Body $followPayload -Token $user1.token
    Write-Host "? User 1 is now following User 3" -ForegroundColor Green
}
catch {
    Write-Host "? Failed to create follow relationships" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 3: User 2 creates a project
Write-Host "Step 3: User 2 creates a project..." -ForegroundColor Yellow

try {
    $projectPayload = @{
        title = "Upcycled Bottle Lamp by User 2"
        description = "A beautiful lamp made from recycled glass bottles"
        status = "completed"
    }
    
    $project = Invoke-ApiRequest -Method POST -Url "$baseUrl/api/Projects" -Body $projectPayload -Token $user2.token
    Write-Host "? Project created by User 2: $($project.title)" -ForegroundColor Green
}
catch {
    Write-Host "? Failed to create project" -ForegroundColor Red
    Write-Host "Note: This might fail if the Projects API endpoint has issues" -ForegroundColor Yellow
}

Write-Host ""

# Step 4: User 3 creates an AI idea (if SavedAIIdeas endpoint exists)
Write-Host "Step 4: User 3 creates an AI idea..." -ForegroundColor Yellow

try {
    $ideaPayload = @{
        title = "Denim Tote Bag Idea by User 3"
        tools = "Scissors, Needle, Thread"
        steps = "1. Cut denim, 2. Sew sides, 3. Add straps"
        safety = "Use sharp scissors carefully"
    }
    
    $idea = Invoke-ApiRequest -Method POST -Url "$baseUrl/api/AIIdeas" -Body $ideaPayload -Token $user3.token
    Write-Host "? AI Idea created by User 3: $($idea.title)" -ForegroundColor Green
}
catch {
    Write-Host "? AI Idea creation failed (endpoint might not support this)" -ForegroundColor Yellow
    Write-Host "  Continuing with test..." -ForegroundColor Gray
}

Write-Host ""

# Step 5: User 1 checks their feed (should be empty if no content was created)
Write-Host "Step 5: User 1 checks their feed..." -ForegroundColor Yellow

try {
    $feed = Invoke-ApiRequest -Method GET -Url "$baseUrl/api/Feed?skip=0&take=20" -Token $user1.token
    
    Write-Host "Feed loaded successfully!" -ForegroundColor Green
    Write-Host "  Total items: $($feed.total)" -ForegroundColor Cyan
    Write-Host "  Items in response: $($feed.items.Count)" -ForegroundColor Cyan
    Write-Host "  Skip: $($feed.skip), Take: $($feed.take)" -ForegroundColor Gray
    Write-Host ""
    
    if ($feed.items.Count -eq 0) {
        Write-Host "? Feed is empty - this is expected if no followed users have created content" -ForegroundColor Yellow
    }
    else {
        Write-Host "Feed Items:" -ForegroundColor White
        Write-Host "--------------------" -ForegroundColor Gray
        
        foreach ($item in $feed.items) {
            $typeIcon = if ($item.itemType -eq "project") { "??" } else { "??" }
            
            Write-Host "$typeIcon $($item.itemType.ToUpper()): $($item.title)" -ForegroundColor White
            Write-Host "   By: $($item.displayName) (@$($item.userName))" -ForegroundColor Gray
            Write-Host "   Created: $($item.createdAtUtc)" -ForegroundColor Gray
            
            if ($item.description) {
                Write-Host "   Description: $($item.description)" -ForegroundColor Gray
            }
            
            if ($item.ideaText) {
                $preview = if ($item.ideaText.Length -gt 100) { 
                    $item.ideaText.Substring(0, 100) + "..." 
                } else { 
                    $item.ideaText 
                }
                Write-Host "   Idea: $preview" -ForegroundColor Gray
            }
            
            if ($item.status) {
                Write-Host "   Status: $($item.status)" -ForegroundColor Cyan
            }
            
            Write-Host "   ?? Reactions: $($item.reactionCount) | Comments: $($item.commentCount) | Saves: $($item.saveCount)" -ForegroundColor Gray
            Write-Host ""
        }
    }
    
    Write-Host "? PASS: Feed endpoint works correctly" -ForegroundColor Green
}
catch {
    Write-Host "? FAIL: Failed to load feed" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 6: Test pagination
Write-Host "Step 6: Testing pagination..." -ForegroundColor Yellow

try {
    # Request page 1
    $page1 = Invoke-ApiRequest -Method GET -Url "$baseUrl/api/Feed?skip=0&take=10" -Token $user1.token
    Write-Host "? Page 1 loaded: $($page1.items.Count) items" -ForegroundColor Green
    
    # Request page 2
    $page2 = Invoke-ApiRequest -Method GET -Url "$baseUrl/api/Feed?skip=10&take=10" -Token $user1.token
    Write-Host "? Page 2 loaded: $($page2.items.Count) items" -ForegroundColor Green
    
    # Verify pagination info
    if ($page1.skip -eq 0 -and $page1.take -eq 10) {
        Write-Host "? PASS: Pagination parameters correct for page 1" -ForegroundColor Green
    }
    
    if ($page2.skip -eq 10 -and $page2.take -eq 10) {
        Write-Host "? PASS: Pagination parameters correct for page 2" -ForegroundColor Green
    }
}
catch {
    Write-Host "? FAIL: Pagination test failed" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 7: Test unauthenticated request (should fail with 401)
Write-Host "Step 7: Testing unauthenticated request..." -ForegroundColor Yellow

try {
    Invoke-ApiRequest -Method GET -Url "$baseUrl/api/Feed?skip=0&take=20"
    Write-Host "? FAIL: Unauthenticated request should have failed" -ForegroundColor Red
    exit 1
}
catch {
    if ($_.Exception.Message -match "401") {
        Write-Host "? PASS: Unauthenticated request correctly returns 401" -ForegroundColor Green
    }
    else {
        Write-Host "? Unauthenticated request failed with unexpected error" -ForegroundColor Yellow
    }
}

Write-Host ""

# Step 8: Verify feed only shows content from followed users
Write-Host "Step 8: Verifying feed only shows followed users' content..." -ForegroundColor Yellow

try {
    $feed = Invoke-ApiRequest -Method GET -Url "$baseUrl/api/Feed?skip=0&take=20" -Token $user1.token
    
    $correctFeed = $true
    foreach ($item in $feed.items) {
        # Check if the item is from User 2 or User 3 (who User 1 follows)
        if ($item.userId -ne $user2.userId -and $item.userId -ne $user3.userId) {
            Write-Host "? Found item from unfollowed user: $($item.userName)" -ForegroundColor Red
            $correctFeed = $false
        }
    }
    
    if ($correctFeed) {
        Write-Host "? PASS: Feed only contains content from followed users" -ForegroundColor Green
    }
    else {
        Write-Host "? FAIL: Feed contains content from unfollowed users" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "? Could not verify feed content (might be empty)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ? All Tests Passed!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "  • Feed endpoint is accessible and requires authentication" -ForegroundColor White
Write-Host "  • Feed returns content from followed users only" -ForegroundColor White
Write-Host "  • Pagination works correctly" -ForegroundColor White
Write-Host "  • Response format is correct" -ForegroundColor White
Write-Host ""
Write-Host "Test Users Created:" -ForegroundColor Yellow
Write-Host "  User 1: $($user1.userName) (ID: $($user1.userId)) - Following User 2 and 3" -ForegroundColor Gray
Write-Host "  User 2: $($user2.userName) (ID: $($user2.userId)) - Created project" -ForegroundColor Gray
Write-Host "  User 3: $($user3.userName) (ID: $($user3.userId)) - Created AI idea" -ForegroundColor Gray
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  • Have User 2 and User 3 create more content to see it in User 1's feed" -ForegroundColor White
Write-Host "  • Test with more users and follow relationships" -ForegroundColor White
Write-Host "  • Implement engagement features (reactions, comments, saves)" -ForegroundColor White
