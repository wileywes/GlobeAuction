

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
