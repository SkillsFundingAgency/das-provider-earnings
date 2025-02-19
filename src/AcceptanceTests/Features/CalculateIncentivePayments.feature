Feature: Calculate incentive payments

Scenario: 16-18 Incentive Payments Generation
	Given an apprenticeship has been created with the following information
	| Age |
	| 18  |
	And the following Price Episodes
	| StartDate  | EndDate    | Price |
	| 2020-08-01 | 2024-07-31 | 15000 |
	When earnings are calculated
	Then Additional Payments are persisted as follows
	| Type              | Amount | DueDate    |
	| ProviderIncentive | 500    | 2020-10-29 |
	| EmployerIncentive | 500    | 2020-10-29 |
	| ProviderIncentive | 500    | 2021-07-31 |
	| EmployerIncentive | 500    | 2021-07-31 |
	#And an EarningsGeneratedEvent is raised with the following incentives as Delivery Periods
	#| Type              | Amount | CalendarMonth | CalendarYear |
	#| EmployerIncentive | 500    | 10            | 2020         |
	#| ProviderIncentive | 500    | 10            | 2020         |
	#| EmployerIncentive | 500    | 7             | 2021         |
	#| ProviderIncentive | 500    | 7             | 2021         |

Scenario: 16-18 Incentive Payments Generation - 90 day apprenticeship
	Given an apprenticeship has been created with the following information
	| Age |
	| 18  |
	And the following Price Episodes
	| StartDate  | EndDate    | Price |
	| 2020-08-01 | 2020-10-29 | 10000 |
	When earnings are calculated
	Then Additional Payments are persisted as follows
	| Type              | Amount | DueDate    |
	| ProviderIncentive | 500    | 2020-10-29 |
	| EmployerIncentive | 500    | 2020-10-29 |
	#And an EarningsGeneratedEvent is raised with the following incentives as Delivery Periods
	#| Type              | Amount | CalendarMonth | CalendarYear |
	#| EmployerIncentive | 500    | 10            | 2020         |
	#| ProviderIncentive | 500    | 10            | 2020         |

	
Scenario: 16-18 Incentive Payments Generation - learner outside of age range
	Given an apprenticeship has been created with the following information
	| Age |
	| 21  |
	And the following Price Episodes
	| StartDate  | EndDate    | Price |
	| 2020-08-01 | 2024-07-31 | 15000 |
	When earnings are calculated
	Then no Additional Payments are persisted
	#And an EarningsGeneratedEvent is raised with no incentives as Delivery Periods
