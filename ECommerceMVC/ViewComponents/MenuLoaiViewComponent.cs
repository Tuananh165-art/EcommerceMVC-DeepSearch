using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceMVC.ViewComponents
{
	public class MenuLoaiViewComponent : ViewComponent
	{
		private readonly Hshop2023Context db;

		public MenuLoaiViewComponent(Hshop2023Context context) => db = context;

		public IViewComponentResult Invoke()
		{
			var data = db.Loais
				.AsEnumerable()
				.Select(lo =>
				{
					var meta = AdminMetadataHelper.ParseCategory(lo.MoTa);
					return new MenuLoaiVM
					{
						MaLoai = lo.MaLoai,
						TenLoai = lo.TenLoai,
						Hinh = lo.Hinh ?? string.Empty,
						SoLuong = lo.HangHoas.Count,
						SortOrder = meta.SortOrder,
						IsVisible = meta.IsVisible
					};
				})
				.Where(x => x.IsVisible)
				.OrderBy(x => x.SortOrder)
				.ThenBy(x => x.TenLoai)
				.ToList();

			return View(data);
		}
	}
}
