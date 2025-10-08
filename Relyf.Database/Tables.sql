
CREATE TABLE app.[User](
    UserId           INT IDENTITY(1,1) PRIMARY KEY,
    Email            NVARCHAR(256) NOT NULL UNIQUE,
    DisplayName      NVARCHAR(120) NOT NULL,
    CountryCode      CHAR(2) NULL,
    CreatedAtUtc     DATETIME2(3) NOT NULL CONSTRAINT DF_User_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc     DATETIME2(3) NULL,
    IsDeleted        BIT NOT NULL CONSTRAINT DF_User_IsDeleted DEFAULT(0)
);
GO

CREATE TABLE app.Material(
    MaterialId       INT IDENTITY(1,1) PRIMARY KEY,
    Name             NVARCHAR(100) NOT NULL UNIQUE,
    Category         NVARCHAR(80) NULL, -- e.g., plastic, metal, fabric, electronic, paper
    Recyclability    TINYINT NULL,      -- 0-100 score (optional)
    Notes            NVARCHAR(400) NULL
);
GO

CREATE TABLE app.Tag(
    TagId            INT IDENTITY(1,1) PRIMARY KEY,
    Name             NVARCHAR(80) NOT NULL UNIQUE
);
GO


CREATE TABLE app.Item(
    ItemId           INT IDENTITY(1,1) PRIMARY KEY,
    UserId           INT NOT NULL,
    Title            NVARCHAR(140) NOT NULL,
    Description      NVARCHAR(1000) NULL,
    ConditionNote    NVARCHAR(200) NULL, -- e.g., broken joystick, torn sleeve
    EstimatedWeightG INT NULL,
    LocationText     NVARCHAR(200) NULL,
    CreatedAtUtc     DATETIME2(3) NOT NULL CONSTRAINT DF_Item_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc     DATETIME2(3) NULL,
    IsDeleted        BIT NOT NULL CONSTRAINT DF_Item_IsDeleted DEFAULT(0),
    CONSTRAINT FK_Item_User FOREIGN KEY(UserId) REFERENCES app.[User](UserId)
);
GO

CREATE TABLE app.ItemMaterial(
    ItemId       INT NOT NULL,
    MaterialId   INT NOT NULL,
    PercentShare TINYINT NULL, -- optional composition split 0-100
    CONSTRAINT PK_ItemMaterial PRIMARY KEY(ItemId, MaterialId),
    CONSTRAINT FK_ItemMaterial_Item FOREIGN KEY(ItemId) REFERENCES app.Item(ItemId) ON DELETE CASCADE,
    CONSTRAINT FK_ItemMaterial_Material FOREIGN KEY(MaterialId) REFERENCES app.Material(MaterialId)
);
GO

CREATE TABLE app.Image(
    ImageId         INT IDENTITY(1,1) PRIMARY KEY,
    OwnerType       NVARCHAR(20) NOT NULL,  -- 'Item' | 'Idea' | 'Project'
    OwnerId         INT NOT NULL,           -- FK resolved via check constraints
    Source          NVARCHAR(20) NOT NULL,  -- 'upload' | 'url' | 'cloudinary'
    Url             NVARCHAR(500) NOT NULL,
    AltText         NVARCHAR(160) NULL,
    CreatedAtUtc    DATETIME2(3) NOT NULL CONSTRAINT DF_Image_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
    -- Polymorphic integrity is enforced at app level; keep queries disciplined
    CONSTRAINT CK_Image_OwnerType CHECK (OwnerType IN ('Item','Idea','Project'))
);
GO


CREATE TABLE app.CoherePrompt(
    CoherePromptId   INT IDENTITY(1,1) PRIMARY KEY,
    UserId           INT NOT NULL,
    ItemId           INT NULL,             -- can be null for general prompts
    Model            NVARCHAR(80) NULL,    -- e.g., command-r-plus
    Temperature      DECIMAL(4,2) NULL,
    TopP             DECIMAL(4,2) NULL,
    PromptText       NVARCHAR(MAX) NOT NULL,
    CreatedAtUtc     DATETIME2(3) NOT NULL CONSTRAINT DF_CoherePrompt_Created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_CoherePrompt_User FOREIGN KEY(UserId) REFERENCES app.[User](UserId),
    CONSTRAINT FK_CoherePrompt_Item FOREIGN KEY(ItemId) REFERENCES app.Item(ItemId)
);
GO

CREATE TABLE app.AiIdea(
    IdeaId           INT IDENTITY(1,1) PRIMARY KEY,
    CoherePromptId   INT NOT NULL,
    ItemId           INT NULL,             -- optional: idea might be generic or linked to an item
    UserId           INT NOT NULL,         -- author (requester)
    Title            NVARCHAR(160) NOT NULL,
    IdeaText         NVARCHAR(MAX) NOT NULL,
    Difficulty       NVARCHAR(20) NULL,    -- 'easy' | 'medium' | 'hard'
    EstTimeMin       INT NULL,
    EstCostUSD       DECIMAL(10,2) NULL,
    TokensIn         INT NULL,
    TokensOut        INT NULL,
    ApiLatencyMs     INT NULL,
    CreatedAtUtc     DATETIME2(3) NOT NULL CONSTRAINT DF_AiIdea_Created DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc     DATETIME2(3) NULL,
    CONSTRAINT FK_AiIdea_Prompt FOREIGN KEY(CoherePromptId) REFERENCES app.CoherePrompt(CoherePromptId) ON DELETE CASCADE,
    CONSTRAINT FK_AiIdea_User FOREIGN KEY(UserId) REFERENCES app.[User](UserId),
    CONSTRAINT FK_AiIdea_Item FOREIGN KEY(ItemId) REFERENCES app.Item(ItemId)
);
GO

CREATE TABLE app.IdeaTag(
    IdeaId   INT NOT NULL,
    TagId    INT NOT NULL,
    CONSTRAINT PK_IdeaTag PRIMARY KEY(IdeaId, TagId),
    CONSTRAINT FK_IdeaTag_Idea FOREIGN KEY(IdeaId) REFERENCES app.AiIdea(IdeaId) ON DELETE CASCADE,
    CONSTRAINT FK_IdeaTag_Tag FOREIGN KEY(TagId) REFERENCES app.Tag(TagId)
);
GO


CREATE TABLE app.Project(
    ProjectId        INT IDENTITY(1,1) PRIMARY KEY,
    IdeaId           INT NULL,            -- project may be spawned from an idea
    UserId           INT NOT NULL,        -- project owner
    Title            NVARCHAR(160) NOT NULL,
    Description      NVARCHAR(MAX) NULL,
    Status           NVARCHAR(20) NOT NULL CONSTRAINT DF_Project_Status DEFAULT('draft'), -- draft|in_progress|completed
    CreatedAtUtc     DATETIME2(3) NOT NULL CONSTRAINT DF_Project_Created DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc     DATETIME2(3) NULL,
    CONSTRAINT FK_Project_Idea FOREIGN KEY(IdeaId) REFERENCES app.AiIdea(IdeaId),
    CONSTRAINT FK_Project_User FOREIGN KEY(UserId) REFERENCES app.[User](UserId)
);
GO

CREATE TABLE app.ProjectStep(
    ProjectStepId    INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId        INT NOT NULL,
    StepNumber       INT NOT NULL,
    Instruction      NVARCHAR(1500) NOT NULL,
    CONSTRAINT UQ_ProjectStep UNIQUE(ProjectId, StepNumber),
    CONSTRAINT FK_ProjectStep_Project FOREIGN KEY(ProjectId) REFERENCES app.Project(ProjectId) ON DELETE CASCADE
);
GO

CREATE TABLE app.ProjectMaterial(
    ProjectId    INT NOT NULL,
    MaterialId   INT NOT NULL,
    QuantityText NVARCHAR(80) NULL,  -- e.g., "2m fabric", "4 screws"
    CONSTRAINT PK_ProjectMaterial PRIMARY KEY(ProjectId, MaterialId),
    CONSTRAINT FK_ProjectMaterial_Project FOREIGN KEY(ProjectId) REFERENCES app.Project(ProjectId) ON DELETE CASCADE,
    CONSTRAINT FK_ProjectMaterial_Material FOREIGN KEY(MaterialId) REFERENCES app.Material(MaterialId)
);
GO


CREATE TABLE app.SavedIdea(
    UserId   INT NOT NULL,
    IdeaId   INT NOT NULL,
    SavedAtUtc DATETIME2(3) NOT NULL CONSTRAINT DF_SavedIdea_SavedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_SavedIdea PRIMARY KEY(UserId, IdeaId),
    CONSTRAINT FK_SavedIdea_User FOREIGN KEY(UserId) REFERENCES app.[User](UserId) ON DELETE CASCADE,
    CONSTRAINT FK_SavedIdea_Idea FOREIGN KEY(IdeaId) REFERENCES app.AiIdea(IdeaId) ON DELETE CASCADE
);
GO

CREATE TABLE app.Reaction(
    ReactionId   INT IDENTITY(1,1) PRIMARY KEY,
    UserId       INT NOT NULL,
    TargetType   NVARCHAR(20) NOT NULL,  -- 'Idea' | 'Project'
    TargetId     INT NOT NULL,
    Kind         NVARCHAR(20) NOT NULL,  -- 'like' | 'upvote' | 'helpful'
    CreatedAtUtc DATETIME2(3) NOT NULL CONSTRAINT DF_Reaction_Created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_Reaction_TargetType CHECK (TargetType IN ('Idea','Project')),
    CONSTRAINT CK_Reaction_Kind CHECK (Kind IN ('like','upvote','helpful')),
    CONSTRAINT UQ_Reaction UNIQUE(UserId, TargetType, TargetId, Kind),
    CONSTRAINT FK_Reaction_User FOREIGN KEY(UserId) REFERENCES app.[User](UserId)
);
GO

CREATE TABLE app.Comment(
    CommentId    INT IDENTITY(1,1) PRIMARY KEY,
    UserId       INT NOT NULL,
    TargetType   NVARCHAR(20) NOT NULL,  -- 'Idea' | 'Project'
    TargetId     INT NOT NULL,
    Body         NVARCHAR(1200) NOT NULL,
    CreatedAtUtc DATETIME2(3) NOT NULL CONSTRAINT DF_Comment_Created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_Comment_TargetType CHECK (TargetType IN ('Idea','Project')),
    CONSTRAINT FK_Comment_User FOREIGN KEY(UserId) REFERENCES app.[User](UserId)
);
GO


CREATE TABLE app.DropoffSite(
    DropoffSiteId  INT IDENTITY(1,1) PRIMARY KEY,
    Name           NVARCHAR(160) NOT NULL,
    AddressLine1   NVARCHAR(160) NULL,
    City           NVARCHAR(80) NULL,
    Region         NVARCHAR(80) NULL,
    PostalCode     NVARCHAR(20) NULL,
    CountryCode    CHAR(2) NULL,
    AcceptedNotes  NVARCHAR(400) NULL, -- what they take
    CreatedAtUtc   DATETIME2(3) NOT NULL CONSTRAINT DF_DropoffSite_Created DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE app.UserDropoff(
    UserDropoffId  INT IDENTITY(1,1) PRIMARY KEY,
    UserId         INT NOT NULL,
    DropoffSiteId  INT NOT NULL,
    MaterialId     INT NULL,                 -- optional per-record detail
    QuantityText   NVARCHAR(80) NULL,
    DroppedAtUtc   DATETIME2(3) NOT NULL,
    CONSTRAINT FK_UserDropoff_User FOREIGN KEY(UserId) REFERENCES app.[User](UserId),
    CONSTRAINT FK_UserDropoff_Site FOREIGN KEY(DropoffSiteId) REFERENCES app.DropoffSite(DropoffSiteId),
    CONSTRAINT FK_UserDropoff_Material FOREIGN KEY(MaterialId) REFERENCES app.Material(MaterialId)
);
GO


CREATE TABLE app.ApiRequestLog(
    ApiRequestLogId INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NULL,
    Provider        NVARCHAR(40) NOT NULL,  -- 'cohere'
    Endpoint        NVARCHAR(200) NOT NULL,
    Model           NVARCHAR(80) NULL,
    PromptHash      VARBINARY(32) NULL,     -- optional SHA-256 of prompt
    TokensIn        INT NULL,
    TokensOut       INT NULL,
    StatusCode      INT NOT NULL,
    DurationMs      INT NULL,
    CreatedAtUtc    DATETIME2(3) NOT NULL CONSTRAINT DF_ApiRequestLog_Created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ApiRequestLog_User FOREIGN KEY(UserId) REFERENCES app.[User](UserId)
);
GO

CREATE TABLE app.Feedback(
    FeedbackId      INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL,
    TargetType      NVARCHAR(20) NOT NULL, -- 'Idea' | 'Project' | 'App'
    TargetId        INT NULL,
    Rating          TINYINT NULL CHECK (Rating BETWEEN 1 AND 5),
    Notes           NVARCHAR(800) NULL,
    CreatedAtUtc    DATETIME2(3) NOT NULL CONSTRAINT DF_Feedback_Created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_Feedback_TargetType CHECK (TargetType IN ('Idea','Project','App')),
    CONSTRAINT FK_Feedback_User FOREIGN KEY(UserId) REFERENCES app.[User](UserId)
);
GO


CREATE INDEX IX_Item_UserId ON app.Item(UserId);
CREATE INDEX IX_ItemMaterial_MaterialId ON app.ItemMaterial(MaterialId);
CREATE INDEX IX_AiIdea_ItemId ON app.AiIdea(ItemId);
CREATE INDEX IX_AiIdea_UserId ON app.AiIdea(UserId);
CREATE INDEX IX_IdeaTag_TagId ON app.IdeaTag(TagId);
CREATE INDEX IX_Project_UserId ON app.Project(UserId);
CREATE INDEX IX_ProjectStep_ProjectId ON app.ProjectStep(ProjectId);
CREATE INDEX IX_Reaction_Target ON app.Reaction(TargetType, TargetId);
CREATE INDEX IX_Comment_Target ON app.Comment(TargetType, TargetId);
CREATE INDEX IX_UserDropoff_User ON app.UserDropoff(UserId);
CREATE INDEX IX_ApiRequestLog_User ON app.ApiRequestLog(UserId);
GO


INSERT INTO app.Material(Name, Category, Recyclability) VALUES
(N'Plastic (PET)', N'plastic', 70),
(N'Aluminum', N'metal', 95),
(N'Fabric (Cotton)', N'fabric', 50),
(N'Electronics', N'electronic', 30);

INSERT INTO app.Tag(Name) VALUES
(N'home-decor'), (N'fashion'), (N'gifts'), (N'repair'), (N'recycling');

