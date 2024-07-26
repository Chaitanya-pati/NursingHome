CREATE TABLE [dbo].[HomeNursing] (
    [Id]                  INT             IDENTITY (1, 1) NOT NULL,
    [AdmissionDate]       DATE            NULL,
    [PatientName]         NVARCHAR (100)  NULL,
    [Address]             NVARCHAR (MAX)  NULL,
    [Age]                 INT             NULL,
    [Condition]           NVARCHAR (MAX)  NULL,
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
    [fkHelperId]          INT             NULL,
    CONSTRAINT [PK__HomeNurs__3214EC0723E6127E] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK__HomeNursi__fkHel__30F848ED] FOREIGN KEY ([fkHelperId]) REFERENCES [dbo].[Helpers] ([Id])
);

