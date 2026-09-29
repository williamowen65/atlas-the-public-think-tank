-- Occasionally local test fail and don't clean up the DB they were using..
-- Instead of manually deleting them, you can use this script
-- SEE line that mentions UNCOMMENT 

USE master;
GO

DECLARE @db sysname;
DECLARE @sql nvarchar(max);

DECLARE db_cursor CURSOR FOR
    SELECT name
    FROM sys.databases
    WHERE name LIKE 'atlas_the_public_think_tank_testing%';

OPEN db_cursor;
FETCH NEXT FROM db_cursor INTO @db;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Build a statement that forces the database into SINGLE_USER and drops it
    SET @sql = N'ALTER DATABASE ' + QUOTENAME(@db) +
               N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; ' +
               N'DROP DATABASE ' + QUOTENAME(@db) + N';';

    PRINT @sql;               -- ← Review these commands first
    -- EXEC sp_executesql @sql; -- ← UNCOMMENT ONLY when you are 100% sure

    FETCH NEXT FROM db_cursor INTO @db;
END

CLOSE db_cursor;
DEALLOCATE db_cursor;
