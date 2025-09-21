CREATE TABLE [dbo].[MarketplaceItem] (
    ItemId INT IDENTITY(1,1) PRIMARY KEY,        -- Unique marketplace item ID
    UserId INT NOT NULL,                         -- Who listed the item
    ProjectId INT NULL,                          -- Optional: Link to a project (if it's a finished product)
    Title NVARCHAR(100) NOT NULL,                -- Item title
    Description NVARCHAR(MAX) NULL,              -- Item description/details
    Price DECIMAL(10,2) NULL,                    -- Price (NULL means free)
    IsForTrade BIT DEFAULT 0,                    -- Whether it's for trade/barter instead of cash
    Status NVARCHAR(20) DEFAULT 'Available',     -- Available, Sold, or Removed
    CreatedAt DATETIME DEFAULT GETDATE(),        -- When listed
    UpdatedAt DATETIME NULL,                     -- When last updated
    CONSTRAINT FK_MarketplaceItem_User FOREIGN KEY (UserId)
        REFERENCES [dbo].[User](UserId) ON DELETE CASCADE,
    CONSTRAINT FK_MarketplaceItem_Project FOREIGN KEY (ProjectId)
        REFERENCES [dbo].[Project](ProjectId) ON DELETE SET NULL
);
GO
