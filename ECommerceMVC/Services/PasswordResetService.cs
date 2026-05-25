using System.Security.Cryptography;
using System.Text;
using ECommerceMVC.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMVC.Services;

public class PasswordResetService : IPasswordResetService
{
    private const int ExpiryMinutes = 10;
    private const int MaxAttempts = 5;
    private const int ResendCooldownSeconds = 60;
    private const int MaxOtpPerWindow = 5;
    private const int WindowMinutes = 15;
    private const int ResetTokenMinutes = 15;
    private readonly Hshop2023Context db;
    private readonly IDataProtector resetTokenProtector;

    public PasswordResetService(Hshop2023Context db, IDataProtectionProvider dataProtectionProvider)
    {
        this.db = db;
        resetTokenProtector = dataProtectionProvider.CreateProtector("DEEPSEARCH.PasswordResetToken.v1");
    }

    public PasswordResetIssueResult CreateOtpForCustomer(KhachHang customer)
    {
        ArgumentNullException.ThrowIfNull(customer);
        var now = DateTime.Now;
        var windowStart = now.AddMinutes(-WindowMinutes);

        var latest = db.PasswordResetOtps
            .Where(x => x.MaKh == customer.MaKh)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        if (latest != null)
        {
            var elapsed = now - latest.CreatedAt;
            if (elapsed.TotalSeconds < ResendCooldownSeconds)
            {
                var retryAfter = Math.Max(1, ResendCooldownSeconds - (int)elapsed.TotalSeconds);
                return PasswordResetIssueResult.Fail("Vui long doi truoc khi gui lai OTP.", retryAfter);
            }
        }

        var recentCount = db.PasswordResetOtps
            .Count(x => x.MaKh == customer.MaKh && x.CreatedAt >= windowStart);

        if (recentCount >= MaxOtpPerWindow)
        {
            var oldestInWindow = db.PasswordResetOtps
                .Where(x => x.MaKh == customer.MaKh && x.CreatedAt >= windowStart)
                .OrderBy(x => x.CreatedAt)
                .Select(x => x.CreatedAt)
                .FirstOrDefault();
            var retryAfter = Math.Max(60, (int)(oldestInWindow.AddMinutes(WindowMinutes) - now).TotalSeconds);
            return PasswordResetIssueResult.Fail("Ban da gui OTP qua nhieu lan. Vui long thu lai sau.", retryAfter);
        }

        var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var activeRecords = db.PasswordResetOtps.Where(x => x.MaKh == customer.MaKh && x.UsedAt == null);
        foreach (var activeRecord in activeRecords)
        {
            activeRecord.UsedAt = now;
        }

        var record = new PasswordResetOtp
        {
            MaKh = customer.MaKh,
            Email = customer.Email,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(ExpiryMinutes),
            AttemptCount = 0,
            OtpHash = HashOtp(customer.MaKh, otp)
        };

        db.PasswordResetOtps.Add(record);
        db.SaveChanges();
        return PasswordResetIssueResult.Ok(otp);
    }

    public PasswordResetValidationResult ValidateOtp(string maKh, string otp)
    {
        maKh = (maKh ?? string.Empty).Trim();
        otp = (otp ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(maKh) || string.IsNullOrWhiteSpace(otp))
        {
            return PasswordResetValidationResult.Fail("Mã OTP không hợp lệ.");
        }

        var record = db.PasswordResetOtps
            .Where(x => x.MaKh == maKh && x.UsedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        if (record == null)
        {
            return PasswordResetValidationResult.Fail("Không tìm thấy mã OTP còn hiệu lực.");
        }

        if (record.ExpiresAt < DateTime.Now)
        {
            return PasswordResetValidationResult.Fail("Mã OTP đã hết hạn.");
        }

        if (record.AttemptCount >= MaxAttempts)
        {
            return PasswordResetValidationResult.Fail("Mã OTP đã vượt quá số lần thử cho phép.");
        }

        var submittedHash = HashOtp(maKh, otp);
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(record.OtpHash),
                Encoding.UTF8.GetBytes(submittedHash)))
        {
            record.AttemptCount = Math.Min(MaxAttempts, record.AttemptCount + 1);
            db.SaveChanges();
            return record.AttemptCount >= MaxAttempts
                ? PasswordResetValidationResult.Fail("Mã OTP đã vượt quá số lần thử cho phép.")
                : PasswordResetValidationResult.Fail("Mã OTP không đúng.");
        }

        record.AttemptCount += 1;
        record.UsedAt = DateTime.Now;
        db.SaveChanges();
        return PasswordResetValidationResult.Ok(record);
    }

    public string IssueResetToken(string maKh, int otpRecordId)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(ResetTokenMinutes).ToUnixTimeSeconds();
        var nonce = Guid.NewGuid().ToString("N");
        var payload = $"{maKh}|{otpRecordId}|{expiresAt}|{nonce}";
        return resetTokenProtector.Protect(payload);
    }

    public PasswordResetTokenValidationResult ValidateResetToken(string token, string maKh)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(maKh))
        {
            return PasswordResetTokenValidationResult.Fail("Phien dat lai mat khau khong hop le.");
        }

        try
        {
            var payload = resetTokenProtector.Unprotect(token);
            var parts = payload.Split('|');
            if (parts.Length != 4)
            {
                return PasswordResetTokenValidationResult.Fail("Phien dat lai mat khau khong hop le.");
            }

            var tokenMaKh = parts[0];
            if (!int.TryParse(parts[1], out var otpRecordId))
            {
                return PasswordResetTokenValidationResult.Fail("Phien dat lai mat khau khong hop le.");
            }

            if (!long.TryParse(parts[2], out var expiresAtUnix))
            {
                return PasswordResetTokenValidationResult.Fail("Phien dat lai mat khau khong hop le.");
            }

            if (!string.Equals(tokenMaKh, maKh, StringComparison.Ordinal))
            {
                return PasswordResetTokenValidationResult.Fail("Phien dat lai mat khau khong khop tai khoan.");
            }

            var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (expiresAtUnix < nowUnix)
            {
                return PasswordResetTokenValidationResult.Fail("Phien dat lai mat khau da het han.");
            }

            return PasswordResetTokenValidationResult.Ok(otpRecordId);
        }
        catch
        {
            return PasswordResetTokenValidationResult.Fail("Phien dat lai mat khau khong hop le.");
        }
    }

    private static string HashOtp(string maKh, string otp)
    {
        var payload = $"{maKh.Trim().ToLowerInvariant()}:{otp.Trim()}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes);
    }
}
