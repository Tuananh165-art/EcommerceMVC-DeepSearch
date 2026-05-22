namespace ECommerceMVC.Data;

public partial class Voucher
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public string DiscountType { get; set; } = "Fixed";
    public double DiscountValue { get; set; }
    public double MinSubtotal { get; set; }
    public double? MaxDiscount { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public bool IsActive { get; set; }
}
