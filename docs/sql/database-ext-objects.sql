/*
Special Audit functions


This script is powerful for analyzing table structures 
and could be a valuable tool for database administrators
 or developers looking to audit or optimize a database.
*/

GO
-- helper function for sp_GetDatabaseStats

DROP FUNCTION IF EXISTS dbo.GetPrimaryKeyColumns;
GO
CREATE FUNCTION dbo.GetPrimaryKeyColumns
(
    @TableIdentifier VARCHAR(255) -- Format: SchemaName.TableName
)
RETURNS VARCHAR(MAX)
AS
BEGIN
    DECLARE @PrimaryKeys VARCHAR(MAX);

    -- Extract schema and table name from the input parameter
    DECLARE @SchemaName NVARCHAR(128);
    DECLARE @TableName NVARCHAR(128);
    
    -- Split the @TableIdentifier into schema and table name
    SET @SchemaName = PARSENAME(@TableIdentifier, 2);
    SET @TableName = PARSENAME(@TableIdentifier, 1);
    
    -- Query to get primary key columns as a comma-separated list
    SELECT @PrimaryKeys = STRING_AGG(COLUMN_NAME, ', ')
    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
    WHERE TABLE_SCHEMA = @SchemaName
      AND TABLE_NAME = @TableName
      AND CONSTRAINT_NAME = (
          SELECT CONSTRAINT_NAME
          FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
          WHERE TABLE_SCHEMA = @SchemaName
            AND TABLE_NAME = @TableName
            AND CONSTRAINT_TYPE = 'PRIMARY KEY'
      );
    
    RETURN @PrimaryKeys;
END;
GO
--  helper function for sp_GetDatabaseStats

-- Drop the function if it already exists
DROP FUNCTION IF EXISTS dbo.GetForeignKeyColumns;
GO

-- Create the function to return foreign key columns
CREATE FUNCTION dbo.GetForeignKeyColumns
(
    @TableIdentifier VARCHAR(255) -- Format: SchemaName.TableName
)
RETURNS VARCHAR(MAX)
AS
BEGIN
    DECLARE @ForeignKeys VARCHAR(MAX);

    -- Extract schema and table name from the input parameter
    DECLARE @SchemaName NVARCHAR(128);
    DECLARE @TableName NVARCHAR(128);
    
    -- Split the @TableIdentifier into schema and table name
    SET @SchemaName = PARSENAME(@TableIdentifier, 2);
    SET @TableName = PARSENAME(@TableIdentifier, 1);
    
    -- Query to get foreign key columns as a comma-separated list
    SELECT @ForeignKeys = STRING_AGG(COLUMN_NAME, ', ')
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS r
    JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE k
        ON r.CONSTRAINT_NAME = k.CONSTRAINT_NAME
    WHERE k.TABLE_SCHEMA = @SchemaName
      AND k.TABLE_NAME = @TableName;

    RETURN @ForeignKeys;
END;


/*
Obtain a table of database tables with rowCount, ColumnCount, DatumCount, Primary Keys, Foreign Keys
*/

GO
DROP PROCEDURE IF EXISTS dbo.sp_GetDatabaseStats;
GO

CREATE PROCEDURE dbo.sp_GetDatabaseStats
AS
BEGIN

	PRINT 'Running Audit: sp_GetDatabaseStats';
    -- Create a temporary table to store the results
    CREATE TABLE #Results (
        TableName VARCHAR(255) NOT NULL,
        "RowCount" INT NOT NULL,
        ColumnCount INT NOT NULL
    );

    -- Construct the dynamic SQL query
    DECLARE @SQLQuery NVARCHAR(MAX) = N'';

    -- Generate the dynamic SQL to gather statistics from all tables
    SELECT @SQLQuery = STRING_AGG(
        N'INSERT INTO #Results (TableName, "RowCount", ColumnCount) ' +
        N'SELECT ''' + s.name + '.' + t.name + ''', ' +
        N'(SELECT COUNT(*) FROM ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ') AS "RowCount", ' +
        N'(SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = ''' + s.name + ''' AND TABLE_NAME = ''' + t.name + ''') AS ColumnCount ' +
        N'FROM ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ';'
    , ' ')
    FROM sys.tables t
    JOIN sys.schemas s ON t.schema_id = s.schema_id;

    -- Execute the generated SQL
    EXEC sp_executesql @SQLQuery;

    -- Create the final results table to store extended data
    DROP TABLE IF EXISTS DatabaseStats;
    CREATE TABLE DatabaseStats (
        TableName VARCHAR(255), 
        "RowCount" INT, 
        ColumnCount INT, 
        DatumCount INT,
        PrimaryKey VARCHAR(MAX),
        ForeignKey VARCHAR(MAX)
    );

    -- Insert data into the final table with calculated columns and foreign/primary keys
    INSERT INTO DatabaseStats (TableName, "RowCount", ColumnCount, DatumCount, PrimaryKey, ForeignKey)
    SELECT DISTINCT 
        TableName, 
        "RowCount", 
        ColumnCount, 
        DatumCount = "RowCount" * ColumnCount,
        PrimaryKey = dbo.GetPrimaryKeyColumns(TableName),
        ForeignKey = dbo.GetForeignKeyColumns(TableName)
    FROM #Results;

    -- Select the final results
    SELECT * FROM DatabaseStats;

    -- Clean up the temporary table
    DROP TABLE #Results;

    -- Optionally, print the dynamic SQL for debugging purposes
    -- PRINT @SQLQuery;
END;
GO
