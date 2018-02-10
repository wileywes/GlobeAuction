select * into AuctionGuests_2017 from AuctionGuests;
DELETE from AuctionGuests;
GO

EXEC sp_rename 'AuctionItems', 'AuctionItems_2017';
GO
select * into AuctionItems from AuctionItems_2017 where 1 = 0;
GO

EXEC sp_rename 'Bidders', 'Bidders_2017';
GO
select * into Bidders from Bidders_2017 where 1 = 0;
GO

EXEC sp_rename 'DonationItems', 'DonationItems_2017';
GO
select * into DonationItems from DonationItems_2017 where 1 = 0;
GO

EXEC sp_rename 'Donors', 'Donors_2017';
GO
select * into Donors from Donors_2017 where 1 = 0;
GO

EXEC sp_rename 'Invoices', 'Invoices_2017';
GO
select * into Invoices from Invoices_2017 where 1 = 0;
GO

EXEC sp_rename 'PayPalTransactions', 'PayPalTransactions_2017';
GO
select * into PayPalTransactions from PayPalTransactions_2017 where 1 = 0;
GO

EXEC sp_rename 'Solicitors', 'Solicitors_2017';
GO
select * into Solicitors from Solicitors_2017 where 1 = 0;
GO

EXEC sp_rename 'StoreItemPurchases', 'StoreItemPurchases_2017';
GO
select * into StoreItemPurchases from StoreItemPurchases_2017 where 1 = 0;
GO

EXEC sp_rename 'StoreItems', 'StoreItems_2017';
GO
select * into StoreItems from StoreItems_2017 where 1 = 0;
GO

EXEC sp_rename 'Students', 'Students_2017';
GO
select * into Students from Students_2017 where 1 = 0;
GO