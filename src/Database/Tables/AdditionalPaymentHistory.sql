CREATE TABLE [Domain].[AdditionalPaymentHistory]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [EarningsProfileId] UNIQUEIDENTIFIER NOT NULL,
    [AcademicYear] SMALLINT NOT NULL, 
    [DeliveryPeriod] TINYINT NOT NULL,
    [Amount] DECIMAL(15,5) NOT NULL,
    [AdditionalPaymentType] NVARCHAR(20) NOT NULL,
    [DueDate] DATETIME NOT NULL
)