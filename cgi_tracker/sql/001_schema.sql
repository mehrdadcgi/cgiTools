/*
  CGI Ticket Manager — SQL Server schema
  Database: cgi_tracker (or your chosen name)
  Users: Client, Support, Admin — username/password login
  Attachments: binary in S3; DB stores location only
*/

-- USE cgi_tracker;
-- GO

CREATE TABLE Roles (
    RoleId        INT IDENTITY(1,1) PRIMARY KEY,
    RoleName      NVARCHAR(50)  NOT NULL,
    Description   NVARCHAR(200) NULL,
    IsActive      BIT           NOT NULL CONSTRAINT DF_Roles_IsActive DEFAULT (1),
    CONSTRAINT UQ_Roles_RoleName UNIQUE (RoleName)
);

CREATE TABLE Users (
    UserId        INT IDENTITY(1,1) PRIMARY KEY,
    RoleId        INT           NOT NULL,
    UserName      NVARCHAR(100) NOT NULL,
    Email         NVARCHAR(256) NOT NULL,
    PasswordHash  NVARCHAR(500) NOT NULL,
    FirstName     NVARCHAR(100) NOT NULL,
    LastName      NVARCHAR(100) NOT NULL,
    IsActive      BIT           NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    CreatedAt     DATETIME2(0)  NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
    LastLoginAt   DATETIME2(0)  NULL,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId),
    CONSTRAINT UQ_Users_UserName UNIQUE (UserName),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);

CREATE TABLE TicketStatuses (
    StatusId      INT IDENTITY(1,1) PRIMARY KEY,
    StatusCode    NVARCHAR(50)  NOT NULL,
    StatusName    NVARCHAR(100) NOT NULL,
    DisplayOrder  INT           NOT NULL,
    IsFinal       BIT           NOT NULL CONSTRAINT DF_TicketStatuses_IsFinal DEFAULT (0),
    IsActive      BIT           NOT NULL CONSTRAINT DF_TicketStatuses_IsActive DEFAULT (1),
    CONSTRAINT UQ_TicketStatuses_StatusCode UNIQUE (StatusCode)
);

CREATE TABLE Tickets (
    TicketId          INT IDENTITY(1,1) PRIMARY KEY,
    TicketNumber      NVARCHAR(30)   NOT NULL,
    Title             NVARCHAR(200)  NOT NULL,
    Description       NVARCHAR(MAX)  NOT NULL,
    StatusId          INT            NOT NULL,
    CreatedByUserId   INT            NOT NULL,
    AssignedToUserId  INT            NULL,
    CreatedAt         DATETIME2(0)   NOT NULL CONSTRAINT DF_Tickets_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt         DATETIME2(0)   NOT NULL CONSTRAINT DF_Tickets_UpdatedAt DEFAULT (SYSUTCDATETIME()),
    CompletedAt       DATETIME2(0)   NULL,
    IsDeleted         BIT            NOT NULL CONSTRAINT DF_Tickets_IsDeleted DEFAULT (0),
    CONSTRAINT UQ_Tickets_TicketNumber UNIQUE (TicketNumber),
    CONSTRAINT FK_Tickets_Status FOREIGN KEY (StatusId) REFERENCES TicketStatuses(StatusId),
    CONSTRAINT FK_Tickets_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Tickets_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES Users(UserId)
);

CREATE TABLE TicketAttachments (
    AttachmentId      INT IDENTITY(1,1) PRIMARY KEY,
    TicketId          INT            NOT NULL,
    UploadedByUserId  INT            NOT NULL,
    FileName          NVARCHAR(255)  NOT NULL,
    FileExtension     NVARCHAR(20)   NOT NULL,
    ContentType       NVARCHAR(100)  NOT NULL,
    FileSizeBytes     BIGINT         NOT NULL,
    S3Bucket          NVARCHAR(100)  NOT NULL,
    S3Key             NVARCHAR(500)  NOT NULL,
    S3Region          NVARCHAR(50)   NULL,
    AttachmentType    NVARCHAR(20)   NOT NULL, -- SqlScript | Document
    UploadedAt        DATETIME2(0)   NOT NULL CONSTRAINT DF_TicketAttachments_UploadedAt DEFAULT (SYSUTCDATETIME()),
    IsDeleted         BIT            NOT NULL CONSTRAINT DF_TicketAttachments_IsDeleted DEFAULT (0),
    CONSTRAINT FK_TicketAttachments_Ticket FOREIGN KEY (TicketId) REFERENCES Tickets(TicketId),
    CONSTRAINT FK_TicketAttachments_User FOREIGN KEY (UploadedByUserId) REFERENCES Users(UserId),
    CONSTRAINT CK_TicketAttachments_Type CHECK (AttachmentType IN (N'SqlScript', N'Document')),
    CONSTRAINT CK_TicketAttachments_Ext CHECK (
        FileExtension IN (N'.sql', N'.txt', N'.doc', N'.docx', N'.pdf', N'.zip')
    ),
    CONSTRAINT UQ_TicketAttachments_S3 UNIQUE (S3Bucket, S3Key)
);

CREATE TABLE TicketStatusHistory (
    HistoryId         BIGINT IDENTITY(1,1) PRIMARY KEY,
    TicketId          INT           NOT NULL,
    FromStatusId      INT           NULL,
    ToStatusId        INT           NOT NULL,
    ChangedByUserId   INT           NOT NULL,
    AssignedToUserId  INT           NULL,
    Comment           NVARCHAR(500) NULL,
    ChangedAt         DATETIME2(0)  NOT NULL CONSTRAINT DF_TicketStatusHistory_ChangedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_TSH_Ticket FOREIGN KEY (TicketId) REFERENCES Tickets(TicketId),
    CONSTRAINT FK_TSH_FromStatus FOREIGN KEY (FromStatusId) REFERENCES TicketStatuses(StatusId),
    CONSTRAINT FK_TSH_ToStatus FOREIGN KEY (ToStatusId) REFERENCES TicketStatuses(StatusId),
    CONSTRAINT FK_TSH_ChangedBy FOREIGN KEY (ChangedByUserId) REFERENCES Users(UserId),
    CONSTRAINT FK_TSH_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES Users(UserId)
);

CREATE INDEX IX_Tickets_OpenList
    ON Tickets (IsDeleted, UpdatedAt DESC)
    INCLUDE (TicketNumber, Title, StatusId, CreatedByUserId, AssignedToUserId);

CREATE INDEX IX_TicketAttachments_Ticket
    ON TicketAttachments (TicketId, IsDeleted);

CREATE INDEX IX_TicketStatusHistory_Ticket
    ON TicketStatusHistory (TicketId, ChangedAt DESC);
GO

/* Seed roles */
INSERT INTO Roles (RoleName, Description) VALUES
    (N'Client',  N'Creates and views tickets'),
    (N'Support', N'Works tickets, changes status, uploads attachments'),
    (N'Admin',   N'Full access including user management');

/* Seed statuses */
INSERT INTO TicketStatuses (StatusCode, StatusName, DisplayOrder, IsFinal) VALUES
    (N'New',          N'New',            1, 0),
    (N'AssignedToMe', N'Assigned to me', 2, 0),
    (N'AssignedToQA', N'Assigned to QA', 3, 0),
    (N'UAT',          N'UAT',            4, 0),
    (N'Completed',    N'Completed',      5, 1),
    (N'Invalid',      N'Invalid',        6, 1),
    (N'Closed',       N'Closed',         7, 1);
GO

/*
  Manage open tickets (Support):
  SELECT TOP 50 t.*
  FROM Tickets t
  INNER JOIN TicketStatuses s ON s.StatusId = t.StatusId
  WHERE t.IsDeleted = 0 AND s.IsFinal = 0
  ORDER BY t.UpdatedAt DESC;
*/
