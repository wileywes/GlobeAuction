
--bidders with guests
select b.*, g.FirstName as GuestFirst, g.LastName as GuestLast, g.TicketType, g.TicketPricePaid
from Bidders b
inner join AuctionGuests g on b.bidderid = g.bidder_bidderid
inner join Students s on s.bidder_bidderid = b.bidderid
order by b.bidderid

--bidders with homerooms
select b.*, s.HomeroomTeacher
from Bidders b
inner join Students s on b.bidderid = s.bidder_bidderid
order by b.bidderid

