Feature: Recalculate earnings following withdrawal before end of qualification period reached

Scenario: Withdrawal made before end of qualifying period; recalc earnings as zero
	Given an apprenticeship has been created
	And the total price is below or at the funding band maximum
	And the earnings for the apprenticeship are calculated
	When a withdrawal was sent prior to completion of qualifying period
	Then the number of instalments is zero
	And a new earnings profile id is set
	And the history of old and new earnings is maintained