

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