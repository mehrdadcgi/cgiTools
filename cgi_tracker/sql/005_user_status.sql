/*
  User approval workflow: Pending | Approved
*/
SET NOCOUNT ON;

IF COL_LENGTH('dbo.AspNetUsers', 'UserStatus') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers
    ADD UserStatus NVARCHAR(50) NOT NULL
        CONSTRAINT DF_AspNetUsers_UserStatus DEFAULT (N'Pending');
END
GO

-- Existing accounts can continue logging in
UPDATE dbo.AspNetUsers
SET UserStatus = N'Approved'
WHERE UserStatus IS NULL
   OR LTRIM(RTRIM(UserStatus)) = N''
   OR UserStatus NOT IN (N'Pending', N'Approved');
GO

SELECT Email, FullName, UserStatus
FROM dbo.AspNetUsers
ORDER BY Email;
GO
