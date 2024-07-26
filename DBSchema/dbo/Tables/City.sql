CREATE TABLE [dbo].[City] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [Name]          NVARCHAR (100) NOT NULL,
    [OfficeAddress] NVARCHAR (MAX) NULL,
    [MobileNumbers] NVARCHAR (100) NULL,
    [Email]         NVARCHAR (100) NULL,
    [ImageBase64]   NVARCHAR (MAX) NULL,
    CONSTRAINT [PK__City__3214EC0770B7DB06] PRIMARY KEY CLUSTERED ([Id] ASC)
);

