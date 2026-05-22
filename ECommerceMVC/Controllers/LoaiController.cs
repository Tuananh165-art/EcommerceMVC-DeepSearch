using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceMVC.Controllers
{
    public class LoaiController : Controller
    {
        private readonly Hshop2023Context _db;

        public LoaiController(Hshop2023Context db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult UploadHinh(int id)
        {
            var loai = _db.Loais.SingleOrDefault(x => x.MaLoai == id);
            if (loai == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy loại hàng.";
                return RedirectToAction("Index", "HangHoa");
            }

            ViewBag.MaLoai = loai.MaLoai;
            ViewBag.TenLoai = loai.TenLoai;
            ViewBag.HinhHienTai = MyUtil.GetLoaiImageUrl(loai.Hinh, loai.MaLoai);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UploadHinh(int id, IFormFile? hinh)
        {
            var loai = _db.Loais.SingleOrDefault(x => x.MaLoai == id);
            if (loai == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy loại hàng.";
                return RedirectToAction("Index", "HangHoa");
            }

            if (hinh == null || hinh.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file ảnh.";
                return RedirectToAction(nameof(UploadHinh), new { id });
            }

            var fileName = MyUtil.UploadHinh(hinh, "Loai");
            if (string.IsNullOrWhiteSpace(fileName))
            {
                TempData["ErrorMessage"] = "Upload ảnh thất bại, vui lòng thử lại.";
                return RedirectToAction(nameof(UploadHinh), new { id });
            }

            loai.Hinh = fileName;
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Đã cập nhật ảnh cho loại '{loai.TenLoai}'.";
            return RedirectToAction("Index", "HangHoa", new { loai = id });
        }
    }
}
