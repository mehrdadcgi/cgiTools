/*
  Optional short description for ticket attachments (support notes on files).
*/
SET NOCOUNT ON;

IF COL_LENGTH('dbo.TicketAttachments', 'Description') IS NULL
BEGIN
    ALTER TABLE dbo.TicketAttachments
    ADD Description NVARCHAR(500) NULL;
END
GO

SELECT TOP (5) AttachmentId, FileName, Description, UploadedAt
FROM dbo.TicketAttachments
ORDER BY AttachmentId DESC;
GO
