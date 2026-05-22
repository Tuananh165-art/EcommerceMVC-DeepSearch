using Microsoft.AspNetCore.Mvc;

namespace ECommerceMVC.Controllers
{
    public class ShowroomController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
