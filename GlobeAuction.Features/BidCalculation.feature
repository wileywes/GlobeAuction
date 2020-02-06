Feature: BidCalculation
	Validate calculations on winners for item when bids are changed

@hook_purgeall
Scenario: Adding a new bid to an item should recalculate the winners
	Given I create these auction items
	| Title | StartingBid |
	Then the winners
	When I press add
	Then the result should be 120 on the screen
