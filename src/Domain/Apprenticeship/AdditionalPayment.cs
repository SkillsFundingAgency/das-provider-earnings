using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class AdditionalPayment
{
    private AdditionalPaymentModel _model;

    public short AcademicYear => _model.AcademicYear;
    public byte DeliveryPeriod => _model.DeliveryPeriod;
    public decimal Amount => _model.Amount;
    public string AdditionalPaymentType => _model.AdditionalPaymentType;
    public DateTime DueDate => _model.DueDate;

    private AdditionalPayment(AdditionalPaymentModel model)
    {
        _model = model;
    }

    public AdditionalPayment(short academicYear, byte deliveryPeriod, decimal amount, DateTime dueDate, string incentiveType)
    {
        _model = new AdditionalPaymentModel
        {
            Key = Guid.NewGuid(),
            AcademicYear = academicYear,
            Amount = amount,
            DeliveryPeriod = deliveryPeriod,
            DueDate = dueDate,
            AdditionalPaymentType = incentiveType
        };
    }

    public AdditionalPaymentModel GetModel(Guid modelEarningsProfileId)
    {
        _model.EarningsProfileId = modelEarningsProfileId;
        return _model;
    }

    public static AdditionalPayment Get(AdditionalPaymentModel model)
    {
        return new AdditionalPayment(model);
    }
}