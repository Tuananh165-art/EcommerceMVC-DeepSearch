using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using ECommerceMVC.Data;
using ECommerceMVC.Helpers;

namespace ECommerceMVC.Controllers
{
    public class HangHoasController : Controller
    {
        private readonly Hshop2023Context _context;

        public HangHoasController(Hshop2023Context context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var customerId = HttpContext.Session.Get<string>(MySetting.CUSTOMER_KEY);
            if (string.IsNullOrWhiteSpace(customerId))
            {
                context.Result = RedirectToAction("DangNhap", "KhachHang", new { returnUrl = Request.Path + Request.QueryString });
                return;
            }

            var customer = _context.KhachHangs.FirstOrDefault(x => x.MaKh == customerId);
            if (customer == null || !customer.HieuLuc || customer.VaiTro != MySetting.ADMIN_ROLE)
            {
                context.Result = RedirectToAction("DangNhap", "KhachHang", new { returnUrl = Request.Path + Request.QueryString });
                return;
            }

            base.OnActionExecuting(context);
        }

        // GET: HangHoas
        public async Task<IActionResult> Index()
        {
            var hshop2023Context = _context.HangHoas.Include(h => h.MaLoaiNavigation).Include(h => h.MaNccNavigation);
            return View(await hshop2023Context.ToListAsync());
        }

        // GET: HangHoas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hangHoa = await _context.HangHoas
                .Include(h => h.MaLoaiNavigation)
                .Include(h => h.MaNccNavigation)
                .FirstOrDefaultAsync(m => m.MaHh == id);
            if (hangHoa == null)
            {
                return NotFound();
            }

            return View(hangHoa);
        }

        // GET: HangHoas/Create
        public IActionResult Create()
        {
            ViewData["MaLoai"] = new SelectList(_context.Loais, "MaLoai", "TenLoai");
            ViewData["MaNcc"] = new SelectList(_context.NhaCungCaps, "MaNcc", "TenCongTy");
            return View();
        }

        // POST: HangHoas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaHh,TenHh,TenAlias,MaLoai,MoTaDonVi,DonGia,Hinh,NgaySx,GiamGia,SoLanXem,SoLuongTon,MoTa,MaNcc")] HangHoa hangHoa, IFormFile? hinhUpload)
        {
            if (hinhUpload != null)
            {
                var fileName = MyUtil.UploadHinh(hinhUpload, "HangHoa");
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    hangHoa.Hinh = fileName;
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(hangHoa);
                await _context.SaveChangesAsync();
                return RedirectToAction("Detail", "HangHoa", new { id = hangHoa.MaHh });
            }
            ViewData["MaLoai"] = new SelectList(_context.Loais, "MaLoai", "TenLoai", hangHoa.MaLoai);
            ViewData["MaNcc"] = new SelectList(_context.NhaCungCaps, "MaNcc", "TenCongTy", hangHoa.MaNcc);
            return View(hangHoa);
        }

        // GET: HangHoas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hangHoa = await _context.HangHoas.FindAsync(id);
            if (hangHoa == null)
            {
                return NotFound();
            }
            ViewData["MaLoai"] = new SelectList(_context.Loais, "MaLoai", "MaLoai", hangHoa.MaLoai);
            ViewData["MaNcc"] = new SelectList(_context.NhaCungCaps, "MaNcc", "MaNcc", hangHoa.MaNcc);
            return View(hangHoa);
        }

        // POST: HangHoas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaHh,TenHh,TenAlias,MaLoai,MoTaDonVi,DonGia,Hinh,NgaySx,GiamGia,SoLanXem,SoLuongTon,MoTa,MaNcc")] HangHoa hangHoa, IFormFile? hinhUpload)
        {
            if (id != hangHoa.MaHh)
            {
                return NotFound();
            }

            if (hinhUpload != null)
            {
                var fileName = MyUtil.UploadHinh(hinhUpload, "HangHoa");
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    hangHoa.Hinh = fileName;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hangHoa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HangHoaExists(hangHoa.MaHh))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaLoai"] = new SelectList(_context.Loais, "MaLoai", "MaLoai", hangHoa.MaLoai);
            ViewData["MaNcc"] = new SelectList(_context.NhaCungCaps, "MaNcc", "MaNcc", hangHoa.MaNcc);
            return View(hangHoa);
        }

        // GET: HangHoas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hangHoa = await _context.HangHoas
                .Include(h => h.MaLoaiNavigation)
                .Include(h => h.MaNccNavigation)
                .FirstOrDefaultAsync(m => m.MaHh == id);
            if (hangHoa == null)
            {
                return NotFound();
            }

            return View(hangHoa);
        }

        // POST: HangHoas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hangHoa = await _context.HangHoas.FindAsync(id);
            if (hangHoa != null)
            {
                _context.HangHoas.Remove(hangHoa);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SyncImagesByAlias()
        {
            var imageDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", "HangHoa");
            if (!Directory.Exists(imageDir))
            {
                TempData["SyncError"] = "Không tìm thấy thư mục ảnh /wwwroot/Hinh/HangHoa.";
                return RedirectToAction(nameof(Index));
            }

            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"
            };

            var invalidFiles = 0;
            var tooLongFiles = 0;
            var ambiguousAliases = 0;
            var unknownFiles = 0;
            var unchanged = 0;
            var updated = 0;

            var filesByAlias = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var duplicateAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var fullPath in Directory.GetFiles(imageDir))
            {
                var fileName = Path.GetFileName(fullPath);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    invalidFiles++;
                    continue;
                }

                var extension = Path.GetExtension(fileName);
                if (!allowedExtensions.Contains(extension))
                {
                    invalidFiles++;
                    continue;
                }

                if (fileName.Length > 50)
                {
                    tooLongFiles++;
                    continue;
                }

                var alias = Path.GetFileNameWithoutExtension(fileName)?.Trim();
                if (string.IsNullOrWhiteSpace(alias))
                {
                    invalidFiles++;
                    continue;
                }

                if (filesByAlias.ContainsKey(alias))
                {
                    duplicateAliases.Add(alias);
                }
                else
                {
                    filesByAlias[alias] = fileName;
                }
            }

            foreach (var duplicateAlias in duplicateAliases)
            {
                filesByAlias.Remove(duplicateAlias);
            }
            ambiguousAliases = duplicateAliases.Count;

            var products = await _context.HangHoas.ToListAsync();
            foreach (var product in products)
            {
                var alias = product.TenAlias?.Trim();
                if (string.IsNullOrWhiteSpace(alias))
                {
                    continue;
                }

                if (filesByAlias.TryGetValue(alias, out var fileName))
                {
                    if (!string.Equals(product.Hinh, fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        product.Hinh = fileName;
                        updated++;
                    }
                    else
                    {
                        unchanged++;
                    }
                }
            }

            var aliasSet = new HashSet<string>(
                products.Where(x => !string.IsNullOrWhiteSpace(x.TenAlias)).Select(x => x.TenAlias!.Trim()),
                StringComparer.OrdinalIgnoreCase
            );
            unknownFiles = filesByAlias.Keys.Count(alias => !aliasSet.Contains(alias));

            if (updated > 0)
            {
                await _context.SaveChangesAsync();
            }

            TempData["SyncSuccess"] = $"Đồng bộ ảnh hoàn tất. Cập nhật: {updated}, Giữ nguyên: {unchanged}, File không khớp alias: {unknownFiles}, Alias trùng file: {ambiguousAliases}, File quá dài: {tooLongFiles}, File không hợp lệ: {invalidFiles}.";
            return RedirectToAction(nameof(Index));
        }

        private bool HangHoaExists(int id)
        {
            return _context.HangHoas.Any(e => e.MaHh == id);
        }
    }
}
