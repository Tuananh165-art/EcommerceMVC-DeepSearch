using ECommerceMVC.Data;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ECommerceMVC.Helpers;

namespace ECommerceMVC.Controllers
{
	public class CartController : Controller
	{
		private readonly Hshop2023Context db;

		public CartController(Hshop2023Context context)
		{
			db = context;
		}

		public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();

		public IActionResult Index()
		{
			return View(Cart);
		}

		public IActionResult AddToCart(int id, int quantity = 1)
		{
			var gioHang = Cart;
			var item = gioHang.SingleOrDefault(p => p.MaHh == id);
			if (item == null)
			{
				var hangHoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
				if (hangHoa == null)
				{
					TempData["Message"] = $"Không tìm thấy hàng hóa có mã {id}";
					return Redirect("/404");
				}
				item = new CartItem
				{
					MaHh = hangHoa.MaHh,
					TenHH = hangHoa.TenHh,
					DonGia = hangHoa.DonGia ?? 0,
					Hinh = hangHoa.Hinh ?? string.Empty,
					SoLuong = quantity
				};
				gioHang.Add(item);
			}
			else
			{
				item.SoLuong += quantity;
			}

			HttpContext.Session.Set(MySetting.CART_KEY, gioHang);

			return RedirectToAction("Index");
		}

		public IActionResult RemoveCart(int id)
		{
			var gioHang = Cart;
			var item = gioHang.SingleOrDefault(p => p.MaHh == id);
			if (item != null)
			{
				gioHang.Remove(item);
				HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
			}
			return RedirectToAction("Index");
		}

		[HttpGet]
		public IActionResult Checkout()
		{
			var gioHang = Cart;
			if (!gioHang.Any())
			{
				TempData["ErrorMessage"] = "Giỏ hàng đang trống, không thể thanh toán.";
				return RedirectToAction(nameof(Index));
			}

			ViewBag.Cart = gioHang;
			var model = new CheckoutVM();
			var maKh = HttpContext.Session.Get<string>(MySetting.CUSTOMER_KEY);
			if (!string.IsNullOrWhiteSpace(maKh))
			{
				var kh = db.KhachHangs.SingleOrDefault(x => x.MaKh == maKh);
				if (kh != null)
				{
					model.MaKh = kh.MaKh;
					model.HoTen = kh.HoTen;
					model.DiaChi = kh.DiaChi ?? string.Empty;
					model.DienThoai = kh.DienThoai;
					model.Email = kh.Email;
				}
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Checkout(CheckoutVM model)
		{
			var gioHang = Cart;
			if (!gioHang.Any())
			{
				TempData["ErrorMessage"] = "Giỏ hàng đang trống, không thể thanh toán.";
				return RedirectToAction(nameof(Index));
			}

			var kh = db.KhachHangs.SingleOrDefault(x => x.MaKh == model.MaKh);
			if (kh == null)
			{
				ModelState.AddModelError(nameof(model.MaKh), "Mã khách hàng không tồn tại.");
			}

			if (!ModelState.IsValid)
			{
				ViewBag.Cart = gioHang;
				return View(model);
			}

			var trangThaiMoi = db.TrangThais
				.OrderBy(x => x.MaTrangThai)
				.FirstOrDefault();

			if (trangThaiMoi == null)
			{
				ModelState.AddModelError(string.Empty, "Chưa cấu hình trạng thái đơn hàng trong hệ thống.");
				ViewBag.Cart = gioHang;
				return View(model);
			}

			using var transaction = db.Database.BeginTransaction();
			try
			{
				var hoaDon = new HoaDon
				{
					MaKh = model.MaKh,
					NgayDat = DateTime.Now,
					NgayCan = DateTime.Now.AddDays(3),
					HoTen = model.HoTen,
					DiaChi = model.DiaChi,
					CachThanhToan = "COD",
					CachVanChuyen = model.CachVanChuyen,
					PhiVanChuyen = model.PhiVanChuyen,
					MaTrangThai = trangThaiMoi.MaTrangThai,
					GhiChu = model.GhiChu
				};

				db.HoaDons.Add(hoaDon);
				db.SaveChanges();

				var chiTietItems = gioHang.Select(item => new ChiTietHd
				{
					MaHd = hoaDon.MaHd,
					MaHh = item.MaHh,
					DonGia = item.DonGia,
					SoLuong = item.SoLuong,
					GiamGia = 0
				});

				db.ChiTietHds.AddRange(chiTietItems);
				db.SaveChanges();

				transaction.Commit();
				HttpContext.Session.Remove(MySetting.CART_KEY);

				return RedirectToAction(nameof(CheckoutSuccess), new { id = hoaDon.MaHd });
			}
			catch
			{
				transaction.Rollback();
				ModelState.AddModelError(string.Empty, "Không thể tạo đơn hàng. Vui lòng thử lại.");
				ViewBag.Cart = gioHang;
				return View(model);
			}
		}

		[HttpGet]
		public IActionResult CheckoutSuccess(int id)
		{
			ViewBag.OrderId = id;
			return View();
		}
	}
}
