using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Enums;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class RecalculateEarningsStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;
    private readonly Random _random = new();

    private ApprenticeshipCreatedEvent? _apprenticeshipCreatedEvent;
    private ApprenticeshipPriceChangedEvent? _apprenticeshipPriceChangedEvent;
    private ApprenticeshipStartDateChangedEvent? _startDateChangedEvent;
    private ApprenticeshipWithdrawnEvent? _apprenticeshipWithdrawnEvent;

    #region Test Values
    private readonly DateTime _dateOfBirth = new DateTime(2000, 1, 1);
    private readonly int _ageAtStartOfApprenticeship = 21;

    private readonly DateTime _startDate = new DateTime(2019, 09, 01);
    private readonly DateTime _endDate = new DateTime(2022, 1, 1);
    private int _expectedNumberOfInstalments = 28;

    private readonly DateTime _startDateEarlierThanOriginal = new DateTime(2019, 08, 15);
    private readonly int _newExpectedNumberOfInstalmentsForEarlierStartDate = 29;
    private readonly DateTime _startDateLaterThanOriginal = new DateTime(2020, 01, 08);
    private readonly int _newExpectedNumberOfInstalmentsForLaterStartDate = 24;
    private readonly DateTime _startDateInNextAcademicYearToOriginal = new DateTime(2021, 12, 30);
    private readonly int _newExpectedNumberOfInstalmentsForStartDateInNextAcademicYear = 1;

    private readonly DateTime _defaultCurrentDateTime = new DateTime(2020, 02, 01);

    private readonly DateTime _changeRequestDate = new DateTime(2020, 1, 1);
    private readonly DateTime _effectiveFromDate = new DateTime(2020, 2, 1);

    private readonly int _originalPrice = 15000;
    private readonly int _fundingBandMaximum = 25000;

    private readonly int _newTrainingPrice = 17000;
    private readonly int _newAssessmentPrice = 3000;
    private readonly int _newTrainingPriceAboveBandMax = 26000;

    private readonly Guid _priceKey = Guid.NewGuid();
    private readonly Guid _priceChangePriceKey = Guid.NewGuid();
    private readonly Guid _episodeKey = Guid.NewGuid();


	private EarningsProfileModel _originalEarningsProfile;
    private ApprenticeshipModel? _updatedApprenticeshipEntity;

    #endregion

    public RecalculateEarningsStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
        TestSystemClock.SetDateTime(_defaultCurrentDateTime);
    }


    #region Arrange
    [Given("an apprenticeship has been created")]
    public void ApprenticeshipCreated()
    {
        _apprenticeshipCreatedEvent = new ApprenticeshipCreatedEvent
        {
            ApprenticeshipKey = Guid.NewGuid(),
            Uln = _random.Next().ToString(),
            ApprovalsApprenticeshipId = 120,
            DateOfBirth = _dateOfBirth,
            Episode = new SFA.DAS.Apprenticeships.Types.ApprenticeshipEpisode
            {
                Prices = new List<ApprenticeshipEpisodePrice>
                {
                    new()
                    {
                        Key = _priceKey,
                        StartDate = _startDate,
                        EndDate = _endDate,
                        FundingBandMaximum = _fundingBandMaximum,
                        TotalPrice = 15000
                    }
                },
                FundingType = Apprenticeships.Enums.FundingType.Levy,
                LegalEntityName = "MyTrawler",
                Key = _episodeKey,
                EmployerAccountId = 114,
                Ukprn = 116,
                TrainingCode = "AbleSeafarer",
                FundingEmployerAccountId = null,
                AgeAtStartOfApprenticeship = _ageAtStartOfApprenticeship,
                FundingPlatform = Apprenticeships.Enums.FundingPlatform.DAS,
            }
        };

        _apprenticeshipPriceChangedEvent = new ApprenticeshipPriceChangedEvent
        {
            ApprenticeshipKey = _apprenticeshipCreatedEvent.ApprenticeshipKey,
            ApprenticeshipId = 123,
            EffectiveFromDate = _effectiveFromDate,
            ApprovedBy = ApprovedBy.Employer,
            ApprovedDate = _changeRequestDate,
            Episode = new SFA.DAS.Apprenticeships.Types.ApprenticeshipEpisode
            {
                Key = _episodeKey,
                Prices = new List<ApprenticeshipEpisodePrice>
                {
                    new()
                    {
                        Key = _priceKey,
                        StartDate = _startDate,
                        EndDate = _effectiveFromDate.AddDays(-1),
                        FundingBandMaximum = _fundingBandMaximum,
                        TotalPrice = 15000
                    },
                    new()
                    {
                        Key = _priceChangePriceKey,
                        TrainingPrice = _newTrainingPrice,
                        EndPointAssessmentPrice = _newAssessmentPrice,
                        StartDate = _effectiveFromDate,
                        EndDate = _endDate,
                        FundingBandMaximum = _fundingBandMaximum,
                        TotalPrice = _newTrainingPrice + _newAssessmentPrice
                    }
                },
                EmployerAccountId = _apprenticeshipCreatedEvent.Episode.EmployerAccountId,
                Ukprn = 123,
                LegalEntityName = "Smiths",
                TrainingCode = "AbleSeafarer",
                FundingEmployerAccountId = null,
                AgeAtStartOfApprenticeship = _ageAtStartOfApprenticeship,
                FundingPlatform = Apprenticeships.Enums.FundingPlatform.DAS,
            }
        };

        _startDateChangedEvent = new ApprenticeshipStartDateChangedEvent
        {
            ApprenticeshipKey = _apprenticeshipCreatedEvent.ApprenticeshipKey,
            ApprenticeshipId = 123,
            ApprovedDate = _changeRequestDate,
            ProviderApprovedBy = "",
            EmployerApprovedBy = "",
            Initiator = "",
            StartDate = _startDate,
            Episode = new SFA.DAS.Apprenticeships.Types.ApprenticeshipEpisode
            {
                Key = _episodeKey,
                Prices = new List<ApprenticeshipEpisodePrice>
                {
                    new()
                    {
                        Key = _priceKey,
                        StartDate = _startDate,
                        EndDate = _endDate,
                        TotalPrice = 15000,
                        FundingBandMaximum = _fundingBandMaximum
                    }
                },
                EmployerAccountId = _apprenticeshipCreatedEvent.Episode.EmployerAccountId,
                Ukprn = 123,
                LegalEntityName = "Smiths",
                TrainingCode = "AbleSeafarer",
                FundingEmployerAccountId = null,
                AgeAtStartOfApprenticeship = _ageAtStartOfApprenticeship,
                FundingPlatform = Apprenticeships.Enums.FundingPlatform.DAS,
            }
        };

        _apprenticeshipWithdrawnEvent = new ApprenticeshipWithdrawnEvent
        { 
            ApprenticeshipId = 123,
            ApprenticeshipKey = _apprenticeshipCreatedEvent.ApprenticeshipKey,
            Reason = "Withdrawal Test"
        };
    }

    [Given("the total price is below or at the funding band maximum")]
    public void SetTotalBelowBandMaximum()
    {
        _apprenticeshipPriceChangedEvent!.Episode.Prices.Last().TotalPrice = _newTrainingPrice + _newAssessmentPrice;
        _apprenticeshipPriceChangedEvent!.Episode.Prices.Last().TrainingPrice = _newTrainingPrice;
        _apprenticeshipPriceChangedEvent!.Episode.Prices.Last().EndPointAssessmentPrice = _newAssessmentPrice;
    }

    [Given("the price change request is for a new total price above the funding band maximum")]
    public void SetTotalAboveBandMaximum()
    {
        _apprenticeshipPriceChangedEvent!.Episode.Prices.Last().TotalPrice = _newTrainingPriceAboveBandMax + _newAssessmentPrice;
        _apprenticeshipPriceChangedEvent!.Episode.Prices.Last().TrainingPrice = _newTrainingPriceAboveBandMax;
        _apprenticeshipPriceChangedEvent!.Episode.Prices.Last().EndPointAssessmentPrice = _newAssessmentPrice;
    }

    [Given("a price change request was sent before the end of R14 of the current academic year")]
    public void SetPriceChangeApprovedDate()
    {
        _apprenticeshipPriceChangedEvent!.ApprovedDate = _changeRequestDate;
    }

    [Given("the price change request is for a new total price up to or at the funding band maximum")]
    public void SetPriceChange()
    {
        _apprenticeshipPriceChangedEvent!.Episode.Prices.Last().TotalPrice = _newTrainingPrice + _newAssessmentPrice;
        _apprenticeshipPriceChangedEvent!.Episode.Prices.Last().EndPointAssessmentPrice = _newAssessmentPrice;
        _apprenticeshipPriceChangedEvent!.Episode.Prices.Last().TrainingPrice = _newTrainingPrice;
    }

    [Given("a start date change request was sent before the end of R14 of the current academic year")]
    public void SetStartDateChangeApprovedDate()
    {
	    _startDateChangedEvent!.ApprovedDate = _changeRequestDate;
    }

    [Given("the new start date is earlier than, and in the same academic year, as the current start date")]
    public void SetEarlierStartDateChange()
    {
	    _startDateChangedEvent!.Episode.Prices.First().StartDate = _startDateEarlierThanOriginal;
        _startDateChangedEvent!.StartDate = _startDateEarlierThanOriginal;
        _expectedNumberOfInstalments = _newExpectedNumberOfInstalmentsForEarlierStartDate;
    }

    [Given("the new start date is later than, and in the same academic year, as the current start date")]
    public void SetLaterStartDateChangeInSameAcademicYear()
    {
        _startDateChangedEvent!.Episode.Prices.First().StartDate = _startDateLaterThanOriginal;
        _startDateChangedEvent!.StartDate = _startDateLaterThanOriginal;
        _expectedNumberOfInstalments = _newExpectedNumberOfInstalmentsForLaterStartDate;
    }

    [Given("the new start date is in the next academic year to the current start date")]
    public void SetLaterStartDateChangeInNextAcademicYear()
    {
        _startDateChangedEvent!.Episode.Prices.First().StartDate = _startDateInNextAcademicYearToOriginal;
        _startDateChangedEvent!.StartDate = _startDateInNextAcademicYearToOriginal;
        _expectedNumberOfInstalments = _newExpectedNumberOfInstalmentsForStartDateInNextAcademicYear;
    }

    [Given(@"there are (.*) earnings")]
    public void SetAgreedPriceAndDuration(int months)
    {
        var startDate = _apprenticeshipCreatedEvent!.Episode.Prices.First().StartDate;
        var endDate = startDate.AddMonths(months);
        _apprenticeshipCreatedEvent.Episode.Prices.First().EndDate = endDate;

        //  These values may get updated in the 'And' clauses
        _startDateChangedEvent!.Episode.Prices.First().StartDate = startDate;
        _startDateChangedEvent!.Episode.Prices.First().EndDate = endDate;
    }

    [Given(@"the (.*) date has been moved (.*) months (.*)")]
    public void AdjustDate(string field, int months, string action)
    {
        var monthChange = action == "earlier" ? -months : months;
        switch(field)
        {
            case "start":
                _startDateChangedEvent!.Episode.Prices.First().StartDate = _apprenticeshipCreatedEvent!.Episode.Prices.First().StartDate.AddMonths(monthChange);
                _startDateChangedEvent!.StartDate = _apprenticeshipCreatedEvent!.Episode.Prices.First().StartDate.AddMonths(monthChange);
                break;
            case "end":
                _startDateChangedEvent!.Episode.Prices.First().EndDate = _apprenticeshipCreatedEvent!.Episode.Prices.First().EndDate.AddMonths(monthChange);
                break;
        }
    }

    [Given(@"the earnings for the apprenticeship are calculated")]
    public async Task PublishApprenticeshipCreatedEvent()
    {
        await _testContext.TestFunction.PublishEvent(_apprenticeshipCreatedEvent);
        await WaitHelper.WaitForItAsync(async() => await EnsureApprenticeshipEntityCreated(), "Failed to publish create");
    }

    #endregion

    #region Act
    [When("the price change is approved by the other party before the end of year")]
    public async Task PublishPriceChangeEvents()
    {
        await _testContext.TestFunction.PublishEvent(_apprenticeshipPriceChangedEvent);
        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish priceChange");
    }

	[When("the start date change is approved")]
	public async Task PublishStartDateChangeEvents()
	{
        await _testContext.TestFunction.PublishEvent(_startDateChangedEvent);
		await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish start date change");
	}

    [When("a withdrawal was sent partway through the apprenticeship")]
    public async Task PublishWithdrawnEvent()
    {
        _apprenticeshipWithdrawnEvent.LastDayOfLearning = new DateTime(2020, 08, 31);
        _expectedNumberOfInstalments = 12;
        await _testContext.TestFunction.PublishEvent(_apprenticeshipWithdrawnEvent);
        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish withdrawal");
    }

    [When("a withdrawal was sent prior to completion of qualifying period")]
    public async Task PublishWithdrawnEventPriorToQualifyingPeriodCompletion()
    {
        _apprenticeshipWithdrawnEvent.LastDayOfLearning = new DateTime(2019, 10, 4);
        _expectedNumberOfInstalments = 0;
        await _testContext.TestFunction.PublishEvent(_apprenticeshipWithdrawnEvent);
        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish withdrawal");
    }

    #endregion

    #region Assert
    [Then("the earnings are recalculated based on the new price")]
    public void AssertEarningsRecalculated()
    {
        var expectedTotal = _newTrainingPrice + _newAssessmentPrice; //todo
        var currentEpisode = _updatedApprenticeshipEntity!.GetCurrentEpisode(TestSystemClock.Instance());
        var actualTotal = currentEpisode.EarningsProfile.OnProgramTotal + currentEpisode.EarningsProfile.CompletionPayment;

        if (expectedTotal != actualTotal)
        {
            Assert.Fail($"Earnings not updated, Expected Total:{expectedTotal}, Actual Total:{actualTotal}");
        }
    }

    [Then("the new price is recorded")]
    public void AssetNewPriceIsRecorded()
    {
        var currentEpisode = _updatedApprenticeshipEntity!.GetCurrentEpisode(TestSystemClock.Instance());
        currentEpisode.Prices.Count.Should().Be(2);
    }

    [Then("the earnings are recalculated based on the funding band maximum")]
    public void AssertEarningsRecalculatedBasedOnBandMaximum()
    {
        var currentEpisode = _updatedApprenticeshipEntity!.GetCurrentEpisode(TestSystemClock.Instance());

        var expectedTotal = _fundingBandMaximum;
        var actualTotal = currentEpisode.EarningsProfile.OnProgramTotal + currentEpisode.EarningsProfile.CompletionPayment;

        if (expectedTotal != actualTotal)
        {
            Assert.Fail($"Earnings not updated correctly, Expected Total:{expectedTotal}, Actual Total:{actualTotal}");
        }
    }

    [Then("the history of old and new earnings is maintained")]
    public void AssertHistoryUpdated()
    {
        var currentEpisode = _updatedApprenticeshipEntity!.GetCurrentEpisode(TestSystemClock.Instance());
        if (currentEpisode.EarningsProfileHistory == null || !currentEpisode.EarningsProfileHistory.Any())
        {
            Assert.Fail("No earning history created");
        }
    }

    [Then("the earnings prior to the effective-from date are 'frozen' and do not change as part of this calculation")]
    public void AssertEarningsFrozen()
    {
        var instalmentsToValidate = GetFrozenInstalments(_updatedApprenticeshipEntity!);

        foreach(var instalment in instalmentsToValidate)
        {
            var expectedInstalment = _originalEarningsProfile.Instalments.FirstOrDefault(x => 
                x.AcademicYear == instalment.AcademicYear &&
                x.DeliveryPeriod == instalment.DeliveryPeriod);

            if (expectedInstalment == null)
            {
                Assert.Fail("Regenerated instalments do not match delivery dates of the original calculations");
                return;
            }

            if (expectedInstalment.Amount != instalment.Amount)
            {
                Assert.Fail($"Frozen amount should be £{expectedInstalment.Amount} but was £{instalment.Amount} for academicYear{instalment.AcademicYear} period:{instalment.DeliveryPeriod}");
            }
        }
    }

    [Then("the number of instalments is determined by the number of census dates passed between the effective-from date and the planned end date of the apprenticeship")]
    [Then("the number of instalments is determined by the number of census dates passed between the new start date and the planned end date of the apprenticeship")]
    [Then("the number of instalments is determined by the number of census dates passed between the start date and the withdrawal date")]
    [Then("the number of instalments is zero")]
    public void AssertNumberOfInstalments()
    {
        var currentEpisode = _updatedApprenticeshipEntity!.GetCurrentEpisode(TestSystemClock.Instance());
        var numberOfInstalments = currentEpisode.EarningsProfile.Instalments.Count;

        if (numberOfInstalments != _expectedNumberOfInstalments)
        {
            Assert.Fail($"Expected {_expectedNumberOfInstalments} but found {numberOfInstalments}");
        }
    }

    [Then("the amount of each instalment is determined as: newPriceLessCompletion - earningsBeforeTheEffectiveFromDate / numberOfInstalments")]
    public void AssertRecalculatedInstamentAmountsAfterPriceChange()
    {
        var currentEpisode = _updatedApprenticeshipEntity!.GetCurrentEpisode(TestSystemClock.Instance());

        var frozenInstalments = GetFrozenInstalments(_updatedApprenticeshipEntity!);
        var earningsBeforeTheEffectiveFromDate = frozenInstalments.Sum(x => x.Amount);

        var numberOfRecalculatedInstalments = currentEpisode.EarningsProfile.Instalments.Count - frozenInstalments.Count;
        var newPriceLessCompletion = currentEpisode.EarningsProfile.OnProgramTotal;

        var expectedMonthlyPrice = Math.Round((newPriceLessCompletion - earningsBeforeTheEffectiveFromDate) / numberOfRecalculatedInstalments, 5);

        var numberOfMatchingInstalments = currentEpisode.EarningsProfile.Instalments
            .Count(x => x.Amount == expectedMonthlyPrice);

        
        if (numberOfMatchingInstalments != numberOfRecalculatedInstalments)
        {
            Assert.Fail($"Expected to find {numberOfRecalculatedInstalments} instalments of £{expectedMonthlyPrice} but found {numberOfMatchingInstalments}");
        }
    }

    [Then("the amount of each instalment is determined as: totalPriceLessCompletion / newNumberOfInstalments")]
    public void AssertRecalculatedInstalmentAmountsAfterStartDateChange()
    {
        var currentEpisode = _updatedApprenticeshipEntity!.GetCurrentEpisode(TestSystemClock.Instance());

        var numberOfRecalculatedInstalments = currentEpisode.EarningsProfile.Instalments.Count;
        var totalPriceLessCompletion = currentEpisode.EarningsProfile.OnProgramTotal;

        var expectedMonthlyPrice = Math.Round(totalPriceLessCompletion / numberOfRecalculatedInstalments, 5);

        var numberOfMatchingInstalments = currentEpisode.EarningsProfile.Instalments
            .Count(x => x.Amount == expectedMonthlyPrice);
        
        if (numberOfMatchingInstalments != numberOfRecalculatedInstalments)
        {
            Assert.Fail($"Expected to find {numberOfRecalculatedInstalments} instalments of £{expectedMonthlyPrice} but found {numberOfMatchingInstalments}");
        }
    }

    [Then("a new earnings profile id is set")]
    public void AssertEarningsProfileId()
    {
        var currentEpisode = _updatedApprenticeshipEntity!.GetCurrentEpisode(TestSystemClock.Instance());

        Assert.That(currentEpisode.EarningsProfile.EarningsProfileId != Guid.Empty &&
                    currentEpisode.EarningsProfile.EarningsProfileId != _originalEarningsProfile.EarningsProfileId);
    }

    [Then("the earnings are recalculated based on the new start date")]
    public void AssertEarningsRecalculatedBasedOnNewStartDate()
    {
        //Left empty on purpose
    }

    [Then(@"the there are (.*) earnings")]
    public void AssertExpectedNumberOfEarnings(int expectedNumberOfEarnings)
    {
        var currentEpisode = _updatedApprenticeshipEntity!.GetCurrentEpisode(TestSystemClock.Instance());

        var matchingInstalments = currentEpisode.EarningsProfile.Instalments.Count;

        if(matchingInstalments != expectedNumberOfEarnings)
        {
            Assert.Fail($"Expected to find {expectedNumberOfEarnings} instalments but found {matchingInstalments}");
        }
    }


    private async Task<bool> EnsureApprenticeshipEntityCreated()
    {
        var apprenticeshipEntity = await GetApprenticeshipEntity();
        if (apprenticeshipEntity == null)
        {
            return false;
        }

        var currentEpisode = apprenticeshipEntity!.GetCurrentEpisode(TestSystemClock.Instance());
        _originalEarningsProfile = currentEpisode.EarningsProfile;
        return true;
    }

    private async Task<ApprenticeshipModel> GetApprenticeshipEntity()
    {
        return await _testContext.SqlDatabase.GetApprenticeship(_apprenticeshipCreatedEvent.ApprenticeshipKey);
    }

    private async Task<bool> EnsureRecalculationHasHappened()
    {
        var apprenticeshipEntity = await GetApprenticeshipEntity();

        var currentEpisode = apprenticeshipEntity!.GetCurrentEpisode(TestSystemClock.Instance());
        if (!currentEpisode.EarningsProfileHistory.Any())
        {
            return false;
        }

        _updatedApprenticeshipEntity = apprenticeshipEntity;
        return true;
    }

    private List<InstalmentModel> GetFrozenInstalments(ApprenticeshipModel apprenticeshipEntity)
    {
        var fromYear = _effectiveFromDate.ToAcademicYear();
        var fromPeriod = _effectiveFromDate.ToDeliveryPeriod();

        var currentEpisode = apprenticeshipEntity!.GetCurrentEpisode(TestSystemClock.Instance());

        return currentEpisode.EarningsProfile.Instalments
            .Where(x => 
                (x.AcademicYear == fromYear && x.DeliveryPeriod < fromPeriod) ||
                x.AcademicYear < fromYear
             ).ToList();
    }

    #endregion
}
