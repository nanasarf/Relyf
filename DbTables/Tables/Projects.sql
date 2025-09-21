CREATE TABLE [dbo].[Project] (
    ProjectId INT IDENTITY(1,1) PRIMARY KEY,   -- Unique project ID
    UserId INT NOT NULL,                       -- FK to User table (project owner)
    Title NVARCHAR(100) NOT NULL,              -- Project title
    Description NVARCHAR(MAX) NULL,            -- Detailed explanation of the project
    Status NVARCHAR(20) DEFAULT 'In Progress', -- Status: In Progress, Completed, Draft
    Visibility NVARCHAR(20) DEFAULT 'Public',  -- Public or Private project
    CreatedAt DATETIME DEFAULT GETDATE(),      -- When project was created
    UpdatedAt DATETIME NULL,                   -- When project was last updated
    CONSTRAINT FK_Project_User FOREIGN KEY (UserId)
        REFERENCES [dbo].[User](UserId) ON DELETE CASCADE
);
GO
