# ECommerceMVC audit + refactor implementation plan

> For Hermes: execute in phases, verify each phase before moving on. Do not bundle schema refactors, data reseeding, UI redesign, and admin expansion into one blind patch.

Goal: đưa project ECommerceMVC về trạng thái ecommerce đồng bộ hơn giữa UI, business flow, database, seed data, image assets, branding và admin tooling.

Architecture:
- Giữ ASP.NET Core MVC + EF Core + SQL Server hiện tại.
- Tách công việc thành 4 phase: audit/verify -> core business fixes -> schema/data cleanup -> admin and polish.
- Mọi thay đổi lớn về DB phải đi kèm script reset/seed mới và verify UI mapping.

Tech stack:
- ASP.NET Core MVC (.NET 9)
- EF Core SQL Server
- Razor views
- SQL seed/reset scripts in Resources/
- MailKit SMTP

---

## Phase 1 — Audit findings already verified

### 1. Favorite hiện đã lưu DB, không phải session
Files:
- Controllers/HangHoaController.cs
- Data/YeuThich.cs
- Data/Hshop2023Context.cs
- Views/HangHoa/ProductItem.cshtml

Verified:
- AddFavourite/RemoveFavourite thao tác trực tiếp với `db.YeuThiches`
- UI favorite state lấy từ bảng `YeuThich` theo `MaKh + MaHh`
- Session chỉ giữ `CUSTOMER_KEY`, không giữ danh sách favorite

Current gap:
- Nút favorite hiện chỉ submit form và reload page; màu active phụ thuộc render server-side
- Cần polish để khi ấn thấy đổi màu rõ ràng, tốt nhất bằng optimistic UI + fallback server render

### 2. Filter và sort cơ bản đã có code
Files:
- Controllers/HangHoaController.cs
- Views/HangHoa/Index.cshtml

Verified:
- Filter category, brand, min/max price có query logic
- Sort có: newest, popular, price_asc, price_desc
- View size có: 12/24/48/96

Current gaps:
- Chưa verify end-to-end runtime sau các patch UI gần đây
- Popular đang dựa vào `SoLanXem`, cần seed data thực tế hơn
- Không có automated regression test cho filter/sort

### 3. Password hashing đã có
Files:
- Services/PasswordService.cs
- Controllers/KhachHangController.cs

Verified:
- Password mới dùng `PasswordHasher<KhachHang>` của ASP.NET Identity
- Có support verify legacy MD5 + RandomKey rồi upgrade hash khi login thành công

Current gap:
- Seed SQL hiện vẫn đang chèn tài khoản admin/customer bằng legacy MD5 + RandomKey
- Chưa có seed chuẩn hoàn toàn bằng hashed format mới

### 4. Email đã có ở create account / login / checkout
Files:
- Controllers/KhachHangController.cs
- Controllers/CartController.cs
- Services/SmtpEmailService.cs
- Program.cs
- appsettings.json

Verified:
- Register: gửi email thông báo nếu SMTP cấu hình
- Login: gửi email thông báo nếu SMTP cấu hình
- Checkout: gửi email xác nhận đơn hàng nếu SMTP cấu hình

Current gap:
- `appsettings.json` đang để trống SMTP => feature chưa chạy thực tế
- Branding email vẫn là HShop
- Chưa có email templates riêng, chỉ inline HTML string

### 5. Dark mode vẫn còn trong hệ thống
Files:
- Views/Shared/_CustomerHead.cshtml
- Views/Shared/_ModernNavbar.cshtml
- wwwroot/js/modern-app.js
- wwwroot/css/modern-theme.css

Verified:
- Theme đọc `localStorage['hshop-theme']`
- Navbar có toggle sun/moon desktop + mobile
- CSS có `[data-theme="dark"]` block

Action:
- Nếu user muốn light-only, cần bỏ theme toggle + ép light theme trong head/app JS/CSS

### 6. Branding HSHOP còn nhiều chỗ
Verified from current code + config:
- Views/Shared/_ModernNavbar.cshtml
- Views/KhachHang/DangNhap.cshtml
- Views/Home/Index.cshtml
- KhachHangController.cs email subject/body
- CartController.cs email subject/body
- appsettings.json `FromName`
- likely more in shared/footer/login/register/copy

Action:
- Cần sweep toàn project `HShop|HSHOP|Premium Furniture` rồi thay bằng `DEEPSEARCH` theo ngữ cảnh

### 7. Admin hiện chưa có admin page riêng đầy đủ
Verified:
- Chỉ có check `VaiTro == ADMIN_ROLE` trong navbar/sidebar
- Admin path hiện nghiêng về scaffold CRUD `HangHoasController`
- Chưa có dashboard riêng tổng quan orders/products/customers/reviews/newsletter

### 8. Seed data + image đang chưa đồng bộ và chưa “ecommerce furniture-grade”
Verified from Resources scripts and wwwroot:
- `Resources/seed_hshop2023.sql` chỉ seed 8 products, 5 categories, 4 suppliers
- Category images như `cat-living.jpg`, `cat-minimal.jpg` được tham chiếu trong SQL nhưng chưa verify tồn tại tương ứng trong wwwroot
- Product names / prices / reused image names đang còn lệch logic:
  - duplicate angle products dùng lại `product1.jpg`, `product2.jpg`
  - giá một số item không hợp lý so với mô tả nội thất
- Many legacy assets tồn tại, naming lộn xộn, còn nhiều ảnh từ dataset cũ

### 9. Schema có phần legacy / dư thừa / chưa thật sạch cho ecommerce furniture
Verified DbSet list:
- Useful core: HangHoa, Loai, NhaCungCap, KhachHang, HoaDon, ChiTietHd, TrangThai, YeuThich, ProductReview, NewsletterSubscription
- Legacy/questionable for current storefront: BanBe, ChuDe, GopY, HoiDap, PhanCong, PhanQuyen, PhongBan, TrangWeb, VChiTietHoaDon, NhanVien

Risk:
- Không nên xóa table ngay khi chưa trace usage đầy đủ trong code + SQL dependencies
- Cần đánh dấu: core / admin / unused / legacy-support

---

## Phase 2 — High priority implementation order

### Task 1: Verify favorite UX and make active state obvious
Objective: nút heart đổi màu rõ ràng khi active và flow không gây khó hiểu cho user.
Files:
- Modify: Views/HangHoa/ProductItem.cshtml
- Modify: Views/HangHoa/Detail.cshtml
- Modify: wwwroot/css/modern-theme.css
- Optional: add AJAX endpoint in Controllers/HangHoaController.cs

Steps:
1. Confirm current `quick-action-btn active` CSS actually changes heart/background enough.
2. If too subtle, add stronger active styles:
   - red/rose heart
   - tinted background
   - border/shadow state
3. Decide implementation mode:
   - safe mode now: server POST + rerender
   - phase 2.1 optional: AJAX toggle for instant no-reload UX
4. Verify logged-in and logged-out behavior.

### Task 2: Remove dark mode and force light mode
Files:
- Modify: Views/Shared/_CustomerHead.cshtml
- Modify: Views/Shared/_ModernNavbar.cshtml
- Modify: wwwroot/js/modern-app.js
- Modify: wwwroot/css/modern-theme.css

Steps:
1. Force `data-theme="light"` in head.
2. Remove theme toggle buttons from navbar/mobile nav.
3. Remove theme toggle JS branch.
4. Keep light tokens only or neutralize dark overrides.
5. Re-test contrast on login/home/shop/detail/cart.

### Task 3: Rebrand HSHOP -> DEEPSEARCH
Files:
- Search all: `HShop|HSHOP|Premium Furniture|hshop-theme|admin@hshop.local|@hshop.local`
- Update UI copy, email subjects, SMTP FromName, login/register copy, navbar logo text

Note:
- Rename localStorage key as part of cleanup if desired (`deepsearch-theme`), or delete entirely with light-only mode.

### Task 4: Build dedicated admin area
Files (expected new):
- Controllers/AdminController.cs
- Views/Admin/Index.cshtml
- Views/Admin/Products.cshtml
- Views/Admin/Orders.cshtml
- Views/Admin/Customers.cshtml
- Views/Admin/Categories.cshtml
- ViewModels/Admin/*.cs

Minimum dashboard scope:
- summary cards: products, categories, orders, customers, favorites, reviews, newsletter subs
- recent orders table
- low stock list
- quick links to CRUD

### Task 5: Audit schema and classify tables
Output needed:
- core tables kept
- optional tables retained
- tables unused in app runtime
- tables to deprecate/remove later

Deliverable:
- schema review note with per-table purpose, fields, relationships, issues, recommendation

### Task 6: Redesign seed data + asset mapping
Files:
- Resources/reset_sample_hshop2023.sql
- Resources/seed_hshop2023.sql
- wwwroot/Hinh/HangHoa/*
- likely category/supplier image folders

Requirements:
- 20-40 products minimum
- meaningful categories for furniture/decor
- consistent supplier set
- realistic prices, descriptions, stock, view counts
- one product -> one main image filename that exists
- category images must exist for every category if UI uses them

### Task 7: Verify UI/DB sync matrix
Need matrix for:
- Product list card fields vs HangHoa columns
- Product detail fields vs HangHoa columns
- Checkout fields vs HoaDon/KhachHang columns
- Register/login/profile vs KhachHang columns
- Reviews vs ProductReview table
- Favorites vs YeuThich table
- Newsletter UI vs NewsletterSubscription table

### Task 8: Remove dead features/code
Candidates to inspect:
- legacy shared partials already deleted/replaced
- old sidebar/search/footer artifacts
- possible unused tables/controllers/views
- old image folders not referenced anymore

---

## Phase 3 — DB/schema detailed review checklist

For each table, verify:
- primary key correct?
- foreign keys correct?
- nullability matches business rule?
- data types right size?
- defaults reasonable?
- index/unique constraints needed?
- current code actually uses it?
- should remain in ecommerce scope?

Critical schema concerns already visible:
1. `HoaDon.CachThanhToan` and `CachVanChuyen` are free text strings.
   Recommendation: eventually normalize into lookup tables or constrained enums.
2. `HangHoa.Hinh` is single string only.
   Recommendation: add `ProductImage` table later if multi-image gallery needed.
3. `YeuThich` should ideally have unique constraint on `(MaKH, MaHH)`.
4. `ProductReview` should have unique constraint on `(MaKh, MaHh)` if not already present.
5. `KhachHang.VaiTro` as int works, but named enum/lookup would be clearer.
6. Legacy tables may reflect old educational schema, not current furniture ecommerce domain.

---

## Phase 4 — Verification checklist after implementation

1. Build
- `dotnet build`

2. Auth
- register works
- login works
- password hashed on new account
- legacy account upgrades hash after login

3. Favorite
- add/remove from list page
- add/remove from detail page
- button color state updates correctly
- persisted after logout/login

4. Shop
- category filter works
- brand filter works
- min/max price filter works
- sort newest/popular/price asc/price desc works

5. Checkout
- cart add/update/remove works
- checkout creates HoaDon + ChiTietHD
- email fires when SMTP configured

6. Admin
- admin route visible only for admin
- dashboard cards render
- product/order/customer lists render

7. UI review
- no overlap, clipping, broken spacing on home/login/shop/detail/cart/admin
- light theme only
- DEEPSEARCH branding consistent

---

## Recommended immediate next execution slice

Slice A (safe, high value, low risk):
1. favorite button active color polish
2. remove dark mode
3. rebrand HSHOP -> DEEPSEARCH
4. runtime verify filter/sort/favorite/login

Slice B (medium risk):
5. create admin dashboard shell
6. schema classification report

Slice C (higher risk/data heavy):
7. redesign seed scripts
8. purge/replace stale images
9. optional schema refactor migrations
