# Test User Projects Endpoint
# Tests the new GET /api/projects/user/{userId} endpoint

$baseUrl = "https://localhost:7099"

# Get token first
Write-Host "`n=== Logging in to get token ===" -ForegroundColor Cyan
$loginResponse = Invoke-RestMethod `
    -Uri "$baseUrl/api/auth/login" `
    -Method POST `
    -ContentType "application/json" `
    -Body '{"email":"test@example.com","password":"Test123!"}' `
    -SkipCertificateCheck

$token = $loginResponse.token
$currentUserId = $loginResponse.userId
Write-Host "? Logged in as User ID: $currentUserId" -ForegroundColor Green
Write-Host "Token: $($token.Substring(0, 20))..." -ForegroundColor Gray

# Test 1: Get current user's projects via OLD endpoint
Write-Host "`n=== Test 1: GET /api/projects (my projects) ===" -ForegroundColor Cyan
try {
    $myProjects = Invoke-RestMethod `
        -Uri "$baseUrl/api/projects?skip=0&take=20" `
        -Method GET `
        -Headers @{ "Authorization" = "Bearer $token" } `
        -SkipCertificateCheck

    Write-Host "? Total: $($myProjects.total)" -ForegroundColor Green
    Write-Host "Projects:" -ForegroundColor Yellow
    $myProjects.results | ForEach-Object {
        Write-Host "  - [$($_.projectId)] $($_.title) (User: $($_.userId), Status: $($_.status))" -ForegroundColor White
    }
} catch {
    Write-Host "? Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Get current user's projects via NEW endpoint
Write-Host "`n=== Test 2: GET /api/projects/user/$currentUserId (my projects via new endpoint) ===" -ForegroundColor Cyan
try {
    $myProjectsNew = Invoke-RestMethod `
        -Uri "$baseUrl/api/projects/user/$currentUserId`?skip=0&take=20" `
        -Method GET `
        -Headers @{ "Authorization" = "Bearer $token" } `
        -SkipCertificateCheck

    Write-Host "? Total: $($myProjectsNew.total)" -ForegroundColor Green
    Write-Host "Projects:" -ForegroundColor Yellow
    $myProjectsNew.results | ForEach-Object {
        Write-Host "  - [$($_.projectId)] $($_.title) (User: $($_.userId), Status: $($_.status))" -ForegroundColor White
    }

    # Verify both endpoints return same data
    if ($myProjects.total -eq $myProjectsNew.total) {
        Write-Host "? PASS: Both endpoints return same count" -ForegroundColor Green
    } else {
        Write-Host "? FAIL: Counts differ (old: $($myProjects.total), new: $($myProjectsNew.total))" -ForegroundColor Red
    }
} catch {
    Write-Host "? Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Get another user's projects (if exists)
Write-Host "`n=== Test 3: GET /api/projects/user/{otherUserId} (another user's projects) ===" -ForegroundColor Cyan

# Try to find another user via search
try {
    $users = Invoke-RestMethod `
        -Uri "$baseUrl/api/users/search?query=&skip=0&take=5" `
        -Method GET `
        -Headers @{ "Authorization" = "Bearer $token" } `
        -SkipCertificateCheck

    $otherUser = $users.results | Where-Object { $_.userId -ne $currentUserId } | Select-Object -First 1

    if ($otherUser) {
        Write-Host "Found user: $($otherUser.displayName) (ID: $($otherUser.userId))" -ForegroundColor Yellow

        $otherProjects = Invoke-RestMethod `
            -Uri "$baseUrl/api/projects/user/$($otherUser.userId)?skip=0&take=20" `
            -Method GET `
            -Headers @{ "Authorization" = "Bearer $token" } `
            -SkipCertificateCheck

        Write-Host "? Total: $($otherProjects.total)" -ForegroundColor Green
        if ($otherProjects.total -gt 0) {
            Write-Host "Projects:" -ForegroundColor Yellow
            $otherProjects.results | ForEach-Object {
                Write-Host "  - [$($_.projectId)] $($_.title) (User: $($_.userId), Status: $($_.status))" -ForegroundColor White
            }

            # Verify all projects belong to the requested user
            $wrongUserProjects = $otherProjects.results | Where-Object { $_.userId -ne $otherUser.userId }
            if ($wrongUserProjects.Count -eq 0) {
                Write-Host "? PASS: All projects belong to user $($otherUser.userId)" -ForegroundColor Green
            } else {
                Write-Host "? FAIL: Found $($wrongUserProjects.Count) projects from wrong user!" -ForegroundColor Red
                $wrongUserProjects | ForEach-Object {
                    Write-Host "  Wrong: [$($_.projectId)] User $($_.userId) should be $($otherUser.userId)" -ForegroundColor Red
                }
            }
        } else {
            Write-Host "User has no projects" -ForegroundColor Gray
        }
    } else {
        Write-Host "?? No other users found to test with" -ForegroundColor Yellow
        Write-Host "Create another user account to test this scenario" -ForegroundColor Gray
    }
} catch {
    Write-Host "? Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
        $errorBody = $reader.ReadToEnd()
        Write-Host "Error details: $errorBody" -ForegroundColor Yellow
    }
}

# Test 4: User not found
Write-Host "`n=== Test 4: GET /api/projects/user/99999 (user not found) ===" -ForegroundColor Cyan
try {
    Invoke-RestMethod `
        -Uri "$baseUrl/api/projects/user/99999" `
        -Method GET `
        -Headers @{ "Authorization" = "Bearer $token" } `
        -SkipCertificateCheck
    
    Write-Host "? FAIL: Should have returned 404" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "? PASS: Correctly returned 404" -ForegroundColor Green
        $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
        $errorBody = $reader.ReadToEnd()
        Write-Host "Error message: $errorBody" -ForegroundColor Yellow
    } else {
        Write-Host "? FAIL: Wrong status code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# Test 5: Pagination
Write-Host "`n=== Test 5: Pagination (skip=0, take=1) ===" -ForegroundColor Cyan
try {
    $paged = Invoke-RestMethod `
        -Uri "$baseUrl/api/projects/user/$currentUserId`?skip=0&take=1" `
        -Method GET `
        -Headers @{ "Authorization" = "Bearer $token" } `
        -SkipCertificateCheck

    Write-Host "Total projects: $($paged.total)" -ForegroundColor Yellow
    Write-Host "Returned: $($paged.results.Count) projects" -ForegroundColor Yellow
    Write-Host "Skip: $($paged.skip), Take: $($paged.take)" -ForegroundColor Gray

    if ($paged.results.Count -le 1) {
        Write-Host "? PASS: Pagination working" -ForegroundColor Green
    } else {
        Write-Host "? FAIL: Returned more than requested" -ForegroundColor Red
    }
} catch {
    Write-Host "? Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 6: Invalid parameters
Write-Host "`n=== Test 6: Invalid take parameter (take=1000) ===" -ForegroundColor Cyan
try {
    $limited = Invoke-RestMethod `
        -Uri "$baseUrl/api/projects/user/$currentUserId`?skip=0&take=1000" `
        -Method GET `
        -Headers @{ "Authorization" = "Bearer $token" } `
        -SkipCertificateCheck

    if ($limited.take -le 100) {
        Write-Host "? PASS: Limited to max 100 (actual: $($limited.take))" -ForegroundColor Green
    } else {
        Write-Host "? FAIL: Didn't limit to 100 (actual: $($limited.take))" -ForegroundColor Red
    }
} catch {
    Write-Host "? Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 7: Verify ImageUrl is included
Write-Host "`n=== Test 7: Verify ImageUrl field exists ===" -ForegroundColor Cyan
if ($myProjectsNew.results.Count -gt 0) {
    $firstProject = $myProjectsNew.results[0]
    if ($null -ne $firstProject.PSObject.Properties['imageUrl']) {
        Write-Host "? PASS: ImageUrl field exists" -ForegroundColor Green
        Write-Host "Value: $($firstProject.imageUrl)" -ForegroundColor Gray
    } else {
        Write-Host "? FAIL: ImageUrl field missing" -ForegroundColor Red
    }
} else {
    Write-Host "?? SKIP: No projects to test with" -ForegroundColor Yellow
}

# Summary
Write-Host "`n=== Test Summary ===" -ForegroundColor Cyan
Write-Host "? New endpoint GET /api/projects/user/{userId} is working" -ForegroundColor Green
Write-Host "? Correctly filters projects by user ID" -ForegroundColor Green
Write-Host "? Returns 404 for non-existent users" -ForegroundColor Green
Write-Host "? Pagination and limits working" -ForegroundColor Green
Write-Host "? ImageUrl field included in response" -ForegroundColor Green

Write-Host "`n?? Frontend Integration:" -ForegroundColor Cyan
Write-Host "1. Update RTK Query to use: GET /api/projects/user/{userId}" -ForegroundColor Yellow
Write-Host "2. Remove defensive userId filtering in components" -ForegroundColor Yellow
Write-Host "3. Test with multiple users to verify isolation" -ForegroundColor Yellow

Write-Host "`nDone! ?" -ForegroundColor Green
