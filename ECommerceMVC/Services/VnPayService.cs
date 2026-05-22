using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ECommerceMVC.Services;

public class VnPayService : IVnPayService
{
    private readonly PaymentGatewaySettings settings;

    public VnPayService(IOptions<PaymentGatewaySettings> options)
    {
        settings = options.Value;
    }

    public string CreatePaymentUrl(CheckoutVM checkout, List<CartItem> cartItems, string customerId, string clientIp, string baseReturnUrl)
    {
        if (string.IsNullOrWhiteSpace(settings.VnPay.TmnCode) || string.IsNullOrWhiteSpace(settings.VnPay.HashSecret) || string.IsNullOrWhiteSpace(settings.VnPay.PaymentUrl))
        {
            throw new InvalidOperationException("Chưa cấu hình đầy đủ VNPay.");
        }

        var totalAmount = cartItems.Sum(x => x.ThanhTien) + Math.Max(0, checkout.PhiVanChuyen);
        if (totalAmount < 5_000 || totalAmount > 1_000_000_000)
        {
            throw new InvalidOperationException("Số tiền thanh toán VNPay phải nằm trong khoảng 5.000 đến 1.000.000.000 VND.");
        }

        var amount = (long)Math.Round(totalAmount * 100, MidpointRounding.AwayFromZero);
        var txnRef = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
        var returnUrl = string.IsNullOrWhiteSpace(settings.VnPay.ReturnUrl) ? baseReturnUrl : settings.VnPay.ReturnUrl;
        var orderInfo = $"Thanh toan don hang cho {customerId}";

        var requestData = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = settings.VnPay.TmnCode,
            ["vnp_Amount"] = amount.ToString(CultureInfo.InvariantCulture),
            ["vnp_CreateDate"] = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            ["vnp_CurrCode"] = "VND",
            ["vnp_IpAddr"] = NormalizeIpAddress(clientIp),
            ["vnp_Locale"] = "vn",
            ["vnp_OrderInfo"] = orderInfo,
            ["vnp_OrderType"] = "other",
            ["vnp_ReturnUrl"] = returnUrl,
            ["vnp_TxnRef"] = txnRef
        };

        var query = BuildRequestData(requestData);
        var secureHash = ComputeHmacSha512(settings.VnPay.HashSecret, query);

        return $"{settings.VnPay.PaymentUrl}?{query}&vnp_SecureHash={secureHash}";
    }

    public VnPayReturnResult ValidateReturn(IQueryCollection query)
    {
        var raw = query.ToDictionary(k => k.Key, v => v.Value.ToString(), StringComparer.Ordinal);
        var providedHash = raw.TryGetValue("vnp_SecureHash", out var hashValue) ? hashValue : string.Empty;
        raw.Remove("vnp_SecureHash");
        raw.Remove("vnp_SecureHashType");

        var sorted = new SortedDictionary<string, string>(raw, StringComparer.Ordinal);
        var hashData = BuildRequestData(sorted);
        var computedHash = ComputeHmacSha512(settings.VnPay.HashSecret, hashData);

        var amount = 0L;
        long.TryParse(query["vnp_Amount"], out amount);

        var responseCode = query["vnp_ResponseCode"].ToString();
        var transactionStatus = query["vnp_TransactionStatus"].ToString();

        return new VnPayReturnResult
        {
            SignatureValid = string.Equals(providedHash, computedHash, StringComparison.OrdinalIgnoreCase),
            IsSuccess = string.Equals(responseCode, "00", StringComparison.OrdinalIgnoreCase)
                && string.Equals(transactionStatus, "00", StringComparison.OrdinalIgnoreCase),
            TransactionReference = query["vnp_TxnRef"].ToString(),
            TransactionNo = query["vnp_TransactionNo"].ToString(),
            ResponseCode = responseCode,
            TransactionStatus = transactionStatus,
            Amount = amount / 100,
            OrderInfo = query["vnp_OrderInfo"].ToString(),
            RawPaymentMethod = "VNPAY"
        };
    }

    private static string BuildRequestData(SortedDictionary<string, string> data)
    {
        return string.Join("&", data
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => $"{x.Key}={WebUtility.UrlEncode(x.Value)}"));
    }

    private static string NormalizeIpAddress(string? clientIp)
    {
        if (string.IsNullOrWhiteSpace(clientIp))
        {
            return "127.0.0.1";
        }

        if (clientIp == "::1")
        {
            return "127.0.0.1";
        }

        if (clientIp.Contains(','))
        {
            clientIp = clientIp.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
        }

        if (clientIp?.Contains(':') == true)
        {
            return "127.0.0.1";
        }

        return clientIp ?? "127.0.0.1";
    }

    private static string ComputeHmacSha512(string key, string inputData)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);

        using var hmac = new HMACSHA512(keyBytes);
        var hashBytes = hmac.ComputeHash(inputBytes);
        return Convert.ToHexString(hashBytes);
    }
}
