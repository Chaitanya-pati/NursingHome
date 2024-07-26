CREATE TABLE [dbo].[Attendance] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [fkHelperId]  INT            NULL,
    [fkNursingId] INT            NULL,
    [Date]        DATE           NULL,
    [Time]        TIME (7)       NULL,
    [Description] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK__Attendan__3214EC071718A8AF] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK__Attendanc__fkHel__37A5467C] FOREIGN KEY ([fkHelperId]) REFERENCES [dbo].[Helpers] ([Id]),
    CONSTRAINT [FK__Attendanc__fkNur__36B12243] FOREIGN KEY ([fkNursingId]) REFERENCES [dbo].[HomeNursing] ([Id])
);

