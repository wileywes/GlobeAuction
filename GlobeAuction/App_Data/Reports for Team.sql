--store purchases that are unpaid but holding quantity (WIP - need to check invoice)
select *
from StoreItemPurchases sip
inner join StoreItems si on si.StoreItemId = sip.StoreItem_StoreItemId
where si.IsRaffleTicket=0
  and si.HasUnlimitedQuantity=0


--bidders with a firesale purchase
select d.BidderNumber, d.Email, d.FirstName, d.LastName, count(*) as FireSaleCount
from bids b
inner join bidders d on d.BidderId = b.Bidder_BidderId
where b.createdate > '2020-05-18'
group by d.BidderNumber, d.Email, d.FirstName, d.LastName

--items with no image
select * from AuctionItems where imageUrl = '' or imageurl is null

--items with expiration date earlier than
select *
from AuctionItems ai
inner join DonationItems di on di.auctionitem_auctionitemid=ai.auctionitemid
where di.expirationdate < '2020-05-18'

--items not in hand
select *
from AuctionItems ai
inner join DonationItems di on di.auctionitem_auctionitemid=ai.auctionitemid
where di.isreceived=0

--solicitors collecting lots of donations
select s.Email, s.Phone, s.FirstName, s.LastName, sum(di.dollarvalue) as TotalDollarValue, count(*) as CountOfDonations
from Solicitors s
inner join DonationItems di on di.Solicitor_SolicitorId=s.SolicitorId
group by s.Email, s.Phone, s.FirstName, s.LastName


--bidders with guests
select b.BidderId, b.BidderNumber, b.FirstName, b.LastName, b.phone, b.Email, b.ZipCode, g.FirstName as GuestFirst, g.LastName as GuestLast, g.TicketType, g.TicketPricePaid
from Bidders b
inner join AuctionGuests g on b.bidderid = g.bidder_bidderid
where b.isDeleted = 0
order by b.bidderid


--bidders with homerooms
select s.HomeroomTeacher, count(distinct b.BidderId) as NumberOfBidders,
  count(*) as NumberOfGuests,
  sum(g.ticketpricepaid) as TicketSales
from Students s
inner join Bidders b on b.bidderid = s.bidder_bidderid
inner join AuctionGuests g on b.bidderid = g.bidder_bidderid
where b.IsDeleted = 0
group by HomeroomTeacher

--Digital certificate items
select ai.UniqueItemNumber as AuctionItemNumber, di.*, d.*
from donationitems di
inner join donors d on di.donor_donorid = d.donorid
left join auctionitems ai on ai.AuctionItemId = di.AuctionItem_AuctionItemId
where di.UseDigitalCertificateForWinner=1
  and IsDeleted=0


--duplicate unpaid bidders to clean up
select bu.biddernumber as NumToDelete, bu.FirstName, bu.LastName, bu.Email, bp.BidderNumber as NumToKeep
from Bidders bu
inner join Bidders bp on bp.LastName = bu.LastName and bp.BidderId <> bu.BidderId
where bu.IsDeleted = 0
 and exists (select * from AuctionGuests ag where ag.Bidder_BidderId=bu.BidderId and ag.TicketPricePaid is null) --has unpaid ticket
 and exists (select * from AuctionGuests ag where ag.Bidder_BidderId=bp.BidderId and ag.TicketPricePaid is not null) --has paid tickets on other bidder
 and bu.IsPaymentReminderSent=1


--Fund-a-Project donations
select i.FirstName, i.LastName, i.Email, si.Title, sip.PricePaid, sip.Quantity, 'Store' as PurchaseType
from StoreItemPurchases sip
inner join StoreItems si on sip.StoreItem_StoreItemID = si.StoreItemID
inner join Invoices i on sip.invoice_invoiceid=i.invoiceid
where i.ispaid=1
  and si.description like 'Fund-A-Project%'
UNION ALL
select i.FirstName, i.LastName, i.Email, ai.Title, b.AmountPaid, 1 as Quantity, 'Auction' as PurchaseType
from AuctionItems ai
inner join Bids b on b.AuctionItem_AuctionItemId=ai.AuctionItemId
inner join Invoices i on b.Invoice_InvoiceId = i.InvoiceId
where ai.Title like '%Fund-a-Project%'
 and b.IsWinning=1
 and i.IsPaid=1
 
--Fund-a-Project donations duplicated by homeroom teacher of bidder
select i.FirstName, i.LastName, i.Email, si.Title, sip.PricePaid, sip.Quantity, 'Store' as PurchaseType, s.HomeroomTeacher
from StoreItemPurchases sip
inner join StoreItems si on sip.StoreItem_StoreItemID = si.StoreItemID
inner join Invoices i on sip.invoice_invoiceid=i.invoiceid
left join Students s on s.Bidder_BidderId = i.Bidder_BidderId
where i.ispaid=1
  and si.description like 'Fund-A-Project%'
UNION ALL
select i.FirstName, i.LastName, i.Email, ai.Title, b.AmountPaid, 1 as Quantity, 'Auction' as PurchaseType, s.HomeroomTeacher
from AuctionItems ai
inner join Bids b on b.AuctionItem_AuctionItemId=ai.AuctionItemId
inner join Invoices i on b.Invoice_InvoiceId = i.InvoiceId
left join Students s on s.Bidder_BidderId = i.Bidder_BidderId
where ai.Title like '%Fund-a-Project%'
 and b.IsWinning=1
 and i.IsPaid=1



--Teacher Treasure winners
select ai.UniqueItemNumber, ai.Title, d.email as DonorEmail, i.biddernumber, i.firstname, i.lastname, i.email as BidderEmail, b.bidamount, b.amountpaid
from Bids b
inner join AuctionItems ai on ai.auctionitemid = b.auctionitem_auctionitemid
inner join AuctionCategories ac on ac.auctioncategoryid = ai.category_auctioncategoryid
inner join Bidders i on i.bidderid = b.bidder_bidderid
inner join DonationItems di on di.auctionitem_auctionitemid = ai.auctionitemid
inner join Donors d on di.donor_donorid = d.donorid
where b.iswinning = 1
  and ac.Name='Teacher Treasure'
  
--GLOBE Perks winners last year
select ai.UniqueItemNumber, ai.Title, d.email as DonorEmail, i.biddernumber, i.firstname, i.lastname, i.email as BidderEmail, b.bidamount, b.amountpaid
from [2020_Bids] b
inner join [2020_AuctionItems] ai on ai.auctionitemid = b.auctionitem_auctionitemid
inner join [2020_AuctionCategories] ac on ac.auctioncategoryid = ai.category_auctioncategoryid
inner join [2020_Bidders] i on i.bidderid = b.bidder_bidderid
inner join [2020_DonationItems] di on di.auctionitem_auctionitemid = ai.auctionitemid
inner join [2020_Donors] d on di.donor_donorid = d.donorid
where b.iswinning = 1
  and ac.Name='GLOBE Perks'
  
--GLOBE Perks that didn't sell last year
select ai.UniqueItemNumber, ai.Title, d.email as DonorEmail
from [2020_AuctionItems] ai
inner join [2020_AuctionCategories] ac on ac.auctioncategoryid = ai.category_auctioncategoryid
inner join [2020_DonationItems] di on di.auctionitem_auctionitemid = ai.auctionitemid
inner join [2020_Donors] d on di.donor_donorid = d.donorid
left join [2020_Bids] b on ai.auctionitemid = b.auctionitem_auctionitemid and b.IsWinning=0
where ac.Name='GLOBE Perks'
 and b.BidId is null

--all winners from a year
select ac.Name as CategoryName, ai.UniqueItemNumber, ai.Title, d.email as DonorEmail, i.biddernumber, i.firstname, i.lastname, i.email as BidderEmail, b.bidamount, b.amountpaid
from [2019_Bids] b
inner join [2019_AuctionItems] ai on ai.auctionitemid = b.auctionitem_auctionitemid
inner join [2019_AuctionCategories] ac on ac.auctioncategoryid = ai.category_auctioncategoryid
inner join [2019_Bidders] i on i.bidderid = b.bidder_bidderid
inner join [2019_DonationItems] di on di.auctionitem_auctionitemid = ai.auctionitemid
inner join [2019_Donors] d on di.donor_donorid = d.donorid
where b.iswinning = 1
  and ac.Name='Live'

select ai.UniqueItemNumber, ai.Title, b.bidamount, b.amountpaid, di.Title, di.Description, di.DollarValue, di.UseDigitalCertificateForWinner, d.*
from DonationItems di
inner join Donors d on di.donor_donorid = d.donorid
inner join AuctionItems ai on ai.AuctionItemId = di.AuctionItem_AuctionItemId
inner join Bids b on b.AuctionItem_AuctionItemId = ai.AuctionItemId and b.IsWinning=1