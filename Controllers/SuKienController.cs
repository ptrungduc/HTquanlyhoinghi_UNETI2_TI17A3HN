using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers
{
    public class SuKienController : Controller
    {
        private readonly AppDbContext _context;

        public SuKienController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? trangThai)
        {
            var query = _context.SuKiens
                .Include(s => s.LoaiSuKien)
                .Include(s => s.DiaDiem)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
<<<<<<< HEAD
                query = query.Where(s =>
                    s.TenSuKien.Contains(search)
                    || s.MoTa.Contains(search)
                    || (s.LoaiSuKien != null && s.LoaiSuKien.TenLoaiSuKien.Contains(search))
                    || (s.DiaDiem != null && s.DiaDiem.TenDiaDiem.Contains(search)));
=======
                query = query.Where(s => s.TenSuKien.Contains(search)
                    || s.MoTa.Contains(search)
                    || s.LoaiSuKien!.TenLoaiSuKien.Contains(search)
                    || s.DiaDiem!.TenDiaDiem.Contains(search));
>>>>>>> 19841cd (23103100176HoangQuocDai tao nut tim kiem va quan ly su kien tuan 1)
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(s => s.TrangThai == trangThai);
            }

            ViewBag.Search = search;
            ViewBag.TrangThai = trangThai;
            return View(await query.OrderByDescending(s => s.ThoiGianBatDau).ToListAsync());
        }

        public async Task<IActionResult> Details(string id)
        {
            var suKien = await _context.SuKiens
                .Include(s => s.LoaiSuKien)
                .Include(s => s.DiaDiem)
                .Include(s => s.PhienSuKiens.OrderBy(p => p.ThoiGianBatDau))
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MaSuKien == id);

            return suKien == null ? NotFound() : View(suKien);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadSelections();
            return View(new SuKien
            {
                MaSuKien = CreateCode("SK"),
                ThoiGianBatDau = DateTime.Now,
                ThoiGianKetThuc = DateTime.Now.AddHours(1)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SuKien suKien)
        {
            ValidateTimeRange(suKien);
            if (!await IsReferenceDataActive(suKien.MaLoaiSuKien, suKien.MaDiaDiem))
            {
                ModelState.AddModelError(string.Empty, "Loại sự kiện và địa điểm phải đang hoạt động.");
            }

            if (!ModelState.IsValid)
            {
                await LoadSelections();
                return View(suKien);
            }

            _context.Add(suKien);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var suKien = await _context.SuKiens.FindAsync(id);
            if (suKien == null)
            {
                return NotFound();
            }

            await LoadSelections();
            return View(suKien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, SuKien suKien)
        {
            if (id != suKien.MaSuKien)
            {
                return NotFound();
            }

            ValidateTimeRange(suKien);
            if (!await IsReferenceDataActive(suKien.MaLoaiSuKien, suKien.MaDiaDiem))
            {
                ModelState.AddModelError(string.Empty, "Loại sự kiện và địa điểm phải đang hoạt động.");
            }

            if (!ModelState.IsValid)
            {
                await LoadSelections();
                return View(suKien);
            }

            _context.Update(suKien);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var suKien = await _context.SuKiens
                .Include(s => s.LoaiSuKien)
                .Include(s => s.DiaDiem)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MaSuKien == id);

            return suKien == null ? NotFound() : View(suKien);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var suKien = await _context.SuKiens.FindAsync(id);
            if (suKien != null)
            {
                _context.SuKiens.Remove(suKien);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadSelections()
        {
            ViewBag.LoaiSuKien = new SelectList(
                await _context.LoaiSuKiens.Where(l => l.TrangThai).OrderBy(l => l.TenLoaiSuKien).ToListAsync(),
                "MaLoaiSuKien", "TenLoaiSuKien");
            ViewBag.DiaDiem = new SelectList(
                await _context.DiaDiems.Where(d => d.TrangThai).OrderBy(d => d.TenDiaDiem).ToListAsync(),
                "MaDiaDiem", "TenDiaDiem");
        }

        private async Task<bool> IsReferenceDataActive(string maLoaiSuKien, string maDiaDiem)
        {
            return await _context.LoaiSuKiens.AnyAsync(l => l.MaLoaiSuKien == maLoaiSuKien && l.TrangThai)
                && await _context.DiaDiems.AnyAsync(d => d.MaDiaDiem == maDiaDiem && d.TrangThai);
        }

        private void ValidateTimeRange(SuKien suKien)
        {
            if (suKien.ThoiGianKetThuc <= suKien.ThoiGianBatDau)
            {
                ModelState.AddModelError(nameof(SuKien.ThoiGianKetThuc), "Thời gian kết thúc phải sau thời gian bắt đầu.");
            }
        }

        private static string CreateCode(string prefix) => $"{prefix}{Guid.NewGuid():N}"[..18].ToUpperInvariant();
    }
}
