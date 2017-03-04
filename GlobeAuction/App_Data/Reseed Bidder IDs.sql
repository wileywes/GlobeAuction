select * from AuctionGuests
select * from AuctionItems where WinningBidderId is not null
select * from Bidders order by BidderID desc
select * from Bidders_copy order by BidderID desc
select * from Invoices where Bidder_BidderId is not null
select * from Students where Bidder_BidderId is not null

--21
select max(BidderId) from Bidders

-- disable all constraints
ALTER TABLE AuctionGuests NOCHECK CONSTRAINT all
ALTER TABLE AuctionItems NOCHECK CONSTRAINT all
ALTER TABLE Invoices NOCHECK CONSTRAINT all
ALTER TABLE Students NOCHECK CONSTRAINT all

SET IDENTITY_INSERT Bidders ON

begin transaction

CREATE TABLE [dbo].[Bidders_copy] (
    [BidderId]              INT NOT NULL,
    [FirstName]             NVARCHAR (MAX) NOT NULL,
    [LastName]              NVARCHAR (MAX) NOT NULL,
    [Phone]                 NVARCHAR (MAX) NOT NULL,
    [Email]                 NVARCHAR (MAX) NOT NULL,
    [ZipCode]               NVARCHAR (MAX) NULL,
    [CreateDate]            DATETIME       NOT NULL,
    [UpdateDate]            DATETIME       NOT NULL,
    [UpdateBy]              NVARCHAR (MAX) NULL,
    [IsPaymentReminderSent] BIT            NOT NULL
);

DELETE FROM [Bidders_copy]

insert into Bidders_copy (BidderId, FirstName, LastName, Phone, Email, ZipCode, CreateDate, UpdateDate, UpdateBy, IsPaymentReminderSent )
select BidderId + 100, FirstName, LastName, Phone, Email, ZipCode, CreateDate, UpdateDate, UpdateBy, IsPaymentReminderSent
from Bidders;

delete from Bidders;

insert into Bidders(BidderId, FirstName, LastName, Phone, Email, ZipCode, CreateDate, UpdateDate, UpdateBy, IsPaymentReminderSent )
select BidderId, FirstName, LastName, Phone, Email, ZipCode, CreateDate, UpdateDate, UpdateBy, IsPaymentReminderSent
from Bidders_copy

UPDATE AuctionGuests SET Bidder_BidderId = Bidder_BidderId + 100
UPDATE AuctionItems SET WinningBidderId = WinningBidderId + 100 where WinningBidderId is not null
UPDATE Invoices SET Bidder_BidderId = Bidder_BidderId + 100 where Bidder_BidderId is not null
UPDATE Students SET Bidder_BidderId = Bidder_BidderId + 100 where Bidder_BidderId is not null
commit

DBCC CHECKIDENT ('Bidders',RESEED, 272)

SET IDENTITY_INSERT Bidders OFF

-- enable all constraints
ALTER TABLE AuctionGuests WITH CHECK CHECK CONSTRAINT all
ALTER TABLE AuctionItems WITH CHECK CHECK CONSTRAINT all
ALTER TABLE Invoices WITH CHECK CHECK CONSTRAINT all
ALTER TABLE Students WITH CHECK CHECK CONSTRAINT all