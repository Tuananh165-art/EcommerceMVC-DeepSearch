IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_YeuThich_MaKH_MaHH'
      AND object_id = OBJECT_ID('dbo.YeuThich')
)
BEGIN
    CREATE UNIQUE INDEX UX_YeuThich_MaKH_MaHH
        ON dbo.YeuThich (MaKH, MaHH)
        WHERE MaKH IS NOT NULL AND MaHH IS NOT NULL;
END
GO
