# User Side Completion Implementation Plan

> **For Hermes:** Use subagent-driven-development skill to implement this plan task-by-task.

**Goal:** Hoàn thiện Page/User side của ECommerceMVC theo 7 nhóm ưu tiên: auth nâng cao, catalog UX, cart/checkout, profile, product detail/review, engagement, và UX polish.

**Architecture:** Giữ ASP.NET Core MVC + EF Core hiện tại, mở rộng bằng các entity mới có mapping rõ trong `Hshop2023Context`, service nhỏ theo domain, Razor views tương ứng, và JavaScript progressive enhancement trong `wwwroot/js/modern-app.js`. Không rewrite app; triển khai theo từng phase nhỏ, sau mỗi phase phải `dotnet build` và chạy live verification trên app.

**Tech Stack:** ASP.NET Core MVC net8.0, EF Core SQL Server, Session auth hiện tại, SMTP email hiện tại, SignalR cho realtime notification/chat, Razor views, vanilla JS.

---

## Baseline hiện tại

Build gần nhất đã pass:

```bash
dotnet build ECommerceMVC.sln -v minimal
# Expected: Build succeeded, 0 Warning(s), 0 Error(s)
```

Các file chính đang có:

- Auth/Profile: `ECommerceMVC/Controllers/KhachHangController.cs`
- Catalog: `ECommerceMVC/Controllers/HangHoaController.cs`
- Cart/Checkout: `ECommerceMVC/Controllers/CartController.cs`
- Home: `ECommerceMVC/Controllers/HomeController.cs`
- DbContext: `ECommerceMVC/Data/Hshop2023Context.cs`
- Customer layout/scripts: `Views/Shared/_LayoutCustomer.cshtml`, `_ModernNavbar.cshtml`, `_CustomerSearchOverlay.cshtml`, `wwwroot/js/modern-app.js`, `wwwroot/css/modern-theme.css`
- Product views: `Views/HangHoa/Index.cshtml`, `Detail.cshtml`, `ProductItem.cshtml`
- Cart views: `Views/Cart/Index.cshtml`, `Checkout.cshtml`, `CheckoutSuccess.cshtml`

## Implementation rules

1. Một phase chỉ merge khi build pass.
2. Nếu thêm bảng/cột, cập nhật cả:
   - data class trong `ECommerceMVC/Data`
   - `DbSet<>` và `OnModelCreating` trong `Hshop2023Context.cs`
   - SQL migration/seed/reset script trong `Resources/` nếu project có script tương ứng.
3. Không lưu secret vào source. OTP/reset token phải hash trước khi lưu DB.
4. Không tin hidden input cho identity/ownership; luôn lấy `MaKh` từ session `MySetting.CUSTOMER_KEY`.
5. Với VNPay, verification thực tế phải dùng public ngrok URL, không dùng localhost callback.
6. Sau code-only build, cần live verification bằng browser cho các flow chính.

---

# Phase 1: Auth nâng cao - forgot password + OTP/email reset

## Task 1.1: Thêm entity lưu OTP reset mật khẩu

**Objective:** Tạo bảng lưu OTP/reset token có expiry, used flag, attempt count.

**Files:**
- Create: `ECommerceMVC/Data/PasswordResetOtp.cs`
- Modify: `ECommerceMVC/Data/Hshop2023Context.cs`

**Implementation:**

Tạo `PasswordResetOtp` với các field:

- `Id` int identity
- `MaKh` string max 20
- `Email` string max 50
- `OtpHash` string max 128
- `ExpiresAt` DateTime
- `UsedAt` DateTime?
- `AttemptCount` int
- `CreatedAt` DateTime
- navigation `KhachHang`

Mapping:

- table `PasswordResetOtp`
- index `(MaKh, CreatedAt)`
- index `(Email, CreatedAt)`
- FK `MaKh -> KhachHang.MaKh`
- delete restrict/no action

**Verification:**

```bash
dotnet build ECommerceMVC.sln -v minimal
```

## Task 1.2: Tạo service OTP reset mật khẩu

**Objective:** Sinh OTP 6 số, hash OTP, validate OTP, chống brute force đơn giản.

**Files:**
- Create: `ECommerceMVC/Services/IPasswordResetService.cs`
- Create: `ECommerceMVC/Services/PasswordResetService.cs`
- Modify: `ECommerceMVC/Program.cs`

**Implementation notes:**

- Sinh OTP bằng `RandomNumberGenerator.GetInt32(100000, 999999)`.
- Hash OTP bằng SHA256 với format input: `${maKh}:${otp}:${createdTicks}` hoặc salt riêng.
- Expiry: 10 phút.
- Max attempts: 5.
- Không trả OTP trong response; chỉ trả cho email service.
- Khi validate đúng, set `UsedAt = DateTime.Now`.

**DI:**

```csharp
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
```

**Verification:** build pass.

## Task 1.3: Thêm ViewModels forgot/reset

**Objective:** Tách input validation khỏi entity.

**Files:**
- Create: `ECommerceMVC/ViewModels/ForgotPasswordVM.cs`
- Create: `ECommerceMVC/ViewModels/VerifyOtpResetPasswordVM.cs`

**Required validation:**

`ForgotPasswordVM`:
- `EmailOrUsername` required max 80.

`VerifyOtpResetPasswordVM`:
- `MaKh` hidden/read-only required max 20.
- `Otp` required regex `^\d{6}$`.
- `NewPassword` required min 6 max 50.
- `ConfirmPassword` compare with `NewPassword`.

## Task 1.4: Thêm actions vào KhachHangController

**Objective:** User có thể request OTP qua email và reset password bằng OTP.

**Files:**
- Modify: `ECommerceMVC/Controllers/KhachHangController.cs`
- Create: `ECommerceMVC/Views/KhachHang/ForgotPassword.cshtml`
- Create: `ECommerceMVC/Views/KhachHang/ResetPasswordWithOtp.cshtml`
- Modify: `ECommerceMVC/Views/KhachHang/DangNhap.cshtml`

**Actions:**

- `ForgotPassword` GET
- `ForgotPassword(ForgotPasswordVM model)` POST
- `ResetPasswordWithOtp(string maKh)` GET
- `ResetPasswordWithOtp(VerifyOtpResetPasswordVM model)` POST

**Security behavior:**

- Nếu email/user không tồn tại, hiển thị generic message: “Nếu tài khoản tồn tại, mã OTP đã được gửi.”
- Không leak user existence.
- Khi reset thành công, gọi `passwordService.SetPassword(khachHang, model.NewPassword)`.
- Xóa session login cũ nếu đang login user đó.

**UI:**

- Thêm link “Quên mật khẩu?” ở `DangNhap.cshtml`.

**Verification:**

- Build pass.
- Live test with SMTP configured: request OTP, kiểm tra email, nhập OTP, login bằng password mới.
- Nếu SMTP không configured, verify app không crash và báo lỗi thân thiện.

---

# Phase 2: Catalog UX - realtime search suggestions, rating filter, best-seller sort

## Task 2.1: Thêm action search suggestions JSON

**Objective:** Search overlay gợi ý realtime sản phẩm khi user nhập từ khóa.

**Files:**
- Modify: `ECommerceMVC/Controllers/HangHoaController.cs`
- Create optional VM: `ECommerceMVC/ViewModels/SearchSuggestionVM.cs`

**Action:**

```csharp
[HttpGet]
public IActionResult SearchSuggestions(string? query, int take = 6)
```

**Behavior:**

- Trim query.
- Nếu query < 2 chars: return empty array.
- `take = Math.Clamp(take, 1, 10)`.
- Query `HangHoas` by `TenHh.Contains(query)` or category/supplier if cheap enough.
- Return JSON fields: `maHh`, `tenHH`, `donGia`, `hinhUrl`, `tenLoai`, `detailUrl`.
- Use `AsNoTracking()` and `Take(take)`.

## Task 2.2: Wire realtime UI vào search overlay

**Objective:** User thấy suggestions không cần submit form.

**Files:**
- Modify: `ECommerceMVC/Views/Shared/_CustomerSearchOverlay.cshtml`
- Modify: `ECommerceMVC/wwwroot/js/modern-app.js`
- Modify: `ECommerceMVC/wwwroot/css/modern-theme.css`

**Implementation:**

- Thêm container `<div class="search-suggestions" data-search-suggestions></div>` dưới input.
- JS debounce 250ms trên `.search-input`.
- Fetch `/HangHoa/SearchSuggestions?query=...`.
- Render product thumbnail/title/price/category.
- Enter vẫn submit form search đầy đủ.
- Escape/close overlay clear suggestions.

**Verification:**

- Mở app, click search, gõ 2+ ký tự, suggestions xuất hiện.
- Click suggestion đi tới detail.

## Task 2.3: Thêm rating filter

**Objective:** Catalog có filter rating sao dựa trên ProductReview average.

**Files:**
- Modify: `ECommerceMVC/ViewModels/HangHoaFilterVM.cs`
- Modify: `ECommerceMVC/Controllers/HangHoaController.cs`
- Modify: `ECommerceMVC/Views/HangHoa/Index.cshtml`
- Modify: `ECommerceMVC/Views/Shared/_CustomerSearchOverlay.cshtml` để preserve `minRating`.

**Implementation:**

- Add `public int? MinRating { get; set; }`.
- Controller nhận `int? minRating`.
- Normalize 1..5.
- Filter bằng average reviews:
  - Nếu `minRating` null: không filter.
  - Nếu set: group `ProductReviews` by MaHh average SoSao, join/filter products có avg >= minRating.
- Add radio/dropdown “Từ 4 sao”, “Từ 3 sao”, etc.

**Pitfall:** EF translation có thể phức tạp. Nếu query lỗi, dùng subquery:

```csharp
var ratedProductIds = db.ProductReviews
    .GroupBy(r => r.MaHh)
    .Where(g => g.Average(r => r.SoSao) >= filter.MinRating.Value)
    .Select(g => g.Key);
baseQuery = baseQuery.Where(p => ratedProductIds.Contains(p.MaHh));
```

## Task 2.4: Best-seller sort theo order details

**Objective:** Sort bán chạy dựa vào tổng quantity đã bán, không dùng lượt xem.

**Files:**
- Modify: `HangHoaFilterVM.cs` nếu cần enum label.
- Modify: `HangHoaController.cs`
- Modify: `Views/HangHoa/Index.cshtml`

**Implementation:**

- Add option value `best_seller` label “Bán chạy”.
- Query sold totals:

```csharp
var soldTotals = db.ChiTietHds
    .GroupBy(c => c.MaHh)
    .Select(g => new { MaHh = g.Key, Sold = g.Sum(x => x.SoLuong) });
```

- Sort products by sold desc, tie by newest/id desc.

**Verification:**

- Compare direct DB result with rendered first products.
- Build pass.

---

# Phase 3: Cart/Checkout - voucher/coupon, stock validation, shipping calculator

## Task 3.1: Thêm Voucher entity và discount service

**Objective:** Có voucher code áp dụng ở cart/checkout.

**Files:**
- Create: `ECommerceMVC/Data/Voucher.cs`
- Create: `ECommerceMVC/Services/IVoucherService.cs`
- Create: `ECommerceMVC/Services/VoucherService.cs`
- Modify: `Hshop2023Context.cs`
- Modify: `Program.cs`

**Voucher fields:**

- `Id` int
- `Code` string max 30 unique
- `Description` string max 120
- `DiscountType` string max 20: `Percent` or `Fixed`
- `DiscountValue` double
- `MinSubtotal` double
- `MaxDiscount` double?
- `StartsAt`, `EndsAt`
- `UsageLimit` int?
- `UsedCount` int
- `IsActive` bool

**Service behavior:**

- Validate active, date, usage, subtotal.
- Calculate discount clamp 0..subtotal.
- Return result with success/error message.

## Task 3.2: Persist voucher in session cart checkout

**Objective:** Voucher applied in cart remains in checkout and order creation.

**Files:**
- Create: `ECommerceMVC/ViewModels/VoucherApplyVM.cs`
- Modify: `ECommerceMVC/ViewModels/CheckoutVM.cs`
- Modify: `ECommerceMVC/Controllers/CartController.cs`
- Modify: `ECommerceMVC/Views/Cart/Index.cshtml`
- Modify: `ECommerceMVC/Views/Cart/Checkout.cshtml`
- Modify: `ECommerceMVC/Views/Cart/CheckoutSuccess.cshtml`

**Implementation:**

- Session key `APPLIED_VOUCHER_CODE`.
- Actions:
  - `ApplyVoucher(string code, string? returnUrl)` POST
  - `RemoveVoucher(string? returnUrl)` POST
- During checkout, recompute voucher server-side; do not trust posted discount.
- Store compact discount note in `HoaDon.GhiChu` if schema cannot be expanded yet.
- If adding columns is acceptable, add `VoucherCode`, `DiscountAmount` to `HoaDon` and update reset scripts.

## Task 3.3: Stock validation on add/update/checkout

**Objective:** Không cho mua vượt tồn kho hoặc sản phẩm hết hàng.

**Files:**
- Modify: `CartController.cs`
- Modify: `Views/Cart/Index.cshtml`
- Modify: `Views/HangHoa/Detail.cshtml`
- Modify: `Views/HangHoa/ProductItem.cshtml`

**Rules:**

- AddToCart: if product not found or `SoLuongTon <= 0`, reject.
- AddToCart quantity clamp to stock.
- UpdateQuantity clamp to current stock.
- Checkout re-load products from DB, verify every cart line <= stock.
- On successful checkout, decrement stock inside same transaction.

**Verification:**

- Set a sample product stock low in DB.
- Try adding/updating beyond stock.
- Place order and confirm stock decremented.

## Task 3.4: Shipping calculator

**Objective:** Phí ship tính tự động theo địa chỉ/tỉnh hoặc phương thức.

**Files:**
- Create: `ECommerceMVC/Services/IShippingFeeService.cs`
- Create: `ECommerceMVC/Services/ShippingFeeService.cs`
- Modify: `CheckoutVM.cs`
- Modify: `CartController.cs`
- Modify: `Views/Cart/Checkout.cshtml`

**Minimal rules:**

- Subtotal >= 3,000,000: free shipping.
- Address contains `Hồ Chí Minh`, `TP.HCM`, `HCM`, `Sài Gòn`: 25,000.
- Address contains `Hà Nội`: 35,000.
- Else: 50,000.
- Express shipping adds 30,000.

**Important:** Recompute on POST from server-side address/subtotal; don't trust hidden `PhiVanChuyen`.

---

# Phase 4: Profile - change password + address book

## Task 4.1: Add change password flow

**Objective:** Logged-in user đổi mật khẩu bằng current password.

**Files:**
- Create: `ECommerceMVC/ViewModels/ChangePasswordVM.cs`
- Modify: `KhachHangController.cs`
- Create: `Views/KhachHang/ChangePassword.cshtml`
- Modify: `Views/Shared/_CustomerAccountNav.cshtml` or `Views/KhachHang/EditProfile.cshtml`

**Actions:**

- `ChangePassword` GET
- `ChangePassword(ChangePasswordVM model)` POST

**Validation:**

- CurrentPassword required.
- NewPassword required min 6 max 50.
- ConfirmPassword compare.
- Verify current with `passwordService.VerifyPassword`.
- Set new with `passwordService.SetPassword`.

## Task 4.2: Add CustomerAddress entity

**Objective:** User quản lý nhiều địa chỉ giao hàng.

**Files:**
- Create: `ECommerceMVC/Data/CustomerAddress.cs`
- Modify: `Hshop2023Context.cs`

**Fields:**

- `Id` int identity
- `MaKh` string max 20
- `ReceiverName` string max 50
- `Phone` string max 24
- `Line1` string max 120
- `Ward` string max 60
- `District` string max 60
- `City` string max 60
- `IsDefault` bool
- `CreatedAt`, `UpdatedAt`

## Task 4.3: Address book CRUD

**Objective:** User add/edit/delete/set-default address.

**Files:**
- Create: `ECommerceMVC/ViewModels/CustomerAddressVM.cs`
- Create: `ECommerceMVC/Controllers/AddressController.cs`
- Create: `Views/Address/Index.cshtml`
- Create: `Views/Address/Form.cshtml`
- Modify: customer account nav.

**Security:** Every query filters by `MaKh == CurrentCustomerId`.

## Task 4.4: Use address book in checkout

**Objective:** Checkout cho chọn saved address và prefill.

**Files:**
- Modify: `CheckoutVM.cs`
- Modify: `CartController.Checkout GET/POST`
- Modify: `Views/Cart/Checkout.cshtml`

**Behavior:**

- GET loads default address first, fallback to KhachHang.DiaChi.
- UI dropdown saved addresses.
- POST validates selected address ownership if selected.

---

# Phase 5: Product detail/review - media, variants, review photos

## Task 5.1: ProductMedia entity for multi-image/video

**Objective:** Product detail supports multiple images and video embeds/files.

**Files:**
- Create: `ECommerceMVC/Data/ProductMedia.cs`
- Modify: `Hshop2023Context.cs`
- Modify: `HangHoaVM.cs`
- Modify: `HangHoaController.BuildDetailViewModel`
- Modify: `Views/HangHoa/Detail.cshtml`

**Fields:**

- `Id` int
- `MaHh` int
- `MediaType` string max 20: `Image`, `VideoUrl`, `VideoFile`
- `Url` string max 250
- `AltText` string max 120
- `SortOrder` int
- `IsPrimary` bool

**UI:** Gallery thumbnails, hero media area, fallback to existing `HangHoa.Hinh`.

## Task 5.2: ProductVariant entity for selectable size/color

**Objective:** Size/màu trở thành lựa chọn mua hàng, không chỉ text specs.

**Files:**
- Create: `ECommerceMVC/Data/ProductVariant.cs`
- Modify: `Hshop2023Context.cs`
- Modify: `CartItem.cs`
- Modify: `CartController.AddToCart`
- Modify: `Views/HangHoa/Detail.cshtml`
- Modify: cart/checkout views to show variant label.

**Fields:**

- `Id`, `MaHh`
- `Color` max 30
- `Size` max 50
- `Sku` max 60
- `AdditionalPrice` double
- `StockQuantity` int
- `IsActive` bool

**Cart behavior:**

- `AddToCart(int id, int? variantId, int quantity = 1, ...)`.
- If variants exist, require valid active variant.
- Price = product price + additional price.
- Stock validation uses variant stock if selected.

## Task 5.3: Review photo upload

**Objective:** Review có thể upload ảnh.

**Files:**
- Create: `ECommerceMVC/Data/ProductReviewPhoto.cs`
- Modify: `ProductReview.cs`
- Modify: `Hshop2023Context.cs`
- Modify: `ProductReviewInputVM` in `HangHoaVM.cs`
- Modify: `HangHoaController.AddReview`
- Modify: `Views/HangHoa/Detail.cshtml`

**Rules:**

- Allow up to 3 images.
- Extensions: `.jpg`, `.jpeg`, `.png`, `.webp`.
- Max size: 3 MB per file.
- Store under `wwwroot/Hinh/Review/`.
- Save short unique filenames.
- Reject invalid file with TempData error.

**Security:** Do not trust client content type only; validate extension and size.

---

# Phase 6: Engagement - compare, in-app notification + SignalR, support chat/ticket

## Task 6.1: Product compare via session

**Objective:** User can compare up to 4 products.

**Files:**
- Modify: `HangHoaController.cs`
- Create: `Views/HangHoa/Compare.cshtml`
- Modify: `Views/HangHoa/ProductItem.cshtml`
- Modify: `Views/HangHoa/Detail.cshtml`
- Modify: `_ModernNavbar.cshtml` or account nav for compare link.

**Actions:**

- `Compare` GET
- `AddCompare(int id, string? returnUrl)` POST
- `RemoveCompare(int id, string? returnUrl)` POST
- `ClearCompare()` POST

**Session key:** `COMPARE_PRODUCT_IDS`.

**Compare fields:** price, category, supplier, stock, color, material, dimensions, warranty, style, rating.

## Task 6.2: In-app notification entity and notification center

**Objective:** User có notification center cho đơn hàng/khuyến mãi/system.

**Files:**
- Create: `ECommerceMVC/Data/UserNotification.cs`
- Create: `ECommerceMVC/Services/INotificationService.cs`
- Create: `ECommerceMVC/Services/NotificationService.cs`
- Create: `ECommerceMVC/Controllers/NotificationController.cs`
- Create: `Views/Notification/Index.cshtml`
- Modify: `Hshop2023Context.cs`
- Modify: `Program.cs`
- Modify: `_ModernNavbar.cshtml`

**Fields:**

- `Id`, `MaKh`
- `Title` max 120
- `Message` max 500
- `Type` max 30
- `Url` max 250 nullable
- `IsRead` bool
- `CreatedAt`, `ReadAt`

**Initial integration:**

- On checkout success, create notification “Đơn hàng #id đã được ghi nhận”.
- Navbar shows unread badge.
- Mark read endpoint.

## Task 6.3: Add SignalR realtime notification

**Objective:** Notification mới đẩy realtime khi user đang online.

**Files:**
- Create: `ECommerceMVC/Hubs/NotificationHub.cs`
- Modify: `Program.cs`
- Modify: `NotificationService.cs`
- Modify: `Views/Shared/_CustomerScripts.cshtml`
- Modify: `wwwroot/js/modern-app.js`

**Implementation:**

- `builder.Services.AddSignalR();`
- `app.MapHub<NotificationHub>("/hubs/notifications");`
- Hub group by customer id from session if accessible. If session in hub is awkward, pass current customer id through authenticated/session endpoint or start with all-client toast then refine.
- Client loads `signalr.min.js` from CDN or local lib.
- On notification, show toast and update badge.

## Task 6.4: Support tickets

**Objective:** User tạo ticket support và xem phản hồi.

**Files:**
- Create: `ECommerceMVC/Data/SupportTicket.cs`
- Create: `ECommerceMVC/Data/SupportTicketMessage.cs`
- Create: `ECommerceMVC/Controllers/SupportController.cs`
- Create: `ECommerceMVC/ViewModels/SupportTicketVM.cs`
- Create: `Views/Support/Index.cshtml`
- Create: `Views/Support/Details.cshtml`
- Create: `Views/Support/Create.cshtml`
- Modify: `Hshop2023Context.cs`
- Modify: footer/navbar support link.

**Behavior:**

- User creates ticket: subject, category, message.
- User sees own tickets only.
- User can add messages while ticket open.
- Admin reply can be implemented later, but data model supports it.

## Task 6.5: Basic realtime shop chat skeleton

**Objective:** Chat với shop có MVP realtime using SignalR, không cần AI yet.

**Files:**
- Create: `ECommerceMVC/Data/ChatThread.cs`
- Create: `ECommerceMVC/Data/ChatMessage.cs`
- Create: `ECommerceMVC/Hubs/ChatHub.cs`
- Create: `ECommerceMVC/Controllers/ChatController.cs`
- Create: `Views/Chat/Index.cshtml`
- Modify: `Program.cs`

**MVP scope:**

- One customer has one support chat thread.
- Messages stored in DB.
- Customer sends message; admin UI can be later or use support backend.
- Add auto-reply placeholder: “Cảm ơn bạn, shop sẽ phản hồi sớm.”

**AI chatbot recommend:** Defer until after chat/thread stable. Later add service that calls catalog query and returns product suggestions based on user text.

---

# Phase 7: UX polish - dark mode toggle, skeleton loading, infinite scroll/lazy load

## Task 7.1: Dark mode toggle thật

**Objective:** User can switch light/dark and preference persists.

**Files:**
- Modify: `wwwroot/js/modern-app.js`
- Modify: `Views/Shared/_ModernNavbar.cshtml`
- Modify: `Views/Shared/_CustomerMobileNav.cshtml` if exists/used
- Modify: `wwwroot/css/modern-theme.css` only if visual bugs appear.

**Current issue:** `applyTheme()` hardcodes light:

```js
document.documentElement.setAttribute('data-theme', 'light');
document.documentElement.setAttribute('data-bs-theme', 'light');
```

**Fix:**

- Read `localStorage.getItem('theme')`.
- Fallback to `window.matchMedia('(prefers-color-scheme: dark)')`.
- Toggle on `[data-theme-toggle]`.
- Update icon/aria-label.

## Task 7.2: Skeleton loading for product grids/search

**Objective:** Perceived loading better when filter/search/infinite fetch happens.

**Files:**
- Modify: `wwwroot/css/modern-theme.css`
- Modify: `wwwroot/js/modern-app.js`
- Modify: `Views/HangHoa/Index.cshtml`

**Implementation:**

- Add `.skeleton-card`, `.skeleton-line`, shimmer animation.
- On filter form submit/page click, show skeleton overlay/product grid placeholders before navigation.
- Search suggestions show 3 skeleton rows while fetch pending.

## Task 7.3: Product list partial endpoint for infinite scroll

**Objective:** Catalog can load next page without full reload.

**Files:**
- Modify: `HangHoaController.cs`
- Create: `Views/HangHoa/_ProductGridItems.cshtml`
- Modify: `Views/HangHoa/Index.cshtml`
- Modify: `wwwroot/js/modern-app.js`

**Implementation:**

- Extract product item rendering to partial that can return cards only.
- New action:

```csharp
[HttpGet]
public IActionResult PageItems(/* same filters */, int page = 1)
```

- Return partial HTML + header `X-Has-More` or JSON `{ html, hasMore, nextPage }`.
- JS observes sentinel using IntersectionObserver.
- Keep existing pagination as fallback for no-JS.

**Verification:**

- Load shop, scroll near bottom, next page appends.
- Filter/sort still works.
- Browser back/URL still sane enough: keep page 1 URL and append only for UX.

## Task 7.4: Lazy-load product images

**Objective:** Faster initial render.

**Files:**
- Modify: `Views/HangHoa/ProductItem.cshtml`
- Modify: `Views/HangHoa/Detail.cshtml` for related products/gallery thumbnails.

**Implementation:**

- Add `loading="lazy"` to non-hero images.
- Add explicit width/height if available to reduce layout shift.
- Keep detail hero eager/high priority.

---

# Cross-phase verification checklist

Run after every phase:

```bash
dotnet build ECommerceMVC.sln -v minimal
```

Run before final handoff:

1. Register/login still works.
2. Forgot password OTP email reset works.
3. Search suggestions show and click through to detail.
4. Catalog filters: category, brand, price, rating; sorts newest, best-seller, price asc/desc.
5. Add cart rejects out-of-stock and clamps quantity.
6. Voucher applies/removes and checkout total is recalculated server-side.
7. COD checkout creates order, sends email path, decrements stock, creates notification.
8. VNPay checkout uses ngrok public URL for callback/IPN and creates order only after success.
9. Profile edit, change password, address book CRUD work.
10. Product detail shows gallery/video/variant; cart line preserves selected variant.
11. Review can include text + rating + up to 3 photos; invalid file rejected.
12. Compare up to 4 products works.
13. Notification center displays unread and mark-read works; SignalR toast updates in browser.
14. Support ticket create/list/detail works.
15. Dark mode persists after reload.
16. Skeleton appears during async loading.
17. Infinite scroll appends products and fallback pagination remains usable.

# Recommended implementation order

Do not implement all 7 phases in one large patch. Recommended order:

1. Phase 1 Auth reset OTP.
2. Phase 2 Catalog UX.
3. Phase 3 Stock validation first, then voucher, then shipping.
4. Phase 4 Change password, then address book.
5. Phase 7 Dark mode + skeleton (safe UX wins), then infinite scroll.
6. Phase 5 Product media/variants/review photos.
7. Phase 6 Compare, notifications, support, then realtime chat.

# Commit plan

Use one commit per task or per small cohesive group:

```bash
git add ECommerceMVC/Data ECommerceMVC/Services ECommerceMVC/Controllers ECommerceMVC/ViewModels ECommerceMVC/Views ECommerceMVC/wwwroot docs/plans

git commit -m "feat(auth): add password reset otp flow"
git commit -m "feat(catalog): add search suggestions and rating filter"
git commit -m "feat(cart): add voucher, stock validation, shipping calculator"
git commit -m "feat(profile): add password change and address book"
git commit -m "feat(product): add media gallery variants and review photos"
git commit -m "feat(engagement): add compare notifications and support"
git commit -m "feat(ux): add dark mode skeletons and infinite scroll"
```

# Open decisions before coding

1. Schema migration style: project đang DB-first/manual scripts hay dùng EF migrations? Nếu manual scripts, thêm SQL vào `Resources/` và update reset seed. Nếu EF migrations, tạo migration.
2. Voucher: lưu discount vào cột mới trên `HoaDon` hay ghi compact vào `GhiChu` để tránh schema lớn?
3. Product variants/media: seed demo data từ local images hay chỉ support schema/UI trước?
4. SignalR auth: tiếp tục dùng session-based customer id hay chuyển dần sang ASP.NET Identity/claims? Với scope hiện tại, giữ session để ít phá code.
5. AI chatbot: cần provider/API nào? Nên để sau khi chat/support ổn định.
