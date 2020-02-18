Feature: BidCalculation
	Validate calculations on winners for item when bids are changed

@hook_purgeall
Scenario: Adding a new bid to an item should recalculate the winners
	Given I create these auction categories
	| Name                 | ItemNumberStart | ItemNumberEnd | BidOpenDateLtz | BidCloseDateLtz | IsFundAProject | IsAvailableForMobileBidding | IsOnlyAvailableToAuctionItems |
	| SpecflowTestCategory | 90000           | 99999         | 1/1/2000       | 1/1/2200        | false          | false                       | false                         |
	Given I create these donation items in category 'SpecflowTestCategory'
	| Title | Description      | Quantity |
	| Test  | Test Description | 1        |
	Then the donation items in the category 'SpecflowTestCategory' are
	| Title | Description      | Quantity |
	| Test  | Test Description | 1        |
