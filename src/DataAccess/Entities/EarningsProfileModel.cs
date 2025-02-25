using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

public abstract class EarningsProfileModelBase
{
    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid EarningsProfileId { get; set; }
    public Guid EpisodeKey { get; set; }
    public decimal OnProgramTotal { get; set; }
    public decimal CompletionPayment { get; set; }

    public EarningsProfileModelBase() { }

    public EarningsProfileModelBase(EarningsProfileModelBase original)
    {
        EarningsProfileId = original.EarningsProfileId;
        EpisodeKey = original.EpisodeKey;
        CompletionPayment = original.CompletionPayment;
        OnProgramTotal = original.OnProgramTotal;
    }
}

[Dapper.Contrib.Extensions.Table("Domain.EarningsProfile")]
[Table("EarningsProfile", Schema = "Domain")]
public class EarningsProfileModel : EarningsProfileModelBase
{
    public List<InstalmentModel> Instalments { get; set; } = null!;
    public List<AdditionalPaymentModel> AdditionalPayments { get; set; } = null!;
}

[Dapper.Contrib.Extensions.Table("Domain.EarningsProfileHistory")]
[Table("EarningsProfileHistory", Schema = "Domain")]
public class EarningsProfileHistoryModel : EarningsProfileModelBase
{
    public EarningsProfileHistoryModel(EarningsProfileModel original, DateTime supersededDate) : base(original)
    {
        SupersededDate = supersededDate;
        Instalments = original.Instalments.Select(x => new InstalmentHistoryModel(x, EarningsProfileId)).ToList();
        AdditionalPayments = original.AdditionalPayments
            .Select(x => new AdditionalPaymentHistoryModel(x, EarningsProfileId)).ToList();
    }

    public EarningsProfileHistoryModel() {}

    public List<InstalmentHistoryModel> Instalments { get; set; } = null!;
    public List<AdditionalPaymentHistoryModel> AdditionalPayments { get; set; } = null!;
    public DateTime SupersededDate { get; set; }
}