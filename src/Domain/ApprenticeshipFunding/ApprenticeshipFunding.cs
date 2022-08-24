﻿namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding
{
    public class ApprenticeshipFunding
    {
        private const decimal AgreedPriceMultiplier = 0.8m;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;
        public decimal OnProgramTotalAmount { get; }
        public decimal CompletionPayment { get; }
        public decimal CappedAgreedPrice { get; set; }

        public ApprenticeshipFunding(decimal agreedPrice, DateTime startDate, DateTime endDate, decimal fundingBandMaximum)
        {
            _startDate = startDate;
            _endDate = endDate;
            CappedAgreedPrice = CalculateCappedAgreedPrice(fundingBandMaximum, agreedPrice);
            OnProgramTotalAmount = CalculateOnProgramTotalAmount(CappedAgreedPrice);
            CompletionPayment = CalculateCompletionPayment(CappedAgreedPrice);
        }

        private decimal CalculateCappedAgreedPrice(decimal fundingBandMaximum, decimal agreedPrice)
        {
            return Math.Min(fundingBandMaximum, agreedPrice);
        }

        private decimal CalculateCompletionPayment(decimal agreedPrice)
        {
            return agreedPrice - OnProgramTotalAmount;
        }

        private decimal CalculateOnProgramTotalAmount(decimal agreedPrice)
        {
            return agreedPrice * AgreedPriceMultiplier;
        }

        public List<Earning> GenerateEarnings()
        {
            var instalmentGenerator = new InstalmentsGenerator();
            var earnings = instalmentGenerator.Generate(OnProgramTotalAmount, _startDate, _endDate);
            return earnings;
        }
    }
}
