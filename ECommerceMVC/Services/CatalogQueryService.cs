using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMVC.Services;

public class CatalogQueryService : ICatalogQueryService
{
    private readonly Hshop2023Context db;

    public CatalogQueryService(Hshop2023Context db)
    {
        this.db = db;
    }

    public List<SearchSuggestionVM> GetSearchSuggestions(string? query, int take = 6)
    {
        query = (query ?? string.Empty).Trim();
        if (query.Length < 2)
        {
            return new List<SearchSuggestionVM>();
        }

        take = Math.Clamp(take, 1, 10);
        return db.HangHoas
            .AsNoTracking()
            .Include(p => p.MaLoaiNavigation)
            .Where(p => p.MoTa == null || !p.MoTa.Contains(AdminMetadataHelper.HiddenProductNeedle))
            .Where(p => p.TenHh.ToLower().Contains(query.ToLower()))
            .OrderByDescending(p => p.NgaySx)
            .ThenByDescending(p => p.MaHh)
            .Take(take)
            .Select(p => new SearchSuggestionVM
            {
                MaHh = p.MaHh,
                TenHH = p.TenHh,
                DonGia = p.DonGia ?? 0,
                HinhUrl = MyUtil.GetHangHoaImageUrl(p.Hinh ?? string.Empty, p.MaHh),
                TenLoai = p.MaLoaiNavigation.TenLoai,
                DetailUrl = $"/HangHoa/Detail/{p.MaHh}"
            })
            .ToList();
    }

    public IQueryable<HangHoa> ApplyRatingFilter(IQueryable<HangHoa> query, int? minRating)
    {
        if (!minRating.HasValue)
        {
            return query;
        }

        var normalized = Math.Clamp(minRating.Value, 1, 5);
        var ratedProductIds = db.ProductReviews
            .GroupBy(r => r.MaHh)
            .Where(g => g.Average(r => r.SoSao) >= normalized)
            .Select(g => g.Key);

        return query.Where(p => ratedProductIds.Contains(p.MaHh));
    }

    public IQueryable<HangHoa> ApplySort(IQueryable<HangHoa> query, string? sort)
    {
        sort = string.IsNullOrWhiteSpace(sort) ? "newest" : sort.Trim().ToLowerInvariant();
        return sort switch
        {
            "best_seller" => query
                .GroupJoin(
                    db.ChiTietHds.GroupBy(c => c.MaHh).Select(g => new { MaHh = g.Key, Sold = g.Sum(x => x.SoLuong) }),
                    p => p.MaHh,
                    s => s.MaHh,
                    (p, sold) => new { Product = p, Sold = sold.Select(x => x.Sold).FirstOrDefault() })
                .OrderByDescending(x => x.Sold)
                .ThenByDescending(x => x.Product.NgaySx)
                .ThenByDescending(x => x.Product.MaHh)
                .Select(x => x.Product),
            "popular" => query.OrderByDescending(p => p.SoLanXem).ThenByDescending(p => p.MaHh),
            "price_asc" => query.OrderBy(p => p.DonGia ?? 0).ThenBy(p => p.MaHh),
            "price_desc" => query.OrderByDescending(p => p.DonGia ?? 0).ThenByDescending(p => p.MaHh),
            _ => query.OrderByDescending(p => p.NgaySx).ThenByDescending(p => p.MaHh)
        };
    }
}
