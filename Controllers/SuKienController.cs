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

        public async Task<IActionResult> Index(
            string? search,
            string? trangThai,
            string sortBy = "ThoiGianBatDau",
            bool sortDescending = true,
            int pageNumber = 1,
            int pageSize = 10)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 5, 50);

            var query = _context.SuKiens
                .Include(s => s.LoaiSuKien)
                .Include(s => s.DiaDiem)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(s =>
                    s.TenSuKien.Contains(search)
                    || s.MoTa.Contains(search)
                    || (s.LoaiSuKien != null && s.LoaiSuKien.TenLoaiSuKien.Contains(search))
                    || (s.DiaDiem != null && s.DiaDiem.TenDiaDiem.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(s => s.TrangThai == trangThai);
            }

            query = ApplySort(query, sortBy, sortDescending);
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            pageNumber = Math.Clamp(pageNumber, 1, Math.Max(1, totalPages));

            ViewBag.Search = search;
            ViewBag.TrangThai = trangThai;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDescending = sortDescending;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            return View(await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync());
        }

        public async Task<IActionResult> Phien(
            string maSuKien,
            string? search,
            string? trangThai,
            string sortBy = "ThoiGianBatDau",
            bool sortDescending = false,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var suKien = await _context.SuKiens
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MaSuKien == maSuKien);

            if (suKien == null)
            {
                return NotFound();
            }

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 5, 50);

            var query = _context.PhienSuKiens
                .Where(p => p.MaSuKien == maSuKien)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(p =>
                    p.TenPhien.Contains(search)
                    || p.DienGia.Contains(search)
                    || p.NoiDung.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(p => p.TrangThai == trangThai);
            }

            query = ApplyPhienSort(query, sortBy, sortDescending);
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            pageNumber = Math.Clamp(pageNumber, 1, Math.Max(1, totalPages));

            ViewBag.SuKien = suKien;
            ViewBag.Search = search;
            ViewBag.TrangThai = trangThai;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDescending = sortDescending;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            return View("Phien", await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync());
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

        private static IQueryable<SuKien> ApplySort(IQueryable<SuKien> query, string sortBy, bool sortDescending)
        {
            return sortBy switch
            {
                "TenSuKien" => sortDescending
                    ? query.OrderByDescending(s => s.TenSuKien)
                    : query.OrderBy(s => s.TenSuKien),
                "ThoiGianKetThuc" => sortDescending
                    ? query.OrderByDescending(s => s.ThoiGianKetThuc)
                    : query.OrderBy(s => s.ThoiGianKetThuc),
                _ => sortDescending
                    ? query.OrderByDescending(s => s.ThoiGianBatDau)
                    : query.OrderBy(s => s.ThoiGianBatDau)
            };
        }

        private static IQueryable<PhienSuKien> ApplyPhienSort(
            IQueryable<PhienSuKien> query,
            string sortBy,
            bool sortDescending)
        {
            return sortBy switch
            {
                "TenPhien" => sortDescending
                    ? query.OrderByDescending(p => p.TenPhien)
                    : query.OrderBy(p => p.TenPhien),
                "ThoiGianKetThuc" => sortDescending
                    ? query.OrderByDescending(p => p.ThoiGianKetThuc)
                    : query.OrderBy(p => p.ThoiGianKetThuc),
                _ => sortDescending
                    ? query.OrderByDescending(p => p.ThoiGianBatDau)
                    : query.OrderBy(p => p.ThoiGianBatDau)
            };
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
