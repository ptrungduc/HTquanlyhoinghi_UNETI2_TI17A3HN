using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers
{
    public class PhienSuKienController : Controller
    {
        private readonly AppDbContext _context;

        public PhienSuKienController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string maSuKien, string? search)
        {
            var suKien = await _context.SuKiens.AsNoTracking().FirstOrDefaultAsync(s => s.MaSuKien == maSuKien);
            if (suKien == null)
            {
                return NotFound();
            }

            var query = _context.PhienSuKiens
                .Where(p => p.MaSuKien == maSuKien)
                .AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(p => p.TenPhien.Contains(search) || p.DienGia.Contains(search) || p.NoiDung.Contains(search));
            }

            ViewBag.SuKien = suKien;
            ViewBag.Search = search;
            return View(await query.OrderBy(p => p.ThoiGianBatDau).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Create(string maSuKien)
        {
            var suKien = await _context.SuKiens.AsNoTracking().FirstOrDefaultAsync(s => s.MaSuKien == maSuKien);
            if (suKien == null)
            {
                return NotFound();
            }

            ViewBag.SuKien = suKien;
            return View(new PhienSuKien
            {
                MaPhienSuKien = CreateCode(),
                MaSuKien = maSuKien,
                ThoiGianBatDau = suKien.ThoiGianBatDau,
                ThoiGianKetThuc = suKien.ThoiGianKetThuc
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhienSuKien phien)
        {
            var suKien = await _context.SuKiens.AsNoTracking().FirstOrDefaultAsync(s => s.MaSuKien == phien.MaSuKien);
            if (suKien == null)
            {
                return NotFound();
            }

            ValidateTimeRange(phien, suKien);
            if (!ModelState.IsValid)
            {
                ViewBag.SuKien = suKien;
                return View(phien);
            }

            _context.Add(phien);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { maSuKien = phien.MaSuKien });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var phien = await _context.PhienSuKiens.FindAsync(id);
            if (phien == null)
            {
                return NotFound();
            }

            ViewBag.SuKien = await _context.SuKiens.AsNoTracking().FirstAsync(s => s.MaSuKien == phien.MaSuKien);
            return View(phien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, PhienSuKien phien)
        {
            if (id != phien.MaPhienSuKien)
            {
                return NotFound();
            }

            var suKien = await _context.SuKiens.AsNoTracking().FirstOrDefaultAsync(s => s.MaSuKien == phien.MaSuKien);
            if (suKien == null)
            {
                return NotFound();
            }

            ValidateTimeRange(phien, suKien);
            if (!ModelState.IsValid)
            {
                ViewBag.SuKien = suKien;
                return View(phien);
            }

            _context.Update(phien);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { maSuKien = phien.MaSuKien });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id, string maSuKien)
        {
            var phien = await _context.PhienSuKiens.FindAsync(id);
            if (phien != null)
            {
                _context.PhienSuKiens.Remove(phien);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { maSuKien });
        }

        private void ValidateTimeRange(PhienSuKien phien, SuKien suKien)
        {
            if (phien.ThoiGianKetThuc <= phien.ThoiGianBatDau)
            {
                ModelState.AddModelError(nameof(PhienSuKien.ThoiGianKetThuc), "Thời gian kết thúc phải sau thời gian bắt đầu.");
            }

            if (phien.ThoiGianBatDau < suKien.ThoiGianBatDau || phien.ThoiGianKetThuc > suKien.ThoiGianKetThuc)
            {
                ModelState.AddModelError(string.Empty, "Thời gian phiên phải nằm trong thời gian của sự kiện.");
            }
        }

        private static string CreateCode() => $"PS{Guid.NewGuid():N}"[..18].ToUpperInvariant();
    }
}
