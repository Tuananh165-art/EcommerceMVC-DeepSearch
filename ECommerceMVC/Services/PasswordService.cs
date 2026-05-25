using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using Microsoft.AspNetCore.Identity;

namespace ECommerceMVC.Services;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<KhachHang> passwordHasher = new();

    public void SetPassword(KhachHang user, string password)
    {
        user.MatKhau = passwordHasher.HashPassword(user, password);
        user.RandomKey = null;
    }

    public void SetLegacyPassword(KhachHang user, string password)
    {
        var randomKey = MyUtil.GenerateRamdomKey();
        user.RandomKey = randomKey;
        user.MatKhau = password.ToMd5Hash(randomKey);
    }

    public bool VerifyPassword(KhachHang user, string password, out bool needsUpgrade)
    {
        needsUpgrade = false;

        if (string.IsNullOrWhiteSpace(user.MatKhau))
        {
            return false;
        }

        var verifyResult = passwordHasher.VerifyHashedPassword(user, user.MatKhau, password);
        if (verifyResult == PasswordVerificationResult.Success || verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            needsUpgrade = verifyResult == PasswordVerificationResult.SuccessRehashNeeded;
            return true;
        }

        if (string.IsNullOrWhiteSpace(user.RandomKey))
        {
            return false;
        }

        var legacyHash = password.ToMd5Hash(user.RandomKey);
        if (!string.Equals(legacyHash, user.MatKhau, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        needsUpgrade = true;
        return true;
    }
}
