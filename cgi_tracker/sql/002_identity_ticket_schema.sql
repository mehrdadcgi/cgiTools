USE cgi_tracker;
GO

/* The ticket tables are empty. Rebuild them to use ASP.NET Identity string user IDs. */
IF EXISTS (SELECT 1 FROM dbo.TicketStatusHistory) OR
   EXISTS (SELECT 1 FROM dbo.TicketAttachments) OR
   EXISTS (SELECT 1 FROM dbo.Tickets)
    THROW 51000, 'Ticket tables contain data; migration stopped to prevent data loss.', 1;
GO

DROP TABLE IF EXISTS dbo.TicketStatusHistory;
DROP TABLE IF EXISTS dbo.TicketAttachments;
DROP TABLE IF EXISTS dbo.Tickets;
DROP TABLE IF EXISTS dbo.TicketStatuses;
GO

CREATE TABLE dbo.TicketStatuses (
    StatusId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TicketStatuses PRIMARY KEY,
    StatusCode    NVARCHAR(50) NOT NULL CONSTRAINT UQ_TicketStatuses_StatusCode UNIQUE,
    StatusName    NVARCHAR(100) NOT NULL,
    DisplayOrder  INT NOT NULL,
    IsFinal       BIT NOT NULL CONSTRAINT DF_TicketStatuses_IsFinal DEFAULT (0),
    IsActive      BIT NOT NULL CONSTRAINT DF_TicketStatuses_IsActive DEFAULT (1)
);

CREATE TABLE dbo.Tickets (
    TicketId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Tickets PRIMARY KEY,
    TicketNumber      NVARCHAR(30) NOT NULL CONSTRAINT UQ_Tickets_TicketNumber UNIQUE,
    Title             NVARCHAR(200) NOT NULL,
    Description       NVARCHAR(MAX) NOT NULL,
    StatusId          INT NOT NULL,
    CreatedByUserId   NVARCHAR(128) NOT NULL,
    AssignedToUserId  NVARCHAR(128) NULL,
    CreatedAt         DATETIME2(0) NOT NULL CONSTRAINT DF_Tickets_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt         DATETIME2(0) NOT NULL CONSTRAINT DF_Tickets_UpdatedAt DEFAULT (SYSUTCDATETIME()),
    CompletedAt       DATETIME2(0) NULL,
    IsDeleted         BIT NOT NULL CONSTRAINT DF_Tickets_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Tickets_Status FOREIGN KEY (StatusId) REFERENCES dbo.TicketStatuses(StatusId),
    CONSTRAINT FK_Tickets_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES dbo.AspNetUsers(Id),
    CONSTRAINT FK_Tickets_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES dbo.AspNetUsers(Id)
);

CREATE TABLE dbo.TicketAttachments (
    AttachmentId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TicketAttachments PRIMARY KEY,
    TicketId          INT NOT NULL,
    UploadedByUserId  NVARCHAR(128) NOT NULL,
    FileName          NVARCHAR(255) NOT NULL,
    FileExtension     NVARCHAR(20) NOT NULL,
    ContentType       NVARCHAR(100) NOT NULL,
    FileSizeBytes     BIGINT NOT NULL,
    S3Bucket          NVARCHAR(100) NOT NULL,
    S3Key             NVARCHAR(500) NOT NULL,
    S3Region          NVARCHAR(50) NULL,
    AttachmentType    NVARCHAR(20) NOT NULL,
    UploadedAt        DATETIME2(0) NOT NULL CONSTRAINT DF_TicketAttachments_UploadedAt DEFAULT (SYSUTCDATETIME()),
    IsDeleted         BIT NOT NULL CONSTRAINT DF_TicketAttachments_IsDeleted DEFAULT (0),
    CONSTRAINT FK_TicketAttachments_Ticket FOREIGN KEY (TicketId) REFERENCES dbo.Tickets(TicketId),
    CONSTRAINT FK_TicketAttachments_User FOREIGN KEY (UploadedByUserId) REFERENCES dbo.AspNetUsers(Id),
    CONSTRAINT CK_TicketAttachments_Type CHECK (AttachmentType IN (N'SqlScript', N'Document')),
    CONSTRAINT CK_TicketAttachments_Ext CHECK (
        FileExtension IN (N'.sql', N'.txt', N'.doc', N'.docx', N'.pdf', N'.zip')
    ),
    CONSTRAINT UQ_TicketAttachments_S3 UNIQUE (S3Bucket, S3Key)
);

CREATE TABLE dbo.TicketStatusHistory (
    HistoryId         BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TicketStatusHistory PRIMARY KEY,
    TicketId          INT NOT NULL,
    FromStatusId      INT NULL,
    ToStatusId        INT NOT NULL,
    ChangedByUserId   NVARCHAR(128) NOT NULL,
    AssignedToUserId  NVARCHAR(128) NULL,
    Comment           NVARCHAR(500) NULL,
    ChangedAt         DATETIME2(0) NOT NULL CONSTRAINT DF_TicketStatusHistory_ChangedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_TSH_Ticket FOREIGN KEY (TicketId) REFERENCES dbo.Tickets(TicketId),
    CONSTRAINT FK_TSH_FromStatus FOREIGN KEY (FromStatusId) REFERENCES dbo.TicketStatuses(StatusId),
    CONSTRAINT FK_TSH_ToStatus FOREIGN KEY (ToStatusId) REFERENCES dbo.TicketStatuses(StatusId),
    CONSTRAINT FK_TSH_ChangedBy FOREIGN KEY (ChangedByUserId) REFERENCES dbo.AspNetUsers(Id),
    CONSTRAINT FK_TSH_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES dbo.AspNetUsers(Id)
);

CREATE INDEX IX_Tickets_OpenList ON dbo.Tickets (IsDeleted, UpdatedAt DESC)
    INCLUDE (TicketNumber, Title, StatusId, CreatedByUserId, AssignedToUserId);
CREATE INDEX IX_TicketAttachments_Ticket ON dbo.TicketAttachments (TicketId, IsDeleted);
CREATE INDEX IX_TicketStatusHistory_Ticket ON dbo.TicketStatusHistory (TicketId, ChangedAt DESC);
GO

INSERT dbo.TicketStatuses (StatusCode, StatusName, DisplayOrder, IsFinal) VALUES
    (N'New', N'New', 1, 0),
    (N'AssignedToMe', N'Assigned to me', 2, 0),
    (N'AssignedToQA', N'Assigned to QA', 3, 0),
    (N'UAT', N'UAT', 4, 0),
    (N'Completed', N'Completed', 5, 1),
    (N'Invalid', N'Invalid', 6, 1),
    (N'Closed', N'Closed', 7, 1);

DECLARE @Roles TABLE (Id NVARCHAR(128), Name NVARCHAR(256));
INSERT @Roles VALUES
    (CONVERT(NVARCHAR(128), NEWID()), N'Client'),
    (CONVERT(NVARCHAR(128), NEWID()), N'Support'),
    (CONVERT(NVARCHAR(128), NEWID()), N'Admin');

INSERT dbo.AspNetRoles (Id, Name)
SELECT r.Id, r.Name
FROM @Roles r
WHERE NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles x WHERE x.Name = r.Name);

/* The first existing template account becomes Admin so the app can be configured. */
DECLARE @FirstUserId NVARCHAR(128) = (SELECT TOP (1) Id FROM dbo.AspNetUsers ORDER BY Id);
DECLARE @AdminRoleId NVARCHAR(128) = (SELECT Id FROM dbo.AspNetRoles WHERE Name = N'Admin');
IF @FirstUserId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM dbo.AspNetUserRoles WHERE UserId = @FirstUserId AND RoleId = @AdminRoleId
)
    INSERT dbo.AspNetUserRoles (UserId, RoleId) VALUES (@FirstUserId, @AdminRoleId);
GO
