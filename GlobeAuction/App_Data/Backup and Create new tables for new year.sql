/*

select t.name,
  (select count(*) from sys.foreign_keys where referenced_object_id=t.object_id) as FksReferencingTable,
  'select * into [2024_' + name + '] from ' + name as BackupScript,
  'ALTER TABLE [' + name + '] NOCHECK CONSTRAINT all' as DisableConstraint,
  'DELETE from ' + name as DeleteScript,
  'ALTER TABLE [' + name + '] WITH CHECK CHECK CONSTRAINT all' as EnableConstraint,
  'select * FROM ' + name as CheckResults
from sys.tables t
where [name] not in ('TicketTypes','__MigrationHistory','Faqs','FaqCategories','ConfigProperties','AuctionCategories')
 and name not like 'AspNet%'
 and name not like '2017_%'
 and name not like '2018_%'
 and name not like '2019_%'
 and name not like '2020_%'
 and name not like '2021_%'
 and name not like '2022_%'
 and name not like '2023_%'
 and name not like '2024_%'
order by (select count(*) from sys.foreign_keys where referenced_object_id=t.object_id) asc

select * into [AuctionCategories] from 2023_AuctionCategories

--RESTORE auction categories
insert into [AuctionCategories] (Name, BidOpenDateLtz, BidCloseDateLtz, IsFundAProject, IsOnlyAvailableToAuctionItems, ItemNumberStart, ItemNumberEnd, IsAvailableForMobileBidding)
select Name, BidOpenDateLtz, BidCloseDateLtz, IsFundAProject, IsOnlyAvailableToAuctionItems, ItemNumberStart, ItemNumberEnd, IsAvailableForMobileBidding
from [2023_AuctionCategories]


--1) run the SELECT INTO statements
select * into [2024_TicketPurchases] from TicketPurchases
select * into [2024_Bids] from Bids
select * into [2024_ShoutOuts] from ShoutOuts
select * into [2024_CatalogFavorites] from CatalogFavorites
select * into [2024_EmailTemplates] from EmailTemplates
select * into [2024_StoreItemPurchases] from StoreItemPurchases
select * into [2024_BundleComponents] from BundleComponents
select * into [2024_Sponsors] from Sponsors
select * into [2024_Students] from Students
select * into [2024_AuctionGuests] from AuctionGuests
select * into [2024_Solicitors] from Solicitors
select * into [2024_DonationItems] from DonationItems
select * into [2024_Donors] from Donors
select * into [2024_PayPalTransactions] from PayPalTransactions
select * into [2024_AuctionItems] from AuctionItems
select * into [2024_Invoices] from Invoices
select * into [2024_StoreItems] from StoreItems
select * into [2024_Bidders] from Bidders

--2) disable check constraints
ALTER TABLE [TicketPurchases] NOCHECK CONSTRAINT all
ALTER TABLE [Bids] NOCHECK CONSTRAINT all
ALTER TABLE [ShoutOuts] NOCHECK CONSTRAINT all
ALTER TABLE [CatalogFavorites] NOCHECK CONSTRAINT all
ALTER TABLE [EmailTemplates] NOCHECK CONSTRAINT all
ALTER TABLE [StoreItemPurchases] NOCHECK CONSTRAINT all
ALTER TABLE [BundleComponents] NOCHECK CONSTRAINT all
ALTER TABLE [Sponsors] NOCHECK CONSTRAINT all
ALTER TABLE [Students] NOCHECK CONSTRAINT all
ALTER TABLE [AuctionGuests] NOCHECK CONSTRAINT all
ALTER TABLE [Solicitors] NOCHECK CONSTRAINT all
ALTER TABLE [DonationItems] NOCHECK CONSTRAINT all
ALTER TABLE [Donors] NOCHECK CONSTRAINT all
ALTER TABLE [PayPalTransactions] NOCHECK CONSTRAINT all
ALTER TABLE [AuctionItems] NOCHECK CONSTRAINT all
ALTER TABLE [Invoices] NOCHECK CONSTRAINT all
ALTER TABLE [StoreItems] NOCHECK CONSTRAINT all
ALTER TABLE [Bidders] NOCHECK CONSTRAINT all

--3) run the DELETE statements
DELETE from TicketPurchases
DELETE from Bids
DELETE from ShoutOuts
DELETE from CatalogFavorites
DELETE from EmailTemplates
DELETE from StoreItemPurchases
DELETE from BundleComponents
DELETE from Sponsors
DELETE from Students
DELETE from AuctionGuests
DELETE from Solicitors
DELETE from DonationItems
DELETE from Donors
DELETE from PayPalTransactions
DELETE from AuctionItems
DELETE from Invoices
DELETE from StoreItems
DELETE from Bidders

--4) re-enable the check constraints
ALTER TABLE [TicketPurchases] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [Bids] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [ShoutOuts] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [CatalogFavorites] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [EmailTemplates] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [StoreItemPurchases] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [BundleComponents] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [Sponsors] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [Students] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [AuctionGuests] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [Solicitors] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [DonationItems] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [Donors] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [PayPalTransactions] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [AuctionItems] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [Invoices] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [StoreItems] WITH CHECK CHECK CONSTRAINT all
ALTER TABLE [Bidders] WITH CHECK CHECK CONSTRAINT all

--5) check the live tables to ensure they are empty
select * FROM TicketPurchases
select * FROM Bids
select * FROM ShoutOuts
select * FROM CatalogFavorites
select * FROM EmailTemplates
select * FROM StoreItemPurchases
select * FROM BundleComponents
select * FROM Sponsors
select * FROM Students
select * FROM AuctionGuests
select * FROM Solicitors
select * FROM DonationItems
select * FROM Donors
select * FROM PayPalTransactions
select * FROM AuctionItems
select * FROM Invoices
select * FROM StoreItems
select * FROM Bidders
*/
