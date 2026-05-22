using ECommerceMVC.Data;

namespace ECommerceMVC.Services;

public class VoucherService : IVoucherService
{
    private readonly Hshop2023Context db;

    public VoucherService(Hshop2023Context db)
    {
        this.db = db;
    }

    public VoucherResult ValidateAndCalculateDiscount(string? code, double subtotal)
    {
        code = (code ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code))
        {
            return VoucherResult.Fail("Vui lòng nhập mã voucher.");
        }

        var now = DateTime.Now;
        var voucher = db.Vouchers.FirstOrDefault(x => x.Code.ToUpper() == code);
        if (voucher == null || !voucher.IsActive || voucher.StartsAt > now || voucher.EndsAt < now)
        {
            return VoucherResult.Fail("Mã voucher không hợp lệ hoặc đã hết hạn.");
        }

        if (voucher.UsageLimit.HasValue && voucher.UsedCount >= voucher.UsageLimit.Value)
        {
            return VoucherResult.Fail("Mã voucher đã hết lượt sử dụng.");
        }

        if (subtotal < voucher.MinSubtotal)
        {
            return VoucherResult.Fail($"Đơn hàng tối thiểu {voucher.MinSubtotal:N0} VND để dùng voucher này.");
        }

        var discount = string.Equals(voucher.DiscountType, "Percent", StringComparison.OrdinalIgnoreCase)
            ? subtotal * Math.Max(0, voucher.DiscountValue) / 100d
            : Math.Max(0, voucher.DiscountValue);

        if (voucher.MaxDiscount.HasValue)
        {
            discount = Math.Min(discount, voucher.MaxDiscount.Value);
        }

        discount = Math.Min(Math.Max(0, discount), subtotal);
        return VoucherResult.Ok(voucher.Code, discount, "Áp dụng voucher thành công.");
    }
}
