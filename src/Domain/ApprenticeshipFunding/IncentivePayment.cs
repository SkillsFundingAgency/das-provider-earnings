namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;

public class IncentivePayment
{
    public short AcademicYear { get; set; }
    public byte DeliveryPeriod { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public string IncentiveType { get; set; } = null!;
}