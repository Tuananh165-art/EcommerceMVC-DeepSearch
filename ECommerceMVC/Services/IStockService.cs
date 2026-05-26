using ECommerceMVC.ViewModels;

namespace ECommerceMVC.Services;

public interface IStockService
{
    StockValidationResult ValidateCart(List<CartItem> cartItems);
    int ClampQuantityToStock(int productId, int requestedQuantity);
    StockValidationResult DecrementStock(List<CartItem> cartItems);
}

public sealed record StockValidationResult(bool Success, string Message)
{
    public static StockValidationResult Ok() => new(true, string.Empty);
    public static StockValidationResult Fail(string message) => new(false, message);
}
