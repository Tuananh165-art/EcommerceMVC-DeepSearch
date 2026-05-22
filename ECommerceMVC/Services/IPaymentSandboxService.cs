using ECommerceMVC.ViewModels;

namespace ECommerceMVC.Services;

public interface IPaymentSandboxService
{
    PaymentSandboxResult ProcessSandboxPayment(string paymentMethod, CheckoutVM checkout, double amount);
}

public class PaymentSandboxResult
{
    public bool IsSandbox { get; set; }
    public bool IsSuccess { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public string TransactionCode { get; set; } = string.Empty;
}
