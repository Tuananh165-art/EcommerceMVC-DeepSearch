using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMVC.Controllers;

public class AdminController : Controller
{
    private readonly Hshop2023Context db;

    public AdminController(Hshop2023Context context) => db = context;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var customerId = HttpContext.Session.Get<string>(MySetting.CUSTOMER_KEY);
        if (string.IsNullOrWhiteSpace(customerId))
        {
            context.Result = RedirectToAction("DangNhap", "KhachHang", new { returnUrl = Request.Path + Request.QueryString });
            return;
        }

        var customer = db.KhachHangs.AsNoTracking().FirstOrDefault(x => x.MaKh == customerId);
        if (customer == null || !customer.HieuLuc || customer.VaiTro != MySetting.ADMIN_ROLE)
        {
            context.Result = RedirectToAction("DangNhap", "KhachHang", new { returnUrl = Request.Path + Request.QueryString });
            return;
        }

        ViewBag.AdminName = customer.HoTen;
        base.OnActionExecuting(context);
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var start30 = today.AddDays(-29);
        var start12Months = new DateTime(today.Year, today.Month, 1).AddMonths(-11);
        var completedOrderTotals = await db.HoaDons
            .Where(h => h.MaTrangThai == 1 || h.MaTrangThai == 3)
            .Join(
                db.ChiTietHds,
                h => h.MaHd,
                ct => ct.MaHd,
                (h, ct) => new
                {
                    h.MaHd,
                    h.NgayDat,
                    h.PhiVanChuyen,
                    LineTotal = ct.SoLuong * ct.DonGia * (1 - ct.GiamGia)
                })
            .GroupBy(x => new { x.MaHd, x.NgayDat, x.PhiVanChuyen })
            .Select(g => new
            {
                g.Key.MaHd,
                g.Key.NgayDat,
                Total = g.Sum(x => x.LineTotal) + g.Key.PhiVanChuyen
            })
            .ToListAsync();
        var allProducts = await BuildProductRows(db.HangHoas.Include(x => x.MaLoaiNavigation).Include(x => x.MaNccNavigation).Include(x => x.ChiTietHds));

        var model = new AdminDashboardVM
        {
            Revenue = completedOrderTotals.Sum(x => x.Total),
            OrderCount = await db.HoaDons.CountAsync(),
            CustomerCount = await db.KhachHangs.CountAsync(),
            ProductCount = await db.HangHoas.CountAsync(),
            PendingOrderCount = await db.HoaDons.CountAsync(x => x.MaTrangThai == 0),
            ShippingOrderCount = await db.HoaDons.CountAsync(x => x.MaTrangThai == 2),
            CompletedOrderCount = await db.HoaDons.CountAsync(x => x.MaTrangThai == 1 || x.MaTrangThai == 3),
            CancelledOrderCount = await db.HoaDons.CountAsync(x => x.MaTrangThai < 0),
            RefundedOrderCount = await db.HoaDons.CountAsync(x => (x.GhiChu ?? "").Contains("refund") || (x.GhiChu ?? "").Contains("hoàn tiền")),
        };

        model.LowStockProducts = allProducts.Where(x => x.IsLowStock).OrderBy(x => x.Stock).Take(8).ToList();
        model.LowStockCount = allProducts.Count(x => x.IsLowStock);

        model.TopProducts = await db.ChiTietHds
            .GroupBy(x => new { x.MaHh, x.MaHhNavigation.TenHh, x.MaHhNavigation.Hinh })
            .Select(g => new AdminTopProductVM
            {
                MaHh = g.Key.MaHh,
                TenHh = g.Key.TenHh,
                Hinh = g.Key.Hinh,
                QuantitySold = g.Sum(x => x.SoLuong),
                Revenue = g.Sum(x => x.SoLuong * x.DonGia * (1 - x.GiamGia))
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(8)
            .ToListAsync();

        model.NewOrders = await BuildOrderRows(db.HoaDons.Include(x => x.MaKhNavigation).Include(x => x.MaTrangThaiNavigation).Include(x => x.ChiTietHds).OrderByDescending(x => x.NgayDat).Take(8));
        model.FailedPaymentCount = model.NewOrders.Count(x => HasPaymentIssue(x.PaymentMethod, x.StatusName, x.Note));

        var dailyRaw = completedOrderTotals
            .Where(h => h.NgayDat.Date >= start30)
            .Select(h => new { Date = h.NgayDat.Date, h.Total })
            .GroupBy(x => x.Date)
            .Select(g => new { Date = g.Key, Value = g.Sum(x => x.Total) })
            .ToList();
        model.DailyRevenue = Enumerable.Range(0, 30).Select(i => start30.AddDays(i)).Select(d => new AdminChartPointVM { Label = d.ToString("dd/MM"), Value = dailyRaw.FirstOrDefault(x => x.Date == d)?.Value ?? 0 }).ToList();

        var monthlyRaw = completedOrderTotals
            .Where(h => h.NgayDat >= start12Months)
            .Select(h => new { h.NgayDat.Year, h.NgayDat.Month, h.Total })
            .GroupBy(x => new { x.Year, x.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Value = g.Sum(x => x.Total) })
            .ToList();
        model.MonthlyRevenue = Enumerable.Range(0, 12).Select(i => start12Months.AddMonths(i)).Select(d => new AdminChartPointVM { Label = d.ToString("MM/yyyy"), Value = monthlyRaw.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month)?.Value ?? 0 }).ToList();

        return View(model);
    }

    public async Task<IActionResult> Products(string? q, int? category, string? stock, string? visible)
    {
        var query = db.HangHoas.Include(x => x.MaLoaiNavigation).Include(x => x.MaNccNavigation).Include(x => x.ChiTietHds).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(x => x.TenHh.Contains(q) || (x.TenAlias != null && x.TenAlias.Contains(q)) || (x.MaNcc != null && x.MaNcc.Contains(q)));
        }
        if (category.HasValue) query = query.Where(x => x.MaLoai == category.Value);
        var rows = await BuildProductRows(query);
        if (stock == "low") rows = rows.Where(x => x.IsLowStock).ToList();
        if (stock == "out") rows = rows.Where(x => x.IsOutOfStock).ToList();
        if (visible == "true") rows = rows.Where(x => x.IsVisible).ToList();
        if (visible == "false") rows = rows.Where(x => !x.IsVisible).ToList();

        await LoadProductSelectLists(category, null);
        ViewBag.Query = q; ViewBag.Stock = stock; ViewBag.Visible = visible;
        return View(rows.OrderBy(x => x.TenHh).ToList());
    }

    [HttpGet]
    public async Task<IActionResult> ProductCreate()
    {
        await LoadProductSelectLists(null, null);
        return View("ProductForm", new AdminProductFormVM { NgaySx = DateTime.Today, IsVisible = true, LowStockThreshold = 5 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductCreate(AdminProductFormVM model, IFormFile? hinhUpload)
    {
        await ValidateProductModel(model, null);
        if (!ModelState.IsValid) { await LoadProductSelectLists(model.MaLoai, model.MaNcc); return View("ProductForm", model); }
        var product = new HangHoa { MaNcc = model.MaNcc, TenHh = model.TenHh, NgaySx = DateTime.Today };
        model.ApplyTo(product);
        if (hinhUpload != null)
        {
            var fileName = MyUtil.UploadHinh(hinhUpload, "HangHoa");
            if (!string.IsNullOrWhiteSpace(fileName)) product.Hinh = fileName;
        }
        else product.Hinh = string.IsNullOrWhiteSpace(model.Hinh) ? null : model.Hinh.Trim();
        db.HangHoas.Add(product);
        await db.SaveChangesAsync();
        TempData["AdminSuccess"] = "Đã thêm sản phẩm mới.";
        return RedirectToAction(nameof(Products));
    }

    [HttpGet]
    public async Task<IActionResult> ProductEdit(int id)
    {
        var product = await db.HangHoas.FindAsync(id);
        if (product == null) return NotFound();
        await LoadProductSelectLists(product.MaLoai, product.MaNcc);
        return View("ProductForm", AdminProductFormVM.FromEntity(product));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductEdit(int id, AdminProductFormVM model, IFormFile? hinhUpload)
    {
        if (id != model.MaHh) return NotFound();
        await ValidateProductModel(model, id);
        if (!ModelState.IsValid) { await LoadProductSelectLists(model.MaLoai, model.MaNcc); return View("ProductForm", model); }
        var product = await db.HangHoas.FindAsync(id);
        if (product == null) return NotFound();
        model.ApplyTo(product);
        if (hinhUpload != null)
        {
            var fileName = MyUtil.UploadHinh(hinhUpload, "HangHoa");
            if (!string.IsNullOrWhiteSpace(fileName)) product.Hinh = fileName;
        }
        else product.Hinh = string.IsNullOrWhiteSpace(model.Hinh) ? product.Hinh : model.Hinh.Trim();
        await db.SaveChangesAsync();
        TempData["AdminSuccess"] = "Đã cập nhật sản phẩm.";
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductToggleVisible(int id)
    {
        var product = await db.HangHoas.FindAsync(id);
        if (product != null)
        {
            var meta = AdminMetadataHelper.ParseProduct(product.MoTa);
            meta.IsVisible = !meta.IsVisible;
            product.MoTa = AdminMetadataHelper.BuildProduct(meta);
            await db.SaveChangesAsync();
            TempData["AdminSuccess"] = meta.IsVisible ? "Đã hiện sản phẩm." : "Đã ẩn sản phẩm.";
        }
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductDelete(int id)
    {
        var product = await db.HangHoas.Include(x => x.ChiTietHds).FirstOrDefaultAsync(x => x.MaHh == id);
        if (product == null) return RedirectToAction(nameof(Products));
        if (product.ChiTietHds.Any()) TempData["AdminError"] = "Không thể xóa sản phẩm đã phát sinh đơn hàng. Hãy ẩn sản phẩm hoặc đặt tồn kho = 0.";
        else { db.HangHoas.Remove(product); await db.SaveChangesAsync(); TempData["AdminSuccess"] = "Đã xóa sản phẩm."; }
        return RedirectToAction(nameof(Products));
    }

    public async Task<IActionResult> Categories()
    {
        var products = await db.HangHoas.GroupBy(x => x.MaLoai).Select(g => new { MaLoai = g.Key, Count = g.Count() }).ToListAsync();
        var categories = await db.Loais.OrderBy(x => x.TenLoai).ToListAsync();
        var rows = categories.Select(c =>
        {
            var meta = AdminMetadataHelper.ParseCategory(c.MoTa);
            return new AdminCategoryRowVM
            {
                MaLoai = c.MaLoai, TenLoai = c.TenLoai, TenLoaiAlias = c.TenLoaiAlias, Hinh = c.Hinh,
                MoTa = meta.Description, ParentCategoryId = meta.ParentId, SortOrder = meta.SortOrder, IsVisible = meta.IsVisible,
                ProductCount = products.FirstOrDefault(p => p.MaLoai == c.MaLoai)?.Count ?? 0,
                VisibleProductCount = products.FirstOrDefault(p => p.MaLoai == c.MaLoai)?.Count ?? 0,
                ParentCategoryName = meta.ParentId.HasValue ? categories.FirstOrDefault(x => x.MaLoai == meta.ParentId.Value)?.TenLoai : null
            };
        }).OrderBy(x => x.SortOrder).ThenBy(x => x.TenLoai).ToList();
        return View(rows);
    }

    [HttpGet]
    public async Task<IActionResult> CategoryCreate() { await LoadCategorySelectList(null); return View("CategoryForm", new AdminCategoryFormVM()); }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoryCreate(AdminCategoryFormVM model, IFormFile? hinhUpload)
    {
        if (!ModelState.IsValid) { await LoadCategorySelectList(model.ParentCategoryId); return View("CategoryForm", model); }
        var category = new Loai(); ApplyCategoryForm(category, model, hinhUpload); db.Loais.Add(category); await db.SaveChangesAsync();
        TempData["AdminSuccess"] = "Đã thêm danh mục."; return RedirectToAction(nameof(Categories));
    }

    [HttpGet]
    public async Task<IActionResult> CategoryEdit(int id)
    {
        var category = await db.Loais.FindAsync(id); if (category == null) return NotFound();
        var meta = AdminMetadataHelper.ParseCategory(category.MoTa); await LoadCategorySelectList(meta.ParentId, id);
        return View("CategoryForm", new AdminCategoryFormVM { MaLoai = category.MaLoai, TenLoai = category.TenLoai, TenLoaiAlias = category.TenLoaiAlias, Hinh = category.Hinh, MoTa = meta.Description, ParentCategoryId = meta.ParentId, SortOrder = meta.SortOrder, IsVisible = meta.IsVisible });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoryEdit(int id, AdminCategoryFormVM model, IFormFile? hinhUpload)
    {
        if (id != model.MaLoai) return NotFound();
        if (model.ParentCategoryId == id) ModelState.AddModelError(nameof(model.ParentCategoryId), "Danh mục cha không được trùng chính nó.");
        if (!ModelState.IsValid) { await LoadCategorySelectList(model.ParentCategoryId, id); return View("CategoryForm", model); }
        var category = await db.Loais.FindAsync(id); if (category == null) return NotFound();
        ApplyCategoryForm(category, model, hinhUpload); await db.SaveChangesAsync(); TempData["AdminSuccess"] = "Đã cập nhật danh mục.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoryToggle(int id)
    {
        var category = await db.Loais.FindAsync(id);
        if (category != null) { var meta = AdminMetadataHelper.ParseCategory(category.MoTa); meta.IsVisible = !meta.IsVisible; category.MoTa = AdminMetadataHelper.BuildCategory(meta); await db.SaveChangesAsync(); TempData["AdminSuccess"] = meta.IsVisible ? "Đã hiện danh mục." : "Đã ẩn danh mục."; }
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoryMove(int id, int delta)
    {
        var category = await db.Loais.FindAsync(id);
        if (category != null) { var meta = AdminMetadataHelper.ParseCategory(category.MoTa); meta.SortOrder = Math.Max(0, meta.SortOrder + delta); category.MoTa = AdminMetadataHelper.BuildCategory(meta); await db.SaveChangesAsync(); TempData["AdminSuccess"] = "Đã cập nhật thứ tự danh mục."; }
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoryDelete(int id)
    {
        var category = await db.Loais.Include(x => x.HangHoas).FirstOrDefaultAsync(x => x.MaLoai == id);
        if (category == null) return RedirectToAction(nameof(Categories));
        if (category.HangHoas.Any()) TempData["AdminError"] = "Không thể xóa danh mục đang có sản phẩm.";
        else { db.Loais.Remove(category); await db.SaveChangesAsync(); TempData["AdminSuccess"] = "Đã xóa danh mục."; }
        return RedirectToAction(nameof(Categories));
    }

    public async Task<IActionResult> Orders(int? status, string? q, string? payment)
    {
        var query = db.HoaDons.Include(x => x.MaKhNavigation).Include(x => x.MaTrangThaiNavigation).Include(x => x.ChiTietHds).AsQueryable();
        if (status.HasValue) query = query.Where(x => x.MaTrangThai == status.Value);
        if (!string.IsNullOrWhiteSpace(payment)) query = query.Where(x => x.CachThanhToan.Contains(payment));
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(x => x.MaHd.ToString().Contains(q) || x.MaKh.Contains(q) || (x.HoTen != null && x.HoTen.Contains(q)) || x.CachThanhToan.Contains(q));
        }
        await LoadStatusSelectList(status); ViewBag.Query = q; ViewBag.Payment = payment;
        return View(await BuildOrderRows(query.OrderByDescending(x => x.NgayDat)));
    }

    public async Task<IActionResult> OrderDetails(int id)
    {
        var order = await db.HoaDons.Include(x => x.MaKhNavigation).Include(x => x.MaTrangThaiNavigation).Include(x => x.ChiTietHds).ThenInclude(x => x.MaHhNavigation).FirstOrDefaultAsync(x => x.MaHd == id);
        if (order == null) return NotFound(); await LoadStatusSelectList(order.MaTrangThai); return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OrderStatus(int id, int statusId, string? adminNote)
    {
        var order = await db.HoaDons.FindAsync(id);
        if (order != null && await db.TrangThais.AnyAsync(x => x.MaTrangThai == statusId))
        {
            order.MaTrangThai = statusId;
            if (IsCompletedStatus(statusId) && (!order.NgayGiao.HasValue || order.NgayGiao.Value.Year <= 1900)) order.NgayGiao = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(adminNote)) order.GhiChu = CompactNote(order.GhiChu, adminNote.Trim());
            await db.SaveChangesAsync(); TempData["AdminSuccess"] = "Đã cập nhật trạng thái đơn hàng.";
        }
        return RedirectToAction(nameof(OrderDetails), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OrderQuickStatus(int id, string actionName)
    {
        var order = await db.HoaDons.FindAsync(id); if (order == null) return RedirectToAction(nameof(Orders));
        int? status = actionName switch { "confirm" => await FindStatusId("xác nhận") ?? await FindStatusId("chờ") ?? 1, "shipping" => await FindStatusId("giao") ?? 2, "complete" => await FindStatusId("hoàn tất") ?? 3, "cancel" => await FindStatusId("hủy") ?? 4, "refund" => await FindStatusId("hoàn tiền") ?? await FindStatusId("hủy") ?? 4, _ => null };
        if (status.HasValue) { order.MaTrangThai = status.Value; if (actionName == "complete") order.NgayGiao = DateTime.Now; if (actionName == "refund") order.GhiChu = CompactNote(order.GhiChu, "refund"); await db.SaveChangesAsync(); TempData["AdminSuccess"] = "Đã cập nhật nhanh trạng thái."; }
        return RedirectToAction(nameof(Orders));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> OrderRefund(int id) => OrderQuickStatus(id, "refund");

    public async Task<IActionResult> Customers(string? q, string? state, string? group)
    {
        var query = db.KhachHangs.Include(x => x.HoaDons).ThenInclude(x => x.ChiTietHds).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) { q = q.Trim(); query = query.Where(x => x.MaKh.Contains(q) || x.HoTen.Contains(q) || x.Email.Contains(q)); }
        if (state == "active") query = query.Where(x => x.HieuLuc); if (state == "locked") query = query.Where(x => !x.HieuLuc);
        var rows = await query.Select(k => new AdminCustomerRowVM { MaKh = k.MaKh, HoTen = k.HoTen, Email = k.Email, DienThoai = k.DienThoai, HieuLuc = k.HieuLuc, VaiTro = k.VaiTro, OrderCount = k.HoaDons.Count, TotalSpent = k.HoaDons.SelectMany(h => h.ChiTietHds).Sum(ct => (double?)(ct.SoLuong * ct.DonGia * (1 - ct.GiamGia))) ?? 0 }).OrderByDescending(x => x.TotalSpent).ToListAsync();
        foreach (var row in rows) row.GroupName = GetCustomerGroup(row.TotalSpent, row.OrderCount, row.VaiTro);
        if (!string.IsNullOrWhiteSpace(group)) rows = rows.Where(x => x.GroupName.Equals(group, StringComparison.OrdinalIgnoreCase)).ToList();
        ViewBag.Query = q; ViewBag.State = state; ViewBag.Group = group;
        return View(rows);
    }

    public async Task<IActionResult> CustomerDetails(string id)
    {
        var customer = await db.KhachHangs.Include(x => x.HoaDons).ThenInclude(x => x.MaTrangThaiNavigation).Include(x => x.HoaDons).ThenInclude(x => x.ChiTietHds).FirstOrDefaultAsync(x => x.MaKh == id);
        if (customer == null) return NotFound(); return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CustomerToggle(string id)
    {
        var customer = await db.KhachHangs.FindAsync(id); if (customer != null) { customer.HieuLuc = !customer.HieuLuc; await db.SaveChangesAsync(); TempData["AdminSuccess"] = customer.HieuLuc ? "Đã mở tài khoản." : "Đã khóa tài khoản."; }
        return RedirectToAction(nameof(Customers));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CustomerRole(string id, int role)
    {
        var customer = await db.KhachHangs.FindAsync(id); if (customer != null) { customer.VaiTro = role == MySetting.ADMIN_ROLE ? MySetting.ADMIN_ROLE : 0; await db.SaveChangesAsync(); TempData["AdminSuccess"] = "Đã cập nhật nhóm/quyền khách hàng."; }
        return RedirectToAction(nameof(CustomerDetails), new { id });
    }

    public async Task<IActionResult> Payments(string? method, string? state)
    {
        var query = db.HoaDons.Include(x => x.MaKhNavigation).Include(x => x.MaTrangThaiNavigation).Include(x => x.ChiTietHds).AsQueryable();
        if (!string.IsNullOrWhiteSpace(method)) query = query.Where(x => x.CachThanhToan.Contains(method));
        var rows = (await BuildOrderRows(query.OrderByDescending(x => x.NgayDat))).Select(x => new AdminPaymentRowVM { MaHd = x.MaHd, CustomerName = x.CustomerName, CustomerId = x.CustomerId, NgayDat = x.NgayDat, StatusName = x.StatusName, StatusId = x.StatusId, PaymentMethod = x.PaymentMethod, Total = x.Total, Note = x.Note, PaymentStatus = GetPaymentStatus(x.PaymentMethod, x.StatusName, x.Note), HasPaymentIssue = HasPaymentIssue(x.PaymentMethod, x.StatusName, x.Note) }).ToList();
        if (state == "error") rows = rows.Where(x => x.HasPaymentIssue).ToList(); if (state == "paid") rows = rows.Where(x => x.PaymentStatus.Contains("Đã thanh toán")).ToList(); if (state == "pending") rows = rows.Where(x => x.PaymentStatus.Contains("Chờ")).ToList(); if (state == "refund") rows = rows.Where(x => x.PaymentStatus.Contains("hoàn tiền", StringComparison.OrdinalIgnoreCase)).ToList();
        ViewBag.Method = method; ViewBag.State = state;
        return View(new AdminPaymentSummaryVM { Rows = rows, TotalTransactions = rows.Count, PaidCount = rows.Count(x => x.PaymentStatus.Contains("Đã thanh toán")), PendingCount = rows.Count(x => x.PaymentStatus.Contains("Chờ")), FailedCount = rows.Count(x => x.HasPaymentIssue), RefundedCount = rows.Count(x => x.PaymentStatus.Contains("hoàn tiền", StringComparison.OrdinalIgnoreCase)), OnlineRevenue = rows.Where(x => x.PaymentStatus.Contains("Đã thanh toán") && !x.PaymentMethod.Contains("COD", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Total) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PaymentMarkFailed(int id)
    {
        var order = await db.HoaDons.FindAsync(id); if (order != null) { order.GhiChu = CompactNote(order.GhiChu, "payment failed"); await db.SaveChangesAsync(); TempData["AdminSuccess"] = "Đã đánh dấu giao dịch lỗi."; }
        return RedirectToAction(nameof(Payments));
    }

    private async Task<List<AdminProductRowVM>> BuildProductRows(IQueryable<HangHoa> query)
    {
        var products = await query.ToListAsync();
        return products.Select(p => { var meta = AdminMetadataHelper.ParseProduct(p.MoTa); return new AdminProductRowVM { MaHh = p.MaHh, TenHh = p.TenHh, TenAlias = p.TenAlias, Hinh = p.Hinh, CategoryName = p.MaLoaiNavigation?.TenLoai ?? "", SupplierName = p.MaNccNavigation?.TenCongTy ?? p.MaNcc, Price = p.DonGia ?? 0, Stock = p.SoLuongTon, ShortDescription = p.MoTaDonVi, MauSac = p.MauSac, KichThuoc = p.KichThuoc, ChatLieu = p.ChatLieu, BaoHanh = p.BaoHanh, PhongCach = p.PhongCach, IsVisible = meta.IsVisible, LowStockThreshold = meta.LowStockThreshold, Sold = p.ChiTietHds.Sum(ct => ct.SoLuong), Revenue = p.ChiTietHds.Sum(ct => ct.SoLuong * ct.DonGia * (1 - ct.GiamGia)) }; }).ToList();
    }

    private async Task<List<AdminOrderRowVM>> BuildOrderRows(IQueryable<HoaDon> query) => await query.Select(h => new AdminOrderRowVM { MaHd = h.MaHd, CustomerName = h.HoTen ?? h.MaKhNavigation.HoTen, CustomerId = h.MaKh, NgayDat = h.NgayDat, StatusName = h.MaTrangThaiNavigation.TenTrangThai, StatusId = h.MaTrangThai, PaymentMethod = h.CachThanhToan, Note = h.GhiChu, Total = h.ChiTietHds.Sum(ct => ct.SoLuong * ct.DonGia * (1 - ct.GiamGia)) + h.PhiVanChuyen }).ToListAsync();
    private async Task LoadProductSelectLists(int? categoryId, string? supplierId) { ViewBag.Categories = new SelectList(await db.Loais.OrderBy(x => x.TenLoai).ToListAsync(), "MaLoai", "TenLoai", categoryId); ViewBag.Suppliers = new SelectList(await db.NhaCungCaps.OrderBy(x => x.TenCongTy).ToListAsync(), "MaNcc", "TenCongTy", supplierId); }
    private async Task LoadCategorySelectList(int? selectedId, int? excludeId = null) { var categories = await db.Loais.Where(x => !excludeId.HasValue || x.MaLoai != excludeId.Value).OrderBy(x => x.TenLoai).ToListAsync(); ViewBag.ParentCategories = new SelectList(categories, "MaLoai", "TenLoai", selectedId); }
    private async Task LoadStatusSelectList(int? selectedId) { ViewBag.Statuses = new SelectList(await db.TrangThais.OrderBy(x => x.MaTrangThai).ToListAsync(), "MaTrangThai", "TenTrangThai", selectedId); }
    private async Task ValidateProductModel(AdminProductFormVM model, int? currentId) { if (!await db.Loais.AnyAsync(x => x.MaLoai == model.MaLoai)) ModelState.AddModelError(nameof(model.MaLoai), "Danh mục không tồn tại."); if (string.IsNullOrWhiteSpace(model.MaNcc) || !await db.NhaCungCaps.AnyAsync(x => x.MaNcc == model.MaNcc)) ModelState.AddModelError(nameof(model.MaNcc), "Nhà cung cấp không tồn tại."); if (!string.IsNullOrWhiteSpace(model.TenAlias) && await db.HangHoas.AnyAsync(x => x.TenAlias == model.TenAlias.Trim() && (!currentId.HasValue || x.MaHh != currentId.Value))) ModelState.AddModelError(nameof(model.TenAlias), "SKU/Alias đã tồn tại."); }
    private void ApplyCategoryForm(Loai category, AdminCategoryFormVM model, IFormFile? hinhUpload) { category.TenLoai = (model.TenLoai ?? string.Empty).Trim(); category.TenLoaiAlias = string.IsNullOrWhiteSpace(model.TenLoaiAlias) ? null : model.TenLoaiAlias.Trim(); if (hinhUpload != null) { var fileName = MyUtil.UploadHinh(hinhUpload, "Loai"); if (!string.IsNullOrWhiteSpace(fileName)) category.Hinh = fileName; } else if (!string.IsNullOrWhiteSpace(model.Hinh)) category.Hinh = model.Hinh.Trim(); category.MoTa = AdminMetadataHelper.BuildCategory(new AdminMetadataHelper.CategoryMeta { Description = model.MoTa, ParentId = model.ParentCategoryId, SortOrder = model.SortOrder, IsVisible = model.IsVisible }); }

    private static bool IsPendingStatus(int id) => id is 0;
    private static bool IsShippingStatus(int id) => id is 2;
    private static bool IsCompletedStatus(int id) => id is 1 or 3;
    private static bool IsCancelledStatus(int id) => id < 0;
    private async Task<int?> FindStatusId(string keyword) => (await db.TrangThais.FirstOrDefaultAsync(x => x.TenTrangThai.Contains(keyword)))?.MaTrangThai;
    private static string CompactNote(string? existing, string addition) { var value = string.IsNullOrWhiteSpace(existing) ? addition : $"{existing}; {addition}"; return value.Length <= 50 ? value : value[..50]; }
    private static string GetCustomerGroup(double totalSpent, int orderCount, int role) { if (role == MySetting.ADMIN_ROLE) return "Admin"; if (totalSpent >= 50_000_000 || orderCount >= 10) return "VIP"; if (totalSpent >= 10_000_000 || orderCount >= 3) return "Thân thiết"; return "Mới"; }
    private static string GetPaymentStatus(string method, string status, string? note) { var data = $"{method} {status} {note}".ToLowerInvariant(); if (data.Contains("lỗi") || data.Contains("fail") || data.Contains("invalid") || data.Contains("hủy")) return "Giao dịch lỗi/hủy"; if (data.Contains("hoàn tiền") || data.Contains("refund")) return "Đã hoàn tiền"; if ((data.Contains("vnpay") || data.Contains("momo")) && (data.Contains("hoàn tất") || data.Contains("completed") || data.Contains("paid"))) return "Đã thanh toán online"; if (data.Contains("cod")) return status.Contains("hoàn", StringComparison.OrdinalIgnoreCase) ? "Đã thu COD" : "Chờ thu COD"; return "Chờ kiểm tra"; }
    private static bool HasPaymentIssue(string method, string status, string? note) { var data = $"{method} {status} {note}".ToLowerInvariant(); return data.Contains("lỗi") || data.Contains("fail") || data.Contains("invalid") || data.Contains("hủy"); }
}
