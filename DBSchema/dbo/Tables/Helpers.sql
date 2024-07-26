CREATE TABLE [dbo].[Helpers] (
    [Id]               INT             IDENTITY (1, 1) NOT NULL,
    [Name]             NVARCHAR (100)  NULL,
    [Image]            NVARCHAR (MAX)  NULL,
    [DateOfBirth]      DATE            NULL,
    [ParentName]       NVARCHAR (100)  NULL,
    [MaritalStatus]    NVARCHAR (20)   NULL,
    [PermanentAddress] NVARCHAR (MAX)  NULL,
    [PresentAddress]   NVARCHAR (MAX)  NULL,
    [Education]        NVARCHAR (MAX)  NULL,
    [LanguagesKnown]   NVARCHAR (100)  NULL,
    [Experience]       NVARCHAR (MAX)  NULL,
    [IdProof]          NVARCHAR (MAX)  NULL,
    [Salary]           DECIMAL (10, 2) NULL,
    [Reference]        NVARCHAR (MAX)  NULL,
    [FamilyMembers]    NVARCHAR (MAX)  NULL,
    CONSTRAINT [PK__Helpers__3214EC077D723D46] PRIMARY KEY CLUSTERED ([Id] ASC)
);

