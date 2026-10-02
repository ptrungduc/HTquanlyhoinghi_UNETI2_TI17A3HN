using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Services;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers
{
    public class DangKyController : Controller
    {
        private readonly AppDbContext _context;
        private readonly DangKyService _dangKyService;

        public DangKyController(
            AppDbContext context,
            DangKyService dangKyService)
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
        public async Task<IActionResult> DangKy(
            string maNguoiThamDu,
            string maSuKien)
        {
            var (hopLe, lyDo) =
                await _dangKyService.KiemTraDieuKienDangKyAsync(
                    maNguoiThamDu,
                    maSuKien);

            if (!hopLe)
            {
                TempData["Loi"] = lyDo;
                return RedirectToAction(
                    "ChiTiet",
                    "SuKien",
                    new { id = maSuKien });
            }

            var dangKy = new DangKyThamDu
            {
                MaDangKy = Guid.NewGuid().ToString("N"),
                MaNguoiThamDu = maNguoiThamDu,
                MaSuKien = maSuKien,
                NgayDangKy = DateTime.Now,
                TrangThai = TrangThaiDangKy.ChoDuyet
            };

            _context.DangKyThamDu.Add(dangKy);
            await _context.SaveChangesAsync();

            TempData["ThongBao"] =
                "Đăng ký thành công, vui lòng chờ duyệt.";

            return RedirectToAction(
                "DanhSachDangKy",
                new { maNguoiThamDu });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(
            string maDangKy,
            string maNguoiThamDu,
            string lyDoHuy)
        {
            var (thanhCong, lyDo) =
                await _dangKyService.HuyDangKyAsync(
                    maDangKy,
                    maNguoiThamDu,
                    lyDoHuy);

            TempData[thanhCong ? "ThongBao" : "Loi"] =
                thanhCong
                    ? "Hủy đăng ký thành công."
                    : lyDo;

            return RedirectToAction(
                "DanhSachDangKy",
                new { maNguoiThamDu });
        }

        // GET: danh sách đăng ký của một người tham dự
        public IActionResult DanhSachDangKy(
            string maNguoiThamDu,
            string trangThai,
            string? maLoaiSuKien,
            string sapXep)
        {
            var query = _context.DangKyThamDu
                .Where(d => d.MaNguoiThamDu == maNguoiThamDu)
                .AsQueryable();

            if (!string.IsNullOrEmpty(trangThai) &&
                Enum.TryParse<TrangThaiDangKy>(
                    trangThai,
                    out var tt))
            {
                query = query.Where(d => d.TrangThai == tt);
            }

            if (!string.IsNullOrWhiteSpace(maLoaiSuKien))
            {
                query = query.Where(
                    d => d.SuKien != null && d.SuKien.MaLoaiSuKien == maLoaiSuKien);
            }

            query = sapXep switch
            {
                "ngaytochuc" =>
                    query.OrderBy(d => d.SuKien == null ? DateTime.MaxValue : d.SuKien.ThoiGianBatDau),

                _ =>
                    query.OrderByDescending(d => d.NgayDangKy)
            };

            var danhSach = query
                .Select(d => new
                {
                    d.MaDangKy,
                    d.MaNguoiThamDu,
                    TenSuKien = d.SuKien == null ? string.Empty : d.SuKien.TenSuKien,
                    d.NgayDangKy,
                    d.TrangThai,
                    ThoiGianBatDau = d.SuKien == null ? (DateTime?)null : d.SuKien.ThoiGianBatDau
                })
                .ToList();

            return View(danhSach);
        }

        // GET: Ban tổ chức tìm kiếm đăng ký
        public IActionResult TimKiem(
            string tenNguoiThamDu,
            string? maSuKien,
            string trangThai,
            string sapXep)
        {
            var query =
                _context.DangKyThamDu.AsQueryable();

            if (!string.IsNullOrEmpty(tenNguoiThamDu))
            {
                query = query.Where(
                    d => d.NguoiThamDu != null && d.NguoiThamDu.HoTen
                        .Contains(tenNguoiThamDu));
            }

            if (!string.IsNullOrWhiteSpace(maSuKien))
            {
                query = query.Where(
                    d => d.MaSuKien == maSuKien);
            }

            if (!string.IsNullOrEmpty(trangThai) &&
                Enum.TryParse<TrangThaiDangKy>(
                    trangThai,
                    out var tt))
            {
                query = query.Where(
                    d => d.TrangThai == tt);
            }

            query = sapXep switch
            {
                "ngaytochuc" =>
                    query.OrderBy(
                        d => d.SuKien == null ? DateTime.MaxValue : d.SuKien.ThoiGianBatDau),

                _ =>
                    query.OrderByDescending(
                        d => d.NgayDangKy)
            };

            var danhSach = query
                .Select(d => new
                {
                    d.MaDangKy,
                    HoTen = d.NguoiThamDu == null ? string.Empty : d.NguoiThamDu.HoTen,
                    TenSuKien = d.SuKien == null ? string.Empty : d.SuKien.TenSuKien,
                    d.NgayDangKy,
                    d.TrangThai
                })
                .ToList();

            return View(danhSach);
        }
    }
}