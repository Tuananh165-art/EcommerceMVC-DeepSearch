using System.ComponentModel.DataAnnotations;
using System.IO;

namespace ECommerceMVC.ViewModels
{
    public class EditProfileVM
    {
        [Display(Name = "Tên đăng nhập")]
        public string MaKh { get; set; } = string.Empty;

        [Display(Name = "Họ tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [MaxLength(50, ErrorMessage = "Họ tên tối đa 50 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [Display(Name = "Giới tính")]
        public bool GioiTinh { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [MaxLength(60, ErrorMessage = "Địa chỉ tối đa 60 ký tự")]
        public string DiaChi { get; set; } = string.Empty;

        [Display(Name = "Điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng số 0")]
        public string DienThoai { get; set; } = string.Empty;

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email chưa đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        public string? Hinh { get; set; }
        public string HinhUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Hinh)) return "/Hinh/KhachHang/default-avatar.svg";
                var safeFile = Path.GetFileName(Hinh.Trim());
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", "KhachHang", safeFile);
                return File.Exists(fullPath) ? $"/Hinh/KhachHang/{safeFile}" : "/Hinh/KhachHang/default-avatar.svg";
            }
        }
    }
}
