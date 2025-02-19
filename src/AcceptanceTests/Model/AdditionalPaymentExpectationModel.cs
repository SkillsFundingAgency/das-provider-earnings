namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model
{
    public class AdditionalPaymentExpectationModel
    {
        public string Type { get; set; }
        public int CalendarMonth { get; set; }
        public int CalendarYear { get; set; }
        public decimal Amount { get; set; }
    }
}
