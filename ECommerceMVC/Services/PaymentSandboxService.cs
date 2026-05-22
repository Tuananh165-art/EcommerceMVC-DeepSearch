using ECommerceMVC.ViewModels;
using Microsoft.Extensions.Options;

namespace ECommerceMVC.Services;

public class PaymentSandboxService : IPaymentSandboxService
{
    private readonly PaymentGatewaySettings settings;

    public PaymentSandboxService(IOptions<PaymentGatewaySettings> options)
    {
        settings = options.Value;
    }

    public PaymentSandboxResult ProcessSandboxPayment(string paymentMethod, CheckoutVM checkout, double amount)
    {
        if (string.Equals(paymentMethod, "VNPAY_SANDBOX", StringComparison.OrdinalIgnoreCase))
        {
            return BuildResult("VNPay Sandbox", paymentMethod, amount, settings.VnPay.ReturnUrl);
        }

        if (string.Equals(paymentMethod, "MOMO_SANDBOX", StringComparison.OrdinalIgnoreCase))
        {
            return BuildResult("MoMo Sandbox", paymentMethod, amount, settings.MoMo.ReturnUrl);
        }

        return new PaymentSandboxResult
        {
            IsSandbox = false,
            IsSuccess = true,
            ProviderName = "COD",
            StatusText = "Thanh toán khi nhận hàng",
            TransactionCode = string.Empty
        };
    }

    private static PaymentSandboxResult BuildResult(string providerName, string paymentMethod, double amount, string callbackUrl)
    {
        var transactionCode = $"{paymentMethod}-{DateTime.Now:yyyyMMddHHmmssfff}";
        var status = $"Thanh toán sandbox thành công - {providerName}";

        if (!string.IsNullOrWhiteSpace(callbackUrl))
        {
            status += " (đã mô phỏng callback)";
        }

        return new PaymentSandboxResult
        {
            IsSandbox = true,
            IsSuccess = amount >= 0,
            ProviderName = providerName,
            StatusText = status,
            TransactionCode = transactionCode
        };
    }
}
