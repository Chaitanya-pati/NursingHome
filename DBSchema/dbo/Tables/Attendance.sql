CREATE TABLE [dbo].[Attendance] (
    [Id]                 INT            IDENTITY (1, 1) NOT NULL,
    [fkHelperId]         INT            NULL,
    [fkNursingId]        INT            NULL,
    [Date]               DATE           NULL,
    [Time]               FLOAT          NULL,
    [Description]        NVARCHAR (MAX) NULL,

    -- GPS Check-In
    [Latitude]           FLOAT          NULL,
    [Longitude]          FLOAT          NULL,
    [GpsAccuracy]        FLOAT          NULL,
    [CheckInTime]        DATETIME       NULL,
    [Address]            NVARCHAR (MAX) NULL,
    [Status]             NVARCHAR (50)  NULL,

    -- GPS Check-Out
    [CheckOutTime]       DATETIME       NULL,
    [CheckOutLatitude]   FLOAT          NULL,
    [CheckOutLongitude]  FLOAT          NULL,
    [CheckOutGpsAccuracy] FLOAT         NULL,
    [CheckOutAddress]    NVARCHAR (MAX) NULL,

    -- Manager Approval
    [TotalHours]         FLOAT          NULL,
    [ManagerRemarks]     NVARCHAR (MAX) NULL,
    [ApprovedBy]         NVARCHAR (100) NULL,
    [ApprovalTimestamp]  DATETIME       NULL,

    CONSTRAINT [PK__Attendan__3214EC071718A8AF] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK__Attendanc__fkHel__37A5467C] FOREIGN KEY ([fkHelperId])  REFERENCES [dbo].[Helpers]     ([Id]),
    CONSTRAINT [FK__Attendanc__fkNur__36B12243] FOREIGN KEY ([fkNursingId]) REFERENCES [dbo].[HomeNursing] ([Id])
);
