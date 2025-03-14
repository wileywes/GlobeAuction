--see accounts that have no roles assigned
select u.Email, u.EmailConfirmed,
  (select count(*) from AspNetUserRoles r where r.UserId=u.Id) as RoleCount
from AspNetUsers u
where (select count(*) from AspNetUserRoles r where r.UserId=u.Id) = 0

--remove accounts that have no roles assigned
delete u
from AspNetUsers u
where (select count(*) from AspNetUserRoles r where r.UserId=u.Id) = 0


--see accounts that have not logged in for 2+ years
select u.Email, u.EmailConfirmed,
  (select count(*) from AspNetUserRoles r where r.UserId=u.Id) as RoleCount
from AspNetUsers u
where u.LastLogin < dateadd(year, -2, getutcdate())

--remove accounts that have no roles assigned
delete u
from AspNetUsers u
where u.LastLogin < dateadd(year, -2, getutcdate())


select *
from AspNetUsers u
left join AspNetUserRoles r on r.UserId=u.Id
where u.email='2ndwind07@gmail.com'