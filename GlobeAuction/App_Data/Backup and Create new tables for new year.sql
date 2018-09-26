/*
select t.name,
  (select count(*) from sys.foreign_keys where referenced_object_id=t.object_id) as FksReferencingTable,
  'select * into 2018_' + name + ' from ' + name as BackupScript,
  'DELETE from ' + name as DeleteScript,
  'EXEC sp_rename ''' + name + ''', ''2017_' + replace(name, '_2017','') + ''';' as RenameScript
from sys.tables t
where [name] not in ('TicketTypes','StoreItems','__MigrationHistory')
 and name not like 'AspNet%'
order by (select count(*) from sys.foreign_keys where referenced_object_id=t.object_id) asc


--run the SELECT INTO statements

EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"

--run the DELETE statements

exec sp_MSforeachtable @command1="print '?'", @command2="ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"


*/
