/*

select t.name,
  (select count(*) from sys.foreign_keys where referenced_object_id=t.object_id) as FksReferencingTable,
  'select * FROM ' + name as ConfirmTargetTablesAreEmpty,
  'ALTER TABLE [' + name + '] NOCHECK CONSTRAINT all' as DisableConstraint,
  'SET IDENTITY_INSERT ' + name + ' ON;' + CHAR(13)+CHAR(10) +CHAR(13)+CHAR(10) +
  'INSERT into ' + name + ' (' + Stuff(
        (
        Select ', ' + C.name
        From sys.COLUMNS As C
        Where C.object_id = T.object_id
        Order By C.column_id
        For Xml Path('')
        ), 1, 2, '') + ') SELECT ' + Stuff(
        (
        Select ', ' + C.name
        From sys.COLUMNS As C
        Where C.object_id = T.object_id
        Order By C.column_id
        For Xml Path('')
        ), 1, 2, '') + ' FROM [2021_' + name + ']' + CHAR(10) +
  'SET IDENTITY_INSERT ' + name + ' OFF;' as CopyData,
  'ALTER TABLE [' + name + '] WITH CHECK CHECK CONSTRAINT all' as EnableConstraint,
  'select * FROM ' + name as CheckResults
from sys.tables t
where [name] not in ('TicketTypes','__MigrationHistory')
 and name not like 'AspNet%'
 and name not like '2017_%'
 and name not like '2018_%'
 and name not like '2019_%'
 and name not like '2020_%'
 and name not like '2021_%'
order by (select count(*) from sys.foreign_keys where referenced_object_id=t.object_id) asc


--1) confirm target tables are empty
select * FROM TicketPurchases
select * FROM Bids
select * FROM CatalogFavorites
select * FROM StoreItemPurchases
select * FROM BundleComponents
select * FROM Students
select * FROM AuctionGuests
select * FROM Solicitors
select * FROM DonationItems
select * FROM Donors
select * FROM AuctionCategories
select * FROM PayPalTransactions
select * FROM AuctionItems
select * FROM Invoices
select * FROM StoreItems
select * FROM Bidders


--2) disable check constraints
ALTER TABLE [TicketPurchases] NOCHECK CONSTRAINT all
ALTER TABLE [Bids] NOCHECK CONSTRAINT all
ALTER TABLE [CatalogFavorites] NOCHECK CONSTRAINT all
ALTER TABLE [StoreItemPurchases] NOCHECK CONSTRAINT all
ALTER TABLE [BundleComponents] NOCHECK CONSTRAINT all
ALTER TABLE [Students] NOCHECK CONSTRAINT all
ALTER TABLE [AuctionGuests] NOCHECK CONSTRAINT all
ALTER TABLE [Solicitors] NOCHECK CONSTRAINT all
ALTER TABLE [DonationItems] NOCHECK CONSTRAINT all
ALTER TABLE [Donors] NOCHECK CONSTRAINT all
ALTER TABLE [AuctionCategories] NOCHECK CONSTRAINT all
ALTER TABLE [PayPalTransactions] NOCHECK CONSTRAINT all
ALTER TABLE [AuctionItems] NOCHECK CONSTRAINT all
ALTER TABLE [Invoices] NOCHECK CONSTRAINT all
ALTER TABLE [StoreItems] NOCHECK CONSTRAINT all
ALTER TABLE [Bidders] NOCHECK CONSTRAINT all

--3) COPY DATA
SET IDENTITY_INSERT TicketPurchases ON;    INSERT into TicketPurchases (TicketPurchaseId, TicketType, TicketPrice, TicketPricePaid, AuctionGuest_AuctionGuestId, Invoice_InvoiceId) SELECT TicketPurchaseId, TicketType, TicketPrice, TicketPricePaid, AuctionGuest_AuctionGuestId, Invoice_InvoiceId FROM [2021_TicketPurchases] SET IDENTITY_INSERT TicketPurchases OFF;
SET IDENTITY_INSERT Bids ON;    INSERT into Bids (BidId, BidAmount, AmountPaid, IsWinning, CreateDate, UpdateDate, UpdateBy, AuctionItem_AuctionItemId, Bidder_BidderId, Invoice_InvoiceId, HasWinnerBeenEmailed) SELECT BidId, BidAmount, AmountPaid, IsWinning, CreateDate, UpdateDate, UpdateBy, AuctionItem_AuctionItemId, Bidder_BidderId, Invoice_InvoiceId, HasWinnerBeenEmailed FROM [2021_Bids] SET IDENTITY_INSERT Bids OFF;
SET IDENTITY_INSERT CatalogFavorites ON;    INSERT into CatalogFavorites (CatalogFavoriteId, AuctionItem_AuctionItemId, Bidder_BidderId) SELECT CatalogFavoriteId, AuctionItem_AuctionItemId, Bidder_BidderId FROM [2021_CatalogFavorites] SET IDENTITY_INSERT CatalogFavorites OFF;
SET IDENTITY_INSERT StoreItemPurchases ON;    INSERT into StoreItemPurchases (StoreItemPurchaseId, Quantity, PricePaid, PurchaseTransaction_PayPalTransactionId, StoreItem_StoreItemId, Bidder_BidderId, Invoice_InvoiceId, Price, IsRafflePrinted, HasWinnerBeenEmailed) SELECT StoreItemPurchaseId, Quantity, PricePaid, PurchaseTransaction_PayPalTransactionId, StoreItem_StoreItemId, Bidder_BidderId, Invoice_InvoiceId, Price, IsRafflePrinted, HasWinnerBeenEmailed FROM [2021_StoreItemPurchases] SET IDENTITY_INSERT StoreItemPurchases OFF;
SET IDENTITY_INSERT BundleComponents ON;    INSERT into BundleComponents (BundleComponentId, StoreItemId, Quantity, BundleParent_StoreItemId, StoreItem_StoreItemId, ComponentUnitPrice) SELECT BundleComponentId, StoreItemId, Quantity, BundleParent_StoreItemId, StoreItem_StoreItemId, ComponentUnitPrice FROM [2021_BundleComponents] SET IDENTITY_INSERT BundleComponents OFF;
SET IDENTITY_INSERT Students ON;    INSERT into Students (StudentId, HomeroomTeacher, Bidder_BidderId) SELECT StudentId, HomeroomTeacher, Bidder_BidderId FROM [2021_Students] SET IDENTITY_INSERT Students OFF;
SET IDENTITY_INSERT AuctionGuests ON;    INSERT into AuctionGuests (AuctionGuestId, FirstName, LastName, TicketType, Bidder_BidderId, TicketPrice) SELECT AuctionGuestId, FirstName, LastName, TicketType, Bidder_BidderId, TicketPrice FROM [2021_AuctionGuests] SET IDENTITY_INSERT AuctionGuests OFF;
SET IDENTITY_INSERT Solicitors ON;    INSERT into Solicitors (SolicitorId, FirstName, LastName, Phone, Email) SELECT SolicitorId, FirstName, LastName, Phone, Email FROM [2021_Solicitors] SET IDENTITY_INSERT Solicitors OFF;
SET IDENTITY_INSERT DonationItems ON;    INSERT into DonationItems (DonationItemId, Description, Restrictions, ExpirationDate, DollarValue, CreateDate, UpdateDate, UpdateBy, IsDeleted, Donor_DonorId, Solicitor_SolicitorId, AuctionItem_AuctionItemId, HasDisplay, Title, UseDigitalCertificateForWinner, Quantity, IsReceived, Category_AuctionCategoryId, IsInStore) SELECT DonationItemId, Description, Restrictions, ExpirationDate, DollarValue, CreateDate, UpdateDate, UpdateBy, IsDeleted, Donor_DonorId, Solicitor_SolicitorId, AuctionItem_AuctionItemId, HasDisplay, Title, UseDigitalCertificateForWinner, Quantity, IsReceived, Category_AuctionCategoryId, IsInStore FROM [2021_DonationItems] SET IDENTITY_INSERT DonationItems OFF;
SET IDENTITY_INSERT Donors ON;    INSERT into Donors (DonorId, BusinessName, Address1, Address2, City, State, Zip, ContactName, Phone, Email, HasTaxReceiptBeenEmailed) SELECT DonorId, BusinessName, Address1, Address2, City, State, Zip, ContactName, Phone, Email, HasTaxReceiptBeenEmailed FROM [2021_Donors] SET IDENTITY_INSERT Donors OFF;
SET IDENTITY_INSERT AuctionCategories ON;    INSERT into AuctionCategories (AuctionCategoryId, Name, BidOpenDateLtz, BidCloseDateLtz, IsFundAProject, IsOnlyAvailableToAuctionItems, ItemNumberStart, ItemNumberEnd, IsAvailableForMobileBidding) SELECT AuctionCategoryId, Name, BidOpenDateLtz, BidCloseDateLtz, IsFundAProject, IsOnlyAvailableToAuctionItems, ItemNumberStart, ItemNumberEnd, IsAvailableForMobileBidding FROM [2021_AuctionCategories] SET IDENTITY_INSERT AuctionCategories OFF;
SET IDENTITY_INSERT PayPalTransactions ON;    INSERT into PayPalTransactions (PayPalTransactionId, PaymentDate, PayerId, PaymentGross, PaymentType, PayerEmail, PayerStatus, TxnId, PaymentStatus, TransactionType, NotificationType, IpnTrackId, Custom) SELECT PayPalTransactionId, PaymentDate, PayerId, PaymentGross, PaymentType, PayerEmail, PayerStatus, TxnId, PaymentStatus, TransactionType, NotificationType, IpnTrackId, Custom FROM [2021_PayPalTransactions] SET IDENTITY_INSERT PayPalTransactions OFF;
SET IDENTITY_INSERT AuctionItems ON;    INSERT into AuctionItems (AuctionItemId, UniqueItemNumber, Description, StartingBid, BidIncrement, CreateDate, UpdateDate, UpdateBy, Title, Quantity, ImageUrl, Category_AuctionCategoryId, IsFixedPrice, IsInFiresale) SELECT AuctionItemId, UniqueItemNumber, Description, StartingBid, BidIncrement, CreateDate, UpdateDate, UpdateBy, Title, Quantity, ImageUrl, Category_AuctionCategoryId, IsFixedPrice, IsInFiresale FROM [2021_AuctionItems] SET IDENTITY_INSERT AuctionItems OFF;
SET IDENTITY_INSERT Invoices ON;    INSERT into Invoices (InvoiceId, IsPaid, WasMarkedPaidManually, CreateDate, UpdateDate, UpdateBy, Bidder_BidderId, PaymentTransaction_PayPalTransactionId, FirstName, LastName, Phone, Email, ZipCode, PaymentMethod, InvoiceType, TotalPaid, IsPaymentReminderSent) SELECT InvoiceId, IsPaid, WasMarkedPaidManually, CreateDate, UpdateDate, UpdateBy, Bidder_BidderId, PaymentTransaction_PayPalTransactionId, FirstName, LastName, Phone, Email, ZipCode, PaymentMethod, InvoiceType, TotalPaid, IsPaymentReminderSent FROM [2021_Invoices] SET IDENTITY_INSERT Invoices OFF;
SET IDENTITY_INSERT StoreItems ON;    INSERT into StoreItems (StoreItemId, Title, Price, CanPurchaseInBidderRegistration, CanPurchaseInAuctionCheckout, CanPurchaseInStore, OnlyVisibleToAdmins, CreateDate, UpdateDate, UpdateBy, Description, ImageUrl, IsDeleted, Quantity, IsRaffleTicket, DonationItem_DonationItemId, IsBundleParent, HasUnlimitedQuantity) SELECT StoreItemId, Title, Price, CanPurchaseInBidderRegistration, CanPurchaseInAuctionCheckout, CanPurchaseInStore, OnlyVisibleToAdmins, CreateDate, UpdateDate, UpdateBy, Description, ImageUrl, IsDeleted, Quantity, IsRaffleTicket, DonationItem_DonationItemId, IsBundleParent, HasUnlimitedQuantity FROM [2021_StoreItems] SET IDENTITY_INSERT StoreItems OFF;
SET IDENTITY_INSERT Bidders ON;    INSERT into Bidders (BidderId, FirstName, LastName, Phone, Email, ZipCode, CreateDate, UpdateDate, UpdateBy, IsPaymentReminderSent, IsDeleted, IsCheckoutNudgeEmailSent, IsCheckoutNudgeTextSent, BidderNumber, AttendedEvent, IsCatalogNudgeEmailSent) SELECT BidderId, FirstName, LastName, Phone, Email, ZipCode, CreateDate, UpdateDate, UpdateBy, IsPaymentReminderSent, IsDeleted, IsCheckoutNudgeEmailSent, IsCheckoutNudgeTextSent, BidderNumber, AttendedEvent, IsCatalogNudgeEmailSent FROM [2021_Bidders] SET IDENTITY_INSERT Bidders OFF;

--4) re-enable the check constraints
ALTER TABLE [TicketPurchases] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [Bids] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [CatalogFavorites] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [StoreItemPurchases] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [BundleComponents] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [Students] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [AuctionGuests] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [Solicitors] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [DonationItems] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [Donors] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [AuctionCategories] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [PayPalTransactions] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [AuctionItems] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [Invoices] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [StoreItems] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [Bidders] WITH CHECK CHECK CONSTRAINT all


--5) check the live tables to ensure they have the data
select * FROM TicketPurchases
select * FROM Bids
select * FROM CatalogFavorites
select * FROM StoreItemPurchases
select * FROM BundleComponents
select * FROM Students
select * FROM AuctionGuests
select * FROM Solicitors
select * FROM DonationItems
select * FROM Donors
select * FROM AuctionCategories
select * FROM PayPalTransactions
select * FROM AuctionItems
select * FROM Invoices
select * FROM StoreItems
select * FROM Bidders

*/
