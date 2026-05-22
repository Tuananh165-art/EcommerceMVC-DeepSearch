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
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [MaxLength(24, ErrorMessage = "Số điện thoại tối đa 24 ký tự")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng số 0")]
        public string DienThoai { get; set; } = string.Empty;

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Địa chỉ nhận hàng")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        [MaxLength(60)]
        public string DiaChi { get; set; } = string.Empty;

        [Display(Name = "Ghi chú")]
        [MaxLength(50, ErrorMessage = "Ghi chú tối đa 50 ký tự")]
        public string? GhiChu { get; set; }

        [Display(Name = "Phương thức thanh toán")]
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [MaxLength(50)]
        public string CachThanhToan { get; set; } = "COD";
        public string CachVanChuyen { get; set; } = "Giao hàng tiêu chuẩn";
        public double PhiVanChuyen { get; set; } = 0;
        public string? VoucherCode { get; set; }
        public double DiscountAmount { get; set; }
    }

    public class CheckoutSuccessVM
    {
        public int OrderId { get; set; }
        public int TotalQuantityPaid { get; set; }
        public double SubtotalPaid { get; set; }
        public double ShippingFee { get; set; }
        public double DiscountAmount { get; set; }
        public string? VoucherCode { get; set; }
        public double TotalPaid { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
    }
}
