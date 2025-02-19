namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm36Data;
#pragma warning disable CS8618

public class GetFm36DataResponse : List<Apprenticeship>
{
    public GetFm36DataResponse()
    {
        
    }

    public GetFm36DataResponse(List<Apprenticeship> apprenticeships)
    {
        AddRange(apprenticeships); 
    }
}

public class Apprenticeship
{
    public long Ukprn { get; set; }
    public Guid Key { get; set; } = Guid.Empty;
    public List<Episode> Episodes { get; set; }
    public string FundingLineType { get; set; }
}

public class Episode
{
    public Guid Key { get; set; }
    public int NumberOfInstalments { get; set; }
    public List<Instalment> Instalments { get; set; }
    public List<AdditionalPayment> AdditionalPayments { get; set; }
    public decimal CompletionPayment { get; set; }
    public decimal OnProgramTotal { get; set; }
}

public class AdditionalPayment
{
    public short AcademicYear { get; set; }
    public byte DeliveryPeriod { get; set; }
    public decimal Amount { get; set; }
    public string AdditionalPaymentType { get; set; }
    public DateTime DueDate { get; set; }
}

public class Instalment
{
    public short AcademicYear { get; set; }
    public byte DeliveryPeriod { get; set; }
    public decimal Amount { get; set; }
}

#pragma warning restore CS8618