Feature: BidCalculation
	Validate calculations on winners for item when bids are changed

@hook_purgeall
Scenario: Adding a new bid to an item should recalculate the winners
	Given I create these donation items
	| Title | Description | Qty | CategoryName |
	Then the donation items are	
	| Title | Description | Qty | CategoryName |
