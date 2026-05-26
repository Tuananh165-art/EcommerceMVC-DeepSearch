using ECommerceMVC.Helpers;

namespace ECommerceMVC.ViewModels
{
    public class LichSuDonHangItemVM
    {
        public int MaHd { get; set; }
        public DateTime NgayDat { get; set; }
        public int StatusId { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string CachThanhToan { get; set; } = string.Empty;
        public int TongSoLuong { get; set; }
        public double TongTien { get; set; }
    }

    public class ChiTietDonHangLineVM
    {
        public int MaHh { get; set; }
        public string TenHh { get; set; } = string.Empty;
        public string Hinh { get; set; } = string.Empty;
        public string HinhUrl => MyUtil.GetHangHoaImageUrl(Hinh, MaHh);
        public int SoLuong { get; set; }
        public double DonGia { get; set; }
        public double GiamGia { get; set; }
        public double ThanhTien => SoLuong * DonGia * (1 - GiamGia);
    }

    public class ChiTietDonHangVM
    {
        public int MaHd { get; set; }
        public DateTime NgayDat { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public string CachThanhToan { get; set; } = string.Empty;
        public string CachVanChuyen { get; set; } = string.Empty;
        public double PhiVanChuyen { get; set; }
        public string? GhiChu { get; set; }
        public List<ChiTietDonHangLineVM> Items { get; set; } = new();
        public double Subtotal => Items.Sum(x => x.ThanhTien);
        public double Total => Subtotal + PhiVanChuyen;
    }
}
