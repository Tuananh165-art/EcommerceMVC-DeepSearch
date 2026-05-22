using ECommerceMVC.Helpers;

namespace ECommerceMVC.ViewModels
{
	public class MenuLoaiVM
	{
		public int MaLoai { get; set; }
		public string TenLoai { get; set; } = string.Empty;
		public string Hinh { get; set; } = string.Empty;
		public string HinhUrl => MyUtil.GetLoaiImageUrl(Hinh, MaLoai);
		public int SoLuong { get; set; }
		public int SortOrder { get; set; } = 100;
		public bool IsVisible { get; set; } = true;
	}
}
