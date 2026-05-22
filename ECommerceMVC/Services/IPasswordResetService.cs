using ECommerceMVC.Data;

namespace ECommerceMVC.Services;

public interface IPasswordResetService
{
    string CreateOtpForCustomer(KhachHang customer);

    PasswordResetValidationResult ValidateOtp(string maKh, string otp);
}

public sealed record PasswordResetValidationResult(bool Success, PasswordResetOtp? OtpRecord, string? ErrorMessage)
{
    public static PasswordResetValidationResult Ok(PasswordResetOtp otpRecord) => new(true, otpRecord, null);

    public static PasswordResetValidationResult Fail(string errorMessage) => new(false, null, errorMessage);
}
