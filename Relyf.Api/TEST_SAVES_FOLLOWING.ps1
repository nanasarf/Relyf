#!/usr/bin/env pwsh
# TEST_SAVES_FOLLOWING.ps1
# Tests the updated Saves endpoint with follow relationship

$BASE_URL = "https://localhost:7139"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  SAVES + FOLLOWING - API TEST" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Login as User A
Write-Host "[1] Logging in as User A..." -ForegroundColor Yellow
$loginA = @{
    username = "testuser"
    password = "Test123!"
} | ConvertTo-Json

$responseA = Invoke-RestMethod -Uri "$BASE_URL/api/Auth/login" `
    -Method POST `
    -Body $loginA `
    -ContentType "application/json" `
    -SkipCertificateCheck

$tokenA = $responseA.token
$userIdA = $responseA.userId
Write-Host "? User A logged in (ID: $userIdA)" -ForegroundColor Green
Write-Host ""

# Step 2: Login as User B
Write-Host "[2] Logging in as User B..." -ForegroundColor Yellow
$loginB = @{
    username = "anotheruser"
    password = "Test123!"
} | ConvertTo-Json

$responseB = Invoke-RestMethod -Uri "$BASE_URL/api/Auth/login" `
    -Method POST `
    -Body $loginB `
    -ContentType "application/json" `
    -SkipCertificateCheck

$tokenB = $responseB.token
$userIdB = $responseB.userId
Write-Host "? User B logged in (ID: $userIdB)" -ForegroundColor Green
Write-Host ""

# Step 3: User A tries to view User B's saves (should fail - not following)
Write-Host "[3] User A tries to view User B's saves (not following)..." -ForegroundColor Yellow
try {
    $headers = @{
        "Authorization" = "Bearer $tokenA"
    }
    
    $savesB = Invoke-RestMethod -Uri "$BASE_URL/api/Saves/user/$userIdB" `
        -Method GET `
        -Headers $headers `
        -SkipCertificateCheck
    
    Write-Host "? UNEXPECTED: Request should have been forbidden" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 403) {
        Write-Host "? Access denied (403 Forbidden) - correct behavior" -ForegroundColor Green
    } else {
        Write-Host "? Unexpected error: $_" -ForegroundColor Red
    }
}
Write-Host ""

# Step 4: User A follows User B
Write-Host "[4] User A follows User B..." -ForegroundColor Yellow
$followRequest = @{
    followingId = $userIdB
} | ConvertTo-Json

$headers = @{
    "Authorization" = "Bearer $tokenA"
}

$follow = Invoke-RestMethod -Uri "$BASE_URL/api/Follow" `
    -Method POST `
    -Body $followRequest `
    -Headers $headers `
    -ContentType "application/json" `
    -SkipCertificateCheck

Write-Host "? User A now follows User B" -ForegroundColor Green
Write-Host ""

# Step 5: User A tries to view User B's saves again (should succeed now)
Write-Host "[5] User A tries to view User B's saves (now following)..." -ForegroundColor Yellow
try {
    $headers = @{
        "Authorization" = "Bearer $tokenA"
    }
    
    $savesB = Invoke-RestMethod -Uri "$BASE_URL/api/Saves/user/$userIdB" `
        -Method GET `
        -Headers $headers `
        -SkipCertificateCheck
    
    Write-Host "? Access granted - User A can now view User B's saves" -ForegroundColor Green
    Write-Host "  Saved ideas count: $($savesB.Count)" -ForegroundColor Gray
    
    if ($savesB.Count -gt 0) {
        Write-Host ""
        Write-Host "  Sample save:" -ForegroundColor Gray
        $savesB[0] | ConvertTo-Json -Depth 3 | Write-Host -ForegroundColor DarkGray
    }
} catch {
    Write-Host "? Access denied: $_" -ForegroundColor Red
    Write-Host "  Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
}
Write-Host ""

# Step 6: User A can still view their own saves
Write-Host "[6] User A views their own saves..." -ForegroundColor Yellow
try {
    $headers = @{
        "Authorization" = "Bearer $tokenA"
    }
    
    $savesA = Invoke-RestMethod -Uri "$BASE_URL/api/Saves/user/$userIdA" `
        -Method GET `
        -Headers $headers `
        -SkipCertificateCheck
    
    Write-Host "? User A can view own saves" -ForegroundColor Green
    Write-Host "  Saved ideas count: $($savesA.Count)" -ForegroundColor Gray
} catch {
    Write-Host "? Failed to view own saves: $_" -ForegroundColor Red
}
Write-Host ""

# Step 7: Cleanup - Unfollow
Write-Host "[7] Cleanup: User A unfollows User B..." -ForegroundColor Yellow
try {
    $headers = @{
        "Authorization" = "Bearer $tokenA"
    }
    
    Invoke-RestMethod -Uri "$BASE_URL/api/Follow/$userIdB" `
        -Method DELETE `
        -Headers $headers `
        -SkipCertificateCheck | Out-Null
    
    Write-Host "? Unfollowed successfully" -ForegroundColor Green
} catch {
    Write-Host "? Unfollow failed: $_" -ForegroundColor Red
}
Write-Host ""

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  TEST COMPLETE" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
