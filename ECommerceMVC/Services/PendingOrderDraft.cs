namespace ECommerceMVC.Services;

public class PendingOrderDraft
{
    public string CustomerId { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string DienThoai { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DiaChi { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
    public string CachThanhToan { get; set; } = "COD";
    public string CachVanChuyen { get; set; } = "Giao hàng tiêu chuẩn";
    public double PhiVanChuyen { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
}
