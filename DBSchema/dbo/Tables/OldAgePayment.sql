CREATE TABLE [dbo].[OldAgePayment] (
    [Id]         INT             IDENTITY (1, 1) NOT NULL,
    [fkOldAgeId] INT             NULL,
    [Date]       DATE            NULL,
    [Amount]     DECIMAL (10, 2) NULL,
    CONSTRAINT [PK__OldAgePa__3214EC07CAC0EEE6] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK__OldAgePay__fkOld__2C3393D0] FOREIGN KEY ([fkOldAgeId]) REFERENCES [dbo].[OldAge] ([Id])
);

