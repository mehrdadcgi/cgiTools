/*
  Track who last updated the ticket description and when.
*/
SET NOCOUNT ON;

IF COL_LENGTH('dbo.Tickets', 'DescriptionUpdatedByUserId') IS NULL
BEGIN
    ALTER TABLE dbo.Tickets
    ADD DescriptionUpdatedByUserId NVARCHAR(128) NULL;
END
GO

IF COL_LENGTH('dbo.Tickets', 'DescriptionUpdatedAt') IS NULL
BEGIN
    ALTER TABLE dbo.Tickets
    ADD DescriptionUpdatedAt DATETIME2(0) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Tickets_DescriptionUpdatedBy'
)
BEGIN
    ALTER TABLE dbo.Tickets
    ADD CONSTRAINT FK_Tickets_DescriptionUpdatedBy
        FOREIGN KEY (DescriptionUpdatedByUserId) REFERENCES dbo.AspNetUsers(Id);
END
GO

SELECT TOP (5) TicketId, TicketNumber, DescriptionUpdatedByUserId, DescriptionUpdatedAt
FROM dbo.Tickets
ORDER BY TicketId DESC;
GO
