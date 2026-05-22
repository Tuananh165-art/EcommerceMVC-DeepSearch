namespace ECommerceMVC.Services;

public interface IShippingFeeService
{
    double Calculate(string? address, string? shippingMethod, double subtotal);
}
