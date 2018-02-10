
--SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AuctionGuests'

EXEC sp_rename 'AuctionGuests', 'AuctionGuests_bak';  --2018 data in wrong table
EXEC sp_rename 'AuctionGuests_2017', 'AuctionGuests'; --2017 data in right table
select * into AuctionGuests_2017 from AuctionGuests;   --2017 data into backup table

EXEC sp_rename 'AuctionItems', 'AuctionItems_bak';  --2018 data in wrong table
EXEC sp_rename 'AuctionItems_2017', 'AuctionItems'; --2017 data in right table
select * into AuctionItems_2017 from AuctionItems;   --2017 data into backup table

EXEC sp_rename 'Bidders', 'Bidders_bak';  --2018 data in wrong table
EXEC sp_rename 'Bidders_2017', 'Bidders'; --2017 data in right table
select * into Bidders_2017 from Bidders;   --2017 data into backup table

EXEC sp_rename 'DonationItems', 'DonationItems_bak';  --2018 data in wrong table
EXEC sp_rename 'DonationItems_2017', 'DonationItems'; --2017 data in right table
select * into DonationItems_2017 from DonationItems;   --2017 data into backup table

EXEC sp_rename 'Donors', 'Donors_bak';  --2018 data in wrong table
EXEC sp_rename 'Donors_2017', 'Donors'; --2017 data in right table
select * into Donors_2017 from Donors;   --2017 data into backup table

EXEC sp_rename 'Invoices', 'Invoices_bak';  --2018 data in wrong table
EXEC sp_rename 'Invoices_2017', 'Invoices'; --2017 data in right table
select * into Invoices_2017 from Invoices;   --2017 data into backup table

EXEC sp_rename 'PayPalTransactions', 'PayPalTransactions_bak';  --2018 data in wrong table
EXEC sp_rename 'PayPalTransactions_2017', 'PayPalTransactions'; --2017 data in right table
select * into PayPalTransactions_2017 from PayPalTransactions;   --2017 data into backup table

EXEC sp_rename 'Solicitors', 'Solicitors_bak';  --2018 data in wrong table
EXEC sp_rename 'Solicitors_2017', 'Solicitors'; --2017 data in right table
select * into Solicitors_2017 from Solicitors;   --2017 data into backup table

EXEC sp_rename 'StoreItemPurchases', 'StoreItemPurchases_bak';  --2018 data in wrong table
EXEC sp_rename 'StoreItemPurchases_2017', 'StoreItemPurchases'; --2017 data in right table
select * into StoreItemPurchases_2017 from StoreItemPurchases;   --2017 data into backup table

EXEC sp_rename 'StoreItems', 'StoreItems_bak';  --2018 data in wrong table
EXEC sp_rename 'StoreItems_2017', 'StoreItems'; --2017 data in right table
select * into StoreItems_2017 from StoreItems;   --2017 data into backup table

EXEC sp_rename 'Students', 'Students_bak';  --2018 data in wrong table
EXEC sp_rename 'Students_2017', 'Students'; --2017 data in right table
select * into Students_2017 from Students;   --2017 data into backup table


select * From Bidders
select * From Bidders_2017
select * From Bidders_bak

DELETE from AuctionGuests;
DELETE from AuctionItems;
DELETE from Bidders;
DELETE from DonationItems;
DELETE from Donors;
DELETE from Invoices;
DELETE from PayPalTransactions;
DELETE from Solicitors;
DELETE from StoreItemPurchases;
DELETE from StoreItems;
DELETE from Students;

--Bidders.BidderNumber
--SotreItems - ,Quantity,IsRaffleTicket,DonationItem_DonationItemId

--no FKs
SET IDENTITY_INSERT PayPalTransactions ON
INSERT into PayPalTransactions (PayPalTransactionId,PaymentDate,PayerId,PaymentGross,PaymentType,PayerEmail,PayerStatus,TxnId,PaymentStatus,TransactionType,NotificationType,IpnTrackId,Custom) select * from PayPalTransactions_bak;   --2018 data into right table
SET IDENTITY_INSERT PayPalTransactions OFF

--bidders and child tables
SET IDENTITY_INSERT Bidders ON
INSERT into Bidders (BidderId,FirstName,LastName,Phone,Email,ZipCode,CreateDate,UpdateDate,UpdateBy,IsPaymentReminderSent,IsDeleted,IsCheckoutNudgeEmailSent,IsCheckoutNudgeTextSent) select BidderId,FirstName,LastName,Phone,Email,ZipCode,CreateDate,UpdateDate,UpdateBy,IsPaymentReminderSent,IsDeleted,IsCheckoutNudgeEmailSent,IsCheckoutNudgeTextSent from Bidders_bak;   --2018 data into right table
SET IDENTITY_INSERT Bidders OFF

SET IDENTITY_INSERT AuctionGuests ON
INSERT into AuctionGuests (AuctionGuestId,FirstName,LastName,TicketType,TicketPricePaid,Bidder_BidderId,TicketTransaction_PayPalTransactionId,TicketPrice) select * from AuctionGuests_bak;   --2018 data into right table
SET IDENTITY_INSERT AuctionGuests OFF

SET IDENTITY_INSERT Students ON
INSERT into Students (StudentId,HomeroomTeacher,Bidder_BidderId) select * from Students_bak;   --2018 data into right table
SET IDENTITY_INSERT Students OFF


--invoices
SET IDENTITY_INSERT Invoices ON
INSERT into Invoices (InvoiceId,IsPaid,WasMarkedPaidManually,CreateDate,UpdateDate,UpdateBy,Bidder_BidderId,PaymentTransaction_PayPalTransactionId,FirstName,LastName,Phone,Email,ZipCode) select * from Invoices_bak;   --2018 data into right table
SET IDENTITY_INSERT Invoices OFF


--donation items
SET IDENTITY_INSERT AuctionItems ON
INSERT into AuctionItems (AuctionItemId,UniqueItemNumber,Description,Category,StartingBid,BidIncrement,CreateDate,UpdateDate,UpdateBy,WinningBidderId,WinningBid,Title,Invoice_InvoiceId) select * from AuctionItems_bak;   --2018 data into right table,
SET IDENTITY_INSERT AuctionItems OFF

SET IDENTITY_INSERT Donors ON
INSERT into Donors (DonorId,BusinessName,Address1,Address2,City,State,Zip,ContactName,Phone,Email,HasTaxReceiptBeenEmailed) select * from Donors_bak;   --2018 data into right table
SET IDENTITY_INSERT Donors OFF

SET IDENTITY_INSERT Solicitors ON
INSERT into Solicitors (SolicitorId,FirstName,LastName,Phone,Email) select * from Solicitors_bak;   --2018 data into right table
SET IDENTITY_INSERT Solicitors OFF

SET IDENTITY_INSERT DonationItems ON
INSERT into DonationItems (DonationItemId,Category,Description,Restrictions,ExpirationDate,DollarValue,CreateDate,UpdateDate,UpdateBy,IsDeleted,Donor_DonorId,Solicitor_SolicitorId,AuctionItem_AuctionItemId,HasDisplay,Title,IsGiftCard) select * from DonationItems_bak;   --2018 data into right table
SET IDENTITY_INSERT DonationItems OFF



--store items
SET IDENTITY_INSERT StoreItems ON
INSERT into StoreItems (StoreItemId,Title,Price,CanPurchaseInBidderRegistration,CanPurchaseInAuctionCheckout,CanPurchaseInStore,OnlyVisibleToAdmins,CreateDate,UpdateDate,UpdateBy,Description,ImageUrl,IsDeleted) select StoreItemId,Title,Price,CanPurchaseInBidderRegistration,CanPurchaseInAuctionCheckout,CanPurchaseInStore,OnlyVisibleToAdmins,CreateDate,UpdateDate,UpdateBy,Description,ImageUrl,IsDeleted from StoreItems_bak;   --2018 data into right table
SET IDENTITY_INSERT StoreItems OFF

SET IDENTITY_INSERT StoreItemPurchases ON
INSERT into StoreItemPurchases (StoreItemPurchaseId,Quantity,PricePaid,PurchaseTransaction_PayPalTransactionId,StoreItem_StoreItemId,Bidder_BidderId,Invoice_InvoiceId) select * from StoreItemPurchases_bak;   --2018 data into right table
SET IDENTITY_INSERT StoreItemPurchases OFF


--verify
select * from AuctionGuests
select * from AuctionItems
select * from Bidders
select * from DonationItems
select * from Donors
select * from Invoices
select * from PayPalTransactions
select * from Solicitors
select * from StoreItemPurchases
select * from StoreItems
select * from Students



DROP TABLE AuctionGuests_bak
DROP TABLE AuctionItems_bak
DROP TABLE Bidders_bak
DROP TABLE DonationItems_bak
DROP TABLE Donors_bak
DROP TABLE Invoices_bak
DROP TABLE PayPalTransactions_bak
DROP TABLE Solicitors_bak
DROP TABLE StoreItemPurchases_bak
DROP TABLE StoreItems_bak
DROP TABLE Students_bak

/*
Bidders
DonationItems
Donors
Invoices
PayPalTransactions
Solicitors
StoreItemPurchases
StoreItems
Students
*/