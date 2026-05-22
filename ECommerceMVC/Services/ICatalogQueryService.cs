using ECommerceMVC.Data;
using ECommerceMVC.ViewModels;

namespace ECommerceMVC.Services;

public interface ICatalogQueryService
{
    List<SearchSuggestionVM> GetSearchSuggestions(string? query, int take = 6);
    IQueryable<HangHoa> ApplyRatingFilter(IQueryable<HangHoa> query, int? minRating);
    IQueryable<HangHoa> ApplySort(IQueryable<HangHoa> query, string? sort);
}
