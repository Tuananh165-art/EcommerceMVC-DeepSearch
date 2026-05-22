namespace ECommerceMVC.Services;

public class ShippingFeeService : IShippingFeeService
{
    public double Calculate(string? address, string? shippingMethod, double subtotal)
    {
        if (subtotal >= 3_000_000)
        {
            return 0;
        }

        var normalizedAddress = (address ?? string.Empty).Trim().ToLowerInvariant();
        var fee = normalizedAddress.Contains("hồ chí minh")
            || normalizedAddress.Contains("ho chi minh")
            || normalizedAddress.Contains("tp.hcm")
            || normalizedAddress.Contains("hcm")
            || normalizedAddress.Contains("sài gòn")
            || normalizedAddress.Contains("sai gon")
                ? 25_000d
                : normalizedAddress.Contains("hà nội") || normalizedAddress.Contains("ha noi")
                    ? 35_000d
                    : 50_000d;

        var normalizedMethod = (shippingMethod ?? string.Empty).Trim().ToLowerInvariant();
        if (normalizedMethod.Contains("nhanh") || normalizedMethod.Contains("express"))
        {
            fee += 30_000d;
        }

        return fee;
    }
}
