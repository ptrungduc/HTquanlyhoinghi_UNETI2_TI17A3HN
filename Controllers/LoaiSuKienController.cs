using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers
{
    public class LoaiSuKienController : Controller
    {
        private readonly AppDbContext _context;

        public LoaiSuKienController(AppDbContext context)
        {
            _context = context;
        }

        private bool LaAdmin()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");

            return vaiTro == "Admin";
        }

        public async Task<IActionResult> Index(
            string? search,
            string? trangThai,
            string sortBy = "TenLoaiSuKien",
            bool sortDescending = false,
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (!LaAdmin())
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 5, 50);

            var query = _context.LoaiSuKiens.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    x.MaLoaiSuKien.Contains(search)
                    || x.TenLoaiSuKien.Contains(search)
                    || x.MoTa.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(trangThai)
                && bool.TryParse(trangThai, out var isActive))
            {
                query = query.Where(x => x.TrangThai == isActive);
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

            var danhSach = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(danhSach);
        }

        private static IQueryable<LoaiSuKien> ApplySort(
            IQueryable<LoaiSuKien> query,
            string sortBy,
            bool sortDescending)
        {
            return sortBy switch
            {
                "MaLoaiSuKien" => sortDescending
                    ? query.OrderByDescending(x => x.MaLoaiSuKien)
                    : query.OrderBy(x => x.MaLoaiSuKien),
                "TrangThai" => sortDescending
                    ? query.OrderByDescending(x => x.TrangThai)
                    : query.OrderBy(x => x.TrangThai),
                _ => sortDescending
                    ? query.OrderByDescending(x => x.TenLoaiSuKien)
                    : query.OrderBy(x => x.TenLoaiSuKien)
            };
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!LaAdmin())
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoaiSuKien model)
        {
            if (!LaAdmin())
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            bool trungMa = await _context.LoaiSuKiens
                .AnyAsync(x => x.MaLoaiSuKien == model.MaLoaiSuKien);

            if (trungMa)
            {
                ModelState.AddModelError(
                    "MaLoaiSuKien",
                    "Mã loại sự kiện đã tồn tại.");
            }

            bool trungTen = await _context.LoaiSuKiens
                .AnyAsync(x => x.TenLoaiSuKien == model.TenLoaiSuKien);

            if (trungTen)
            {
                ModelState.AddModelError(
                    "TenLoaiSuKien",
                    "Tên loại sự kiện đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.TrangThai = true;

            _context.LoaiSuKiens.Add(model);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (!LaAdmin())
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var loaiSuKien = await _context.LoaiSuKiens
                .FirstOrDefaultAsync(x => x.MaLoaiSuKien == id);

            if (loaiSuKien == null)
            {
                return NotFound();
            }

            return View(loaiSuKien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            LoaiSuKien model)
        {
            if (!LaAdmin())
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            if (id != model.MaLoaiSuKien)
            {
                return NotFound();
            }

            bool trungTen = await _context.LoaiSuKiens
                .AnyAsync(x =>
                    x.TenLoaiSuKien == model.TenLoaiSuKien
                    && x.MaLoaiSuKien != model.MaLoaiSuKien);

            if (trungTen)
            {
                ModelState.AddModelError(
                    "TenLoaiSuKien",
                    "Tên loại sự kiện đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var loaiSuKien = await _context.LoaiSuKiens
                .FirstOrDefaultAsync(x =>
                    x.MaLoaiSuKien == id);

            if (loaiSuKien == null)
            {
                return NotFound();
            }

            loaiSuKien.TenLoaiSuKien = model.TenLoaiSuKien;
            loaiSuKien.MoTa = model.MoTa;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NgungHoatDong(string id)
        {
            if (!LaAdmin())
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var loaiSuKien = await _context.LoaiSuKiens
                .FirstOrDefaultAsync(x =>
                    x.MaLoaiSuKien == id);

            if (loaiSuKien == null)
            {
                return NotFound();
            }

            loaiSuKien.TrangThai = false;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KichHoat(string id)
        {
            if (!LaAdmin())
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var loaiSuKien = await _context.LoaiSuKiens
                .FirstOrDefaultAsync(x =>
                    x.MaLoaiSuKien == id);

            if (loaiSuKien == null)
            {
                return NotFound();
            }

            loaiSuKien.TrangThai = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}