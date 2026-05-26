using ECommerceMVC.Data;
using ECommerceMVC.Models;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ECommerceMVC.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly Hshop2023Context _db;

		public HomeController(ILogger<HomeController> logger, Hshop2023Context db)
		{
			_logger = logger;
			_db = db;
		}

		public IActionResult Index()
		{
			var ratingQuery = _db.ProductReviews
				.GroupBy(x => x.MaHh)
				.Select(g => new
				{
					MaHh = g.Key,
					SoDanhGia = g.Count(),
					DiemDanhGia = (int)Math.Round(g.Average(x => x.SoSao), MidpointRounding.AwayFromZero),
					DiemDanhGiaTrungBinh = g.Average(x => (double)x.SoSao)
				});

				var products = _db.HangHoas
				.Include(x => x.MaLoaiNavigation)
				.GroupJoin(
					ratingQuery,
					p => p.MaHh,
					r => r.MaHh,
					(p, rating) => new { p, rating = rating.FirstOrDefault() })
				.OrderByDescending(x => x.p.MaHh)
				.Take(6)
				.Select(x => new HangHoaVM
				{
					MaHh = x.p.MaHh,
					TenHH = x.p.TenHh,
					DonGia = x.p.DonGia ?? 0,
					Hinh = x.p.Hinh ?? string.Empty,
					MoTaNgan = x.p.MoTaDonVi ?? string.Empty,
					TenLoai = x.p.MaLoaiNavigation.TenLoai,
					SoLuongTon = x.p.SoLuongTon,
					SoDanhGia = x.rating != null ? x.rating.SoDanhGia : 0,
					DiemDanhGia = x.rating != null ? x.rating.DiemDanhGia : 0,
					DiemDanhGiaTrungBinh = x.rating != null ? x.rating.DiemDanhGiaTrungBinh : 0
				})
				.ToList();

			return View(products);
		}

        [Route("/404")]
        public IActionResult PageNotFound()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
