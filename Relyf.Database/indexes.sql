/*
    Relyf - consolidated indexes & unique constraints
    Target: SQL Server, schema: app
    Safe to re-run: each CREATE guarded with IF NOT EXISTS.
*/

/* ==============================
   Users
   ==============================*/
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_User_Email' AND object_id = OBJECT_ID('app.[User]'))
    CREATE UNIQUE INDEX UX_User_Email ON app.[User] (Email);


/* ==============================
   Tags & IdeaTags
   ==============================*/
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Tag_Name' AND object_id = OBJECT_ID('app.Tag'))
    CREATE UNIQUE INDEX UX_Tag_Name ON app.Tag (Name);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_IdeaTag_Idea_Tag' AND object_id = OBJECT_ID('app.IdeaTag'))
    CREATE UNIQUE INDEX UX_IdeaTag_Idea_Tag ON app.IdeaTag (IdeaId, TagId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_IdeaTag_TagId' AND object_id = OBJECT_ID('app.IdeaTag'))
    CREATE INDEX IX_IdeaTag_TagId ON app.IdeaTag (TagId);


/* ==============================
   Materials & Item/Project materials
   ==============================*/
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Material_Name' AND object_id = OBJECT_ID('app.Material'))
    CREATE UNIQUE INDEX UX_Material_Name ON app.Material (Name);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Material_Category_Name' AND object_id = OBJECT_ID('app.Material'))
    CREATE INDEX IX_Material_Category_Name ON app.Material (Category, Name);

-- ItemMaterial
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_ItemMaterial_Item_Material' AND object_id = OBJECT_ID('app.ItemMaterial'))
    CREATE UNIQUE INDEX UX_ItemMaterial_Item_Material ON app.ItemMaterial (ItemId, MaterialId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ItemMaterial_Item' AND object_id = OBJECT_ID('app.ItemMaterial'))
    CREATE INDEX IX_ItemMaterial_Item ON app.ItemMaterial (ItemId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ItemMaterial_Material' AND object_id = OBJECT_ID('app.ItemMaterial'))
    CREATE INDEX IX_ItemMaterial_Material ON app.ItemMaterial (MaterialId);

-- ProjectMaterial
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_ProjectMaterial_Project_Material' AND object_id = OBJECT_ID('app.ProjectMaterial'))
    CREATE UNIQUE INDEX UX_ProjectMaterial_Project_Material ON app.ProjectMaterial (ProjectId, MaterialId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProjectMaterial_Project' AND object_id = OBJECT_ID('app.ProjectMaterial'))
    CREATE INDEX IX_ProjectMaterial_Project ON app.ProjectMaterial (ProjectId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProjectMaterial_Material' AND object_id = OBJECT_ID('app.ProjectMaterial'))
    CREATE INDEX IX_ProjectMaterial_Material ON app.ProjectMaterial (MaterialId);


/* ==============================
   Reactions & Saves
   ==============================*/
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Reaction_User_Target_Kind' AND object_id = OBJECT_ID('app.Reaction'))
    CREATE UNIQUE INDEX UX_Reaction_User_Target_Kind ON app.Reaction (UserId, TargetType, TargetId, Kind);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reaction_Target_Kind' AND object_id = OBJECT_ID('app.Reaction'))
    CREATE INDEX IX_Reaction_Target_Kind ON app.Reaction (TargetType, TargetId, Kind);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_SavedIdea_User_Idea' AND object_id = OBJECT_ID('app.SavedIdea'))
    CREATE UNIQUE INDEX UX_SavedIdea_User_Idea ON app.SavedIdea (UserId, IdeaId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SavedIdea_User_SavedAt' AND object_id = OBJECT_ID('app.SavedIdea'))
    CREATE INDEX IX_SavedIdea_User_SavedAt ON app.SavedIdea (UserId, SavedAtUtc DESC);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SavedIdea_Idea' AND object_id = OBJECT_ID('app.SavedIdea'))
    CREATE INDEX IX_SavedIdea_Idea ON app.SavedIdea (IdeaId);


/* ==============================
   Ideas, Comments, Feedback
   ==============================*/
-- Fast search/order
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AiIdea_User_Deleted_Id' AND object_id = OBJECT_ID('app.AiIdea'))
    CREATE INDEX IX_AiIdea_User_Deleted_Id ON app.AiIdea (UserId, IsDeleted, IdeaId DESC);

-- Comments by (type, target)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Comment_Idea' AND object_id = OBJECT_ID('app.Comment'))
    CREATE INDEX IX_Comment_Idea ON app.Comment (TargetType, TargetId);

-- Feedback by (type, target)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Feedback_Target' AND object_id = OBJECT_ID('app.Feedback'))
    CREATE INDEX IX_Feedback_Target ON app.Feedback (TargetType, TargetId);

-- Optional: prevent duplicate per-user rating on non-App targets (filtered unique index)
/*
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Feedback_User_Target' AND object_id = OBJECT_ID('app.Feedback'))
    CREATE UNIQUE INDEX UX_Feedback_User_Target ON app.Feedback (UserId, TargetType, TargetId)
    WHERE TargetType IN ('Idea','Project');
*/


/* ==============================
   Dropoff sites & user dropoffs
   ==============================*/
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DropoffSite_Name' AND object_id = OBJECT_ID('app.DropoffSite'))
    CREATE INDEX IX_DropoffSite_Name ON app.DropoffSite (Name);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DropoffSite_City_Name' AND object_id = OBJECT_ID('app.DropoffSite'))
    CREATE INDEX IX_DropoffSite_City_Name ON app.DropoffSite (City, Name);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DropoffSite_Country_Region_Name' AND object_id = OBJECT_ID('app.DropoffSite'))
    CREATE INDEX IX_DropoffSite_Country_Region_Name ON app.DropoffSite (CountryCode, Region, Name);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserDropoff_User_DroppedAt' AND object_id = OBJECT_ID('app.UserDropoff'))
    CREATE INDEX IX_UserDropoff_User_DroppedAt ON app.UserDropoff (UserId, DroppedAtUtc DESC);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserDropoff_Site' AND object_id = OBJECT_ID('app.UserDropoff'))
    CREATE INDEX IX_UserDropoff_Site ON app.UserDropoff (DropoffSiteId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserDropoff_Material' AND object_id = OBJECT_ID('app.UserDropoff'))
    CREATE INDEX IX_UserDropoff_Material ON app.UserDropoff (MaterialId);


/* ==============================
   API Request Logs
   ==============================*/
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ApiRequestLog_Id' AND object_id = OBJECT_ID('app.ApiRequestLog'))
    CREATE INDEX IX_ApiRequestLog_Id ON app.ApiRequestLog (ApiRequestLogId DESC);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ApiRequestLog_Status' AND object_id = OBJECT_ID('app.ApiRequestLog'))
    CREATE INDEX IX_ApiRequestLog_Status ON app.ApiRequestLog (StatusCode);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ApiRequestLog_User' AND object_id = OBJECT_ID('app.ApiRequestLog'))
    CREATE INDEX IX_ApiRequestLog_User ON app.ApiRequestLog (UserId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ApiRequestLog_Model' AND object_id = OBJECT_ID('app.ApiRequestLog'))
    CREATE INDEX IX_ApiRequestLog_Model ON app.ApiRequestLog (Model);


/* ==============================
   Optional: Full-Text for AiIdea (Title, IdeaText)
   ==============================*/
/*
-- Run once (if no FT catalog exists):
-- CREATE FULLTEXT CATALOG RelyfFT AS DEFAULT;

-- Create FT index (requires a unique, non-nullable single-column key)
-- Adjust PK/unique index name accordingly
CREATE FULLTEXT INDEX ON app.AiIdea(Title LANGUAGE 1033, IdeaText LANGUAGE 1033)
KEY INDEX PK_AiIdea;
*/
