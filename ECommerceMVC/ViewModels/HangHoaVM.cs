using ECommerceMVC.Helpers;
using System.ComponentModel.DataAnnotations;

namespace ECommerceMVC.ViewModels
{
	public class HangHoaVM
	{
		public int MaHh { get; set; }
		public string TenHH { get; set; } = string.Empty;
		public string Hinh { get; set; } = string.Empty;
		public string HinhUrl => MyUtil.GetHangHoaImageUrl(Hinh, MaHh);
		public double DonGia { get; set; }
		public string MoTaNgan { get; set; } = string.Empty;
		public string TenLoai { get; set; } = string.Empty;
		public bool IsFavourite { get; set; }
		public int DiemDanhGia { get; set; }
		public double DiemDanhGiaTrungBinh { get; set; }
		public int SoDanhGia { get; set; }
	}

	public class ChiTietHangHoaVM
	{
		public int MaHh { get; set; }
		public string TenHH { get; set; } = string.Empty;
		public string Hinh { get; set; } = string.Empty;
		public string HinhUrl => MyUtil.GetHangHoaImageUrl(Hinh, MaHh);
		public double DonGia { get; set; }
		public string MoTaNgan { get; set; } = string.Empty;
		public string TenLoai { get; set; } = string.Empty;
		public string ChiTiet { get; set; } = string.Empty;
		public string MauSac { get; set; } = string.Empty;
		public string ChatLieu { get; set; } = string.Empty;
		public string KichThuoc { get; set; } = string.Empty;
		public string BaoHanh { get; set; } = string.Empty;
		public string PhongCach { get; set; } = string.Empty;
		public string TenNhaCungCap { get; set; } = string.Empty;
		public int DiemDanhGia { get; set; }
		public int SoLuongTon { get; set; }
		public bool IsFavourite { get; set; }
		public bool CanReview { get; set; }
		public bool HasReviews => Reviews.Any();
		public bool HasDetailedSpecs => !string.IsNullOrWhiteSpace(ChatLieu) || !string.IsNullOrWhiteSpace(KichThuoc) || !string.IsNullOrWhiteSpace(BaoHanh) || !string.IsNullOrWhiteSpace(PhongCach);
		public string TinhTrangKho => SoLuongTon <= 0 ? "Hết hàng" : SoLuongTon <= 5 ? $"Sắp hết • còn {SoLuongTon}" : "Sẵn hàng";
		public string MucGia => DonGia >= 4000000 ? "Cao cấp" : DonGia >= 2000000 ? "Trung cao" : DonGia >= 800000 ? "Tiêu chuẩn" : "Trang trí nhỏ";
		public List<ProductReviewVM> Reviews { get; set; } = new();
		public List<HangHoaVM> RelatedProducts { get; set; } = new();
		public ProductReviewInputVM NewReview { get; set; } = new();
	}

	public class ProductReviewVM
	{
		public string MaKh { get; set; } = string.Empty;
		public string HoTen { get; set; } = string.Empty;
		public int SoSao { get; set; }
		public string NoiDung { get; set; } = string.Empty;
		public DateTime NgayTao { get; set; }
	}

	public class ProductReviewInputVM
	{
		public int MaHh { get; set; }

		[Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5")]
		public int SoSao { get; set; } = 5;

		[Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá")]
		[StringLength(500, ErrorMessage = "Đánh giá tối đa 500 ký tự")]
		public string NoiDung { get; set; } = string.Empty;
	}
}
