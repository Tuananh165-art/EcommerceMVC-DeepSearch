# ECommerceMVC - Project Structure Map (Chi tiet theo dang cay)

## 1) Tong quan solution

```text
F:\ECommerceMVC
|-- ECommerceMVC.sln                         # Solution chinh
|-- README.md                                # Mo ta du an, setup, tinh nang tong quan
|-- DEPLOY.md                                # Huong dan deploy
|-- .github/                                 # CI/CD, templates (neu co)
|-- Resources/                               # Tai nguyen bo sung ngoai source chinh
|-- ECommerceMVC/                            # Web app ASP.NET Core MVC (main)
|-- ECommerceMVC.Tests/                      # Du an test (hien dang chua co test case)
```

## 2) Cau truc chi tiet web app `ECommerceMVC/`

```text
ECommerceMVC/
|-- Program.cs                               # DI, middleware pipeline, session, route mac dinh, bootstrap schema
|-- ECommerceMVC.csproj                      # Cau hinh project + package references
|-- appsettings.json                         # Cau hinh mac dinh (connection/email/payment)
|-- appsettings.Development.json             # Override cho moi truong dev
|-- .env                                     # Bien moi truong local (DB/SMTP/VNPAY/...)
|-- .env.example                             # Mau .env cho local
|-- .env.production.example                  # Mau .env cho production
|-- Dockerfile                               # Build image app
|-- docker-compose.yml                       # Chay app + db bang compose
|-- deploy.sh                                # Script deploy
|-- docker-init-db.sh                        # Script khoi tao DB trong container
|-- HShopScript.sql                          # Script CSDL lon/co so du lieu goc
|-- seed_hshop2023.sql                       # Seed du lieu mau
|-- reset_sample_hshop2023.sql               # Reset data mau
|-- patch_*.sql                              # Patch schema/du lieu theo feature
|-- fix_*.sql                                # Script sua loi encoding/du lieu
|-- phase4_test_accounts.sql                 # Tai khoan test
|
|-- Controllers/                             # Tang dieu huong request + xu ly flow
|   |-- HomeController.cs                    # Trang chu, privacy, error, 404
|   |-- HangHoaController.cs                 # Shop catalog: filter, sort, paginate, detail, review, favourite
|   |-- CartController.cs                    # Gio hang, voucher, checkout, VNPAY return/IPN, lich su don hang
|   |-- KhachHangController.cs               # Dang ky/dang nhap, profile, upload avatar, forgot/reset password OTP
|   |-- PasswordRecoveryController.cs        # API style endpoint cho quy trinh OTP reset password
|   |-- AdminController.cs                   # Dashboard + CRUD product/category/order/customer/payment (admin)
|   |-- NewsletterController.cs              # Dang ky nhan ban tin
|   |-- LoaiController.cs                    # Upload hinh cho loai
|   |-- ShowroomController.cs                # Trang showroom 3D
|   `-- HangHoasController.cs                # Scaffold CRUD HangHoa (quan tri co ban/legacy)
|
|-- Data/                                    # Entity + DbContext EF Core
|   |-- Hshop2023Context.cs                  # DbSet, mapping table/column, relation
|   |-- HangHoa.cs                           # San pham
|   |-- Loai.cs                              # Danh muc
|   |-- KhachHang.cs                         # Tai khoan khach hang
|   |-- HoaDon.cs                            # Don hang
|   |-- ChiTietHd.cs                         # Dong san pham trong don hang
|   |-- Voucher.cs                           # Ma giam gia
|   |-- YeuThich.cs                          # Danh sach yeu thich
|   |-- ProductReview.cs                     # Danh gia san pham
|   |-- PasswordResetOtp.cs                  # OTP reset mat khau
|   |-- GioHangItem.cs                       # Gio hang persistent theo user
|   |-- NewsletterSubscription.cs            # Dang ky newsletter
|   |-- NhaCungCap.cs                        # Nha cung cap/thuong hieu
|   |-- TrangThai.cs                         # Trang thai don
|   |-- VChiTietHoaDon.cs                    # View/aggregate chi tiet hoa don
|   |-- NhanVien.cs, PhanCong.cs, PhanQuyen.cs, PhongBan.cs
|   |-- BanBe.cs, ChuDe.cs, GopY.cs, HoiDap.cs, TrangWeb.cs
|   `-- (cac entity khac)                    # Mo ta bang nghiep vu phu tro trong schema HShop
|
|-- Services/                                # Business services + interface
|   |-- ICatalogQueryService.cs / CatalogQueryService.cs
|   |   # Logic filter/sort/rating + search suggestion cho catalog
|   |-- IStockService.cs / StockService.cs
|   |   # Kiem tra ton kho, clamp so luong, tru kho khi dat hang
|   |-- IVoucherService.cs / VoucherService.cs
|   |   # Validate voucher, tinh discount
|   |-- IShippingFeeService.cs / ShippingFeeService.cs
|   |   # Tinh phi van chuyen
|   |-- IVnPayService.cs / VnPayService.cs
|   |   # Tao URL thanh toan VNPAY + xac minh callback/IPN
|   |-- IPaymentSandboxService.cs / PaymentSandboxService.cs
|   |   # Mo phong ket qua thanh toan (sandbox)
|   |-- IEmailService.cs / SmtpEmailService.cs
|   |   # Gui email qua SMTP
|   |-- IPasswordService.cs / PasswordService.cs
|   |   # Hash/verify mat khau, support legacy hash upgrade
|   |-- IPasswordResetService.cs / PasswordResetService.cs
|   |   # Tao OTP, verify OTP, issue/verify reset token
|   |-- EmailTemplates.cs                    # Mau email dang ky/dang nhap/dat hang
|   |-- PaymentGatewaySettings.cs            # Option model cho VNPAY/MoMo
|   |-- SmtpSettings.cs                      # Option model SMTP
|   |-- AdminSecuritySettings.cs             # Secret code cap role admin khi dang ky
|   |-- PendingOrderDraft.cs                 # Du lieu tam cho flow thanh toan online
|   |-- DotEnvLoader.cs                      # Nap bien moi truong tu file .env
|   `-- DbSchemaBootstrapper.cs              # Tu dong tao/cap nhat schema bo sung khi app start
|
|-- Helpers/                                 # Utility / metadata / extension
|   |-- MySetting.cs                         # Session key, role const
|   |-- MyUtil.cs                            # Upload file, tao key, utility chuoi
|   |-- SessionExtensions.cs                 # Set/Get object vao Session
|   |-- DataEncryptionExtensions.cs          # Ho tro ma hoa/giai ma du lieu
|   |-- AutoMapperProfile.cs                 # Mapping Entity <-> ViewModel
|   |-- AdminMetadataHelper.cs               # Parse/build metadata an/visible, parent/sort
|   |-- OrderStatusHelper.cs                 # Nhom trang thai: pending/shipping/completed/cancelled
|   `-- AppTime.cs                           # Helper thoi gian dung chung
|
|-- ViewModels/                              # DTO cho View
|   |-- HangHoaVM.cs                         # VM item san pham (gia, ton, da ban, rating, favourite)
|   |-- HangHoaFilterVM.cs                   # Input filter trang shop
|   |-- HangHoaListPageVM.cs                 # Tong hop list + filter + paging option
|   |-- CartItem.cs, CartModel.cs            # VM gio hang
|   |-- CheckoutVM.cs                        # VM checkout (khach hang, thanh toan, shipping, tong tien)
|   |-- LichSuDonHangVM.cs                   # VM lich su va chi tiet don
|   |-- DangNhapVM.cs, RegisterVM.cs         # VM auth
|   |-- ForgotPasswordVM.cs                  # VM yeu cau OTP
|   |-- VerifyOtpResetPasswordVM.cs          # VM xac nhan OTP + mat khau moi
|   |-- EditProfileVM.cs                     # VM cap nhat ho so/anh dai dien
|   |-- NewsletterSubscribeVM.cs             # VM newsletter
|   |-- SearchSuggestionVM.cs                # VM goi y tim kiem realtime
|   |-- MenuLoaiVM.cs                        # VM menu danh muc
|   `-- AdminViewModels.cs                   # Cum VM dashboard, product row/form, order row, customer row...
|
|-- ViewComponents/                          # Thanh phan UI tai su dung
|   |-- MenuLoaiViewComponent.cs             # Render menu danh muc
|   `-- CartViewComponent.cs                 # Render mini cart
|
|-- Views/                                   # Razor UI
|   |-- _ViewImports.cshtml                  # using/tag helpers dung chung
|   |-- _ViewStart.cshtml                    # layout mac dinh
|   |
|   |-- Home/
|   |   |-- Index.cshtml                     # Landing/homepage
|   |   |-- Privacy.cshtml
|   |   `-- PageNotFound.cshtml
|   |
|   |-- HangHoa/
|   |   |-- Index.cshtml                     # Trang danh sach san pham (shop + filter)
|   |   |-- Detail.cshtml                    # Trang chi tiet san pham + review/favourite
|   |   `-- ProductItem.cshtml               # Partial card/list item
|   |
|   |-- Cart/
|   |   |-- Index.cshtml                     # Trang gio hang
|   |   |-- Checkout.cshtml                  # Form thanh toan
|   |   |-- CheckoutSuccess.cshtml           # Ket qua dat hang thanh cong
|   |   |-- LichSuDonHang.cshtml             # Lich su don cua user
|   |   `-- ChiTietDonHang.cshtml            # Chi tiet 1 don
|   |
|   |-- KhachHang/
|   |   |-- DangNhap.cshtml                  # Login
|   |   |-- DangKy.cshtml                    # Register
|   |   |-- EditProfile.cshtml               # Ho so + upload avatar AJAX
|   |   |-- ForgotPassword.cshtml            # Yeu cau OTP
|   |   `-- ResetPasswordWithOtp.cshtml      # Xac minh OTP + dat lai mat khau
|   |
|   |-- Admin/
|   |   |-- _AdminLayout.cshtml              # Layout admin
|   |   |-- Index.cshtml                     # Dashboard KPI + chart
|   |   |-- Products.cshtml / ProductForm.cshtml
|   |   |-- Categories.cshtml / CategoryForm.cshtml
|   |   |-- Orders.cshtml / OrderDetails.cshtml
|   |   |-- Customers.cshtml / CustomerDetails.cshtml
|   |   `-- Payments.cshtml
|   |
|   |-- Shared/
|   |   |-- _Layout.cshtml                   # Layout chung co ban
|   |   |-- _LayoutCustomer.cshtml           # Layout front-office
|   |   |-- _CustomerHead.cshtml             # Head resources cho customer theme
|   |   |-- _ModernNavbar.cshtml             # Navbar modern
|   |   |-- _CustomerFooter.cshtml           # Footer
|   |   |-- _CustomerSidebar.cshtml          # Sidebar danh muc/filter
|   |   |-- _CustomerAccountNav.cshtml       # Menu tai khoan
|   |   |-- _CustomerSearchOverlay.cshtml    # Overlay tim kiem
|   |   |-- _CustomerNewsletter.cshtml       # Block newsletter
|   |   |-- _CustomerScripts.cshtml          # Script chung customer
|   |   |-- _CustomerFlashMessage.cshtml     # Toast/alert message
|   |   |-- _ValidationScriptsPartial.cshtml # jQuery validation partial
|   |   |-- Error.cshtml                     # Trang loi
|   |   `-- Components/                      # View cho ViewComponent
|   |
|   |-- Showroom/Index.cshtml                # Trang nhung showroom 3D
|   |-- Loai/UploadHinh.cshtml               # Upload hinh loai
|   `-- HangHoas/*.cshtml                    # Scaffold CRUD legacy cho HangHoa
|
|-- wwwroot/                                 # Static files
|   |-- css/                                 # modern-theme.css, modern-animations.css, auth-luxury.css...
|   |-- js/                                  # modern-app.js, modern-icons.js...
|   |-- Hinh/                                # Anh product/category/customer upload
|   |-- showroom3d/                          # Asset + html module showroom
|   |-- amado/                               # Theme asset cu/nguon template
|   |-- lib/                                 # Thu vien frontend ben thu 3 (bootstrap/jquery/...)
|   |-- img/, scss/                          # Tai nguyen giao dien
|   `-- *.html                               # File mau/debug static tham khao
|
|-- docs/plans/admin-console-full-implementation.md
|   # Tai lieu ke hoach phat trien admin console
|
|-- Properties/launchSettings.json           # Profile chay local
|-- Models/ErrorViewModel.cs                 # VM cho trang Error
|-- bin/, obj/                               # Build artifacts (tu dong sinh, khong sua tay)
```

## 3) Workflow nghiep vu chinh

### 3.1 Duyet va tim san pham (Catalog)
1. User vao `HangHoaController.Index`.
2. He thong doc filter (loai, gia, brand, material, style, color, rating, favourite, sort, page).
3. Query `HangHoas` + relation, loai bo san pham bi an qua `AdminMetadataHelper`.
4. Tinh them metric: `SoLuongDaBan`, diem danh gia TB, trang thai yeu thich.
5. Day vao `HangHoaListPageVM` va render `Views/HangHoa/Index.cshtml`.

### 3.2 Auth + profile
1. Dang ky/dang nhap qua `KhachHangController`.
2. Mat khau hash boi `PasswordService`, session luu `CUSTOMER_KEY`.
3. Dang nhap thanh cong co merge gio session vao gio persistent.
4. Quen mat khau dung OTP (`PasswordResetService`) + email SMTP.
5. Profile update + upload avatar theo endpoint rieng de tranh va cham validation form.

### 3.3 Gio hang -> Checkout -> Don hang
1. Add/update/remove item qua `CartController` + `SessionExtensions`.
2. Validate ton kho (`StockService`) truoc checkout.
3. Ap voucher (`VoucherService`), tinh ship (`ShippingFeeService`).
4. Chon COD/sandbox/VNPAY; VNPAY dung `VnPayService` tao URL va verify callback/IPN.
5. Dat don thanh cong: tao `HoaDon` + `ChiTietHd`, tru ton kho, gui email xac nhan.
6. User xem `LichSuDonHang` va `ChiTietDonHang`.

### 3.4 Quan tri admin
1. `AdminController.OnActionExecuting` chan truy cap neu khong phai admin.
2. Dashboard `Index`: KPI tong quan, doanh thu ngay/thang, top san pham, canh bao ton.
3. Quan ly san pham/danh muc: CRUD, an/hien, sap xep, metadata.
4. Quan ly don: doi trang thai, thao tac nhanh (refund/fail...), xem chi tiet.
5. Quan ly khach hang va thanh toan: khoa/mo, doi role, danh dau giao dich loi.

## 4) Thanh phan can biet de maintain
- `Program.cs` + `Services/*Settings*`: diem vao cau hinh runtime.
- `Data/Hshop2023Context.cs`: tac dong den toan bo truy van DB.
- `Controllers/CartController.cs` + `Services/*Payment*`, `*Stock*`, `*Voucher*`: luong dat hang quan trong nhat.
- `Controllers/AdminController.cs` + `ViewModels/AdminViewModels.cs`: logic dashboard/quan tri lon nhat.
- `Views/Shared/_LayoutCustomer.cshtml` + `wwwroot/css/modern-theme.css`: bo khung giao dien customer.

## 5) Ghi chu pham vi
- Tai lieu nay mo ta "file/folder co y nghia nghiep vu".
- Thu muc build (`bin/`, `obj/`), cache IDE (`.vs/`), git metadata (`.git/`) va vendor static (`wwwroot/lib/`) duoc ghi nhan o muc he thong, khong phan tich tung file con vi khong chua logic domain.
