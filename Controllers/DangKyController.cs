using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers
{
    public class DangKyController : Controller
    {
        private readonly AppDbContext _context;
        private readonly DangKyService _dangKyService;

        public DangKyController(AppDbContext context, DangKyService dangKyService)
        {
            _context = context;
            _dangKyService = dangKyService;
        }

        public IActionResult DangKy(string maSuKien)
        {
            ViewBag.MaSuKien = maSuKien;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(string maNguoiThamDu, string maSuKien)
        {
            var (hopLe, lyDo) = await _dangKyService.KiemTraDieuKienDangKyAsync(
                maNguoiThamDu,
                maSuKien);

            if (!hopLe)
            {
                TempData["Loi"] = lyDo;
                return RedirectToAction("ChiTiet", "SuKien", new { id = maSuKien });
            }

            _context.DangKyThamDus.Add(new DangKyThamDu
            {
                MaDangKy = Guid.NewGuid().ToString("N"),
                MaNguoiThamDu = maNguoiThamDu,
                MaSuKien = maSuKien,
                NgayDangKy = DateTime.Now,
                TrangThai = TrangThaiDangKy.ChoDuyet
            });

            await _context.SaveChangesAsync();
            TempData["ThongBao"] = "Đăng ký thành công, vui lòng chờ duyệt.";

            return RedirectToAction("DanhSachDangKy", new { maNguoiThamDu });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(
            string maDangKy,
            string maNguoiThamDu,
            string lyDoHuy)
        {
            var (thanhCong, lyDo) = await _dangKyService.HuyDangKyAsync(
                maDangKy,
                maNguoiThamDu,
                lyDoHuy);

            TempData[thanhCong ? "ThongBao" : "Loi"] = thanhCong
                ? "Hủy đăng ký thành công."
                : lyDo;

            return RedirectToAction("DanhSachDangKy", new { maNguoiThamDu });
        }

        public async Task<IActionResult> DanhSachDangKy(
            string maNguoiThamDu,
            string? trangThai,
            string? maLoaiSuKien,
            DateTime? tuNgayDangKy,
            DateTime? denNgayDangKy,
            string? sapXep)
        {
            ViewBag.MaNguoiThamDu = maNguoiThamDu;
            ViewBag.LoaiSuKien = await _context.LoaiSuKiens
                .AsNoTracking()
                .OrderBy(loai => loai.TenLoaiSuKien)
                .ToListAsync();

            if (tuNgayDangKy.HasValue &&
                denNgayDangKy.HasValue &&
                tuNgayDangKy.Value.Date > denNgayDangKy.Value.Date)
            {
                ModelState.AddModelError(string.Empty, "Từ ngày không được sau đến ngày.");
            }

            var query = _context.DangKyThamDus
                .AsNoTracking()
                .Where(dangKy => dangKy.MaNguoiThamDu == maNguoiThamDu);

            if (Enum.TryParse<TrangThaiDangKy>(trangThai, out var trangThaiEnum))
            {
                query = query.Where(dangKy => dangKy.TrangThai == trangThaiEnum);
            }

            if (!string.IsNullOrWhiteSpace(maLoaiSuKien))
            {
                query = query.Where(dangKy =>
                    dangKy.SuKien != null &&
                    dangKy.SuKien.MaLoaiSuKien == maLoaiSuKien);
            }

            if (tuNgayDangKy.HasValue)
            {
                query = query.Where(dangKy =>
                    dangKy.NgayDangKy >= tuNgayDangKy.Value.Date);
            }

            if (denNgayDangKy.HasValue)
            {
                query = query.Where(dangKy =>
                    dangKy.NgayDangKy < denNgayDangKy.Value.Date.AddDays(1));
            }

            query = sapXep == "ngaytochuc"
                ? query.OrderBy(dangKy => dangKy.SuKien == null
                    ? DateTime.MaxValue
                    : dangKy.SuKien.ThoiGianBatDau)
                : query.OrderByDescending(dangKy => dangKy.NgayDangKy);

            var danhSach = await query
                .Select(dangKy => new
                {
                    dangKy.MaDangKy,
                    dangKy.MaNguoiThamDu,
                    TenSuKien = dangKy.SuKien == null
                        ? string.Empty
                        : dangKy.SuKien.TenSuKien,
                    dangKy.NgayDangKy,
                    dangKy.TrangThai,
                    ThoiGianBatDau = dangKy.SuKien == null
                        ? (DateTime?)null
                        : dangKy.SuKien.ThoiGianBatDau,
                    ThoiGianKetThuc = dangKy.SuKien == null
                        ? (DateTime?)null
                        : dangKy.SuKien.ThoiGianKetThuc
                })
                .ToListAsync();

            return View(danhSach);
        }

        public async Task<IActionResult> TimKiem(
            string? tenNguoiThamDu,
            string? maSuKien,
            string? trangThai,
            string? maLoaiSuKien,
            DateTime? tuNgayDangKy,
            DateTime? denNgayDangKy,
            string? sapXep)
        {
            ViewBag.LoaiSuKien = await _context.LoaiSuKiens
                .AsNoTracking()
                .OrderBy(loai => loai.TenLoaiSuKien)
                .ToListAsync();

            var query = _context.DangKyThamDus
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tenNguoiThamDu))
            {
                var tuKhoa = tenNguoiThamDu.Trim();
                query = query.Where(dangKy =>
                    dangKy.NguoiThamDu != null &&
                    dangKy.NguoiThamDu.HoTen.Contains(tuKhoa));
            }

            if (!string.IsNullOrWhiteSpace(maSuKien))
            {
                query = query.Where(dangKy => dangKy.MaSuKien == maSuKien);
            }

            if (Enum.TryParse<TrangThaiDangKy>(trangThai, out var trangThaiEnum))
            {
                query = query.Where(dangKy => dangKy.TrangThai == trangThaiEnum);
            }

            if (!string.IsNullOrWhiteSpace(maLoaiSuKien))
            {
                query = query.Where(dangKy =>
                    dangKy.SuKien != null &&
                    dangKy.SuKien.MaLoaiSuKien == maLoaiSuKien);
            }

            if (tuNgayDangKy.HasValue &&
                denNgayDangKy.HasValue &&
                tuNgayDangKy.Value.Date > denNgayDangKy.Value.Date)
            {
                ModelState.AddModelError(string.Empty, "Từ ngày không được sau đến ngày.");
                return View(await query
                    .Select(dangKy => new
                    {
                        dangKy.MaDangKy,
                        HoTen = dangKy.NguoiThamDu == null
                            ? string.Empty
                            : dangKy.NguoiThamDu.HoTen,
                        TenSuKien = dangKy.SuKien == null
                            ? string.Empty
                            : dangKy.SuKien.TenSuKien,
                        dangKy.NgayDangKy,
                        ThoiGianBatDau = dangKy.SuKien == null
                            ? (DateTime?)null
                            : dangKy.SuKien.ThoiGianBatDau,
                        dangKy.TrangThai
                    })
                    .ToListAsync());
            }

            if (tuNgayDangKy.HasValue)
            {
                query = query.Where(dangKy =>
                    dangKy.NgayDangKy >= tuNgayDangKy.Value.Date);
            }

            if (denNgayDangKy.HasValue)
            {
                query = query.Where(dangKy =>
                    dangKy.NgayDangKy < denNgayDangKy.Value.Date.AddDays(1));
            }

            query = sapXep == "ngaytochuc"
                ? query.OrderBy(dangKy => dangKy.SuKien == null
                    ? DateTime.MaxValue
                    : dangKy.SuKien.ThoiGianBatDau)
                : query.OrderByDescending(dangKy => dangKy.NgayDangKy);

            var danhSach = await query
                .Select(dangKy => new
                {
                    dangKy.MaDangKy,
                    HoTen = dangKy.NguoiThamDu == null
                        ? string.Empty
                        : dangKy.NguoiThamDu.HoTen,
                    TenSuKien = dangKy.SuKien == null
                        ? string.Empty
                        : dangKy.SuKien.TenSuKien,
                    dangKy.NgayDangKy,
                    ThoiGianBatDau = dangKy.SuKien == null
                        ? (DateTime?)null
                        : dangKy.SuKien.ThoiGianBatDau,
                    dangKy.TrangThai
                })
                .ToListAsync();

            return View(danhSach);
        }
    }
}
