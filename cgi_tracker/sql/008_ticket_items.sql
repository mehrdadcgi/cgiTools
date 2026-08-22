/*
  Support/Admin work items on a ticket.
*/
SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.TicketItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TicketItems (
        ItemId            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TicketItems PRIMARY KEY,
        TicketId          INT NOT NULL,
        Title             NVARCHAR(200) NOT NULL,
        Description       NVARCHAR(MAX) NOT NULL,
        ItemStatus        NVARCHAR(50) NOT NULL CONSTRAINT DF_TicketItems_Status DEFAULT (N'Open'),
        CreatedByUserId   NVARCHAR(128) NOT NULL,
        CreatedAt         DATETIME2(0) NOT NULL CONSTRAINT DF_TicketItems_CreatedAt DEFAULT (SYSUTCDATETIME()),
        IsDeleted         BIT NOT NULL CONSTRAINT DF_TicketItems_IsDeleted DEFAULT (0),
        CONSTRAINT FK_TicketItems_Ticket FOREIGN KEY (TicketId) REFERENCES dbo.Tickets(TicketId),
        CONSTRAINT FK_TicketItems_User FOREIGN KEY (CreatedByUserId) REFERENCES dbo.AspNetUsers(Id),
        CONSTRAINT CK_TicketItems_Status CHECK (ItemStatus IN (N'Open', N'In Progress', N'Completed', N'Cancelled'))
    );

    CREATE INDEX IX_TicketItems_Ticket
        ON dbo.TicketItems (TicketId, IsDeleted, CreatedAt DESC);
END
GO

SELECT TOP (5) *
FROM dbo.TicketItems
ORDER BY ItemId DESC;
GO
