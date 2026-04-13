using System.ComponentModel.DataAnnotations;

namespace ECommerceMVC.ViewModels
{
    public class DangNhapVM
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [MaxLength(20, ErrorMessage = "Tên đăng nhập tối đa 20 ký tự")]
        public string MaKh { get; set; } = string.Empty;

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string MatKhau { get; set; } = string.Empty;

        [Display(Name = "Ghi nhớ")]
        public bool GhiNho { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
