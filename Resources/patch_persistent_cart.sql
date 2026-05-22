IF OBJECT_ID('dbo.GioHangItem', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.GioHangItem
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaKH NVARCHAR(20) NOT NULL,
        MaHH INT NOT NULL,
        SoLuong INT NOT NULL CONSTRAINT DF_GioHangItem_SoLuong DEFAULT (1),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_GioHangItem_CreatedAt DEFAULT (GETDATE()),
        UpdatedAt DATETIME NOT NULL CONSTRAINT DF_GioHangItem_UpdatedAt DEFAULT (GETDATE())
    );

    CREATE UNIQUE INDEX UX_GioHangItem_MaKH_MaHH
        ON dbo.GioHangItem(MaKH, MaHH);

    ALTER TABLE dbo.GioHangItem
        ADD CONSTRAINT FK_GioHangItem_KhachHang
            FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH)
            ON DELETE CASCADE;

    ALTER TABLE dbo.GioHangItem
        ADD CONSTRAINT FK_GioHangItem_HangHoa
            FOREIGN KEY (MaHH) REFERENCES dbo.HangHoa(MaHH)
            ON DELETE CASCADE;
END;
