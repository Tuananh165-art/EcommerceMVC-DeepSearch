using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMVC.Controllers;

public class PasswordRecoveryController : Controller
{
    private readonly Hshop2023Context db;
    private readonly IPasswordResetService passwordResetService;
    private readonly IPasswordService passwordService;
    private readonly IEmailService emailService;

    public PasswordRecoveryController(
        Hshop2023Context db,
        IPasswordResetService passwordResetService,
        IPasswordService passwordService,
        IEmailService emailService)
    {
        this.db = db;
        this.passwordResetService = passwordResetService;
        this.passwordService = passwordService;
        this.emailService = emailService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SendOtp([FromForm] string emailOrUsername)
    {
        var lookup = (emailOrUsername ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(lookup))
        {
            return Json(new { ok = false, message = "Vui long nhap email hoac ten dang nhap." });
        }

        var khachHang = db.KhachHangs.FirstOrDefault(x => x.HieuLuc && (x.MaKh.ToLower() == lookup || x.Email.ToLower() == lookup));
        if (khachHang == null)
        {
            return Json(new { ok = false, message = "Khong tim thay tai khoan." });
        }

        var issue = passwordResetService.CreateOtpForCustomer(khachHang);
        if (!issue.Success || string.IsNullOrWhiteSpace(issue.Otp))
        {
            return Json(new { ok = false, message = issue.ErrorMessage ?? "Khong the gui OTP.", retryAfter = issue.RetryAfterSeconds });
        }

        var subject = "[DEEPSEARCH] OTP dat lai mat khau";
        var body = BuildOtpEmail(khachHang.HoTen, issue.Otp, khachHang.MaKh);
        if (!emailService.TrySend(khachHang.Email, subject, body, out var emailError))
        {
            return Json(new { ok = false, message = $"Khong gui duoc email OTP ({emailError})." });
        }

        return Json(new { ok = true, maKh = khachHang.MaKh, maskedEmail = MaskEmail(khachHang.Email), resendAfter = 60 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult VerifyOtp([FromForm] string maKh, [FromForm] string otp)
    {
        maKh = (maKh ?? string.Empty).Trim();
        otp = (otp ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(maKh) || string.IsNullOrWhiteSpace(otp))
        {
            return Json(new { ok = false, message = "Vui long nhap day du OTP." });
        }

        var result = passwordResetService.ValidateOtp(maKh, otp);
        if (!result.Success || result.OtpRecord == null)
        {
            return Json(new { ok = false, message = result.ErrorMessage ?? "OTP khong hop le." });
        }

        var resetToken = passwordResetService.IssueResetToken(maKh, result.OtpRecord.Id);
        return Json(new { ok = true, resetToken });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CompleteReset([FromForm] string maKh, [FromForm] string resetToken, [FromForm] string newPassword, [FromForm] string confirmPassword)
    {
        maKh = (maKh ?? string.Empty).Trim();
        newPassword = (newPassword ?? string.Empty).Trim();
        confirmPassword = (confirmPassword ?? string.Empty).Trim();

        var account = db.KhachHangs.SingleOrDefault(x => x.MaKh == maKh && x.HieuLuc);
        if (account == null)
        {
            return Json(new { ok = false, message = "Khong tim thay tai khoan hop le." });
        }

        var tokenValidation = passwordResetService.ValidateResetToken(resetToken ?? string.Empty, maKh);
        if (!tokenValidation.Success)
        {
            return Json(new { ok = false, message = tokenValidation.ErrorMessage ?? "Phien dat lai mat khau khong hop le." });
        }

        if (!ValidateStrongPassword(newPassword))
        {
            return Json(new { ok = false, message = "Mat khau qua yeu." });
        }

        if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
        {
            return Json(new { ok = false, message = "Xac nhan mat khau khong khop." });
        }

        passwordService.SetPassword(account, newPassword);
        db.SaveChanges();

        HttpContext.Session.Remove(MySetting.CART_KEY);
        HttpContext.Session.Remove(MySetting.CUSTOMER_KEY);

        return Json(new { ok = true, redirectUrl = Url.Action("DangNhap", "KhachHang") });
    }

    private static bool ValidateStrongPassword(string password)
    {
        if (password.Length < 8) return false;
        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));
        return hasUpper && hasLower && hasDigit && hasSpecial;
    }

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1) return "***";
        var head = email[..Math.Min(2, at)];
        return $"{head}***{email[at..]}";
    }

    private static string BuildOtpEmail(string fullName, string otp, string username)
    {
        return $@"<div style='font-family:Segoe UI,Arial,sans-serif;background:#f8f3ed;padding:24px;'>
<div style='max-width:620px;margin:0 auto;background:#fffdf9;border:1px solid #eadfce;border-radius:16px;overflow:hidden;'>
<div style='padding:22px 28px;background:linear-gradient(135deg,#fbf1e3,#d7b38c);color:#2d1f16;'>
<div style='font-size:28px;font-weight:600;letter-spacing:0.06em;'>DEEPSEARCH</div>
<div style='margin-top:8px;font-size:13px;opacity:0.85;'>Luxury Account Security</div>
</div>
<div style='padding:26px 28px;color:#3a2a1e;'>
<p>Xin chao {fullName},</p>
<p>Ban vua yeu cau dat lai mat khau cho tai khoan <strong>{username}</strong>.</p>
<p style='margin:18px 0 8px;'>Ma OTP cua ban:</p>
<div style='font-size:36px;font-weight:700;letter-spacing:0.32em;color:#6a432d;'>{otp}</div>
<p style='margin-top:14px;'>Ma co hieu luc trong <strong>10 phut</strong> va chi duoc dung mot lan.</p>
<p style='margin-top:16px;font-size:13px;color:#7a6351;'>Neu ban khong yeu cau thao tac nay, vui long bo qua email.</p>
<a href='https://deepsearch.local' style='display:inline-block;margin-top:16px;background:#8f5b3e;color:#fff;text-decoration:none;padding:11px 18px;border-radius:999px;'>Quay lai DEEPSEARCH</a>
</div></div></div>";
    }
}



