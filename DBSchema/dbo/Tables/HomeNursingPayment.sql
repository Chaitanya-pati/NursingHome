CREATE TABLE [dbo].[HomeNursingPayment] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [fkNursingId] INT             NULL,
    [Date]        DATE            NULL,
    [Amount]      DECIMAL (10, 2) NULL,
    CONSTRAINT [PK__HomeNurs__3214EC0771EF2C0D] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK__HomeNursi__fkNur__33D4B598] FOREIGN KEY ([fkNursingId]) REFERENCES [dbo].[HomeNursing] ([Id])
);

