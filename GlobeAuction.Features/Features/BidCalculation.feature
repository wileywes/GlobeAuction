Feature: BidCalculation
	Validate calculations on winners for item when bids are changed

@hook_purgeall
Scenario: Adding a new bid to an item should recalculate the winners
	Given I create these auction categories
	| Name                 | ItemNumberStart | ItemNumberEnd | BidOpenDateLtz | BidCloseDateLtz | IsFundAProject | IsAvailableForMobileBidding | IsOnlyAvailableToAuctionItems |
	| SpecflowTestCategory | 90000           | 99999         | 1/1/2000       | 1/1/2200        | false          | false                       | false                         |
	Given I create these donation items in category 'SpecflowTestCategory'
	| Title | Description      | Quantity | DollarValue |
	| Test  | Test Description | 1        | 100         |
	Then the donation items in the category 'SpecflowTestCategory' are
	| Title | Description      | Quantity | DollarValue |
	| Test  | Test Description | 1        | 100         |
	When I convert the created donation items to single auction items
	Then the auction items in the category 'SpecflowTestCategory' are
	| UniqueItemNumber | Title | Description      | StartingBid | BidIncrement | Quantity |
	| 90000            | Test  | Test Description | 40          | 5            | 1        |
	When I create the following bidders using the 'Register and Mark Paid (Cash)' button
	| FirstName | LastName | Phone        | Email           | ZipCode |
	| John      | Smith    | 123-123-1234 | john@gmail.com  | 30001   |
	| Sally     | Fields   | 456-456-4567 | sally@gmail.com | 30002   |
	Then the bidders in the system are
	| BidderNumber | FirstName | LastName | Phone        | Email           | ZipCode | GuestCount | TicketsPaid | ItemsCount | ItemsPaid | TotalPaid | PaymentMethod |
	| 1            | John      | Smith    | 123-123-1234 | john@gmail.com  | 30001   | 1          | 1           | 0          | 0         | 1.00      | Cash          |
	| 2            | Sally     | Fields   | 456-456-4567 | sally@gmail.com | 30002   | 1          | 1           | 0          | 0         | 1.00      | Cash          |
	When I log in as bidder number '1' with last name 'Smith' and email 'john@gmail.com'
	Then I enter a bid of '1.00' for item number '90000'

