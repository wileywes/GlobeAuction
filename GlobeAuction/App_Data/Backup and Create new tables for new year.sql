/*

select t.name,
  (select count(*) from sys.foreign_keys where referenced_object_id=t.object_id) as FksReferencingTable,
  'select * into [2019_' + name + '] from ' + name as BackupScript,
  'DELETE from ' + name as DeleteScript,
  --  'EXEC sp_rename ''' + name + ''', ''2017_' + replace(name, '_2017','') + ''';' as RenameScript -- only used once
  'select * FROM ' + name as CheckResults
from sys.tables t
where [name] not in ('TicketTypes','__MigrationHistory')
 and name not like 'AspNet%'
 and name not like '2018_%'
 and name not like '2019_%'
order by (select count(*) from sys.foreign_keys where referenced_object_id=t.object_id) asc


--1) run the SELECT INTO statements
select * into [2019_TicketPurchases] from TicketPurchases
select * into [2019_StoreItemPurchases] from StoreItemPurchases
select * into [2019_Bids] from Bids
select * into [2019_Students] from Students
select * into [2019_CatalogFavorites] from CatalogFavorites
select * into [2019_BundleComponents] from BundleComponents
select * into [2019_AuctionGuests] from AuctionGuests
select * into [2019_DonationItems] from DonationItems
select * into [2019_Donors] from Donors
select * into [2019_Solicitors] from Solicitors
select * into [2019_AuctionCategories] from AuctionCategories
select * into [2019_PayPalTransactions] from PayPalTransactions
select * into [2019_Invoices] from Invoices
select * into [2019_AuctionItems] from AuctionItems
select * into [2019_Bidders] from Bidders
select * into [2019_StoreItems] from StoreItems

--2) disable check constraints
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"

--3) run the DELETE statements
DELETE from TicketPurchases
DELETE from StoreItemPurchases
DELETE from Bids
DELETE from Students
DELETE from CatalogFavorites
DELETE from BundleComponents
DELETE from AuctionGuests
DELETE from DonationItems
DELETE from Donors
DELETE from Solicitors
DELETE from AuctionCategories
DELETE from PayPalTransactions
DELETE from Invoices
DELETE from AuctionItems
DELETE from Bidders
DELETE from StoreItems

--4) re-enable the check constraints
exec sp_MSforeachtable @command1="print '?'", @command2="ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"

--5) check the live tables to ensure they are empty
select * FROM TicketPurchases
select * FROM StoreItemPurchases
select * FROM Bids
select * FROM Students
select * FROM CatalogFavorites
select * FROM BundleComponents
select * FROM AuctionGuests
select * FROM DonationItems
select * FROM Donors
select * FROM Solicitors
select * FROM AuctionCategories
select * FROM PayPalTransactions
select * FROM Invoices
select * FROM AuctionItems
select * FROM Bidders
select * FROM StoreItems
*/
