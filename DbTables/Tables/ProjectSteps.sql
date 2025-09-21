CREATE TABLE [dbo].[ProjectStep] (
    StepId INT IDENTITY(1,1) PRIMARY KEY,     -- Unique step ID
    ProjectId INT NOT NULL,                   -- FK to Project table
    StepNumber INT NOT NULL,                  -- Order of the step (1, 2, 3…)
    Instruction NVARCHAR(MAX) NOT NULL,       -- Step details
    MediaUrl NVARCHAR(255) NULL,              -- Optional image/video link
    CreatedAt DATETIME DEFAULT GETDATE(),     -- When the step was created
    UpdatedAt DATETIME NULL,                  -- When the step was updated
    CONSTRAINT FK_ProjectStep_Project FOREIGN KEY (ProjectId)
        REFERENCES [dbo].[Project](ProjectId) ON DELETE CASCADE
);
GO
