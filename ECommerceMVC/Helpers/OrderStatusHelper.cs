namespace ECommerceMVC.Helpers;

public static class OrderStatusHelper
{
    public const int New = 0;
    public const int PendingConfirmation = 1;
    public const int Shipping = 2;
    public const int Completed = 3;
    public const int Cancelled = 4;

    public static readonly int[] PendingStatusIds = [New, PendingConfirmation];
    public static readonly int[] ShippingStatusIds = [Shipping];
    public static readonly int[] CompletedStatusIds = [Completed];
    public static readonly int[] CancelledStatusIds = [Cancelled];

    public static bool IsPending(int statusId) => PendingStatusIds.Contains(statusId);
    public static bool IsShipping(int statusId) => ShippingStatusIds.Contains(statusId);
    public static bool IsCompleted(int statusId) => CompletedStatusIds.Contains(statusId);
    public static bool IsCancelled(int statusId) => statusId < 0 || CancelledStatusIds.Contains(statusId);

    public static string GetAdminStatusCss(int statusId)
    {
        if (IsCompleted(statusId)) return "status-chip status-success";
        if (IsCancelled(statusId)) return "status-chip status-failed";
        if (IsShipping(statusId)) return "status-chip status-info";
        return "status-chip status-pending";
    }

    public static string GetDashboardStatusCss(int statusId)
    {
        if (IsPending(statusId)) return "status-pending";
        if (IsCompleted(statusId)) return "status-success";
        if (IsCancelled(statusId)) return "status-failed";
        return "status-info";
    }

    public static string GetUserHistoryBadgeCss(int statusId)
    {
        if (IsCompleted(statusId)) return "bg-success-light text-success";
        if (IsCancelled(statusId)) return "bg-danger-light text-danger";
        if (IsShipping(statusId)) return "bg-info-light text-info";
        return "bg-warning-light text-warning";
    }

    public static string GetUserHistoryIcon(int statusId)
    {
        if (IsCompleted(statusId)) return "check-circle";
        if (IsCancelled(statusId)) return "x";
        if (IsShipping(statusId)) return "truck";
        return "clock";
    }
}
