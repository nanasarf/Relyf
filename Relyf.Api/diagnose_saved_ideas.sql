-- Diagnostic script for SavedIdea issues

-- 1. Check if SavedIdea table exists and has data
SELECT COUNT(*) AS TotalSavedIdeas
FROM app.SavedIdea;

-- 2. Show sample saved ideas
SELECT TOP 10 *
FROM app.SavedIdea
ORDER BY SavedAtUtc DESC;

-- 3. Check if AiIdea table has data
SELECT COUNT(*) AS TotalAiIdeas
FROM app.AiIdea
WHERE IsDeleted = 0;

-- 4. Check for orphaned saves (saves pointing to non-existent or deleted ideas)
SELECT COUNT(*) AS OrphanedSaves
FROM app.SavedIdea s
LEFT JOIN app.AiIdea i ON i.IdeaId = s.IdeaId
WHERE i.IdeaId IS NULL OR i.IsDeleted = 1;

-- 5. Test the JOIN used in SaveRepository.ListForUserAsync
SELECT  i.IdeaId,
        i.Title,
        CASE WHEN LEN(i.IdeaText) > 140 THEN LEFT(i.IdeaText, 140) + '...' ELSE i.IdeaText END AS Preview,
        s.SavedAtUtc,
        s.UserId
FROM app.SavedIdea s
JOIN app.AiIdea i ON i.IdeaId = s.IdeaId
ORDER BY s.SavedAtUtc DESC;

-- 6. Check save counts per user
SELECT 
    u.UserId,
    u.UserName,
    u.DisplayName,
    COUNT(s.IdeaId) AS SaveCount
FROM app.[User] u
LEFT JOIN app.SavedIdea s ON s.UserId = u.UserId
GROUP BY u.UserId, u.UserName, u.DisplayName
HAVING COUNT(s.IdeaId) > 0
ORDER BY SaveCount DESC;

-- 7. Verify the exact query used in UserRepository.GetProfileAsync
SELECT 
    (SELECT COUNT(*) FROM app.[SavedIdea] WHERE UserId = 1) as SaveCountForUser1;
