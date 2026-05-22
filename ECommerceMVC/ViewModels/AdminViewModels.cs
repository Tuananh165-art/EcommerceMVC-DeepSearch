using System.ComponentModel.DataAnnotations;
using ECommerceMVC.Data;
using ECommerceMVC.Helpers;

namespace ECommerceMVC.ViewModels;

public class AdminDashboardVM
{
    public double Revenue { get; set; }
    public int OrderCount { get; set; }
    public int CustomerCount { get; set; }
    public int ProductCount { get; set; }
    public int PendingOrderCount { get; set; }
    public int ShippingOrderCount { get; set; }
    public int CompletedOrderCount { get; set; }
    public int CancelledOrderCount { get; set; }
    public int RefundedOrderCount { get; set; }
    public int FailedPaymentCount { get; set; }
    public int LowStockCount { get; set; }
    public List<AdminTopProductVM> TopProducts { get; set; } = new();
    public List<AdminOrderRowVM> NewOrders { get; set; } = new();
    public List<AdminProductRowVM> LowStockProducts { get; set; } = new();
    public List<AdminChartPointVM> DailyRevenue { get; set; } = new();
    public List<AdminChartPointVM> MonthlyRevenue { get; set; } = new();
}

public class AdminTopProductVM
{
    public int MaHh { get; set; }
    public string TenHh { get; set; } = string.Empty;
    public string? Hinh { get; set; }
    public int QuantitySold { get; set; }
    public double Revenue { get; set; }
}

public class AdminChartPointVM
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
}

public class AdminProductRowVM
{
    public int MaHh { get; set; }
    public string TenHh { get; set; } = string.Empty;
    public string? TenAlias { get; set; }
    public string? Hinh { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public double Price { get; set; }
    public int Stock { get; set; }
    public string? ShortDescription { get; set; }
    public string? MauSac { get; set; }
    public string? KichThuoc { get; set; }
    public string? ChatLieu { get; set; }
    public string? BaoHanh { get; set; }
    public string? PhongCach { get; set; }
    public bool IsVisible { get; set; }
    public int LowStockThreshold { get; set; }
    public bool IsLowStock => Stock <= LowStockThreshold;
    public bool IsOutOfStock => Stock <= 0;
    public int Sold { get; set; }
    public double Revenue { get; set; }
}

public class AdminProductFormVM
{
    public int MaHh { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "Tên sản phẩm")]
    public string TenHh { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "SKU / Alias")]
    public string? TenAlias { get; set; }

    [Display(Name = "Danh mục")]
    public int MaLoai { get; set; }

    [StringLength(50)]
    [Display(Name = "Mô tả ngắn")]
    public string? MoTaDonVi { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Giá")]
    public double? DonGia { get; set; }

    [StringLength(50)]
    public string? Hinh { get; set; }

    [Display(Name = "Ngày sản xuất")]
    public DateTime NgaySx { get; set; } = DateTime.Today;

    [Range(0, 1)]
    [Display(Name = "Giảm giá")]
    public double GiamGia { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Tồn kho")]
    public int SoLuongTon { get; set; }

    [StringLength(30)]
    [Display(Name = "Màu sắc")]
    public string? MauSac { get; set; }

    [StringLength(80)]
    [Display(Name = "Chất liệu")]
    public string? ChatLieu { get; set; }

    [StringLength(80)]
    [Display(Name = "Kích thước / Size")]
    public string? KichThuoc { get; set; }

    [StringLength(40)]
    [Display(Name = "Bảo hành")]
    public string? BaoHanh { get; set; }

    [StringLength(50)]
    [Display(Name = "Phong cách")]
    public string? PhongCach { get; set; }

    [Display(Name = "Mô tả chi tiết")]
    public string? MoTa { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "Nhà cung cấp")]
    public string MaNcc { get; set; } = string.Empty;

    [Display(Name = "Hiển thị ở shop")]
    public bool IsVisible { get; set; } = true;

    [Range(0, int.MaxValue)]
    [Display(Name = "Ngưỡng cảnh báo tồn kho")]
    public int LowStockThreshold { get; set; } = 5;

    public int SoLanXem { get; set; }

    public static AdminProductFormVM FromEntity(HangHoa item)
    {
        var meta = AdminMetadataHelper.ParseProduct(item.MoTa);
        return new()
        {
            MaHh = item.MaHh,
            TenHh = item.TenHh,
            TenAlias = item.TenAlias,
            MaLoai = item.MaLoai,
            MoTaDonVi = item.MoTaDonVi,
            DonGia = item.DonGia,
            Hinh = item.Hinh,
            NgaySx = item.NgaySx,
            GiamGia = item.GiamGia,
            SoLanXem = item.SoLanXem,
            SoLuongTon = item.SoLuongTon,
            MauSac = item.MauSac,
            ChatLieu = item.ChatLieu,
            KichThuoc = item.KichThuoc,
            BaoHanh = item.BaoHanh,
            PhongCach = item.PhongCach,
            MoTa = meta.Description,
            IsVisible = meta.IsVisible,
            LowStockThreshold = meta.LowStockThreshold,
            MaNcc = item.MaNcc
        };
    }

    public void ApplyTo(HangHoa item)
    {
        item.TenHh = (TenHh ?? string.Empty).Trim();
        item.TenAlias = string.IsNullOrWhiteSpace(TenAlias) ? null : TenAlias.Trim();
        item.MaLoai = MaLoai;
        item.MoTaDonVi = string.IsNullOrWhiteSpace(MoTaDonVi) ? null : MoTaDonVi.Trim();
        item.DonGia = DonGia ?? 0;
        item.NgaySx = NgaySx == default ? DateTime.Today : NgaySx;
        item.GiamGia = Math.Clamp(GiamGia, 0, 1);
        item.SoLuongTon = Math.Max(0, SoLuongTon);
        item.MauSac = string.IsNullOrWhiteSpace(MauSac) ? null : MauSac.Trim();
        item.ChatLieu = string.IsNullOrWhiteSpace(ChatLieu) ? null : ChatLieu.Trim();
        item.KichThuoc = string.IsNullOrWhiteSpace(KichThuoc) ? null : KichThuoc.Trim();
        item.BaoHanh = string.IsNullOrWhiteSpace(BaoHanh) ? null : BaoHanh.Trim();
        item.PhongCach = string.IsNullOrWhiteSpace(PhongCach) ? null : PhongCach.Trim();
        item.MoTa = AdminMetadataHelper.BuildProduct(new AdminMetadataHelper.ProductMeta
        {
            Description = string.IsNullOrWhiteSpace(MoTa) ? null : MoTa.Trim(),
            IsVisible = IsVisible,
            LowStockThreshold = Math.Max(0, LowStockThreshold)
        });
        item.MaNcc = (MaNcc ?? string.Empty).Trim();
    }
}

public class AdminCategoryFormVM
{
    public int MaLoai { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "Tên danh mục")]
    public string TenLoai { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "Alias")]
    public string? TenLoaiAlias { get; set; }

    [StringLength(50)]
    [Display(Name = "Hình")]
    public string? Hinh { get; set; }

    [Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    [Display(Name = "Danh mục cha")]
    public int? ParentCategoryId { get; set; }

    [Display(Name = "Thứ tự sắp xếp")]
    public int SortOrder { get; set; } = 100;

    [Display(Name = "Hiển thị")]
    public bool IsVisible { get; set; } = true;
}

public class AdminCategoryRowVM : AdminCategoryFormVM
{
    public int ProductCount { get; set; }
    public string? ParentCategoryName { get; set; }
    public int VisibleProductCount { get; set; }
}

public class AdminOrderRowVM
{
    public int MaHd { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public DateTime NgayDat { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int StatusId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public double Total { get; set; }
    public string? Note { get; set; }
    public bool IsPaymentIssue { get; set; }
}

public class AdminCustomerRowVM
{
    public string MaKh { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DienThoai { get; set; }
    public bool HieuLuc { get; set; }
    public int VaiTro { get; set; }
    public int OrderCount { get; set; }
    public double TotalSpent { get; set; }
    public string GroupName { get; set; } = string.Empty;
}

public class AdminPaymentRowVM : AdminOrderRowVM
{
    public string PaymentStatus { get; set; } = string.Empty;
    public bool HasPaymentIssue { get; set; }
}

public class AdminPaymentSummaryVM
{
    public int TotalTransactions { get; set; }
    public int PaidCount { get; set; }
    public int PendingCount { get; set; }
    public int FailedCount { get; set; }
    public int RefundedCount { get; set; }
    public double OnlineRevenue { get; set; }
    public List<AdminPaymentRowVM> Rows { get; set; } = new();
}
