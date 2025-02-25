using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.IncentivePayments
{
    [TestFixture]
    public class IncentivePaymentsTests
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [TestCase(15)]
        [TestCase(19)]
        public void Should_Return_No_IncentivePayments_When_Outside_Age_Range(int ageAtStartOfApprenticeship)
        {
            // Arrange
            var apprenticeshipStartDate = _fixture.Create<DateTime>();
            var apprenticeshipEndDate = apprenticeshipStartDate.AddYears(1);

            // Act
            var result = Calculations.IncentivePayments.Generate16to18IncentivePayments(ageAtStartOfApprenticeship, apprenticeshipStartDate, apprenticeshipEndDate);

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void Should_Return_90_Day_Incentives_For_Each_Party_When_Short_Apprenticeship()
        {
            // Arrange
            var ageAtStartOfApprenticeship = 17;
            var apprenticeshipStartDate = _fixture.Create<DateTime>();
            var apprenticeshipEndDate = apprenticeshipStartDate.AddMonths(6);

            var expectedIncentiveDate = apprenticeshipStartDate.AddDays(89);

            // Act
            var result = Calculations.IncentivePayments.Generate16to18IncentivePayments(ageAtStartOfApprenticeship, apprenticeshipStartDate, apprenticeshipEndDate);

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainEquivalentOf(new IncentivePayment
            {
                AcademicYear = expectedIncentiveDate.ToAcademicYear(),
                DueDate = expectedIncentiveDate,
                Amount = 500,
                DeliveryPeriod = expectedIncentiveDate.ToDeliveryPeriod(),
                IncentiveType = "ProviderIncentive"
            });
            result.Should().ContainEquivalentOf(new IncentivePayment
            {
                AcademicYear = expectedIncentiveDate.ToAcademicYear(),
                DueDate = expectedIncentiveDate,
                Amount = 500,
                DeliveryPeriod = expectedIncentiveDate.ToDeliveryPeriod(),
                IncentiveType = "EmployerIncentive"
            });
        }

        [Test]
        public void Should_Return_90_Day_and_365_Day_Incentives_For_Each_Party_When_Apprenticeship_Longer_Than_A_Year()
        {
            // Arrange
            var ageAtStartOfApprenticeship = 17;
            var apprenticeshipStartDate = _fixture.Create<DateTime>();
            var apprenticeshipEndDate = apprenticeshipStartDate.AddYears(1);

            var firstIncentiveDate = apprenticeshipStartDate.AddDays(89);
            var secondIncentiveDate = apprenticeshipStartDate.AddDays(364);

            // Act
            var result = Calculations.IncentivePayments.Generate16to18IncentivePayments(ageAtStartOfApprenticeship, apprenticeshipStartDate, apprenticeshipEndDate);

            // Assert
            result.Should().HaveCount(4);
            result.Should().ContainEquivalentOf(new IncentivePayment
            {
                AcademicYear = firstIncentiveDate.ToAcademicYear(),
                DueDate = firstIncentiveDate,
                Amount = 500,
                DeliveryPeriod = firstIncentiveDate.ToDeliveryPeriod(),
                IncentiveType = "ProviderIncentive"
            });
            result.Should().ContainEquivalentOf(new IncentivePayment
            {
                AcademicYear = firstIncentiveDate.ToAcademicYear(),
                DueDate = firstIncentiveDate,
                Amount = 500,
                DeliveryPeriod = firstIncentiveDate.ToDeliveryPeriod(),
                IncentiveType = "EmployerIncentive"
            });
            result.Should().ContainEquivalentOf(new IncentivePayment
            {
                AcademicYear = secondIncentiveDate.ToAcademicYear(),
                DueDate = secondIncentiveDate,
                Amount = 500,
                DeliveryPeriod = secondIncentiveDate.ToDeliveryPeriod(),
                IncentiveType = "ProviderIncentive"
            });
            result.Should().ContainEquivalentOf(new IncentivePayment
            {
                AcademicYear = secondIncentiveDate.ToAcademicYear(),
                DueDate = secondIncentiveDate,
                Amount = 500,
                DeliveryPeriod = secondIncentiveDate.ToDeliveryPeriod(),
                IncentiveType = "EmployerIncentive"
            });
        }
    }
}
