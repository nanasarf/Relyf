CREATE TABLE [dbo].[SavedProject] (
    SavedProjectId INT IDENTITY(1,1) PRIMARY KEY,  -- Unique saved project ID
    UserId INT NOT NULL,                           -- The user who saved the project
    ProjectId INT NOT NULL,                        -- The project being saved
    SavedAt DATETIME DEFAULT GETDATE(),            -- When the project was saved
    CONSTRAINT FK_SavedProject_User FOREIGN KEY (UserId)
        REFERENCES [dbo].[User](UserId) ON DELETE CASCADE,
    CONSTRAINT FK_SavedProject_Project FOREIGN KEY (ProjectId)
        REFERENCES [dbo].[Project](ProjectId) ON DELETE CASCADE,
    CONSTRAINT UQ_SavedProject UNIQUE (UserId, ProjectId) -- Prevent saving duplicates
);
GO
