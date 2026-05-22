# Full Admin Console Implementation Plan

> For Hermes: implement directly in this repo, verify with build and runtime route checks.

Goal: Build a truly separate admin console for the ASP.NET Core MVC ecommerce app with complete dashboard, product, category, order, customer, and payment management features.

Architecture: Use a dedicated AdminController, dedicated admin Razor layout and admin-only views. Keep user-facing controllers/layout untouched except optional admin link and category visibility/sort integration. Use existing EF Core entities and legacy schema safely; where schema lacks fields (category parent/sort/visible, payment transaction table), use compact metadata/derived statuses without breaking existing DB.

Tech Stack: ASP.NET Core MVC net8.0, Razor views, EF Core SQL Server, existing session auth via MySetting.CUSTOMER_KEY and MySetting.ADMIN_ROLE.

---

## Acceptance checklist

- /Admin is a separate UI, not _LayoutCustomer, not HangHoas scaffold pages.
- Admin guard applies to all admin actions.
- Dashboard shows revenue, order count, customer count, product count, pending orders, top products, recent orders, daily/monthly charts.
- Products: list/search/filter, add/edit/delete, upload image, price, stock, description, SKU/alias, variants via color/size/material/warranty/style, duplicate SKU check, low-stock/hidden metadata.
- Categories: create/edit/delete, upload image, sub-category, sort, hide/show, apply visible/sort to user category menu.
- Orders: list/filter/search, details, status update for pending/shipping/completed/cancel/refund, compact admin note.
- Customers: list/search, order history, lock/unlock, role/group update, customer group display.
- Payments: list/filter, payment status detection, failed transactions, refund marking, method summary.
- Build succeeds with 0 errors.
- Runtime /Admin redirects to login when unauthenticated; admin route renders after login/admin session if available.

---

## Task 1: Audit current admin implementation

Files:
- Inspect: ECommerceMVC/Controllers/AdminController.cs
- Inspect: ECommerceMVC/ViewModels/AdminViewModels.cs
- Inspect: ECommerceMVC/Views/Admin/*.cshtml
- Inspect: ECommerceMVC/ViewComponents/MenuLoaiViewComponent.cs
- Inspect: ECommerceMVC/Views/Shared/_ModernNavbar.cshtml

Steps:
1. Read all files above.
2. Confirm missing pieces:
   - no explicit admin plan doc
   - product visibility/low-stock metadata incomplete
   - category visibility/sort not applied in user menu
   - order list lacks search and status shortcut buttons
   - payment page lacks method summary cards and refund action
   - admin navbar still points to old HangHoas/Create instead of /Admin
3. Update todo before coding.

Verification:
- Existing build baseline: dotnet build ECommerceMVC/ECommerceMVC.csproj -v minimal

---

## Task 2: Create reusable admin metadata helper

Files:
- Create: ECommerceMVC/Helpers/AdminMetadataHelper.cs
- Modify: ECommerceMVC/Controllers/AdminController.cs
- Modify: ECommerceMVC/ViewComponents/MenuLoaiViewComponent.cs

Implementation:
- Add helper to parse/build Loai.MoTa metadata:
  description + ParentId + SortOrder + IsVisible
- Add helper to parse/build HangHoa.MoTa metadata:
  description + IsVisible + LowStockThreshold
- Preserve human description before marker.

Verification:
- dotnet build succeeds.

---

## Task 3: Complete admin ViewModels

Files:
- Modify: ECommerceMVC/ViewModels/AdminViewModels.cs

Implementation:
- Add Dashboard metrics: CancelledOrderCount, RefundedOrderCount, FailedPaymentCount, LowStockCount.
- Add product row VM instead of passing raw HangHoa to Products view.
- Add product form fields: IsVisible, LowStockThreshold, existing image/manual image name.
- Add order filter VM or ViewBag-compatible status/search/method fields.
- Add payment summary VM.

Verification:
- dotnet build succeeds after controller/views updated.

---

## Task 4: Upgrade AdminController data/actions

Files:
- Modify: ECommerceMVC/Controllers/AdminController.cs

Implementation details:
- Use AdminMetadataHelper.
- Dashboard:
  - revenue from completed orders
  - counts for pending, cancelled, refunded, failed payments, low stock
  - top products and recent orders
- Products:
  - return AdminProductRowVM with visibility and low stock state
  - filters: q, category, stock=low/out, visible=true/false
  - ProductCreate/Edit validates duplicate SKU/TenAlias under 50 chars
  - ProductToggleVisible action
  - ProductDelete blocks if order details exist
- Categories:
  - use helper metadata
  - CategoryMove action for sort up/down
  - CategoryToggle visible
- Orders:
  - search by order id/customer/name/payment
  - filters by status/payment
  - shortcut actions: confirm, shipping, complete, cancel, refund using TrangThai lookup where possible
  - OrderStatus accepts optional adminNote compacted into GhiChu
- Customers:
  - role update, lock/unlock
  - group based on total/order count
- Payments:
  - method/status filters
  - summary cards
  - PaymentMarkFailed, PaymentRefund actions if feasible by GhiChu/status

Verification:
- dotnet build succeeds.

---

## Task 5: Upgrade admin layout and nav

Files:
- Modify: ECommerceMVC/Views/Admin/_AdminLayout.cshtml
- Modify: ECommerceMVC/Views/Shared/_ModernNavbar.cshtml

Implementation:
- Admin layout remains separate and responsive.
- Add active menu highlighting.
- Add quick action buttons.
- User navbar admin link points to Admin/Index instead of old HangHoas/Create.

Verification:
- /Admin HTML contains DEEPSEARCH Admin, not customer footer/newsletter.

---

## Task 6: Upgrade Dashboard view

Files:
- Modify: ECommerceMVC/Views/Admin/Index.cshtml

Implementation:
- Render all summary cards.
- Render daily/monthly charts.
- Render top products, recent orders, low-stock list.
- Add quick links to Products, Orders, Payments.

Verification:
- Razor compiles.

---

## Task 7: Upgrade Product admin pages

Files:
- Modify: ECommerceMVC/Views/Admin/Products.cshtml
- Modify: ECommerceMVC/Views/Admin/ProductForm.cshtml

Implementation:
- Use AdminProductRowVM.
- Add filters for category, low stock/out stock/visible hidden.
- Add visibility toggle button.
- Show SKU, price, stock, variants, image, supplier.
- Form includes all required product fields and metadata fields.

Verification:
- Razor compiles.

---

## Task 8: Upgrade Category admin pages and user category integration

Files:
- Modify: ECommerceMVC/Views/Admin/Categories.cshtml
- Modify: ECommerceMVC/Views/Admin/CategoryForm.cshtml
- Modify: ECommerceMVC/ViewComponents/MenuLoaiViewComponent.cs
- Modify: ECommerceMVC/ViewModels/MenuLoaiVM.cs if needed

Implementation:
- Category list shows root/sub hierarchy, sort, visible, product count.
- Move up/down changes SortOrder.
- User category component only shows IsVisible categories sorted by SortOrder then name.

Verification:
- dotnet build succeeds.

---

## Task 9: Upgrade Order and Payment pages

Files:
- Modify: ECommerceMVC/Views/Admin/Orders.cshtml
- Modify: ECommerceMVC/Views/Admin/OrderDetails.cshtml
- Modify: ECommerceMVC/Views/Admin/Payments.cshtml

Implementation:
- Orders page has search/status/payment filters and quick status buttons.
- OrderDetails has status dropdown, note field, refund/cancel actions.
- Payments page has summary cards, filters, failed/refund status badges, action links.

Verification:
- Razor compiles.

---

## Task 10: Upgrade Customer pages

Files:
- Modify: ECommerceMVC/Views/Admin/Customers.cshtml
- Modify: ECommerceMVC/Views/Admin/CustomerDetails.cshtml

Implementation:
- Customers page has status/group filters and lock/unlock.
- Details show profile, account state, role selector, order history with totals.

Verification:
- Razor compiles.

---

## Task 11: Full build and runtime verification

Commands:
- dotnet build ECommerceMVC/ECommerceMVC.csproj -v minimal
- dotnet run --project ECommerceMVC/ECommerceMVC.csproj --urls http://127.0.0.1:5099
- curl -i -L --max-time 20 http://127.0.0.1:5099/Admin

Expected:
- Build succeeded, 0 errors.
- /Admin redirects to /KhachHang/DangNhap?returnUrl=%2FAdmin when unauthenticated.
- If logged in as admin in browser, /Admin renders admin dashboard.
