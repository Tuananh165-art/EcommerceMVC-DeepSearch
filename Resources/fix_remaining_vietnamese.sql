/* Quick patch for remaining Vietnamese mojibake after previous conversion */
SET NOCOUNT ON;

-- 1) Targeted fix for warranty field first
UPDATE dbo.HangHoa
SET BaoHanh = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(BaoHanh,
    N'thẠng', N'tháng'),
    N'ThẠng', N'Tháng'),
    N'thÁng', N'tháng'),
    N'THẠNG', N'THÁNG'),
    N'th�ng', N'tháng')
WHERE BaoHanh IS NOT NULL
  AND (
      BaoHanh LIKE N'%Ạng%'
      OR BaoHanh LIKE N'%Áng%'
      OR BaoHanh LIKE N'%�ng%'
      OR BaoHanh LIKE N'%th%ng%'
  );

-- 2) Generic fix for all NVARCHAR columns with common broken forms
DECLARE @sql NVARCHAR(MAX) = N'';

;WITH cte AS (
    SELECT s.name AS SchemaName, t.name AS TableName, c.name AS ColumnName
    FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    JOIN sys.columns c ON c.object_id = t.object_id
    JOIN sys.types ty ON c.user_type_id = ty.user_type_id
    WHERE t.is_ms_shipped = 0
      AND ty.name IN ('nvarchar', 'nchar')
      AND c.is_computed = 0
)
SELECT @sql = @sql + N'
UPDATE ' + QUOTENAME(SchemaName) + N'.' + QUOTENAME(TableName) + N'
SET ' + QUOTENAME(ColumnName) + N' = REPLACE(REPLACE(REPLACE(REPLACE(' + QUOTENAME(ColumnName) + N',
    N''thẠng'', N''tháng''),
    N''ThẠng'', N''Tháng''),
    N''thÁng'', N''tháng''),
    N''THẠNG'', N''THÁNG'')
WHERE ' + QUOTENAME(ColumnName) + N' IS NOT NULL
  AND (' + QUOTENAME(ColumnName) + N' LIKE N''%thẠng%'' OR ' + QUOTENAME(ColumnName) + N' LIKE N''%ThẠng%'' OR ' + QUOTENAME(ColumnName) + N' LIKE N''%thÁng%'' OR ' + QUOTENAME(ColumnName) + N' LIKE N''%THẠNG%'');
'
FROM cte;

EXEC sp_executesql @sql;

-- Verify sample problem field
SELECT TOP 20 MaHH, TenHH, BaoHanh
FROM dbo.HangHoa
WHERE BaoHanh IS NOT NULL
ORDER BY MaHH DESC;
