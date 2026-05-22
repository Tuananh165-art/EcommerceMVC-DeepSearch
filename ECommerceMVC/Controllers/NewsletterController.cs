using ECommerceMVC.Data;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceMVC.Controllers;

public class NewsletterController : Controller
{
    private readonly Hshop2023Context db;

    public NewsletterController(Hshop2023Context context)
    {
        db = context;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Subscribe(NewsletterSubscribeVM model)
    {
        model.Email = (model.Email ?? string.Empty).Trim().ToLowerInvariant();

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Email không hợp lệ.";
            return RedirectToSafe(model.ReturnUrl);
        }

        var exists = db.NewsletterSubscriptions.Any(x => x.Email == model.Email);
        if (exists)
        {
            TempData["ErrorMessage"] = "Email này đã đăng ký trước đó.";
            return RedirectToSafe(model.ReturnUrl);
        }

        db.NewsletterSubscriptions.Add(new NewsletterSubscription
        {
            Email = model.Email,
            CreatedAt = DateTime.Now,
            IsActive = true
        });
        db.SaveChanges();

        TempData["SuccessMessage"] = "Đăng ký nhận tin thành công.";
        return RedirectToSafe(model.ReturnUrl);
    }

    private IActionResult RedirectToSafe(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "HangHoa");
    }
}