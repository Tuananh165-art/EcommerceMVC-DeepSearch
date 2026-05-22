using System;

namespace ECommerceMVC.Data;

public class GioHangItem
{
    public int Id { get; set; }
    public string MaKh { get; set; } = string.Empty;
    public int MaHh { get; set; }
    public int SoLuong { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual KhachHang? MaKhNavigation { get; set; }
    public virtual HangHoa? MaHhNavigation { get; set; }
}
