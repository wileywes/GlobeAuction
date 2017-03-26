select * from aspnetusers
select * from aspnetroles
select * from aspnetuserlogins
select * from aspnetuserroles

/*
delete from aspnetusers
*/

select * from AspNetUsers u
inner join AspNetUserRoles ur on u.id = ur.UserId
inner join AspNetRoles r on ur.RoleId = r.Id
where u.EmailConfirmed = 0

--mark unconfirmed accounts with roles as confirmed
update u set EmailConfirmed=1
from AspNetUsers u
inner join AspNetUserRoles ur on u.id = ur.UserId
inner join AspNetRoles r on ur.RoleId = r.Id
where u.EmailConfirmed = 0

