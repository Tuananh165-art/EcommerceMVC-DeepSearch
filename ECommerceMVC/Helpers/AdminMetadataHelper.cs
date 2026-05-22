using System.Text.Json;

namespace ECommerceMVC.Helpers;

public static class AdminMetadataHelper
{
    private const string CategoryMarker = "||admin:";
    private const string ProductMarker = "||adminProduct:";
    public const string HiddenProductNeedle = "||adminProduct:{\"IsVisible\":false";

    public sealed class CategoryMeta
    {
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; } = 100;
        public bool IsVisible { get; set; } = true;
    }

    public sealed class ProductMeta
    {
        public string? Description { get; set; }
        public bool IsVisible { get; set; } = true;
        public int LowStockThreshold { get; set; } = 5;
    }

    public static CategoryMeta ParseCategory(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return new CategoryMeta();
        var parts = value.Split(CategoryMarker, 2, StringSplitOptions.None);
        var description = parts[0].Trim();
        if (parts.Length == 1) return new CategoryMeta { Description = EmptyToNull(description) };

        try
        {
            var dto = JsonSerializer.Deserialize<CategoryMetaDto>(parts[1]);
            return new CategoryMeta
            {
                Description = EmptyToNull(description),
                ParentId = dto?.ParentId,
                SortOrder = dto?.SortOrder ?? 100,
                IsVisible = dto?.IsVisible ?? true
            };
        }
        catch
        {
            return new CategoryMeta { Description = value };
        }
    }

    public static string BuildCategory(CategoryMeta meta)
    {
        var dto = new CategoryMetaDto
        {
            ParentId = meta.ParentId,
            SortOrder = meta.SortOrder,
            IsVisible = meta.IsVisible
        };
        return $"{meta.Description?.Trim() ?? string.Empty}{CategoryMarker}{JsonSerializer.Serialize(dto)}";
    }

    public static ProductMeta ParseProduct(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return new ProductMeta();
        var parts = value.Split(ProductMarker, 2, StringSplitOptions.None);
        var description = parts[0].Trim();
        if (parts.Length == 1) return new ProductMeta { Description = EmptyToNull(description) };

        try
        {
            var dto = JsonSerializer.Deserialize<ProductMetaDto>(parts[1]);
            return new ProductMeta
            {
                Description = EmptyToNull(description),
                IsVisible = dto?.IsVisible ?? true,
                LowStockThreshold = Math.Max(0, dto?.LowStockThreshold ?? 5)
            };
        }
        catch
        {
            return new ProductMeta { Description = value };
        }
    }

    public static string? BuildProduct(ProductMeta meta)
    {
        var dto = new ProductMetaDto
        {
            IsVisible = meta.IsVisible,
            LowStockThreshold = Math.Max(0, meta.LowStockThreshold)
        };
        var value = $"{meta.Description?.Trim() ?? string.Empty}{ProductMarker}{JsonSerializer.Serialize(dto)}";
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public static string? StripProductMetadata(string? value) => ParseProduct(value).Description;
    public static string? StripCategoryMetadata(string? value) => ParseCategory(value).Description;

    private static string? EmptyToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed class CategoryMetaDto
    {
        public int? ParentId { get; set; }
        public int SortOrder { get; set; } = 100;
        public bool IsVisible { get; set; } = true;
    }

    private sealed class ProductMetaDto
    {
        public bool IsVisible { get; set; } = true;
        public int LowStockThreshold { get; set; } = 5;
    }
}
