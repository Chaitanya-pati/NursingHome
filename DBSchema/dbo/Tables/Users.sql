CREATE TABLE [dbo].[Users] (
    [Id]                   INT            IDENTITY (1, 1) NOT NULL,
    [FirstName]            NVARCHAR (100) NULL,
    [LastName]             NVARCHAR (100) NULL,
    [UserName]             NVARCHAR (50)  NULL,
    [Password]             NVARCHAR (100) NULL,
    [Roles]                NVARCHAR (MAX) NULL,
    [MobileNo]             NVARCHAR (20)  NULL,
    [ImageString]          NVARCHAR (MAX) NULL,
    [IdProof]              NVARCHAR (MAX) NULL,
    [fkCountry]            INT            NULL,
    [fkState]              INT            NULL,
    [fkCity]               INT            NULL,
    [PinCode]              NVARCHAR (20)  NULL,
    [HighestQualification] NVARCHAR (100) NULL,
    CONSTRAINT [PK__Users__3214EC077557BDBB] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK__Users__fkCity__3D5E1FD2] FOREIGN KEY ([fkCity]) REFERENCES [dbo].[City] ([Id]),
    CONSTRAINT [FK__Users__fkState__3C69FB99] FOREIGN KEY ([fkState]) REFERENCES [dbo].[State] ([Id]),
    CONSTRAINT [FK__Users__HighestQu__3B75D760] FOREIGN KEY ([fkCountry]) REFERENCES [dbo].[Country] ([Id]),
    CONSTRAINT [UQ__Users__C9F284564C103849] UNIQUE NONCLUSTERED ([UserName] ASC)
);

