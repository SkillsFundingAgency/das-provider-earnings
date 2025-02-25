using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship
{
    public class EarningsProfile<T> where T : EarningsProfileModel, new()
    {
        protected T Model;

        public EarningsProfile() { }

        public EarningsProfile(T model)
        {
            Model = model;
        }

        public T GetModel()
        {
            return Model;
        }
    }

    public class EarningsProfile : EarningsProfile<EarningsProfileModel>
    {
        private List<Instalment> _instalments;

        public EarningsProfile(decimal onProgramTotal, List<Instalment> instalments, List<AdditionalPayment> additionalPayments, decimal completionPayment, Guid episodeKey)
        {
            Model = new EarningsProfileModel();
            Model.EarningsProfileId = Guid.NewGuid();
            Model.OnProgramTotal = onProgramTotal;
            Model.Instalments = instalments.Select(x => x.GetModel(Model.EarningsProfileId)).ToList();
            Model.AdditionalPayments = additionalPayments.Select(x => x.GetModel(Model.EarningsProfileId)).ToList();
            _instalments = instalments;
            Model.CompletionPayment = completionPayment;
            Model.EpisodeKey = episodeKey;
        }

        public EarningsProfile(EarningsProfileModel model)
        {
            Model = model;
            _instalments = model.Instalments?.Select(Instalment.Get).ToList();
        }

        public Guid EarningsProfileId => Model.EarningsProfileId;
        public decimal OnProgramTotal => Model.OnProgramTotal;
        public IReadOnlyCollection<Instalment> Instalments => new ReadOnlyCollection<Instalment>(_instalments);
        public IReadOnlyCollection<AdditionalPayment> AdditionalPayments => Model.AdditionalPayments.Select((AdditionalPayment.Get)).ToList().AsReadOnly();
        public decimal CompletionPayment => Model.CompletionPayment;

        public void AddInstalments(List<Instalment> instalments)
        {

        }

        public static EarningsProfile Get(EarningsProfileModel model)
        {
            return new EarningsProfile(model);
        }
    }
}