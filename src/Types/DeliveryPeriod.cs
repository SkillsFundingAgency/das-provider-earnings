namespace SFA.DAS.Funding.ApprenticeshipEarnings.Types;

public class DeliveryPeriod
{
    public DeliveryPeriod(byte calendarMonth, short calenderYear, byte period, short academicYear, decimal learningAmount, string fundingLineType, string instalmentType)
    {
        CalendarMonth = calendarMonth;
        CalenderYear = calenderYear;
        Period = period;
        AcademicYear = academicYear;
        LearningAmount = learningAmount;
        FundingLineType = fundingLineType;
        InstalmentType = instalmentType;
    }

    public byte CalendarMonth { get; set; }
    public short CalenderYear { get; set; }
    public byte Period { get; set; }
    public short AcademicYear { get; set; }
    public decimal LearningAmount { get; set; }
    public string FundingLineType { get; set; }
    public string InstalmentType { get; set; }
}