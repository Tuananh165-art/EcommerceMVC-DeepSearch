using ECommerceMVC.ViewModels;

namespace ECommerceMVC.Services;

public interface IVnPayService
{
    string CreatePaymentUrl(CheckoutVM checkout, List<CartItem> cartItems, string customerId, string clientIp, string baseReturnUrl);
    VnPayReturnResult ValidateReturn(IQueryCollection query);
}

public class VnPayReturnResult
{
    public bool IsSuccess { get; set; }
    public bool SignatureValid { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public string TransactionNo { get; set; } = string.Empty;
    public string ResponseCode { get; set; } = string.Empty;
    public string TransactionStatus { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string OrderInfo { get; set; } = string.Empty;
    public string RawPaymentMethod { get; set; } = "VNPAY";
}
