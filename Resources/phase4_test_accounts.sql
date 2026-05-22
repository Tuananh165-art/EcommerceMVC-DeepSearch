SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRAN;

    UPDATE dbo.KhachHang
    SET
        HoTen = N'Administrator',
        GioiTinh = CAST(1 AS bit),
        NgaySinh = CAST('1990-01-01' AS datetime),
        DiaChi = N'Admin Address',
        DienThoai = N'0900000000',
        Email = N'admin@hshop.local',
        Hinh = N'Photo.gif',
        MatKhau = N'04376e05653ab6b9576eafa1b88e6e0b',
        RandomKey = N'A1B2C3',
        HieuLuc = CAST(1 AS bit),
        VaiTro = 1
    WHERE MaKH = N'admin';

    IF @@ROWCOUNT = 0
    BEGIN
        INSERT INTO dbo.KhachHang
        (
            MaKH, HoTen, GioiTinh, NgaySinh, DiaChi, DienThoai, Email,
            Hinh, MatKhau, RandomKey, HieuLuc, VaiTro
        )
        VALUES
        (
            N'admin', N'Administrator', CAST(1 AS bit), CAST('1990-01-01' AS datetime),
            N'Admin Address', N'0900000000', N'admin@hshop.local',
            N'Photo.gif', N'04376e05653ab6b9576eafa1b88e6e0b', N'A1B2C3', CAST(1 AS bit), 1
        );
    END;

    UPDATE dbo.KhachHang
    SET
        HoTen = N'Sample Customer 01',
        GioiTinh = CAST(1 AS bit),
        NgaySinh = CAST('1998-05-10' AS datetime),
        DiaChi = N'123 Nguyen Hue, District 1, HCMC',
        DienThoai = N'0911000001',
        Email = N'customer01@hshop.local',
        Hinh = N'Photo.gif',
        MatKhau = N'cb32f18ad3205e52acd78e77d572adbb',
        RandomKey = N'C1D2E3',
        HieuLuc = CAST(1 AS bit),
        VaiTro = 0
    WHERE MaKH = N'customer01';

    IF @@ROWCOUNT = 0
    BEGIN
        INSERT INTO dbo.KhachHang
        (
            MaKH, HoTen, GioiTinh, NgaySinh, DiaChi, DienThoai, Email,
            Hinh, MatKhau, RandomKey, HieuLuc, VaiTro
        )
        VALUES
        (
            N'customer01', N'Sample Customer 01', CAST(1 AS bit), CAST('1998-05-10' AS datetime),
            N'123 Nguyen Hue, District 1, HCMC', N'0911000001', N'customer01@hshop.local',
            N'Photo.gif', N'cb32f18ad3205e52acd78e77d572adbb', N'C1D2E3', CAST(1 AS bit), 0
        );
    END;

    COMMIT TRAN;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    THROW;
END CATCH;
