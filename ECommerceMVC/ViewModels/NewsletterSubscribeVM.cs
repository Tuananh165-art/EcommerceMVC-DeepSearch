using System.ComponentModel.DataAnnotations;

namespace ECommerceMVC.ViewModels;

public class NewsletterSubscribeVM
{
    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}