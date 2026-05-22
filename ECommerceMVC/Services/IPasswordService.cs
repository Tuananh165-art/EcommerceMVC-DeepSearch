using ECommerceMVC.Data;

namespace ECommerceMVC.Services;

public interface IPasswordService
{
    void SetPassword(KhachHang user, string password);
    bool VerifyPassword(KhachHang user, string password, out bool needsUpgrade);
}
