/*
  Add Full Name and Company Name to AspNetUsers for registration.
*/
SET NOCOUNT ON;

IF COL_LENGTH('dbo.AspNetUsers', 'FullName') IS NULL
    ALTER TABLE dbo.AspNetUsers ADD FullName NVARCHAR(200) NULL;

IF COL_LENGTH('dbo.AspNetUsers', 'CompanyName') IS NULL
    ALTER TABLE dbo.AspNetUsers ADD CompanyName NVARCHAR(200) NULL;

SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'AspNetUsers'
  AND COLUMN_NAME IN (N'FullName', N'CompanyName');
GO
