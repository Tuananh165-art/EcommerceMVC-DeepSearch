using System.ComponentModel.DataAnnotations;

namespace ECommerceMVC.ViewModels
{
    public class DangNhapVM
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [MaxLength(20)]
        public string MaKh { get; set; } = string.Empty;

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = string.Empty;

        [Display(Name = "Ghi nhớ")]
        public bool GhiNho { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
