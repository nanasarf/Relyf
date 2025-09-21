CREATE TABLE [dbo].[Message] (
    MessageId INT IDENTITY(1,1) PRIMARY KEY,
    SenderId INT NOT NULL,
    ReceiverId INT NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    IsRead BIT DEFAULT 0,
    SentAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Message_Sender FOREIGN KEY (SenderId)
        REFERENCES [dbo].[User](UserId) ON DELETE CASCADE,
    CONSTRAINT FK_Message_Receiver FOREIGN KEY (ReceiverId)
        REFERENCES [dbo].[User](UserId) ON DELETE CASCADE
);
GO
