using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.Services;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMVC.Controllers
{
	public class HangHoaController : Controller
	{
		private static readonly int[] AllowedPageSizes = [12, 24, 48, 96];
		private readonly Hshop2023Context db;
		private readonly ICatalogQueryService catalogQueryService;

		public HangHoaController(Hshop2023Context conetxt, ICatalogQueryService catalogQueryService)
		{
			db = conetxt;
			this.catalogQueryService = catalogQueryService;
		}

		public IActionResult Index(
			int? loai,
			string? sort,
			int view = 12,
			int page = 1,
			List<string>? brands = null,
			List<string>? materials = null,
			List<string>? styles = null,
			List<string>? colors = null,
			double? minPrice = null,
			double? maxPrice = null,
			string? query = null,
			int? minRating = null,
			bool onlyFavourite = false,
			string? mode = null)
		{
			var filter = new HangHoaFilterVM
			{
				Loai = loai,
				Sort = string.IsNullOrWhiteSpace(sort) ? "newest" : sort.Trim().ToLowerInvariant(),
				View = AllowedPageSizes.Contains(view) ? view : 12,
				Page = page < 1 ? 1 : page,
				Brands = (brands ?? new List<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().ToList(),
				Materials = (materials ?? new List<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().ToList(),
				Styles = (styles ?? new List<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().ToList(),
				Colors = (colors ?? new List<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().ToList(),
				MinPrice = minPrice,
				MaxPrice = maxPrice,
				Query = string.IsNullOrWhiteSpace(query) ? null : query.Trim(),
				MinRating = minRating.HasValue ? Math.Clamp(minRating.Value, 1, 5) : null,
				OnlyFavourite = onlyFavourite,
				Mode = string.Equals(mode, "list", StringComparison.OrdinalIgnoreCase) ? "list" : "shop"
			};

			if (filter.MinPrice.HasValue && filter.MinPrice.Value < 0)
			{
				filter.MinPrice = 0;
			}

			if (filter.MaxPrice.HasValue && filter.MaxPrice.Value < 0)
			{
				filter.MaxPrice = 0;
			}

			if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue && filter.MinPrice.Value > filter.MaxPrice.Value)
			{
				(filter.MinPrice, filter.MaxPrice) = (filter.MaxPrice, filter.MinPrice);
			}

			var customerId = HttpContext.Session.Get<string>(MySetting.CUSTOMER_KEY);

			var baseQuery = db.HangHoas
				.AsNoTracking()
				.Include(p => p.MaLoaiNavigation)
				.Include(p => p.MaNccNavigation)
				.Where(p => p.MoTa == null || !p.MoTa.Contains(AdminMetadataHelper.HiddenProductNeedle))
				.AsQueryable();

			if (filter.Loai.HasValue)
			{
				baseQuery = baseQuery.Where(p => p.MaLoai == filter.Loai.Value);
			}

			if (!string.IsNullOrWhiteSpace(filter.Query))
			{
				baseQuery = baseQuery.Where(p => p.TenHh.Contains(filter.Query));
			}

			if (filter.Brands.Any())
			{
				baseQuery = baseQuery.Where(p => filter.Brands.Contains(p.MaNcc));
			}
			if (filter.Materials.Any())
			{
				baseQuery = baseQuery.Where(p => p.ChatLieu != null && filter.Materials.Contains(p.ChatLieu));
			}
			if (filter.Styles.Any())
			{
				baseQuery = baseQuery.Where(p => p.PhongCach != null && filter.Styles.Contains(p.PhongCach));
			}
			if (filter.Colors.Any())
			{
				baseQuery = baseQuery.Where(p => p.MauSac != null && filter.Colors.Contains(p.MauSac));
			}

			if (filter.MinPrice.HasValue)
			{
				baseQuery = baseQuery.Where(p => (p.DonGia ?? 0) >= filter.MinPrice.Value);
			}

			if (filter.MaxPrice.HasValue)
			{
				baseQuery = baseQuery.Where(p => (p.DonGia ?? 0) <= filter.MaxPrice.Value);
			}

			if (filter.OnlyFavourite)
			{
				if (string.IsNullOrWhiteSpace(customerId))
				{
					baseQuery = baseQuery.Where(p => false);
				}
				else
				{
					var favouriteIds = db.YeuThiches
						.Where(x => x.MaKh == customerId && x.MaHh.HasValue)
						.Select(x => x.MaHh!.Value);
					baseQuery = baseQuery.Where(p => favouriteIds.Contains(p.MaHh));
				}
			}

			baseQuery = catalogQueryService.ApplyRatingFilter(baseQuery, filter.MinRating);
			baseQuery = catalogQueryService.ApplySort(baseQuery, filter.Sort);

			var totalItems = baseQuery.Count();
			var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)filter.View));
			if (filter.Page > totalPages)
			{
				filter.Page = totalPages;
			}

			var items = baseQuery
				.Skip((filter.Page - 1) * filter.View)
				.Take(filter.View)
				.Select(p => new HangHoaVM
				{
					MaHh = p.MaHh,
					TenHH = p.TenHh,
					DonGia = p.DonGia ?? 0,
					Hinh = p.Hinh ?? string.Empty,
					MoTaNgan = p.MoTaDonVi ?? string.Empty,
					TenLoai = p.MaLoaiNavigation.TenLoai,
					SoLuongTon = p.SoLuongTon
				})
				.ToList();

			if (items.Count > 0)
			{
				var itemIds = items.Select(x => x.MaHh).ToList();
				var soldLookup = db.ChiTietHds
					.AsNoTracking()
					.Where(x => itemIds.Contains(x.MaHh) && OrderStatusHelper.CompletedStatusIds.Contains(x.MaHdNavigation.MaTrangThai))
					.GroupBy(x => x.MaHh)
					.Select(g => new
					{
						MaHh = g.Key,
						SoLuongDaBan = g.Sum(x => x.SoLuong)
					})
					.ToDictionary(x => x.MaHh, x => x.SoLuongDaBan);

				var ratingLookup = db.ProductReviews
					.AsNoTracking()
					.Where(x => itemIds.Contains(x.MaHh))
					.GroupBy(x => x.MaHh)
					.Select(g => new
					{
						MaHh = g.Key,
						SoDanhGia = g.Count(),
						DiemDanhGiaTrungBinh = g.Average(x => (double)x.SoSao)
					})
					.ToDictionary(x => x.MaHh, x => x);

				HashSet<int> favouriteIds = new();
				if (!string.IsNullOrWhiteSpace(customerId))
				{
					favouriteIds = db.YeuThiches
						.AsNoTracking()
						.Where(x => x.MaKh == customerId && x.MaHh.HasValue && itemIds.Contains(x.MaHh.Value))
						.Select(x => x.MaHh!.Value)
						.ToHashSet();
				}

				foreach (var item in items)
				{
					if (soldLookup.TryGetValue(item.MaHh, out var sold))
					{
						item.SoLuongDaBan = sold;
					}

					if (ratingLookup.TryGetValue(item.MaHh, out var rating))
					{
						item.SoDanhGia = rating.SoDanhGia;
						item.DiemDanhGiaTrungBinh = rating.DiemDanhGiaTrungBinh;
						item.DiemDanhGia = (int)Math.Round(rating.DiemDanhGiaTrungBinh, MidpointRounding.AwayFromZero);
					}

					item.IsFavourite = favouriteIds.Contains(item.MaHh);
				}
			}

			var brandOptions = db.NhaCungCaps
				.AsNoTracking()
				.OrderBy(x => x.TenCongTy)
				.Select(x => new BrandOptionVM
				{
					Value = x.MaNcc,
					Label = x.TenCongTy
				})
				.ToList();

			var vm = new HangHoaListPageVM
			{
				Items = items,
				Filter = filter,
				BrandOptions = brandOptions,
				MaterialOptions = db.HangHoas
					.AsNoTracking()
					.Where(x => x.ChatLieu != null && x.ChatLieu != string.Empty)
					.Select(x => x.ChatLieu!)
					.Distinct()
					.OrderBy(x => x)
					.Take(20)
					.ToList(),
				StyleOptions = db.HangHoas
					.AsNoTracking()
					.Where(x => x.PhongCach != null && x.PhongCach != string.Empty)
					.Select(x => x.PhongCach!)
					.Distinct()
					.OrderBy(x => x)
					.Take(20)
					.ToList(),
				ColorOptions = db.HangHoas
					.AsNoTracking()
					.Where(x => x.MauSac != null && x.MauSac != string.Empty)
					.Select(x => x.MauSac!)
					.Distinct()
					.OrderBy(x => x)
					.Take(20)
					.ToList(),
				TotalItems = totalItems,
				CurrentPage = filter.Page,
				PageSize = filter.View,
				TotalPages = totalPages
			};

			return View(vm);
		}

		[HttpGet]
		public IActionResult SearchSuggestions(string? query, int take = 6)
		{
			return Json(catalogQueryService.GetSearchSuggestions(query, take));
		}

		public IActionResult Discount()
		{
			return RedirectToAction(nameof(Index), new { sort = "price_desc" });
		}

		public IActionResult NewThisWeek()
		{
			return RedirectToAction(nameof(Index), new { sort = "newest" });
		}

		public IActionResult Favourite()
		{
			var customerId = HttpContext.Session.Get<string>(MySetting.CUSTOMER_KEY);
			if (string.IsNullOrWhiteSpace(customerId))
			{
				return RedirectToAction("DangNhap", "KhachHang", new { returnUrl = Url.Action(nameof(Favourite), "HangHoa") });
			}

			var favoriteIds = db.YeuThiches
				.Where(x => x.MaKh == customerId && x.MaHh.HasValue)
				.Select(x => x.MaHh!.Value)
				.ToList();

			var items = db.HangHoas
				.AsNoTracking()
				.Include(p => p.MaLoaiNavigation)
				.Where(p => p.MoTa == null || !p.MoTa.Contains(AdminMetadataHelper.HiddenProductNeedle))
				.Where(p => favoriteIds.Contains(p.MaHh))
				.OrderByDescending(p => p.NgaySx)
				.Select(p => new HangHoaVM
				{
					MaHh = p.MaHh,
					TenHH = p.TenHh,
					DonGia = p.DonGia ?? 0,
					Hinh = p.Hinh ?? string.Empty,
					MoTaNgan = p.MoTaDonVi ?? string.Empty,
					TenLoai = p.MaLoaiNavigation.TenLoai,
					SoLuongTon = p.SoLuongTon,
					IsFavourite = true
				})
				.ToList();

			var brandOptions = db.NhaCungCaps
				.AsNoTracking()
				.OrderBy(x => x.TenCongTy)
				.Select(x => new BrandOptionVM
				{
					Value = x.MaNcc,
					Label = x.TenCongTy
				})
				.ToList();

			var vm = new HangHoaListPageVM
			{
				Items = items,
				Filter = new HangHoaFilterVM { View = 12, Page = 1, Sort = "newest" },
				BrandOptions = brandOptions,
				TotalItems = items.Count,
				CurrentPage = 1,
				PageSize = items.Count == 0 ? 1 : items.Count,
				TotalPages = 1
			};

			return View("Index", vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult AddFavourite(int id, string? returnUrl = null)
		{
			var customerId = HttpContext.Session.Get<string>(MySetting.CUSTOMER_KEY);
			if (string.IsNullOrWhiteSpace(customerId))
			{
				var targetReturnUrl = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
					? returnUrl
					: Url.Action(nameof(Index), "HangHoa");
				return RedirectToAction("DangNhap", "KhachHang", new { returnUrl = targetReturnUrl });
			}

			var exists = db.YeuThiches.Any(x => x.MaKh == customerId && x.MaHh == id);
			if (!exists)
			{
				db.YeuThiches.Add(new YeuThich
				{
					MaKh = customerId,
					MaHh = id,
					NgayChon = DateTime.Now
				});
				db.SaveChanges();
				TempData["SuccessMessage"] = "Đã thêm sản phẩm vào danh sách yêu thích.";
			}
			else
			{
				TempData["SuccessMessage"] = "Sản phẩm đã có trong danh sách yêu thích.";
			}

			if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}

			return RedirectToAction(nameof(Favourite));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult RemoveFavourite(int id, string? returnUrl = null)
		{
			var customerId = HttpContext.Session.Get<string>(MySetting.CUSTOMER_KEY);
			if (string.IsNullOrWhiteSpace(customerId))
			{
				var targetReturnUrl = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
					? returnUrl
					: Url.Action(nameof(Favourite), "HangHoa");
				return RedirectToAction("DangNhap", "KhachHang", new { returnUrl = targetReturnUrl });
			}

			var favourite = db.YeuThiches.FirstOrDefault(x => x.MaKh == customerId && x.MaHh == id);
			if (favourite != null)
			{
				db.YeuThiches.Remove(favourite);
				db.SaveChanges();
				TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi danh sách yêu thích.";
			}
			else
			{
				TempData["ErrorMessage"] = "Không tìm thấy sản phẩm trong danh sách yêu thích.";
			}

			if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}

			return RedirectToAction(nameof(Favourite));
		}

		public IActionResult Detail(int id)
		{
			var customerId = HttpContext.Session.Get<string>(MySetting.CUSTOMER_KEY);
			var result = BuildDetailViewModel(id, customerId);
			if (result == null)
			{
				TempData["Message"] = $"Không thấy sản phẩm có mã {id}";
				return Redirect("/404");
			}

			return View(result);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult AddReview(ProductReviewInputVM model, string? returnUrl = null)
		{
			var customerId = HttpContext.Session.Get<string>(MySetting.CUSTOMER_KEY);
			if (string.IsNullOrWhiteSpace(customerId))
			{
				var targetReturnUrl = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
					? returnUrl
					: Url.Action(nameof(Detail), "HangHoa", new { id = model.MaHh });
				return RedirectToAction("DangNhap", "KhachHang", new { returnUrl = targetReturnUrl });
			}

			var purchased = db.HoaDons.Any(x => x.MaKh == customerId && x.ChiTietHds.Any(c => c.MaHh == model.MaHh));
			if (!purchased)
			{
				TempData["ErrorMessage"] = "Bạn cần mua sản phẩm trước khi đánh giá.";
				return RedirectToAction(nameof(Detail), new { id = model.MaHh });
			}

			var reviewed = db.ProductReviews.Any(x => x.MaHh == model.MaHh && x.MaKh == customerId);
			if (reviewed)
			{
				TempData["ErrorMessage"] = "Bạn đã đánh giá sản phẩm này rồi.";
				return RedirectToAction(nameof(Detail), new { id = model.MaHh });
			}

			if (!ModelState.IsValid)
			{
				TempData["ErrorMessage"] = "Dữ liệu đánh giá chưa hợp lệ.";
				return RedirectToAction(nameof(Detail), new { id = model.MaHh });
			}

			db.ProductReviews.Add(new ProductReview
			{
				MaHh = model.MaHh,
				MaKh = customerId,
				SoSao = model.SoSao,
				NoiDung = model.NoiDung.Trim(),
				NgayTao = DateTime.Now
			});
			db.SaveChanges();

			TempData["SuccessMessage"] = "Cảm ơn bạn đã gửi đánh giá.";
			if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}

			return RedirectToAction(nameof(Detail), new { id = model.MaHh });
		}

		private ChiTietHangHoaVM? BuildDetailViewModel(int id, string? customerId)
		{
			var data = db.HangHoas
				.AsNoTracking()
				.Include(p => p.MaLoaiNavigation)
				.Include(p => p.MaNccNavigation)
				.SingleOrDefault(p => p.MaHh == id && (p.MoTa == null || !p.MoTa.Contains(AdminMetadataHelper.HiddenProductNeedle)));
			if (data == null)
			{
				return null;
			}

			var reviews = db.ProductReviews
				.AsNoTracking()
				.Where(x => x.MaHh == id)
				.OrderByDescending(x => x.NgayTao)
				.Join(
					db.KhachHangs.AsNoTracking(),
					r => r.MaKh,
					k => k.MaKh,
					(r, k) => new ProductReviewVM
					{
						MaKh = r.MaKh,
						HoTen = k.HoTen,
						SoSao = r.SoSao,
						NoiDung = r.NoiDung,
						NgayTao = r.NgayTao
					})
				.ToList();

			var isFavourite = false;
			var canReview = false;
			if (!string.IsNullOrWhiteSpace(customerId))
			{
				isFavourite = db.YeuThiches.Any(x => x.MaKh == customerId && x.MaHh == id);
				var purchased = db.HoaDons.Any(x => x.MaKh == customerId && x.ChiTietHds.Any(c => c.MaHh == id));
				var reviewed = db.ProductReviews.Any(x => x.MaHh == id && x.MaKh == customerId);
				canReview = purchased && !reviewed;
			}

			var avgStar = reviews.Any()
				? (int)Math.Round(reviews.Average(x => x.SoSao), MidpointRounding.AwayFromZero)
				: 0;

			var relatedProducts = db.HangHoas
				.AsNoTracking()
				.Include(p => p.MaLoaiNavigation)
				.Where(p => p.MaHh != id && (p.MaLoai == data.MaLoai || p.MaNcc == data.MaNcc))
				.OrderByDescending(p => p.MaLoai == data.MaLoai)
				.ThenByDescending(p => p.NgaySx)
				.ThenByDescending(p => p.MaHh)
				.Take(4)
				.Select(p => new HangHoaVM
				{
					MaHh = p.MaHh,
					TenHH = p.TenHh,
					DonGia = p.DonGia ?? 0,
					Hinh = p.Hinh ?? string.Empty,
					MoTaNgan = p.MoTaDonVi ?? string.Empty,
					TenLoai = p.MaLoaiNavigation.TenLoai,
					SoLuongTon = p.SoLuongTon
				})
				.ToList();

			return new ChiTietHangHoaVM
			{
				MaHh = data.MaHh,
				TenHH = data.TenHh,
				DonGia = data.DonGia ?? 0,
				ChiTiet = data.MoTa ?? string.Empty,
				Hinh = data.Hinh ?? string.Empty,
				MoTaNgan = data.MoTaDonVi ?? string.Empty,
				TenLoai = data.MaLoaiNavigation.TenLoai,
				MauSac = data.MauSac ?? string.Empty,
				ChatLieu = data.ChatLieu ?? string.Empty,
				KichThuoc = data.KichThuoc ?? string.Empty,
				BaoHanh = data.BaoHanh ?? string.Empty,
				PhongCach = data.PhongCach ?? string.Empty,
				TenNhaCungCap = data.MaNccNavigation.TenCongTy,
				SoLuongTon = data.SoLuongTon,
				DiemDanhGia = avgStar,
				IsFavourite = isFavourite,
				CanReview = canReview,
				Reviews = reviews,
				RelatedProducts = relatedProducts,
				NewReview = new ProductReviewInputVM { MaHh = data.MaHh, SoSao = 5 }
			};
		}
	}
}
