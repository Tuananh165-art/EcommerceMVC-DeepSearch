using System.ComponentModel.DataAnnotations;

namespace ECommerceMVC.ViewModels;

public class ForgotPasswordVM
{
    [Display(Name = "Email hoặc tên đăng nhập")]
    [Required(ErrorMessage = "Vui lòng nhập email hoặc tên đăng nhập")]
    [MaxLength(80, ErrorMessage = "Thông tin tìm kiếm tối đa 80 ký tự")]
    public string EmailOrUsername { get; set; } = string.Empty;
}
