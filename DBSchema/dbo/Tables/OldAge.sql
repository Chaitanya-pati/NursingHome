CREATE TABLE [dbo].[OldAge] (
    [Id]                  INT             IDENTITY (1, 1) NOT NULL,
    [AdmissionDate]       DATE            NULL,
    [PatientName]         NVARCHAR (100)  NULL,
    [Address]             NVARCHAR (MAX)  NULL,
    [Age]                 INT             NULL,
    [Condition]           NVARCHAR (100)  NULL,
    [CustomerName]        NVARCHAR (100)  NULL,
    [MobileNo]            NVARCHAR (20)   NULL,
    [TypesofServices]     NVARCHAR (100)  NULL,
    [RegistrationCharges] DECIMAL (10, 2) NULL,
    [MonthlyPayment]      DECIMAL (10, 2) NULL,
    [PeriodFrom]          DATE            NULL,
    [PeriodTo]            DATE            NULL,
    [IdProof]             NVARCHAR (MAX)  NULL,
    [PaymentStatus]       NVARCHAR (50)   NULL,
    [CreatedDate]         DATETIME        NULL,
    [UpdatedDate]         DATETIME        NULL,
    [SUser]               NVARCHAR (50)   NULL,
    CONSTRAINT [PK__OldAge__3214EC07242C81C6] PRIMARY KEY CLUSTERED ([Id] ASC)
);

