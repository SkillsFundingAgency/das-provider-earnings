using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

public abstract class AdditionalPaymentModelBase
{
    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }
    public Guid EarningsProfileId { get; set; }
    public short AcademicYear { get; set; }
    public byte DeliveryPeriod { get; set; }
    [Precision(15, 5)]
    public decimal Amount { get; set; }
    public string AdditionalPaymentType { get; set; }

    public DateTime DueDate { get; set; }

    public AdditionalPaymentModelBase(AdditionalPaymentModelBase original, Guid earningsProfileId)
    {
        Key = original.Key;
        EarningsProfileId = earningsProfileId;
        Amount = original.Amount;
        DeliveryPeriod = original.DeliveryPeriod;
        AcademicYear = original.AcademicYear;
        AdditionalPaymentType = original.AdditionalPaymentType;
        DueDate = original.DueDate;
    }

    public AdditionalPaymentModelBase() { }
}

[Dapper.Contrib.Extensions.Table("Domain.AdditionalPayment")]
[Table("AdditionalPayment", Schema = "Domain")]
public class AdditionalPaymentModel : AdditionalPaymentModelBase
{

}

[Dapper.Contrib.Extensions.Table("Domain.AdditionalPaymentHistory")]
[Table("AdditionalPaymentHistory", Schema = "Domain")]
public class AdditionalPaymentHistoryModel : AdditionalPaymentModelBase
{
    public AdditionalPaymentHistoryModel(AdditionalPaymentModelBase original, Guid earningsProfileId) : base(original, earningsProfileId) { }
    public AdditionalPaymentHistoryModel() { }
}