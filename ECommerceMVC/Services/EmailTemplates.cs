namespace ECommerceMVC.Services;

public static class EmailTemplates
{
    public static string BuildRegisterSuccess(string fullName, string username, bool isAdmin)
    {
        return $"<p>Xin chào {fullName},</p><p>Bạn đã đăng ký tài khoản thành công tại DEEPSEARCH.</p><p>Tên đăng nhập: <strong>{username}</strong></p><p>Vai trò tài khoản: <strong>{(isAdmin ? "Admin" : "Khách hàng")}</strong></p>";
    }

    public static string BuildLoginNotice(string fullName, string username, DateTime loginTime)
    {
        return $"<p>Xin chào {fullName},</p><p>Tài khoản của bạn vừa đăng nhập thành công vào DEEPSEARCH.</p><p>Thời gian: <strong>{loginTime:dd/MM/yyyy HH:mm:ss}</strong></p><p>Tài khoản: <strong>{username}</strong></p>";
    }

    public static string BuildCheckoutSuccess(string fullName, int orderId, string paymentStatus, string paymentSummary, string orderLines, double subtotal, double shippingFee, double total)
    {
        return $"<p>Xin chào {fullName},</p><p>Đơn hàng <strong>#{orderId}</strong> đã được tạo thành công.</p><p>Trạng thái thanh toán: <strong>{paymentStatus}</strong></p><p>Phương thức: <strong>{paymentSummary}</strong></p><ul>{orderLines}</ul><p>Tạm tính: {subtotal:N0} VND</p><p>Phí vận chuyển: {shippingFee:N0} VND</p><p><strong>Tổng thanh toán: {total:N0} VND</strong></p><p>Cảm ơn bạn đã mua sắm tại DEEPSEARCH.</p>";
    }
}
