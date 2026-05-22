/*
    Fix mojibake Vietnamese text in SQL Server (UTF-8 interpreted as ANSI), e.g. "thÃ¡ng" -> "tháng"
    Run on database: Hshop2023
*/
SET NOCOUNT ON;

DECLARE @Pattern NVARCHAR(200) = N'%Ã%';

IF OBJECT_ID('tempdb..#MojibakeColumns') IS NOT NULL DROP TABLE #MojibakeColumns;
CREATE TABLE #MojibakeColumns
(
    SchemaName SYSNAME,
    TableName SYSNAME,
    ColumnName SYSNAME,
    BeforeCount INT NULL,
    UpdatedCount INT NULL
);

INSERT INTO #MojibakeColumns (SchemaName, TableName, ColumnName)
SELECT s.name, t.name, c.name
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
JOIN sys.columns c ON c.object_id = t.object_id
JOIN sys.types ty ON c.user_type_id = ty.user_type_id
WHERE t.is_ms_shipped = 0
  AND ty.name IN ('nvarchar', 'nchar', 'varchar', 'char')
  AND c.is_computed = 0;

DECLARE @Schema SYSNAME, @Table SYSNAME, @Column SYSNAME;
DECLARE @sql NVARCHAR(MAX);
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
SELECT SchemaName, TableName, ColumnName
FROM #MojibakeColumns;

OPEN cur;
FETCH NEXT FROM cur INTO @Schema, @Table, @Column;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'
        UPDATE m
        SET BeforeCount = x.Cnt
        FROM #MojibakeColumns m
        CROSS APPLY (
            SELECT COUNT(1) AS Cnt
            FROM ' + QUOTENAME(@Schema) + N'.' + QUOTENAME(@Table) + N'
            WHERE ' + QUOTENAME(@Column) + N' IS NOT NULL
              AND (
                    ' + QUOTENAME(@Column) + N' LIKE N''%Ã%''
                 OR ' + QUOTENAME(@Column) + N' LIKE N''%Â%''
                 OR ' + QUOTENAME(@Column) + N' LIKE N''%Ä%''
                 OR ' + QUOTENAME(@Column) + N' LIKE N''%áº%''
                 OR ' + QUOTENAME(@Column) + N' LIKE N''%á»%''
                  )
        ) x
        WHERE m.SchemaName = @Schema AND m.TableName = @Table AND m.ColumnName = @Column;

        UPDATE ' + QUOTENAME(@Schema) + N'.' + QUOTENAME(@Table) + N'
        SET ' + QUOTENAME(@Column) + N' = CONVERT(NVARCHAR(MAX), CONVERT(VARCHAR(MAX), ' + QUOTENAME(@Column) + N') COLLATE Latin1_General_100_CI_AS_SC_UTF8)
        WHERE ' + QUOTENAME(@Column) + N' IS NOT NULL
          AND (
                ' + QUOTENAME(@Column) + N' LIKE N''%Ã%''
             OR ' + QUOTENAME(@Column) + N' LIKE N''%Â%''
             OR ' + QUOTENAME(@Column) + N' LIKE N''%Ä%''
             OR ' + QUOTENAME(@Column) + N' LIKE N''%áº%''
             OR ' + QUOTENAME(@Column) + N' LIKE N''%á»%''
              );

        UPDATE m
        SET UpdatedCount = @@ROWCOUNT
        FROM #MojibakeColumns m
        WHERE m.SchemaName = @Schema AND m.TableName = @Table AND m.ColumnName = @Column;
    ';

    EXEC sp_executesql @sql, N'@Schema SYSNAME, @Table SYSNAME, @Column SYSNAME', @Schema, @Table, @Column;

    FETCH NEXT FROM cur INTO @Schema, @Table, @Column;
END

CLOSE cur;
DEALLOCATE cur;

SELECT SchemaName, TableName, ColumnName, BeforeCount, UpdatedCount
FROM #MojibakeColumns
WHERE ISNULL(BeforeCount, 0) > 0 OR ISNULL(UpdatedCount, 0) > 0
ORDER BY ISNULL(BeforeCount, 0) DESC, SchemaName, TableName, ColumnName;
