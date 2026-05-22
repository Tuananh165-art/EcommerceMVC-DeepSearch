using System.Security.Cryptography;
using System.Text;
using ECommerceMVC.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMVC.Services;

public class PasswordResetService : IPasswordResetService
{
    private const int ExpiryMinutes = 10;
    private const int MaxAttempts = 5;
    private readonly Hshop2023Context db;

    public PasswordResetService(Hshop2023Context db)
    {
        this.db = db;
    }

    public string CreateOtpForCustomer(KhachHang customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var now = DateTime.Now;
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
        return otp;
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

    private static string HashOtp(string maKh, string otp)
    {
        var payload = $"{maKh.Trim().ToLowerInvariant()}:{otp.Trim()}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes);
    }
}
