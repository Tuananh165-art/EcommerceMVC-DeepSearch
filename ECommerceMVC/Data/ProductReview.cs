using System;

namespace ECommerceMVC.Data;

public partial class ProductReview
{
    public int Id { get; set; }

    public int MaHh { get; set; }

    public string MaKh { get; set; } = null!;

    public int SoSao { get; set; }

    public string NoiDung { get; set; } = null!;

    public DateTime NgayTao { get; set; }
}