namespace ECommerceMVC.Services;

public interface IVoucherService
{
    VoucherResult ValidateAndCalculateDiscount(string? code, double subtotal);
}

public sealed record VoucherResult(bool Success, string Code, double DiscountAmount, string Message)
{
    public static VoucherResult Fail(string message) => new(false, string.Empty, 0, message);
    public static VoucherResult Ok(string code, double discountAmount, string message) => new(true, code, discountAmount, message);
}
