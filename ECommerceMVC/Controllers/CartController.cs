using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.Services;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMVC.Controllers
{
	public class CartController : Controller
	{
		private const string PendingVnPayOrderSessionKey = "PENDING_VNPAY_ORDER";
		private const string AppliedVoucherSessionKey = "APPLIED_VOUCHER_CODE";
		private readonly Hshop2023Context db;
		private readonly IEmailService emailService;
		private readonly IPaymentSandboxService paymentSandboxService;
		private readonly IVnPayService vnPayService;
		private readonly IVoucherService voucherService;
		private readonly IShippingFeeService shippingFeeService;
		private readonly IStockService stockService;
		private readonly ILogger<CartController> logger;

		public CartController(
			Hshop2023Context context,
			IEmailService emailService,
			IPaymentSandboxService paymentSandboxService,
			IVnPayService vnPayService,
			IVoucherService voucherService,
			IShippingFeeService shippingFeeService,
			IStockService stockService,
			ILogger<CartController> logger)
		{
			db = context;
			this.emailService = emailService;
			this.paymentSandboxService = paymentSandboxService;
			this.vnPayService = vnPayService;
			this.voucherService = voucherService;
			this.shippingFeeService = shippingFeeService;
			this.stockService = stockService;
			this.logger = logger;
		}

		public List<CartItem> Cart => GetCart();

		private string? CurrentCustomerId => HttpContext.Session.Get<string>(MySetting.CUSTOMER_KEY);
		private string? AppliedVoucherCode => HttpContext.Session.Get<string>(AppliedVoucherSessionKey);

		public IActionResult Index()
		{
			PopulateCartPricingViewBag(Cart);
			return View(Cart);
		}

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult AddToCart(int id, int quantity = 1, string? returnUrl = null)
        {
            try
            {
                quantity = Math.Max(1, quantity);
                var gioHang = Cart;
                var item = gioHang.SingleOrDefault(p => p.MaHh == id);
                if (item == null)
                {
                    var hangHoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
                    if (hangHoa == null)
                    {
                        TempData["Message"] = $"Khong tim thay hang hoa co ma {id}";
                        return Redirect("/404");
                    }
                    if (hangHoa.SoLuongTon <= 0)
                    {
                        TempData["ErrorMessage"] = "San pham da het hang.";
                        return RedirectToSafeReturn(returnUrl);
                    }
                    quantity = Math.Min(quantity, hangHoa.SoLuongTon);
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
                    item.SoLuong = stockService.ClampQuantityToStock(id, item.SoLuong + quantity);
                }

                if (TrySaveCart(gioHang, out var saveError))
                {
                    TempData["SuccessMessage"] = "Da them san pham vao gio hang.";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Da them vao gio tam thoi nhung chua dong bo duoc du lieu ({saveError}).";
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AddToCart loi cho MaHh={MaHh}, MaKh={MaKh}, Qty={Qty}", id, CurrentCustomerId, quantity);
                TempData["ErrorMessage"] = "Khong the them san pham vao gio hang luc nay. Vui long thu lai.";
            }

            return RedirectToSafeReturn(returnUrl);
        }

		public IActionResult RemoveCart(int id)
		{
			var gioHang = Cart;
			var item = gioHang.SingleOrDefault(p => p.MaHh == id);
			if (item != null)
			{
				gioHang.Remove(item);
				SaveCart(gioHang);
			}
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult UpdateQuantity(int id, int quantity)
		{
			var gioHang = Cart;
			var item = gioHang.SingleOrDefault(p => p.MaHh == id);
			if (item == null)
			{
				return RedirectToAction(nameof(Index));
			}

			var clampedQuantity = stockService.ClampQuantityToStock(id, quantity);
			if (clampedQuantity <= 0)
			{
				gioHang.Remove(item);
				TempData["ErrorMessage"] = "Sáº£n pháº©m Ä‘Ã£ háº¿t hÃ ng vÃ  Ä‘Æ°á»£c xÃ³a khá»i giá».";
			}
			else
			{
				item.SoLuong = clampedQuantity;
			}
			SaveCart(gioHang);

			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ApplyVoucher(string code, string? returnUrl = null)
		{
			var cart = Cart;
			var subtotal = cart.Sum(x => x.ThanhTien);
			var result = voucherService.ValidateAndCalculateDiscount(code, subtotal);
			if (result.Success)
			{
				HttpContext.Session.Set(AppliedVoucherSessionKey, result.Code);
				TempData["SuccessMessage"] = result.Message;
			}
			else
			{
				HttpContext.Session.Remove(AppliedVoucherSessionKey);
				TempData["ErrorMessage"] = result.Message;
			}

			return RedirectToSafeReturn(returnUrl);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult RemoveVoucher(string? returnUrl = null)
		{
			HttpContext.Session.Remove(AppliedVoucherSessionKey);
			TempData["SuccessMessage"] = "ÄÃ£ xÃ³a voucher khá»i Ä‘Æ¡n hÃ ng.";
			return RedirectToSafeReturn(returnUrl);
		}

		[HttpGet]
		public IActionResult Checkout()
		{
			if (string.IsNullOrWhiteSpace(CurrentCustomerId))
			{
				return RedirectToAction("DangNhap", "KhachHang", new { returnUrl = Url.Action(nameof(Checkout), "Cart") });
			}

			var gioHang = Cart;
			if (!gioHang.Any())
			{
				TempData["ErrorMessage"] = "Giá» hÃ ng Ä‘ang trá»‘ng, khÃ´ng thá»ƒ thanh toÃ¡n.";
				return RedirectToAction(nameof(Index));
			}

			ViewBag.Cart = gioHang;
			var stockResult = stockService.ValidateCart(gioHang);
			if (!stockResult.Success)
			{
				TempData["ErrorMessage"] = stockResult.Message;
				return RedirectToAction(nameof(Index));
			}

			var model = new CheckoutVM();
			var kh = db.KhachHangs.SingleOrDefault(x => x.MaKh == CurrentCustomerId);
			if (kh != null)
			{
				model.MaKh = kh.MaKh;
				model.HoTen = kh.HoTen;
				model.DiaChi = kh.DiaChi ?? string.Empty;
				model.DienThoai = kh.DienThoai ?? string.Empty;
				model.Email = kh.Email;
			}

			ApplyCheckoutPricing(model, gioHang);
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Checkout(CheckoutVM model)
		{
			if (string.IsNullOrWhiteSpace(CurrentCustomerId))
			{
				return RedirectToAction("DangNhap", "KhachHang", new { returnUrl = Url.Action(nameof(Checkout), "Cart") });
			}

			var gioHang = Cart;
			if (!gioHang.Any())
			{
				TempData["ErrorMessage"] = "Giá» hÃ ng Ä‘ang trá»‘ng, khÃ´ng thá»ƒ thanh toÃ¡n.";
				return RedirectToAction(nameof(Index));
			}

			model.MaKh = CurrentCustomerId!;
			model.CachThanhToan = string.IsNullOrWhiteSpace(model.CachThanhToan) ? "COD" : model.CachThanhToan.Trim();
			ApplyCheckoutPricing(model, gioHang);
			var stockResult = stockService.ValidateCart(gioHang);
			if (!stockResult.Success)
			{
				ModelState.AddModelError(string.Empty, stockResult.Message);
			}

			var kh = db.KhachHangs.SingleOrDefault(x => x.MaKh == CurrentCustomerId);
			if (kh == null)
			{
				ModelState.AddModelError(nameof(model.MaKh), "KhÃ¡ch hÃ ng khÃ´ng tá»“n táº¡i.");
			}

			if (!ModelState.IsValid)
			{
				ViewBag.Cart = gioHang;
				return View(model);
			}

			if (string.Equals(model.CachThanhToan, "VNPAY", StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					var paymentUrl = vnPayService.CreatePaymentUrl(
						model,
						gioHang,
						CurrentCustomerId!,
						HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
						Url.Action(nameof(VnPayReturn), "Cart", null, Request.Scheme) ?? string.Empty);

					var transactionReference = GetQueryParameter(paymentUrl, "vnp_TxnRef");
					HttpContext.Session.Set(PendingVnPayOrderSessionKey, new PendingOrderDraft
					{
						CustomerId = CurrentCustomerId!,
						HoTen = model.HoTen,
						DienThoai = model.DienThoai,
						Email = model.Email,
						DiaChi = model.DiaChi,
						GhiChu = model.GhiChu,
						CachThanhToan = "VNPAY",
						CachVanChuyen = string.IsNullOrWhiteSpace(model.CachVanChuyen) ? "Giao hÃ ng tiÃªu chuáº©n" : model.CachVanChuyen,
						PhiVanChuyen = model.PhiVanChuyen,
						TransactionReference = transactionReference
					});

					return Redirect(paymentUrl);
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(nameof(model.CachThanhToan), $"KhÃ´ng thá»ƒ khá»Ÿi táº¡o thanh toÃ¡n VNPay ({ex.Message}).");
					ViewBag.Cart = gioHang;
					return View(model);
				}
			}

			if (kh == null)
			{
				ModelState.AddModelError(nameof(model.MaKh), "KhÃ´ng tÃ¬m tháº¥y thÃ´ng tin khÃ¡ch hÃ ng Ä‘á»ƒ thanh toÃ¡n.");
				ViewBag.Cart = gioHang;
				return View(model);
			}

			return CompleteNonVnPayCheckout(model, gioHang, kh);
		}

		[HttpGet]
		public IActionResult VnPayReturn()
		{
			if (string.IsNullOrWhiteSpace(CurrentCustomerId))
			{
				return RedirectToAction("DangNhap", "KhachHang", new { returnUrl = Url.Action(nameof(Checkout), "Cart") });
			}

			var pendingOrder = HttpContext.Session.Get<PendingOrderDraft>(PendingVnPayOrderSessionKey);
			var gioHang = Cart;
			if (pendingOrder == null || !gioHang.Any())
			{
				TempData["ErrorMessage"] = "KhÃ´ng tÃ¬m tháº¥y dá»¯ liá»‡u Ä‘Æ¡n hÃ ng chá» thanh toÃ¡n VNPay.";
				return RedirectToAction(nameof(Checkout));
			}

			var result = vnPayService.ValidateReturn(Request.Query);
			if (!result.SignatureValid)
			{
				TempData["ErrorMessage"] = "Chá»¯ kÃ½ VNPay khÃ´ng há»£p lá»‡.";
				return RedirectToAction(nameof(Checkout));
			}

			if (!result.IsSuccess)
			{
				TempData["ErrorMessage"] = $"Thanh toÃ¡n VNPay chÆ°a thÃ nh cÃ´ng (mÃ£ pháº£n há»“i: {result.ResponseCode}).";
				return RedirectToAction(nameof(Checkout));
			}

			if (!string.Equals(result.TransactionReference, pendingOrder.TransactionReference, StringComparison.Ordinal))
			{
				TempData["ErrorMessage"] = "MÃ£ giao dá»‹ch VNPay khÃ´ng khá»›p vá»›i Ä‘Æ¡n hÃ ng chá».";
				return RedirectToAction(nameof(Checkout));
			}

			var checkout = new CheckoutVM
			{
				MaKh = pendingOrder.CustomerId,
				HoTen = pendingOrder.HoTen,
				DienThoai = pendingOrder.DienThoai,
				Email = pendingOrder.Email,
				DiaChi = pendingOrder.DiaChi,
				GhiChu = pendingOrder.GhiChu,
				CachThanhToan = "VNPAY",
				CachVanChuyen = pendingOrder.CachVanChuyen,
				PhiVanChuyen = pendingOrder.PhiVanChuyen
			};

			var kh = db.KhachHangs.SingleOrDefault(x => x.MaKh == pendingOrder.CustomerId);
			if (kh == null)
			{
				TempData["ErrorMessage"] = "KhÃ´ng tÃ¬m tháº¥y khÃ¡ch hÃ ng Ä‘á»ƒ hoÃ n táº¥t Ä‘Æ¡n VNPay.";
				return RedirectToAction(nameof(Checkout));
			}

			HttpContext.Session.Remove(PendingVnPayOrderSessionKey);
			return CompleteSuccessfulCheckout(
				checkout,
				gioHang,
				kh,
				"VNPay",
				$"VNPay - MÃ£ GD: {result.TransactionNo}",
				$"Thanh toÃ¡n VNPay thÃ nh cÃ´ng - Ref {result.TransactionReference}");
		}

		[HttpGet]
		public IActionResult VnPayIpn()
		{
			var result = vnPayService.ValidateReturn(Request.Query);
			if (!result.SignatureValid)
			{
				return Json(new { RspCode = "97", Message = "Invalid checksum" });
			}

			if (!result.IsSuccess)
			{
				return Json(new { RspCode = "00", Message = "Confirm Failed Payment" });
			}

			return Json(new { RspCode = "00", Message = "Confirm Success" });
		}

		private IActionResult CompleteNonVnPayCheckout(CheckoutVM model, List<CartItem> gioHang, KhachHang kh)
		{
			var paymentResult = paymentSandboxService.ProcessSandboxPayment(model.CachThanhToan, model, gioHang.Sum(x => x.ThanhTien) + Math.Max(0, model.PhiVanChuyen));
			if (!paymentResult.IsSuccess)
			{
				ModelState.AddModelError(nameof(model.CachThanhToan), "KhÃ´ng thá»ƒ xá»­ lÃ½ thanh toÃ¡n. Vui lÃ²ng thá»­ láº¡i.");
				ViewBag.Cart = gioHang;
				return View(nameof(Checkout), model);
			}

			var paymentMethodLabel = paymentResult.IsSandbox
				? $"{paymentResult.ProviderName} ({paymentResult.TransactionCode})"
				: "COD";

			return CompleteSuccessfulCheckout(model, gioHang, kh, paymentResult.ProviderName, paymentMethodLabel, paymentResult.StatusText);
		}

		private IActionResult CompleteSuccessfulCheckout(
			CheckoutVM model,
			List<CartItem> gioHang,
			KhachHang kh,
			string providerName,
			string paymentMethodLabel,
			string paymentStatus)
		{
			var trangThaiMoi = db.TrangThais.OrderBy(x => x.MaTrangThai).FirstOrDefault();
			if (trangThaiMoi == null)
			{
				ModelState.AddModelError(string.Empty, "ChÆ°a cáº¥u hÃ¬nh tráº¡ng thÃ¡i Ä‘Æ¡n hÃ ng trong há»‡ thá»‘ng.");
				ViewBag.Cart = gioHang;
				return View(nameof(Checkout), model);
			}

			var shippingFee = Math.Max(0, model.PhiVanChuyen);
			var subtotal = gioHang.Sum(c => c.SoLuong * c.DonGia);
			var total = subtotal + shippingFee;

			using var transaction = db.Database.BeginTransaction();
			try
			{
				var ghiChuThanhToan = string.IsNullOrWhiteSpace(model.GhiChu)
					? paymentStatus
					: $"{model.GhiChu} | {paymentStatus}";
				if (ghiChuThanhToan.Length > 50)
				{
					ghiChuThanhToan = ghiChuThanhToan[..50];
				}

				var hoaDon = new HoaDon
				{
					MaKh = kh.MaKh,
					NgayDat = DateTime.Now,
					NgayCan = DateTime.Now.AddDays(3),
					HoTen = model.HoTen,
					DiaChi = model.DiaChi,
					CachThanhToan = paymentMethodLabel,
					CachVanChuyen = string.IsNullOrWhiteSpace(model.CachVanChuyen) ? "Giao hÃ ng tiÃªu chuáº©n" : model.CachVanChuyen,
					PhiVanChuyen = shippingFee,
					MaTrangThai = trangThaiMoi.MaTrangThai,
					GhiChu = ghiChuThanhToan
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
				ClearCart();

				var orderLines = string.Join(string.Empty, gioHang.Select(item =>
					$"<li>{item.TenHH} x{item.SoLuong}: {(item.SoLuong * item.DonGia):N0} VND</li>"));
				var subject = $"[DEEPSEARCH] Xác nhận đơn hàng #{hoaDon.MaHd}";
				var body = EmailTemplates.BuildCheckoutSuccess(kh.HoTen, hoaDon.MaHd, paymentStatus, paymentMethodLabel, orderLines, subtotal, shippingFee, total);

				if (emailService.TrySend(kh.Email, subject, body, out var emailError))
				{
					TempData["SuccessMessage"] = $"Đặt hàng thành công qua {providerName}. Email xác nhận đã được gửi.";
				}
				else
				{
					TempData["ErrorMessage"] = $"Đặt hàng thành công nhưng chưa gửi được email xác nhận ({emailError}).";
				}

				return RedirectToAction(nameof(CheckoutSuccess), new { id = hoaDon.MaHd });
			}
			catch
			{
				transaction.Rollback();
				ModelState.AddModelError(string.Empty, "Không thể tạo đơn hàng. Vui lòng thử lại.");
				ViewBag.Cart = gioHang;
				return View(nameof(Checkout), model);
			}
		}

		[HttpGet]
		public IActionResult CheckoutSuccess(int id)
		{
			if (string.IsNullOrWhiteSpace(CurrentCustomerId))
			{
				return RedirectToAction("DangNhap", "KhachHang");
			}

			var order = db.HoaDons
				.Include(x => x.ChiTietHds)
				.SingleOrDefault(x => x.MaHd == id && x.MaKh == CurrentCustomerId);
			if (order == null)
			{
				return RedirectToAction(nameof(LichSuDonHang));
			}

			var subtotal = order.ChiTietHds.Sum(c => c.SoLuong * (c.DonGia - c.GiamGia));
			var model = new CheckoutSuccessVM
			{
				OrderId = order.MaHd,
				TotalQuantityPaid = order.ChiTietHds.Sum(c => c.SoLuong),
				SubtotalPaid = subtotal,
				ShippingFee = order.PhiVanChuyen,
				TotalPaid = subtotal + order.PhiVanChuyen,
				PaymentMethod = order.CachThanhToan,
				PaymentStatus = order.GhiChu ?? "ÄÃ£ ghi nháº­n Ä‘Æ¡n hÃ ng"
			};

			return View(model);
		}

		[HttpGet]
		public IActionResult LichSuDonHang()
		{
			if (string.IsNullOrWhiteSpace(CurrentCustomerId))
			{
				return RedirectToAction("DangNhap", "KhachHang", new { returnUrl = Url.Action(nameof(LichSuDonHang), "Cart") });
			}

			var items = db.HoaDons
				.Where(x => x.MaKh == CurrentCustomerId)
				.OrderByDescending(x => x.NgayDat)
				.Select(x => new LichSuDonHangItemVM
				{
					MaHd = x.MaHd,
					NgayDat = x.NgayDat,
					TrangThai = x.MaTrangThaiNavigation.TenTrangThai,
					CachThanhToan = x.CachThanhToan,
					TongSoLuong = x.ChiTietHds.Sum(c => c.SoLuong),
					TongTien = x.ChiTietHds.Sum(c => c.SoLuong * (c.DonGia - c.GiamGia)) + x.PhiVanChuyen
				})
				.ToList();

			return View(items);
		}

		[HttpGet]
		public IActionResult ChiTietDonHang(int id)
		{
			if (string.IsNullOrWhiteSpace(CurrentCustomerId))
			{
				return RedirectToAction("DangNhap", "KhachHang", new { returnUrl = Url.Action(nameof(ChiTietDonHang), "Cart", new { id }) });
			}

			var data = db.HoaDons
				.Include(x => x.MaTrangThaiNavigation)
				.Include(x => x.ChiTietHds)
					.ThenInclude(c => c.MaHhNavigation)
				.SingleOrDefault(x => x.MaHd == id && x.MaKh == CurrentCustomerId);

			if (data == null)
			{
				return RedirectToAction(nameof(LichSuDonHang));
			}

			var model = new ChiTietDonHangVM
			{
				MaHd = data.MaHd,
				NgayDat = data.NgayDat,
				HoTen = data.HoTen ?? string.Empty,
				DiaChi = data.DiaChi,
				TrangThai = data.MaTrangThaiNavigation.TenTrangThai,
				CachThanhToan = data.CachThanhToan,
				CachVanChuyen = data.CachVanChuyen,
				PhiVanChuyen = data.PhiVanChuyen,
				GhiChu = data.GhiChu,
				Items = data.ChiTietHds.Select(c => new ChiTietDonHangLineVM
				{
					MaHh = c.MaHh,
					TenHh = c.MaHhNavigation.TenHh,
					Hinh = c.MaHhNavigation.Hinh ?? string.Empty,
					SoLuong = c.SoLuong,
					DonGia = c.DonGia,
					GiamGia = c.GiamGia
				}).ToList()
			};

			return View(model);
		}

		private IActionResult RedirectToSafeReturn(string? returnUrl)
		{
			if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}

			return RedirectToAction(nameof(Index));
		}

		private List<CartItem> GetCart()
		{
			var sessionCart = HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();
			if (string.IsNullOrWhiteSpace(CurrentCustomerId))
			{
				return sessionCart;
			}

			try
			{
				var dbCart = db.GioHangItems
					.AsNoTracking()
					.Where(x => x.MaKh == CurrentCustomerId)
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
							SoLuong = Math.Max(0, c.SoLuong)
						})
					.ToList();

				if (dbCart.Count == 0 && sessionCart.Count > 0)
				{
					TrySaveCart(sessionCart, out _);
					return sessionCart;
				}

				HttpContext.Session.Set(MySetting.CART_KEY, dbCart);
				return dbCart;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "KhÃ´ng thá»ƒ táº£i giá» hÃ ng DB cho MaKh={MaKh}. Fallback session cart.", CurrentCustomerId);
				return sessionCart;
			}
		}

		private void SaveCart(List<CartItem> cart)
		{
			HttpContext.Session.Set(MySetting.CART_KEY, cart);
			if (string.IsNullOrWhiteSpace(CurrentCustomerId))
			{
				return;
			}

			var now = DateTime.Now;
			var normalized = cart
				.Where(x => x.SoLuong > 0)
				.GroupBy(x => x.MaHh)
				.Select(g => new { MaHh = g.Key, SoLuong = g.Sum(x => x.SoLuong) })
				.ToList();

			var existed = db.GioHangItems
				.Where(x => x.MaKh == CurrentCustomerId)
				.ToList();

			var existedMap = existed.ToDictionary(x => x.MaHh, x => x);
			foreach (var row in normalized)
			{
				if (existedMap.TryGetValue(row.MaHh, out var item))
				{
					item.SoLuong = row.SoLuong;
					item.UpdatedAt = now;
				}
				else
				{
					db.GioHangItems.Add(new GioHangItem
					{
						MaKh = CurrentCustomerId!,
						MaHh = row.MaHh,
						SoLuong = row.SoLuong,
						CreatedAt = now,
						UpdatedAt = now
					});
				}
			}

			var keepIds = normalized.Select(x => x.MaHh).ToHashSet();
			var toDelete = existed.Where(x => !keepIds.Contains(x.MaHh)).ToList();
			if (toDelete.Count > 0)
			{
				db.GioHangItems.RemoveRange(toDelete);
			}

			db.SaveChanges();
		}

		private bool TrySaveCart(List<CartItem> cart, out string? errorMessage)
		{
			errorMessage = null;
			try
			{
				SaveCart(cart);
				return true;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Lá»—i lÆ°u giá» hÃ ng cho MaKh={MaKh}.", CurrentCustomerId);
				errorMessage = ex.InnerException?.Message ?? ex.Message;
				return false;
			}
		}

		private void ClearCart()
		{
			HttpContext.Session.Remove(MySetting.CART_KEY);
			if (string.IsNullOrWhiteSpace(CurrentCustomerId))
			{
				return;
			}

			var items = db.GioHangItems.Where(x => x.MaKh == CurrentCustomerId).ToList();
			if (items.Count == 0)
			{
				return;
			}

			db.GioHangItems.RemoveRange(items);
			db.SaveChanges();
		}

		private void ApplyCheckoutPricing(CheckoutVM model, List<CartItem> cartItems)
		{
			var subtotal = cartItems.Sum(x => x.ThanhTien);
			var shippingFee = shippingFeeService.Calculate(model.DiaChi, model.CachVanChuyen, subtotal);
			var voucher = voucherService.ValidateAndCalculateDiscount(AppliedVoucherCode, subtotal);
			var discount = voucher.Success ? voucher.DiscountAmount : 0d;

			model.DiscountAmount = discount;
			model.PhiVanChuyen = shippingFee;
			model.VoucherCode = voucher.Success ? voucher.Code : null;
		}

		private void PopulateCartPricingViewBag(List<CartItem> cartItems)
		{
			var subtotal = cartItems.Sum(x => x.ThanhTien);
			var shippingFee = shippingFeeService.Calculate(null, null, subtotal);
			var voucher = voucherService.ValidateAndCalculateDiscount(AppliedVoucherCode, subtotal);
			var discount = voucher.Success ? voucher.DiscountAmount : 0d;

			ViewBag.CartSubtotal = subtotal;
			ViewBag.ShippingFee = shippingFee;
			ViewBag.CartDiscount = discount;
			ViewBag.CartTotal = Math.Max(0d, subtotal - discount + shippingFee);
			ViewBag.AppliedVoucherCode = voucher.Success ? voucher.Code : null;
			ViewBag.AppliedVoucherMessage = voucher.Success ? voucher.Message : null;
		}

		private static string GetQueryParameter(string url, string key)
		{
			var uri = new Uri(url);
			var query = uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
			foreach (var part in query)
			{
				var pieces = part.Split('=', 2);
				if (pieces.Length == 2 && string.Equals(pieces[0], key, StringComparison.Ordinal))
				{
					return Uri.UnescapeDataString(pieces[1]);
				}
			}

			return string.Empty;
		}
	}
}
