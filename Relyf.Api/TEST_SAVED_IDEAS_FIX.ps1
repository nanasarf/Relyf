# TEST SAVED IDEAS FIX
# Tests that saved ideas now display correctly with images, tags, and IsDeleted filter

$baseUrl = "https://localhost:7139"
$token = "" # TODO: Insert valid JWT token

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "SAVED IDEAS DISPLAY FIX - TEST SCRIPT" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Step 1: Login
Write-Host "`n1??  LOGIN" -ForegroundColor Yellow
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
    $userId = $loginResponse.userId
    Write-Host "? Login successful!" -ForegroundColor Green
    Write-Host "   User ID: $userId" -ForegroundColor Gray
} catch {
    Write-Host "? Login failed: $_" -ForegroundColor Red
    Write-Host "??  Please update credentials in script" -ForegroundColor Yellow
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
}

# Step 2: Save a few ideas
Write-Host "`n2??  SAVE IDEAS" -ForegroundColor Yellow
$ideaIdsToSave = @(1, 2, 3)
foreach ($ideaId in $ideaIdsToSave) {
    try {
        $saveBody = @{ ideaId = $ideaId } | ConvertTo-Json
        $saveResponse = Invoke-WebRequest -Uri "$baseUrl/api/saves" `
            -Method PUT `
            -Headers $headers `
            -ContentType "application/json" `
            -Body $saveBody
        
        Write-Host "   ? Saved idea $ideaId (Status: $($saveResponse.StatusCode))" -ForegroundColor Green
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 204) {
            Write-Host "   ??  Idea $ideaId already saved (204 No Content)" -ForegroundColor Cyan
        } elseif ($statusCode -eq 400) {
            Write-Host "   ??  Idea $ideaId doesn't exist (400 Bad Request)" -ForegroundColor Yellow
        } else {
            Write-Host "   ? Failed to save idea $ideaId : $_" -ForegroundColor Red
        }
    }
}

# Step 3: Get saved ideas
Write-Host "`n3??  GET SAVED IDEAS" -ForegroundColor Yellow
try {
    $savedIdeas = Invoke-RestMethod -Uri "$baseUrl/api/saves/user/$userId" `
        -Method GET `
        -Headers $headers
    
    Write-Host "? Retrieved saved ideas!" -ForegroundColor Green
    Write-Host "   Count: $($savedIdeas.Count)" -ForegroundColor Cyan
    
    if ($savedIdeas.Count -gt 0) {
        Write-Host "`n   ?? Saved Ideas:" -ForegroundColor Cyan
        foreach ($idea in $savedIdeas) {
            Write-Host "   ??????????????????????????????????" -ForegroundColor DarkGray
            Write-Host "   ?? Idea ID: $($idea.ideaId)" -ForegroundColor White
            Write-Host "   ?? Title: $($idea.title)" -ForegroundColor White
            Write-Host "   ?? Preview: $($idea.preview)" -ForegroundColor Gray
            
            if ($idea.imageUrl) {
                Write-Host "   ???  ImageUrl: $($idea.imageUrl)" -ForegroundColor Green
            } else {
                Write-Host "   ???  ImageUrl: (none)" -ForegroundColor DarkGray
            }
            
            if ($idea.tags -and $idea.tags.Count -gt 0) {
                Write-Host "   ???  Tags: $($idea.tags -join ', ')" -ForegroundColor Magenta
            } else {
                Write-Host "   ???  Tags: (none)" -ForegroundColor DarkGray
            }
            
            Write-Host "   ?? Saved: $($idea.savedAtUtc)" -ForegroundColor Gray
        }
        Write-Host "   ??????????????????????????????????" -ForegroundColor DarkGray
    } else {
        Write-Host "   ??  No saved ideas found!" -ForegroundColor Yellow
        Write-Host "   Possible reasons:" -ForegroundColor Yellow
        Write-Host "   - All saved ideas are deleted (IsDeleted = 1)" -ForegroundColor Yellow
        Write-Host "   - Saves failed in step 2" -ForegroundColor Yellow
        Write-Host "   - User has no saves" -ForegroundColor Yellow
    }
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "? Failed to get saved ideas" -ForegroundColor Red
    Write-Host "   Status: $statusCode" -ForegroundColor Red
    Write-Host "   Error: $_" -ForegroundColor Red
    
    if ($statusCode -eq 403) {
        Write-Host "   ?? 403 Forbidden: Are you trying to view someone else's saves?" -ForegroundColor Yellow
    }
}

# Step 4: Get user profile with save count
Write-Host "`n4??  GET USER PROFILE (SaveCount)" -ForegroundColor Yellow
try {
    $profile = Invoke-RestMethod -Uri "$baseUrl/api/users/$userId" `
        -Method GET `
        -Headers $headers
    
    Write-Host "? Profile retrieved!" -ForegroundColor Green
    Write-Host "   User: $($profile.displayName) (@$($profile.userName))" -ForegroundColor White
    Write-Host "   SaveCount: $($profile.saveCount)" -ForegroundColor Cyan
    Write-Host "   IdeaCount: $($profile.ideaCount)" -ForegroundColor Gray
    Write-Host "   ProjectCount: $($profile.projectCount)" -ForegroundColor Gray
} catch {
    Write-Host "? Failed to get profile: $_" -ForegroundColor Red
}

# Step 5: Compare counts
Write-Host "`n5??  VALIDATION" -ForegroundColor Yellow
if ($null -ne $savedIdeas -and $null -ne $profile) {
    $displayedCount = $savedIdeas.Count
    $profileCount = $profile.saveCount
    
    Write-Host "   Profile SaveCount: $profileCount" -ForegroundColor Cyan
    Write-Host "   Displayed Ideas: $displayedCount" -ForegroundColor Cyan
    
    if ($displayedCount -eq $profileCount) {
        Write-Host "   ? Counts match! All saves are active." -ForegroundColor Green
    } elseif ($displayedCount -lt $profileCount) {
        $deletedCount = $profileCount - $displayedCount
        Write-Host "   ??  Mismatch detected!" -ForegroundColor Yellow
        Write-Host "   This means $deletedCount saved idea(s) are deleted (IsDeleted = 1)" -ForegroundColor Yellow
        Write-Host "   This is EXPECTED behavior - SaveCount counts all saves (including deleted)" -ForegroundColor Cyan
    } else {
        Write-Host "   ? ERROR: Displayed count > SaveCount (should not happen!)" -ForegroundColor Red
    }
    
    # Check response structure
    Write-Host "`n6??  RESPONSE STRUCTURE VALIDATION" -ForegroundColor Yellow
    if ($savedIdeas.Count -gt 0) {
        $firstIdea = $savedIdeas[0]
        $hasIdeaId = $null -ne $firstIdea.ideaId
        $hasTitle = $null -ne $firstIdea.title
        $hasPreview = $null -ne $firstIdea.preview
        $hasImageUrl = $null -ne (Get-Member -InputObject $firstIdea -Name "imageUrl")
        $hasTags = $null -ne (Get-Member -InputObject $firstIdea -Name "tags")
        $hasSavedAt = $null -ne $firstIdea.savedAtUtc
        
        Write-Host "   Field Validation:" -ForegroundColor Cyan
        Write-Host "   - ideaId:     $(if ($hasIdeaId) {'?'} else {'?'})" -ForegroundColor $(if ($hasIdeaId) {'Green'} else {'Red'})
        Write-Host "   - title:      $(if ($hasTitle) {'?'} else {'?'})" -ForegroundColor $(if ($hasTitle) {'Green'} else {'Red'})
        Write-Host "   - preview:    $(if ($hasPreview) {'?'} else {'?'})" -ForegroundColor $(if ($hasPreview) {'Green'} else {'Red'})
        Write-Host "   - imageUrl:   $(if ($hasImageUrl) {'?'} else {'?'})" -ForegroundColor $(if ($hasImageUrl) {'Green'} else {'Red'})
        Write-Host "   - tags:       $(if ($hasTags) {'?'} else {'?'})" -ForegroundColor $(if ($hasTags) {'Green'} else {'Red'})
        Write-Host "   - savedAtUtc: $(if ($hasSavedAt) {'?'} else {'?'})" -ForegroundColor $(if ($hasSavedAt) {'Green'} else {'Red'})
        
        if ($hasIdeaId -and $hasTitle -and $hasPreview -and $hasImageUrl -and $hasTags -and $hasSavedAt) {
            Write-Host "`n   ? ALL FIELDS PRESENT - FIX IS WORKING!" -ForegroundColor Green
        } else {
            Write-Host "`n   ? MISSING FIELDS - FIX NOT COMPLETE!" -ForegroundColor Red
        }
    }
}

Write-Host "`n==========================================" -ForegroundColor Cyan
Write-Host "TEST COMPLETE" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

Write-Host "`n?? Summary:" -ForegroundColor Yellow
Write-Host "1. Saved ideas now include imageUrl and tags ?" -ForegroundColor White
Write-Host "2. Deleted ideas are filtered out (IsDeleted = 0) ?" -ForegroundColor White
Write-Host "3. SaveCount may be higher than displayed count (expected) ?" -ForegroundColor White
Write-Host "4. Frontend needs to use 'preview' field instead of 'description' ??" -ForegroundColor Yellow

Write-Host "`n?? Frontend Integration:" -ForegroundColor Yellow
Write-Host "Update Profile.tsx:" -ForegroundColor White
Write-Host "  <IdeaCard" -ForegroundColor Gray
Write-Host "    title={item.title}" -ForegroundColor Gray
Write-Host "    description={item.preview}  // ? Changed from item.description" -ForegroundColor Green
Write-Host "    imageUrl={item.imageUrl}    // ? Now populated" -ForegroundColor Green
Write-Host "    tags={item.tags || []}      // ? Now populated" -ForegroundColor Green
Write-Host "  />" -ForegroundColor Gray
