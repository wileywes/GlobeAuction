select u.Email, u.EmailConfirmed,
  (select count(*) from AspNetUserRoles r where r.UserId=u.Id) as RoleCount
from AspNetUsers u
where (select count(*) from AspNetUserRoles r where r.UserId=u.Id) = 0

delete u
from AspNetUsers u
where (select count(*) from AspNetUserRoles r where r.UserId=u.Id) = 0


select *
from AspNetUsers u
left join AspNetUserRoles r on r.UserId=u.Id
where u.email='2ndwind07@gmail.com'