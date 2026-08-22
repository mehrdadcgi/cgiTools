/*
  Ticket allocated resource hours (1-100).
*/
SET NOCOUNT ON;

IF COL_LENGTH('dbo.Tickets', 'AllocatedHours') IS NULL
BEGIN
    ALTER TABLE dbo.Tickets
    ADD AllocatedHours INT NULL
        CONSTRAINT CK_Tickets_AllocatedHours CHECK (AllocatedHours IS NULL OR (AllocatedHours BETWEEN 1 AND 100));
END
GO

SELECT TOP (5) TicketId, TicketNumber, AllocatedHours
FROM dbo.Tickets
ORDER BY TicketId DESC;
GO
