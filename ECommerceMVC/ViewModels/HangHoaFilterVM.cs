namespace ECommerceMVC.ViewModels;

public class HangHoaFilterVM
{
    public int? Loai { get; set; }
    public string Sort { get; set; } = "newest";
    public int View { get; set; } = 12;
    public int Page { get; set; } = 1;
    public List<string> Brands { get; set; } = new();
    public double? MinPrice { get; set; }
    public double? MaxPrice { get; set; }
    public string? Query { get; set; }
    public int? MinRating { get; set; }
    public List<string> Materials { get; set; } = new();
    public List<string> Styles { get; set; } = new();
    public List<string> Colors { get; set; } = new();
    public bool OnlyFavourite { get; set; }
    public string Mode { get; set; } = "shop";
}
