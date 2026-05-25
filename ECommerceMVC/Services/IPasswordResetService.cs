using ECommerceMVC.Data;

namespace ECommerceMVC.Services;

public interface IPasswordResetService
{
    PasswordResetIssueResult CreateOtpForCustomer(KhachHang customer);

    PasswordResetValidationResult ValidateOtp(string maKh, string otp);

    string IssueResetToken(string maKh, int otpRecordId);

    PasswordResetTokenValidationResult ValidateResetToken(string token, string maKh);
}

public sealed record PasswordResetValidationResult(bool Success, PasswordResetOtp? OtpRecord, string? ErrorMessage)
{
    public static PasswordResetValidationResult Ok(PasswordResetOtp otpRecord) => new(true, otpRecord, null);

    public static PasswordResetValidationResult Fail(string errorMessage) => new(false, null, errorMessage);
}

public sealed record PasswordResetIssueResult(bool Success, string? Otp, string? ErrorMessage, int RetryAfterSeconds)
{
    public static PasswordResetIssueResult Ok(string otp) => new(true, otp, null, 0);

    public static PasswordResetIssueResult Fail(string errorMessage, int retryAfterSeconds) => new(false, null, errorMessage, retryAfterSeconds);
}

public sealed record PasswordResetTokenValidationResult(bool Success, int OtpRecordId, string? ErrorMessage)
{
    public static PasswordResetTokenValidationResult Ok(int otpRecordId) => new(true, otpRecordId, null);

    public static PasswordResetTokenValidationResult Fail(string errorMessage) => new(false, 0, errorMessage);
}
