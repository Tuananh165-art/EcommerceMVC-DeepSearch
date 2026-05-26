using ECommerceMVC.Data;
using ECommerceMVC.ViewModels;

namespace ECommerceMVC.Services;

public class StockService : IStockService
{
    private readonly Hshop2023Context db;

    public StockService(Hshop2023Context db)
    {
        this.db = db;
    }

    public StockValidationResult ValidateCart(List<CartItem> cartItems)
    {
        foreach (var item in cartItems)
        {
            var product = db.HangHoas.FirstOrDefault(x => x.MaHh == item.MaHh);
            if (product == null)
            {
                return StockValidationResult.Fail($"Sản phẩm {item.TenHH} không còn tồn tại.");
            }

            if (product.SoLuongTon <= 0)
            {
                return StockValidationResult.Fail($"Sản phẩm {product.TenHh} đã hết hàng.");
            }

            if (item.SoLuong > product.SoLuongTon)
            {
                return StockValidationResult.Fail($"Sản phẩm {product.TenHh} chỉ còn {product.SoLuongTon} sản phẩm.");
            }
        }

        return StockValidationResult.Ok();
    }

    public int ClampQuantityToStock(int productId, int requestedQuantity)
    {
        requestedQuantity = Math.Max(1, requestedQuantity);
        var product = db.HangHoas.FirstOrDefault(x => x.MaHh == productId);
        if (product == null || product.SoLuongTon <= 0)
        {
            return 0;
        }

        return Math.Clamp(requestedQuantity, 1, product.SoLuongTon);
    }

    public StockValidationResult DecrementStock(List<CartItem> cartItems)
    {
        foreach (var item in cartItems)
        {
            var product = db.HangHoas.FirstOrDefault(x => x.MaHh == item.MaHh);
            if (product == null)
            {
                return StockValidationResult.Fail($"Sản phẩm {item.TenHH} không còn tồn tại.");
            }

            if (item.SoLuong > product.SoLuongTon)
            {
                return StockValidationResult.Fail($"Sản phẩm {product.TenHh} chỉ còn {product.SoLuongTon} sản phẩm.");
            }

            product.SoLuongTon = Math.Max(0, product.SoLuongTon - item.SoLuong);
        }

        return StockValidationResult.Ok();
    }
}
