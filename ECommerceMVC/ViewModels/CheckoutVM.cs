using System.ComponentModel.DataAnnotations;

namespace ECommerceMVC.ViewModels
{
    public class CheckoutVM
    {
        [Display(Name = "Mã khách hàng")]
        [Required(ErrorMessage = "Vui lòng nhập mã khách hàng")]
        [MaxLength(20)]
        public string MaKh { get; set; } = string.Empty;

        [Display(Name = "Họ tên người nhận")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [MaxLength(50)]
        public string HoTen { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        [MaxLength(24)]
        public string? DienThoai { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Display(Name = "Địa chỉ nhận hàng")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        [MaxLength(60)]
        public string DiaChi { get; set; } = string.Empty;

        [Display(Name = "Ghi chú")]
        [MaxLength(200)]
        public string? GhiChu { get; set; }

        public string CachThanhToan { get; set; } = "COD";
        public string CachVanChuyen { get; set; } = "Giao hàng tiêu chuẩn";
        public double PhiVanChuyen { get; set; } = 0;
    }
}
