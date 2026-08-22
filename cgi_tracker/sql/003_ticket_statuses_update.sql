/*
  Align TicketStatuses with the app workflow:
  New, Assigned to me, Assigned to QA, UAT, Completed, Invalid, Closed
*/
SET NOCOUNT ON;

MERGE dbo.TicketStatuses AS target
USING (VALUES
    (N'New',           N'New',             1, 0),
    (N'AssignedToMe',  N'Assigned to me',  2, 0),
    (N'AssignedToQA',  N'Assigned to QA',  3, 0),
    (N'UAT',           N'UAT',             4, 0),
    (N'Completed',     N'Completed',       5, 1),
    (N'Invalid',       N'Invalid',         6, 1),
    (N'Closed',        N'Closed',          7, 1)
) AS source (StatusCode, StatusName, DisplayOrder, IsFinal)
ON target.StatusCode = source.StatusCode
WHEN MATCHED THEN
    UPDATE SET
        StatusName = source.StatusName,
        DisplayOrder = source.DisplayOrder,
        IsFinal = source.IsFinal,
        IsActive = 1
WHEN NOT MATCHED BY TARGET THEN
    INSERT (StatusCode, StatusName, DisplayOrder, IsFinal, IsActive)
    VALUES (source.StatusCode, source.StatusName, source.DisplayOrder, source.IsFinal, 1);

-- Rename legacy Ready for UAT -> UAT if still present and UAT already exists separately
IF EXISTS (SELECT 1 FROM dbo.TicketStatuses WHERE StatusCode = N'ReadyForUAT')
   AND EXISTS (SELECT 1 FROM dbo.TicketStatuses WHERE StatusCode = N'UAT')
BEGIN
    DECLARE @OldUat INT = (SELECT StatusId FROM dbo.TicketStatuses WHERE StatusCode = N'ReadyForUAT');
    DECLARE @NewUat INT = (SELECT StatusId FROM dbo.TicketStatuses WHERE StatusCode = N'UAT');

    UPDATE dbo.Tickets SET StatusId = @NewUat WHERE StatusId = @OldUat;
    UPDATE dbo.TicketStatusHistory SET FromStatusId = @NewUat WHERE FromStatusId = @OldUat;
    UPDATE dbo.TicketStatusHistory SET ToStatusId = @NewUat WHERE ToStatusId = @OldUat;
    UPDATE dbo.TicketStatuses SET IsActive = 0 WHERE StatusId = @OldUat;
END
ELSE IF EXISTS (SELECT 1 FROM dbo.TicketStatuses WHERE StatusCode = N'ReadyForUAT')
BEGIN
    UPDATE dbo.TicketStatuses
    SET StatusCode = N'UAT', StatusName = N'UAT', DisplayOrder = 4, IsFinal = 0, IsActive = 1
    WHERE StatusCode = N'ReadyForUAT';
END

SELECT StatusId, StatusCode, StatusName, DisplayOrder, IsFinal, IsActive
FROM dbo.TicketStatuses
ORDER BY DisplayOrder;
GO
