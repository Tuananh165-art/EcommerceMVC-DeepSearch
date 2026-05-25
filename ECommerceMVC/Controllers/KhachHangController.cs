using AutoMapper;
using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.Services;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECommerceMVC.Controllers
{
	public class KhachHangController : Controller
	{
		private readonly Hshop2023Context db;
		private readonly IMapper _mapper;
		private readonly IEmailService emailService;
		private readonly IPasswordService passwordService;
		private readonly IPasswordResetService passwordResetService;
		private readonly AdminSecuritySettings adminSecuritySettings;
		private readonly ILogger<KhachHangController> logger;

		public KhachHangController(
			Hshop2023Context context,
			IMapper mapper,
			IEmailService emailService,
			IPasswordService passwordService,
			IPasswordResetService passwordResetService,
			IOptions<AdminSecuritySettings> adminSecurityOptions,
			ILogger<KhachHangController> logger)
		{
			db = context;
			_mapper = mapper;
			this.emailService = emailService;
			this.passwordService = passwordService;
			this.passwordResetService = passwordResetService;
			adminSecuritySettings = adminSecurityOptions.Value;
			this.logger = logger;
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
			model.AdminSecretCode = (model.AdminSecretCode ?? string.Empty).Trim();

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
					passwordService.SetPassword(khachHang, model.MatKhau);
					khachHang.HieuLuc = true;
					khachHang.VaiTro = IsAdminRegistration(model.AdminSecretCode) ? MySetting.ADMIN_ROLE : 0;
					khachHang.RandomKey = MyUtil.GenerateRamdomKey();
					khachHang.Hinh = "default-avatar.svg";

					if (Hinh != null)
					{
						khachHang.Hinh = MyUtil.UploadHinh(Hinh, "KhachHang");
						if (string.IsNullOrWhiteSpace(khachHang.Hinh))
						{
							khachHang.Hinh = "default-avatar.svg";
						}
					}

					db.Add(khachHang);
					try
					{
						db.SaveChanges();
					}
					catch (DbUpdateException ex) when (IsMatKhauTruncatedError(ex))
					{
						logger.LogWarning(ex, "DB schema cũ (MatKhau ngắn) cho MaKh={MaKh}. Fallback sang legacy password hash.", model.MaKh);
						passwordService.SetLegacyPassword(khachHang, model.MatKhau);
						db.SaveChanges();
					}

					HttpContext.Session.Set(MySetting.CUSTOMER_KEY, khachHang.MaKh);

					var registerSubject = "[DEEPSEARCH] Đăng ký tài khoản thành công";
					var registerBody = $"<p>Xin chào {khachHang.HoTen},</p><p>Bạn đã đăng ký tài khoản thành công tại DEEPSEARCH.</p><p>Tên đăng nhập: <strong>{khachHang.MaKh}</strong></p><p>Vai trò tài khoản: <strong>{(khachHang.VaiTro == MySetting.ADMIN_ROLE ? "Admin" : "Khách hàng")}</strong></p>";
					if (emailService.TrySend(khachHang.Email, registerSubject, registerBody, out var registerEmailError))
					{
						TempData["SuccessMessage"] = "Đăng ký thành công. Email xác nhận đã được gửi.";
					}
					else
					{
						TempData["SuccessMessage"] = "Đăng ký thành công.";
						TempData["ErrorMessage"] = $"Đăng ký thành công nhưng chưa gửi được email xác nhận ({registerEmailError}).";
					}

					return RedirectToAction("Index", "HangHoa");
				}
				catch (Exception ex)
				{
					var innerMessage = ex.InnerException?.Message;
					logger.LogError(ex, "Đăng ký thất bại cho MaKh={MaKh}, Email={Email}. Inner: {InnerMessage}", model.MaKh, model.Email, innerMessage);
					var safeMessage = string.IsNullOrWhiteSpace(innerMessage) ? ex.Message : innerMessage;
					ModelState.AddModelError(string.Empty, $"Không thể đăng ký tài khoản. Vui lòng thử lại. ({safeMessage})");
				}
			}
			return View(model);
		}

		[HttpGet]
		public IActionResult DangNhap(string? returnUrl = null)
		{
			var customerId = HttpContext.Session.Get<string>(MySetting.CUSTOMER_KEY);
			if (!string.IsNullOrWhiteSpace(customerId))
			{
				var currentUser = db.KhachHangs.AsNoTracking().SingleOrDefault(x => x.MaKh == customerId);
				if (currentUser?.VaiTro == MySetting.ADMIN_ROLE)
				{
					return RedirectToAction("Index", "Admin");
				}

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

			if (!passwordService.VerifyPassword(khachHang, model.MatKhau, out var needsUpgrade))
			{
				ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
				return View(model);
			}

			if (needsUpgrade)
			{
				try
				{
					passwordService.SetPassword(khachHang, model.MatKhau);
					db.SaveChanges();
				}
				catch
				{
				}
			}

			HttpContext.Session.Set(MySetting.CUSTOMER_KEY, khachHang.MaKh);
			MergeSessionCartToPersistentCart(khachHang.MaKh);

			var loginSubject = "[DEEPSEARCH] Đăng nhập tài khoản";
			var loginBody = $"<p>Xin chào {khachHang.HoTen},</p><p>Tài khoản của bạn vừa đăng nhập thành công vào DEEPSEARCH.</p><p>Thời gian: <strong>{DateTime.Now:dd/MM/yyyy HH:mm:ss}</strong></p><p>Tài khoản: <strong>{khachHang.MaKh}</strong></p>";
			if (emailService.TrySend(khachHang.Email, loginSubject, loginBody, out var loginEmailError))
			{
				TempData["SuccessMessage"] = "Đăng nhập thành công. Email thông báo đã được gửi.";
			}
			else
			{
				TempData["SuccessMessage"] = "Đăng nhập thành công.";
				TempData["ErrorMessage"] = $"Đăng nhập thành công nhưng chưa gửi được email thông báo ({loginEmailError}).";
			}

			if (khachHang.VaiTro == MySetting.ADMIN_ROLE)
			{
				return RedirectToAction("Index", "Admin");
			}

			if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
			{
				return Redirect(model.ReturnUrl);
			}

			return RedirectToAction("Index", "HangHoa");
		}

		[HttpGet]
		public IActionResult ForgotPassword()
		{
			return View(new ForgotPasswordVM());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ForgotPassword(ForgotPasswordVM model)
		{
			model.EmailOrUsername = (model.EmailOrUsername ?? string.Empty).Trim();
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var lookup = model.EmailOrUsername.ToLowerInvariant();
			var khachHang = db.KhachHangs.FirstOrDefault(x => x.MaKh.ToLower() == lookup || x.Email.ToLower() == lookup);
			if (khachHang != null && khachHang.HieuLuc)
			{
				var otp = passwordResetService.CreateOtpForCustomer(khachHang);
				var subject = "[DEEPSEARCH] Mã OTP đặt lại mật khẩu";
				var body = $"<p>Xin chào {khachHang.HoTen},</p><p>Mã OTP đặt lại mật khẩu của bạn là: <strong style='font-size:20px'>{otp}</strong></p><p>Mã có hiệu lực trong 10 phút. Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>";
				if (!emailService.TrySend(khachHang.Email, subject, body, out var emailError))
				{
					TempData["ErrorMessage"] = $"Không gửi được email OTP ({emailError}). Vui lòng kiểm tra cấu hình SMTP.";
					return RedirectToAction(nameof(ForgotPassword));
				}

				TempData["SuccessMessage"] = "Mã OTP đặt lại mật khẩu đã được gửi tới email của bạn.";
				return RedirectToAction(nameof(ResetPasswordWithOtp), new { maKh = khachHang.MaKh });
			}

			TempData["SuccessMessage"] = "Nếu tài khoản tồn tại, mã OTP đặt lại mật khẩu đã được gửi tới email đã đăng ký.";
			return RedirectToAction(nameof(DangNhap));
		}

		[HttpGet]
		public IActionResult ResetPasswordWithOtp(string maKh)
		{
			if (string.IsNullOrWhiteSpace(maKh))
			{
				return RedirectToAction(nameof(ForgotPassword));
			}

			return View(new VerifyOtpResetPasswordVM { MaKh = maKh.Trim() });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ResetPasswordWithOtp(VerifyOtpResetPasswordVM model)
		{
			model.MaKh = (model.MaKh ?? string.Empty).Trim();
			model.Otp = (model.Otp ?? string.Empty).Trim();
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var khachHang = db.KhachHangs.SingleOrDefault(x => x.MaKh == model.MaKh && x.HieuLuc);
			if (khachHang == null)
			{
				ModelState.AddModelError(string.Empty, "Không tìm thấy tài khoản hợp lệ.");
				return View(model);
			}

			var result = passwordResetService.ValidateOtp(model.MaKh, model.Otp);
			if (!result.Success)
			{
				ModelState.AddModelError(nameof(model.Otp), result.ErrorMessage ?? "Mã OTP không hợp lệ.");
				return View(model);
			}

			passwordService.SetPassword(khachHang, model.NewPassword);
			try
			{
				db.SaveChanges();
			}
			catch (DbUpdateException ex) when (IsMatKhauTruncatedError(ex))
			{
				logger.LogWarning(ex, "DB schema cũ (MatKhau ngắn) khi reset password cho MaKh={MaKh}. Fallback legacy hash.", model.MaKh);
				passwordService.SetLegacyPassword(khachHang, model.NewPassword);
				db.SaveChanges();
			}
			HttpContext.Session.Remove(MySetting.CART_KEY);
			HttpContext.Session.Remove(MySetting.CUSTOMER_KEY);
			TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập bằng mật khẩu mới.";
			return RedirectToAction(nameof(DangNhap));
		}

		[HttpGet]
		public IActionResult EditProfile()
		{
			var customerId = HttpContext.Session.Get<string>(MySetting.CUSTOMER_KEY);
			if (string.IsNullOrWhiteSpace(customerId))
			{
				return RedirectToAction("DangNhap", "KhachHang", new { returnUrl = Url.Action(nameof(EditProfile), "KhachHang") });
			}

			var khachHang = db.KhachHangs.SingleOrDefault(x => x.MaKh == customerId);
			if (khachHang == null)
			{
				return RedirectToAction("DangNhap", "KhachHang");
			}

			var model = new EditProfileVM
			{
				MaKh = khachHang.MaKh,
				HoTen = khachHang.HoTen,
				GioiTinh = khachHang.GioiTinh,
				NgaySinh = khachHang.NgaySinh,
				DiaChi = khachHang.DiaChi ?? string.Empty,
				DienThoai = khachHang.DienThoai ?? string.Empty,
				Email = khachHang.Email,
				Hinh = khachHang.Hinh
			};

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult EditProfile(EditProfileVM model)
		{
			var customerId = HttpContext.Session.Get<string>(MySetting.CUSTOMER_KEY);
			if (string.IsNullOrWhiteSpace(customerId))
			{
				return RedirectToAction("DangNhap", "KhachHang", new { returnUrl = Url.Action(nameof(EditProfile), "KhachHang") });
			}

			var khachHang = db.KhachHangs.SingleOrDefault(x => x.MaKh == customerId);
			if (khachHang == null)
			{
				return RedirectToAction("DangNhap", "KhachHang");
			}

			if (db.KhachHangs.Any(x => x.MaKh != customerId && x.Email == model.Email))
			{
				ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng.");
			}

			if (!ModelState.IsValid)
			{
				model.MaKh = customerId;
				model.Hinh = khachHang.Hinh;
				return View(model);
			}

			khachHang.HoTen = (model.HoTen ?? string.Empty).Trim();
			khachHang.Email = (model.Email ?? string.Empty).Trim();
			khachHang.DiaChi = (model.DiaChi ?? string.Empty).Trim();
			khachHang.DienThoai = (model.DienThoai ?? string.Empty).Trim();
			khachHang.GioiTinh = model.GioiTinh;
			khachHang.NgaySinh = model.NgaySinh ?? khachHang.NgaySinh;

			db.SaveChanges();
			TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công.";
			return RedirectToAction(nameof(EditProfile));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult UploadAvatar(IFormFile avatarFile)
		{
			var customerId = HttpContext.Session.Get<string>(MySetting.CUSTOMER_KEY);
			if (string.IsNullOrWhiteSpace(customerId))
				return Json(new { success = false, message = "Chưa đăng nhập." });

			if (avatarFile == null || avatarFile.Length == 0)
				return Json(new { success = false, message = "File không hợp lệ." });

			var allowed = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif" };
			if (!allowed.Contains(avatarFile.ContentType.ToLowerInvariant()))
				return Json(new { success = false, message = "Chỉ chấp nhận ảnh JPG, PNG, WEBP, GIF." });

			if (avatarFile.Length > 5 * 1024 * 1024)
				return Json(new { success = false, message = "Ảnh không được lớn hơn 5MB." });

			var khachHang = db.KhachHangs.SingleOrDefault(x => x.MaKh == customerId);
			if (khachHang == null)
				return Json(new { success = false, message = "Không tìm thấy tài khoản." });

			try
			{
				var fileName = MyUtil.UploadHinh(avatarFile, "KhachHang");
				if (string.IsNullOrWhiteSpace(fileName))
					return Json(new { success = false, message = "Lưu ảnh thất bại. Vui lòng thử lại." });

				// Delete old avatar file
				if (!string.IsNullOrWhiteSpace(khachHang.Hinh))
				{
					var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", "KhachHang", Path.GetFileName(khachHang.Hinh));
					if (System.IO.File.Exists(oldPath))
					{
						try { System.IO.File.Delete(oldPath); } catch { }
					}
				}

				khachHang.Hinh = fileName;
				db.SaveChanges();

				return Json(new { success = true, url = $"/Hinh/KhachHang/{fileName}", message = "Cập nhật ảnh đại diện thành công!" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult DangXuat()
		{
			HttpContext.Session.Remove(MySetting.CART_KEY);
			HttpContext.Session.Remove(MySetting.CUSTOMER_KEY);
			TempData["SuccessMessage"] = "Đã đăng xuất.";
			return RedirectToAction("Index", "HangHoa");
		}

		private bool IsAdminRegistration(string? submittedSecret)
		{
			return !string.IsNullOrWhiteSpace(adminSecuritySettings.SecretCode)
				&& string.Equals(adminSecuritySettings.SecretCode.Trim(), submittedSecret?.Trim(), StringComparison.Ordinal);
		}

		private static bool IsMatKhauTruncatedError(DbUpdateException ex)
		{
			var message = ex.InnerException?.Message ?? ex.Message;
			return message.Contains("String or binary data would be truncated", StringComparison.OrdinalIgnoreCase)
				&& message.Contains("KhachHang", StringComparison.OrdinalIgnoreCase)
				&& message.Contains("MatKhau", StringComparison.OrdinalIgnoreCase);
		}

		private void MergeSessionCartToPersistentCart(string customerId)
		{
			var sessionCart = HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();
			var now = DateTime.Now;
			var existed = db.GioHangItems.Where(x => x.MaKh == customerId).ToList();
			var existedMap = existed.ToDictionary(x => x.MaHh, x => x);

			foreach (var item in sessionCart.Where(x => x.SoLuong > 0))
			{
				if (existedMap.TryGetValue(item.MaHh, out var dbItem))
				{
					dbItem.SoLuong += item.SoLuong;
					dbItem.UpdatedAt = now;
				}
				else
				{
					db.GioHangItems.Add(new GioHangItem
					{
						MaKh = customerId,
						MaHh = item.MaHh,
						SoLuong = item.SoLuong,
						CreatedAt = now,
						UpdatedAt = now
					});
				}
			}

			db.SaveChanges();

			var merged = db.GioHangItems
				.AsNoTracking()
				.Where(x => x.MaKh == customerId)
				.Join(
					db.HangHoas.AsNoTracking(),
					c => c.MaHh,
					h => h.MaHh,
					(c, h) => new CartItem
					{
						MaHh = h.MaHh,
						TenHH = h.TenHh,
						DonGia = h.DonGia ?? 0,
						Hinh = h.Hinh ?? string.Empty,
						SoLuong = c.SoLuong
					})
				.ToList();

			HttpContext.Session.Set(MySetting.CART_KEY, merged);
		}
	}
}
