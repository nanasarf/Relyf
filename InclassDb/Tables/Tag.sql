CREATE TABLE [dbo].[Tag] (
    TagId INT IDENTITY(1,1) PRIMARY KEY,   -- Unique tag ID
    Name NVARCHAR(50) NOT NULL UNIQUE,     -- Tag name (e.g., Plastic, DIY, Metal)
    Description NVARCHAR(255) NULL,        -- Optional short description of the tag
    CreatedAt DATETIME DEFAULT GETDATE()   -- When the tag was created
);
GO

-- Junction table for many-to-many relationship between Projects and Tags
CREATE TABLE [dbo].[ProjectTag] (
    ProjectTagId INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    TagId INT NOT NULL,
    CONSTRAINT FK_ProjectTag_Project FOREIGN KEY (ProjectId)
        REFERENCES [dbo].[Project](ProjectId) ON DELETE CASCADE,
    CONSTRAINT FK_ProjectTag_Tag FOREIGN KEY (TagId)
        REFERENCES [dbo].[Tag](TagId) ON DELETE CASCADE,
    CONSTRAINT UQ_ProjectTag UNIQUE (ProjectId, TagId) -- Prevent duplicates
);
GO
