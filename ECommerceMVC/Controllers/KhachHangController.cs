using AutoMapper;
using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceMVC.Controllers
{
	public class KhachHangController : Controller
	{
		private readonly Hshop2023Context db;
		private readonly IMapper _mapper;

		public KhachHangController(Hshop2023Context context, IMapper mapper)
		{
			db = context;
			_mapper = mapper;
		}

		[HttpGet]
		public IActionResult DangKy()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult DangKy(RegisterVM model, IFormFile? Hinh)
		{
			model.MaKh = (model.MaKh ?? string.Empty).Trim();
			model.HoTen = (model.HoTen ?? string.Empty).Trim();
			model.Email = (model.Email ?? string.Empty).Trim();
			model.DiaChi = (model.DiaChi ?? string.Empty).Trim();
			model.DienThoai = (model.DienThoai ?? string.Empty).Trim();

			if (db.KhachHangs.Any(x => x.MaKh == model.MaKh))
			{
				ModelState.AddModelError(nameof(model.MaKh), "Tên đăng nhập đã tồn tại.");
			}

			if (db.KhachHangs.Any(x => x.Email == model.Email))
			{
				ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng.");
			}

			if (ModelState.IsValid)
			{
				try
				{
					var khachHang = _mapper.Map<KhachHang>(model);
					khachHang.NgaySinh = model.NgaySinh ?? DateTime.Today;
					khachHang.RandomKey = MyUtil.GenerateRamdomKey();
					khachHang.MatKhau = model.MatKhau.ToMd5Hash(khachHang.RandomKey);
					khachHang.HieuLuc = true;//sẽ xử lý khi dùng Mail để active
					khachHang.VaiTro = 0;

					if (Hinh != null)
					{
						khachHang.Hinh = MyUtil.UploadHinh(Hinh, "KhachHang");
					}

					db.Add(khachHang);
					db.SaveChanges();

					HttpContext.Session.Set(MySetting.CUSTOMER_KEY, khachHang.MaKh);
					TempData["SuccessMessage"] = "Đăng ký thành công.";
					return RedirectToAction("Index", "HangHoa");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, $"Không thể đăng ký tài khoản. Vui lòng thử lại. ({ex.Message})");
				}
			}
			return View(model);
		}

		[HttpGet]
		public IActionResult DangNhap(string? returnUrl = null)
		{
			if (!string.IsNullOrWhiteSpace(HttpContext.Session.Get<string>(MySetting.CUSTOMER_KEY)))
			{
				return RedirectToAction("Index", "HangHoa");
			}

			return View(new DangNhapVM { ReturnUrl = returnUrl });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult DangNhap(DangNhapVM model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var khachHang = db.KhachHangs.SingleOrDefault(x => x.MaKh == model.MaKh);
			if (khachHang == null || !khachHang.HieuLuc)
			{
				ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
				return View(model);
			}

			var hash = model.MatKhau.ToMd5Hash(khachHang.RandomKey);
			if (!string.Equals(hash, khachHang.MatKhau, StringComparison.OrdinalIgnoreCase))
			{
				ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
				return View(model);
			}

			HttpContext.Session.Set(MySetting.CUSTOMER_KEY, khachHang.MaKh);
			TempData["SuccessMessage"] = "Đăng nhập thành công.";

			if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
			{
				return Redirect(model.ReturnUrl);
			}

			return RedirectToAction("Index", "HangHoa");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult DangXuat()
		{
			HttpContext.Session.Remove(MySetting.CUSTOMER_KEY);
			TempData["SuccessMessage"] = "Đã đăng xuất.";
			return RedirectToAction("Index", "HangHoa");
		}
	}
}
