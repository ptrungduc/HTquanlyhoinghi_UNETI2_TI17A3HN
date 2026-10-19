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

        private IActionResult? KiemTraQuyenQuanLy()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("MaTaiKhoan")))
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (!string.Equals(vaiTro, "Admin", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(vaiTro, "BanToChuc", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(vaiTro, "Ban tổ chức", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            return null;
        }

        public async Task<IActionResult> Index(
            string? search,
            string? trangThai,
            string? maLoaiSuKien,
            string? maDiaDiem,
            DateTime? ngayBatDauTu,
            DateTime? ngayBatDauDen,
            string? tinhTrangDangKy,
            int? soLuongTu,
            int? soLuongDen,
            string sortBy = "ThoiGianBatDau",
            bool sortDescending = true,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var ketQuaPhanQuyen = KiemTraQuyenQuanLy();
            if (ketQuaPhanQuyen != null)
            {
                return ketQuaPhanQuyen;
            }

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 5, 50);
            var thoiDiemHienTai = DateTime.Now;

            IQueryable<SuKien> query = _context.SuKiens
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
                    || (s.DiaDiem != null && s.DiaDiem.TenDiaDiem.Contains(search))
                    || s.PhienSuKiens.Any(p => p.TenPhien.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(s => s.TrangThai == trangThai);
            }

            if (!string.IsNullOrWhiteSpace(maLoaiSuKien))
            {
                query = query.Where(s => s.MaLoaiSuKien == maLoaiSuKien);
            }

            if (!string.IsNullOrWhiteSpace(maDiaDiem))
            {
                query = query.Where(s => s.MaDiaDiem == maDiaDiem);
            }

            if (ngayBatDauTu.HasValue)
            {
                var ngayTu = ngayBatDauTu.Value.Date;
                query = query.Where(s => s.ThoiGianKetThuc.Date >= ngayTu);
            }

            if (ngayBatDauDen.HasValue)
            {
                var ngayDen = ngayBatDauDen.Value.Date;
                query = query.Where(s => s.ThoiGianBatDau.Date <= ngayDen);
            }

            if (string.Equals(tinhTrangDangKy, "ConHan", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.HanDangKy > thoiDiemHienTai);
            }
            else if (string.Equals(tinhTrangDangKy, "HetHan", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.HanDangKy <= thoiDiemHienTai);
            }

            if (soLuongTu.HasValue)
            {
                query = query.Where(s => s.SoLuongToiDa >= soLuongTu.Value);
            }

            if (soLuongDen.HasValue)
            {
                query = query.Where(s => s.SoLuongToiDa <= soLuongDen.Value);
            }

            query = ApplySort(query, sortBy, sortDescending);
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            pageNumber = Math.Clamp(pageNumber, 1, Math.Max(1, totalPages));

            ViewBag.Search = search;
            ViewBag.TrangThai = trangThai;
            ViewBag.MaLoaiSuKien = maLoaiSuKien;
            ViewBag.MaDiaDiem = maDiaDiem;
            ViewBag.NgayBatDauTu = ngayBatDauTu?.ToString("yyyy-MM-dd");
            ViewBag.NgayBatDauDen = ngayBatDauDen?.ToString("yyyy-MM-dd");
            ViewBag.TinhTrangDangKy = tinhTrangDangKy;
            ViewBag.SoLuongTu = soLuongTu;
            ViewBag.SoLuongDen = soLuongDen;
            ViewBag.LoaiSuKien = new SelectList(
                await _context.LoaiSuKiens.AsNoTracking().OrderBy(l => l.TenLoaiSuKien).ToListAsync(),
                "MaLoaiSuKien", "TenLoaiSuKien", maLoaiSuKien);
            ViewBag.DiaDiem = new SelectList(
                await _context.DiaDiems.AsNoTracking().OrderBy(d => d.TenDiaDiem).ToListAsync(),
                "MaDiaDiem", "TenDiaDiem", maDiaDiem);
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
            var ketQuaPhanQuyen = KiemTraQuyenQuanLy();
            if (ketQuaPhanQuyen != null)
            {
                return ketQuaPhanQuyen;
            }

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
                    || p.NoiDung.Contains(search)
                    || p.PhongKhuVuc.Contains(search));
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

        [HttpGet]
        public async Task<IActionResult> CreatePhien(string maSuKien)
        {
            var ketQuaPhanQuyen = KiemTraQuyenQuanLy();
            if (ketQuaPhanQuyen != null)
            {
                return ketQuaPhanQuyen;
            }

            var suKien = await _context.SuKiens
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MaSuKien == maSuKien);

            if (suKien == null)
            {
                return NotFound();
            }

            if (suKien.TrangThai == nameof(TrangThaiSuKien.HoanThanh))
            {
                TempData["ThongBao"] = "Không thể thêm phiên cho sự kiện đã hoàn thành.";
                return RedirectToAction(nameof(Phien), new { maSuKien });
            }

            ViewBag.SuKien = suKien;
            return View(new PhienSuKien
            {
                MaSuKien = suKien.MaSuKien,
                ThoiGianBatDau = suKien.ThoiGianBatDau,
                ThoiGianKetThuc = suKien.ThoiGianKetThuc,
                SoLuongToiDa = suKien.SoLuongToiDa
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePhien(string maSuKien, PhienSuKien phien)
        {
            var ketQuaPhanQuyen = KiemTraQuyenQuanLy();
            if (ketQuaPhanQuyen != null)
            {
                return ketQuaPhanQuyen;
            }

            var suKien = await _context.SuKiens
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MaSuKien == maSuKien);

            if (suKien == null || phien.MaSuKien != maSuKien)
            {
                return NotFound();
            }

            if (suKien.TrangThai == nameof(TrangThaiSuKien.HoanThanh))
            {
                TempData["ThongBao"] = "Không thể thêm phiên cho sự kiện đã hoàn thành.";
                return RedirectToAction(nameof(Phien), new { maSuKien });
            }

            ValidatePhienTimeRange(phien, suKien);
            await ValidatePhienCapacityAndRoomAsync(phien, suKien);
            ModelState.Remove(nameof(PhienSuKien.MaPhienSuKien));
            phien.MaPhienSuKien = CreateCode("PSK");
            if (!ModelState.IsValid)
            {
                ViewBag.SuKien = suKien;
                return View(phien);
            }

            _context.PhienSuKiens.Add(phien);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Phien), new { maSuKien });
        }

        [HttpGet]
        public async Task<IActionResult> EditPhien(string maSuKien, string id)
        {
            var ketQuaPhanQuyen = KiemTraQuyenQuanLy();
            if (ketQuaPhanQuyen != null)
            {
                return ketQuaPhanQuyen;
            }

            var phien = await _context.PhienSuKiens
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.MaPhienSuKien == id && p.MaSuKien == maSuKien);
            var suKien = await _context.SuKiens
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MaSuKien == maSuKien);

            if (phien == null || suKien == null)
            {
                return NotFound();
            }

            if (suKien.TrangThai == nameof(TrangThaiSuKien.HoanThanh))
            {
                TempData["ThongBao"] = "Không thể sửa phiên của sự kiện đã hoàn thành.";
                return RedirectToAction(nameof(Phien), new { maSuKien });
            }

            ViewBag.SuKien = suKien;
            return View(phien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPhien(string maSuKien, string id, PhienSuKien phien)
        {
            var ketQuaPhanQuyen = KiemTraQuyenQuanLy();
            if (ketQuaPhanQuyen != null)
            {
                return ketQuaPhanQuyen;
            }

            if (id != phien.MaPhienSuKien || phien.MaSuKien != maSuKien)
            {
                return NotFound();
            }

            var existingPhien = await _context.PhienSuKiens
                .FirstOrDefaultAsync(p => p.MaPhienSuKien == id && p.MaSuKien == maSuKien);
            var suKien = await _context.SuKiens
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MaSuKien == maSuKien);

            if (existingPhien == null || suKien == null)
            {
                return NotFound();
            }

            if (suKien.TrangThai == nameof(TrangThaiSuKien.HoanThanh))
            {
                TempData["ThongBao"] = "Không thể sửa phiên của sự kiện đã hoàn thành.";
                return RedirectToAction(nameof(Phien), new { maSuKien });
            }

            ValidatePhienTimeRange(phien, suKien);
            await ValidatePhienCapacityAndRoomAsync(phien, suKien, id);
            if (await _context.DangKyPhiens.AnyAsync(d => d.MaPhien == id)
                && (existingPhien.ThoiGianBatDau != phien.ThoiGianBatDau
                    || existingPhien.ThoiGianKetThuc != phien.ThoiGianKetThuc
                    || !string.Equals(existingPhien.PhongKhuVuc, phien.PhongKhuVuc, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(
                    nameof(PhienSuKien.ThoiGianBatDau),
                    "Không thể đổi lịch hoặc phòng của phiên đã phát sinh đăng ký.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.SuKien = suKien;
                return View(phien);
            }

            existingPhien.TenPhien = phien.TenPhien;
            existingPhien.ThoiGianBatDau = phien.ThoiGianBatDau;
            existingPhien.ThoiGianKetThuc = phien.ThoiGianKetThuc;
            existingPhien.PhongKhuVuc = phien.PhongKhuVuc.Trim();
            existingPhien.SoLuongToiDa = phien.SoLuongToiDa;
            existingPhien.DienGia = phien.DienGia;
            existingPhien.NoiDung = phien.NoiDung;
            existingPhien.TrangThai = phien.TrangThai;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Phien), new { maSuKien });
        }

        [HttpGet]
        public async Task<IActionResult> DeletePhien(string maSuKien, string id)
        {
            var ketQuaPhanQuyen = KiemTraQuyenQuanLy();
            if (ketQuaPhanQuyen != null)
            {
                return ketQuaPhanQuyen;
            }

            var phien = await _context.PhienSuKiens
                .Include(p => p.SuKien)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.MaPhienSuKien == id && p.MaSuKien == maSuKien);

            return phien == null ? NotFound() : View(phien);
        }

        [HttpPost, ActionName("DeletePhien")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePhienConfirmed(string maSuKien, string id)
        {
            var ketQuaPhanQuyen = KiemTraQuyenQuanLy();
            if (ketQuaPhanQuyen != null)
            {
                return ketQuaPhanQuyen;
            }

            var phien = await _context.PhienSuKiens
                .FirstOrDefaultAsync(p => p.MaPhienSuKien == id && p.MaSuKien == maSuKien);

            if (phien == null)
            {
                return NotFound();
            }

            if (await _context.DangKyPhiens.AnyAsync(d => d.MaPhien == id))
            {
                TempData["ThongBao"] = "Không thể xóa phiên đã có người đăng ký.";
                return RedirectToAction(nameof(Phien), new { maSuKien });
            }

            _context.PhienSuKiens.Remove(phien);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Phien), new { maSuKien });
        }

        public async Task<IActionResult> Details(string id)
        {
            var ketQuaPhanQuyen = KiemTraQuyenQuanLy();
            if (ketQuaPhanQuyen != null)
            {
                return ketQuaPhanQuyen;
            }

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
            var ketQuaPhanQuyen = KiemTraQuyenQuanLy();
            if (ketQuaPhanQuyen != null)
            {
                return ketQuaPhanQuyen;
            }

            await LoadSelections();
            return View(new SuKien
            {
                MaSuKien = CreateCode("SK"),
                ThoiGianBatDau = DateTime.Now.AddDays(1),
                ThoiGianKetThuc = DateTime.Now.AddDays(1).AddHours(1),
                HanDangKy = DateTime.Now,
                SoLuongToiDa = 1
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SuKien suKien)
        {
            var ketQuaPhanQuyen = KiemTraQuyenQuanLy();
            if (ketQuaPhanQuyen != null)
            {
                return ketQuaPhanQuyen;
            }

            await ValidateSuKienAsync(suKien);
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
            var ketQuaPhanQuyen = KiemTraQuyenQuanLy();
            if (ketQuaPhanQuyen != null)
            {
                return ketQuaPhanQuyen;
            }

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
            var ketQuaPhanQuyen = KiemTraQuyenQuanLy();
            if (ketQuaPhanQuyen != null)
            {
                return ketQuaPhanQuyen;
            }

            if (id != suKien.MaSuKien)
            {
                return NotFound();
            }

            await ValidateSuKienAsync(suKien, id);

            if (await _context.PhienSuKiens.AnyAsync(p =>
                    p.MaSuKien == id
                    && (p.ThoiGianBatDau < suKien.ThoiGianBatDau
                        || p.ThoiGianKetThuc > suKien.ThoiGianKetThuc)))
            {
                ModelState.AddModelError(
                    nameof(SuKien.ThoiGianBatDau),
                    "Khoảng thời gian sự kiện không được loại bỏ các phiên đã có.");
            }

            if (!ModelState.IsValid)
            {
                await LoadSelections();
                return View(suKien);
            }

            var existingSuKien = await _context.SuKiens
                .FirstOrDefaultAsync(s => s.MaSuKien == id);
            if (existingSuKien == null)
            {
                return NotFound();
            }

            existingSuKien.TenSuKien = suKien.TenSuKien;
            existingSuKien.MaLoaiSuKien = suKien.MaLoaiSuKien;
            existingSuKien.MaDiaDiem = suKien.MaDiaDiem;
            existingSuKien.ThoiGianBatDau = suKien.ThoiGianBatDau;
            existingSuKien.ThoiGianKetThuc = suKien.ThoiGianKetThuc;
            existingSuKien.HanDangKy = suKien.HanDangKy;
            existingSuKien.SoLuongToiDa = suKien.SoLuongToiDa;
            existingSuKien.MoTa = suKien.MoTa;
            existingSuKien.TrangThai = suKien.TrangThai;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var ketQuaPhanQuyen = KiemTraQuyenQuanLy();
            if (ketQuaPhanQuyen != null)
            {
                return ketQuaPhanQuyen;
            }

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
            var ketQuaPhanQuyen = KiemTraQuyenQuanLy();
            if (ketQuaPhanQuyen != null)
            {
                return ketQuaPhanQuyen;
            }

            var suKien = await _context.SuKiens.FindAsync(id);
            if (suKien == null)
            {
                return NotFound();
            }

            var maCacPhien = _context.PhienSuKiens
                .Where(p => p.MaSuKien == id)
                .Select(p => p.MaPhienSuKien);
            if (await _context.DangKyThamDus.AnyAsync(d => d.MaSuKien == id)
                || await _context.DangKyPhiens.AnyAsync(d => maCacPhien.Contains(d.MaPhien)))
            {
                TempData["ThongBao"] = "Không thể xóa sự kiện đã phát sinh đăng ký tham dự hoặc đăng ký phiên.";
                return RedirectToAction(nameof(Index));
            }

            _context.SuKiens.Remove(suKien);
            await _context.SaveChangesAsync();
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

        private async Task ValidateSuKienAsync(SuKien suKien, string? existingId = null)
        {
            ValidateTimeRange(suKien);
            if (suKien.HanDangKy > suKien.ThoiGianBatDau)
            {
                ModelState.AddModelError(
                    nameof(SuKien.HanDangKy),
                    "Hạn đăng ký không được sau thời điểm bắt đầu sự kiện.");
            }

            if (!Enum.TryParse<TrangThaiSuKien>(suKien.TrangThai, out var trangThai)
                || !Enum.IsDefined(trangThai))
            {
                ModelState.AddModelError(nameof(SuKien.TrangThai), "Trạng thái sự kiện không hợp lệ.");
            }

            if (!await IsReferenceDataActive(suKien.MaLoaiSuKien, suKien.MaDiaDiem))
            {
                ModelState.AddModelError(string.Empty, "Loại sự kiện và địa điểm phải đang hoạt động.");
                return;
            }

            var diaDiem = await _context.DiaDiems
                .AsNoTracking()
                .FirstAsync(d => d.MaDiaDiem == suKien.MaDiaDiem);
            if (suKien.SoLuongToiDa > diaDiem.SucChuaToiDa)
            {
                ModelState.AddModelError(
                    nameof(SuKien.SoLuongToiDa),
                    $"Số lượng tối đa không được vượt sức chứa địa điểm ({diaDiem.SucChuaToiDa}).");
            }

            if (existingId == null)
            {
                return;
            }

            var existingSuKien = await _context.SuKiens
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MaSuKien == existingId);
            if (existingSuKien != null
                && existingSuKien.MaDiaDiem != suKien.MaDiaDiem
                && await _context.PhienSuKiens.AnyAsync(p => p.MaSuKien == existingId))
            {
                ModelState.AddModelError(
                    nameof(SuKien.MaDiaDiem),
                    "Không thể đổi địa điểm khi sự kiện đã có phiên chương trình.");
            }

            var soDangKyHieuLuc = await _context.DangKyThamDus
                .CountAsync(d => d.MaSuKien == existingId
                    && d.TrangThai != TrangThaiDangKy.DaHuy
                    && d.TrangThai != TrangThaiDangKy.TuChoi);
            if (suKien.SoLuongToiDa < soDangKyHieuLuc)
            {
                ModelState.AddModelError(
                    nameof(SuKien.SoLuongToiDa),
                    $"Số lượng tối đa không được thấp hơn số đăng ký hiện có ({soDangKyHieuLuc}).");
            }
        }

        private async Task ValidatePhienCapacityAndRoomAsync(
            PhienSuKien phien,
            SuKien suKien,
            string? existingId = null)
        {
            phien.PhongKhuVuc = phien.PhongKhuVuc?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(phien.PhongKhuVuc))
            {
                ModelState.AddModelError(
                    nameof(PhienSuKien.PhongKhuVuc),
                    "Phòng/khu vực không được để trống.");
                return;
            }

            if (phien.SoLuongToiDa > suKien.SoLuongToiDa)
            {
                ModelState.AddModelError(
                    nameof(PhienSuKien.SoLuongToiDa),
                    "Số lượng tối đa của phiên không được vượt số lượng tối đa của sự kiện.");
            }

            if (existingId != null)
            {
                var soDangKyHieuLuc = await _context.DangKyPhiens
                    .CountAsync(d => d.MaPhien == existingId
                        && d.TrangThai != TrangThaiDangKy.DaHuy
                        && d.TrangThai != TrangThaiDangKy.TuChoi);
                if (phien.SoLuongToiDa < soDangKyHieuLuc)
                {
                    ModelState.AddModelError(
                        nameof(PhienSuKien.SoLuongToiDa),
                        $"Số lượng tối đa không được thấp hơn số đăng ký hiện có ({soDangKyHieuLuc}).");
                }
            }

            var queryPhienCungPhong = _context.PhienSuKiens
                .Where(p =>
                    p.SuKien != null
                    && p.SuKien.MaDiaDiem == suKien.MaDiaDiem
                    && p.PhongKhuVuc == phien.PhongKhuVuc
                    && phien.ThoiGianBatDau < p.ThoiGianKetThuc
                    && phien.ThoiGianKetThuc > p.ThoiGianBatDau);
            if (existingId != null)
            {
                queryPhienCungPhong = queryPhienCungPhong
                    .Where(p => p.MaPhienSuKien != existingId);
            }

            var phienTrungPhong = await queryPhienCungPhong.AnyAsync();
            if (phienTrungPhong)
            {
                ModelState.AddModelError(
                    nameof(PhienSuKien.PhongKhuVuc),
                    "Phòng/khu vực này đã có phiên khác trong cùng khoảng thời gian.");
            }

            var cacTrangThaiPhienHopLe = new[] { "SapDienRa", "DangDienRa", "DaKetThuc", "Ngung", "DaHuy" };
            if (!cacTrangThaiPhienHopLe.Contains(phien.TrangThai, StringComparer.Ordinal))
            {
                ModelState.AddModelError(nameof(PhienSuKien.TrangThai), "Trạng thái phiên không hợp lệ.");
            }
        }

        private static IQueryable<SuKien> ApplySort(IQueryable<SuKien> query, string sortBy, bool sortDescending)
        {
            return sortBy switch
            {
                "TenSuKien" => sortDescending
                    ? query.OrderByDescending(s => s.TenSuKien).ThenBy(s => s.MaSuKien)
                    : query.OrderBy(s => s.TenSuKien).ThenBy(s => s.MaSuKien),
                "HanDangKy" => sortDescending
                    ? query.OrderByDescending(s => s.HanDangKy).ThenBy(s => s.MaSuKien)
                    : query.OrderBy(s => s.HanDangKy).ThenBy(s => s.MaSuKien),
                "SoLuongToiDa" => sortDescending
                    ? query.OrderByDescending(s => s.SoLuongToiDa).ThenBy(s => s.MaSuKien)
                    : query.OrderBy(s => s.SoLuongToiDa).ThenBy(s => s.MaSuKien),
                "ThoiGianKetThuc" => sortDescending
                    ? query.OrderByDescending(s => s.ThoiGianKetThuc).ThenBy(s => s.MaSuKien)
                    : query.OrderBy(s => s.ThoiGianKetThuc).ThenBy(s => s.MaSuKien),
                _ => sortDescending
                    ? query.OrderByDescending(s => s.ThoiGianBatDau).ThenBy(s => s.MaSuKien)
                    : query.OrderBy(s => s.ThoiGianBatDau).ThenBy(s => s.MaSuKien)
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
            if (suKien.ThoiGianKetThuc < suKien.ThoiGianBatDau)
            {
                ModelState.AddModelError(nameof(SuKien.ThoiGianKetThuc), "Ngày kết thúc phải bằng hoặc sau ngày bắt đầu.");
            }
        }

        private void ValidatePhienTimeRange(PhienSuKien phien, SuKien suKien)
        {
            if (phien.ThoiGianKetThuc <= phien.ThoiGianBatDau)
            {
                ModelState.AddModelError(
                    nameof(PhienSuKien.ThoiGianKetThuc),
                    "Thời gian kết thúc phải sau thời gian bắt đầu.");
            }

            if (phien.ThoiGianBatDau < suKien.ThoiGianBatDau
                || phien.ThoiGianKetThuc > suKien.ThoiGianKetThuc)
            {
                ModelState.AddModelError(
                    nameof(PhienSuKien.ThoiGianBatDau),
                    "Thời gian phiên phải nằm trong khoảng thời gian của sự kiện.");
            }
        }

        private static string CreateCode(string prefix) => $"{prefix}{Guid.NewGuid():N}"[..18].ToUpperInvariant();
    }
}
