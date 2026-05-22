namespace ECommerceMVC.Data;

public partial class PasswordResetOtp
{
    public int Id { get; set; }

    public string MaKh { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string OtpHash { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public int AttemptCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual KhachHang MaKhNavigation { get; set; } = null!;
}
