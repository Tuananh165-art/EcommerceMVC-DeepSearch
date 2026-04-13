SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRAN;

    ------------------------------------------------------------
    -- 1) Loai (Categories)
    ------------------------------------------------------------
    SET IDENTITY_INSERT dbo.Loai ON;

    MERGE dbo.Loai AS target
    USING (VALUES
        (1, N'Điện tử', N'dien-tu', N'Thiết bị công nghệ và phụ kiện', N'cat-electronics.jpg'),
        (2, N'Gia dụng', N'gia-dung', N'Đồ dùng trong gia đình', N'cat-home.jpg'),
        (3, N'Thời trang', N'thoi-trang', N'Quần áo và phụ kiện thời trang', N'cat-fashion.jpg'),
        (4, N'Sức khỏe', N'suc-khoe', N'Sản phẩm chăm sóc sức khỏe', N'cat-health.jpg'),
        (5, N'Sách', N'sach', N'Sách và tài liệu học tập', N'cat-book.jpg')
    ) AS src (MaLoai, TenLoai, TenLoaiAlias, MoTa, Hinh)
    ON target.MaLoai = src.MaLoai
    WHEN MATCHED THEN
        UPDATE SET
            TenLoai = src.TenLoai,
            TenLoaiAlias = src.TenLoaiAlias,
            MoTa = src.MoTa,
            Hinh = src.Hinh
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (MaLoai, TenLoai, TenLoaiAlias, MoTa, Hinh)
        VALUES (src.MaLoai, src.TenLoai, src.TenLoaiAlias, src.MoTa, src.Hinh);

    SET IDENTITY_INSERT dbo.Loai OFF;

    ------------------------------------------------------------
    -- 2) NhaCungCap (Suppliers)
    ------------------------------------------------------------
    MERGE dbo.NhaCungCap AS target
    USING (VALUES
        (N'NCC001', N'Công ty Thiết bị Sao Việt', N'ncc-saoviet.png', N'Nguyễn Văn A', N'saoviet@example.com', N'0901000001', N'Q1, TP.HCM', N'Nhà cung cấp chính ngành điện tử'),
        (N'NCC002', N'Công ty Gia dụng Minh An', N'ncc-minhan.png', N'Trần Thị B', N'minhan@example.com', N'0901000002', N'Thủ Đức, TP.HCM', N'Nhà cung cấp sản phẩm gia dụng'),
        (N'NCC003', N'Công ty Thời trang VinaStyle', N'ncc-vinastyle.png', N'Lê Văn C', N'vinastyle@example.com', N'0901000003', N'Bình Thạnh, TP.HCM', N'Nhà cung cấp thời trang')
    ) AS src (MaNCC, TenCongTy, Logo, NguoiLienLac, Email, DienThoai, DiaChi, MoTa)
    ON target.MaNCC = src.MaNCC
    WHEN MATCHED THEN
        UPDATE SET
            TenCongTy = src.TenCongTy,
            Logo = src.Logo,
            NguoiLienLac = src.NguoiLienLac,
            Email = src.Email,
            DienThoai = src.DienThoai,
            DiaChi = src.DiaChi,
            MoTa = src.MoTa
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (MaNCC, TenCongTy, Logo, NguoiLienLac, Email, DienThoai, DiaChi, MoTa)
        VALUES (src.MaNCC, src.TenCongTy, src.Logo, src.NguoiLienLac, src.Email, src.DienThoai, src.DiaChi, src.MoTa);

    ------------------------------------------------------------
    -- 3) TrangThai (Order Status)
    ------------------------------------------------------------
    MERGE dbo.TrangThai AS target
    USING (VALUES
        (0, N'Mới tạo', N'Đơn hàng mới được tạo'),
        (1, N'Chờ xác nhận', N'Đơn đang chờ nhân viên xác nhận'),
        (2, N'Đang giao', N'Đơn đang được vận chuyển'),
        (3, N'Hoàn tất', N'Đơn giao thành công'),
        (4, N'Đã hủy', N'Đơn đã bị hủy')
    ) AS src (MaTrangThai, TenTrangThai, MoTa)
    ON target.MaTrangThai = src.MaTrangThai
    WHEN MATCHED THEN
        UPDATE SET
            TenTrangThai = src.TenTrangThai,
            MoTa = src.MoTa
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (MaTrangThai, TenTrangThai, MoTa)
        VALUES (src.MaTrangThai, src.TenTrangThai, src.MoTa);

    ------------------------------------------------------------
    -- 4) HangHoa (Products)
    ------------------------------------------------------------
    SET IDENTITY_INSERT dbo.HangHoa ON;

    MERGE dbo.HangHoa AS target
    USING (VALUES
        (1, N'Laptop Văn Phòng A15', N'laptop-van-phong-a15', 1, N'Chiếc', 15990000.0, N'laptop-a15.jpg', CAST('2025-01-01' AS datetime), 0.0, 0, N'Laptop 15 inch phù hợp văn phòng', N'NCC001'),
        (2, N'Tai nghe Bluetooth X2', N'tai-nghe-bluetooth-x2', 1, N'Cái', 790000.0, N'tainghe-x2.jpg', CAST('2025-01-15' AS datetime), 0.0, 0, N'Tai nghe không dây pin 20 giờ', N'NCC001'),
        (3, N'Nồi chiên không dầu 5L', N'noi-chien-khong-dau-5l', 2, N'Cái', 1890000.0, N'noi-chien-5l.jpg', CAST('2025-02-01' AS datetime), 0.0, 0, N'Nồi chiên 5 lít, điều khiển cơ', N'NCC002'),
        (4, N'Máy xay sinh tố MX-300', N'may-xay-sinh-to-mx300', 2, N'Cái', 950000.0, N'may-xay-mx300.jpg', CAST('2025-02-10' AS datetime), 0.0, 0, N'Máy xay 3 cối đa năng', N'NCC002'),
        (5, N'Áo thun nam basic', N'ao-thun-nam-basic', 3, N'Cái', 199000.0, N'ao-thun-basic.jpg', CAST('2025-03-01' AS datetime), 0.0, 0, N'Áo thun cotton thoáng mát', N'NCC003'),
        (6, N'Quần jean nữ slim fit', N'quan-jean-nu-slim-fit', 3, N'Cái', 459000.0, N'quan-jean-slim.jpg', CAST('2025-03-05' AS datetime), 0.0, 0, N'Jean co giãn nhẹ, form slim', N'NCC003'),
        (7, N'Vitamin C 1000mg', N'vitamin-c-1000mg', 4, N'Hộp', 320000.0, N'vitamin-c.jpg', CAST('2025-03-15' AS datetime), 0.0, 0, N'Hỗ trợ tăng đề kháng', N'NCC001'),
        (8, N'Sách C# Thực chiến', N'sach-csharp-thuc-chien', 5, N'Quyển', 185000.0, N'sach-csharp.jpg', CAST('2025-03-20' AS datetime), 0.0, 0, N'Bài tập và dự án thực tế với C#', N'NCC001')
    ) AS src (MaHH, TenHH, TenAlias, MaLoai, MoTaDonVi, DonGia, Hinh, NgaySX, GiamGia, SoLanXem, MoTa, MaNCC)
    ON target.MaHH = src.MaHH
    WHEN MATCHED THEN
        UPDATE SET
            TenHH = src.TenHH,
            TenAlias = src.TenAlias,
            MaLoai = src.MaLoai,
            MoTaDonVi = src.MoTaDonVi,
            DonGia = src.DonGia,
            Hinh = src.Hinh,
            NgaySX = src.NgaySX,
            GiamGia = src.GiamGia,
            SoLanXem = src.SoLanXem,
            MoTa = src.MoTa,
            MaNCC = src.MaNCC
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (MaHH, TenHH, TenAlias, MaLoai, MoTaDonVi, DonGia, Hinh, NgaySX, GiamGia, SoLanXem, MoTa, MaNCC)
        VALUES (src.MaHH, src.TenHH, src.TenAlias, src.MaLoai, src.MoTaDonVi, src.DonGia, src.Hinh, src.NgaySX, src.GiamGia, src.SoLanXem, src.MoTa, src.MaNCC);

    SET IDENTITY_INSERT dbo.HangHoa OFF;

    COMMIT TRAN;

    SELECT N'[SEED_OK]' AS Msg, 'Loai' AS Tbl, COUNT(*) AS Cnt FROM dbo.Loai
    UNION ALL
    SELECT N'[SEED_OK]', 'NhaCungCap', COUNT(*) FROM dbo.NhaCungCap
    UNION ALL
    SELECT N'[SEED_OK]', 'TrangThai', COUNT(*) FROM dbo.TrangThai
    UNION ALL
    SELECT N'[SEED_OK]', 'HangHoa', COUNT(*) FROM dbo.HangHoa;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    DECLARE @Err NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(N'Seed failed: %s', 16, 1, @Err);
END CATCH;
