namespace ECommerceMVC.ViewModels;

public class HangHoaListPageVM
{
    public List<HangHoaVM> Items { get; set; } = new();
    public HangHoaFilterVM Filter { get; set; } = new();
    public List<BrandOptionVM> BrandOptions { get; set; } = new();
    public List<string> MaterialOptions { get; set; } = new();
    public List<string> StyleOptions { get; set; } = new();
    public List<string> ColorOptions { get; set; } = new();
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int StartItem => TotalItems == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;
    public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
}

public class BrandOptionVM
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}
