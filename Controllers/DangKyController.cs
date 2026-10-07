using Microsoft.AspNetCore.Mvc;
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

        // =========================================================
        // 1. HIỂN THỊ TRANG ĐĂNG KÝ
        // =========================================================
        public IActionResult DangKy(string maSuKien)
        {
            ViewBag.MaSuKien = maSuKien;

            return View();
        }

        // =========================================================
        // 2. XỬ LÝ ĐĂNG KÝ SỰ KIỆN
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(
            int maNguoiThamDu,
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
                MaNguoiThamDu = maNguoiThamDu,
                MaSuKien = maSuKien,
                NgayDangKy = DateTime.Now,
                TrangThai = TrangThaiDangKy.ChoDuyet
            };

            _context.DangKyThamDus.Add(dangKy);

            await _context.SaveChangesAsync();

            TempData["ThongBao"] =
                "Đăng ký thành công, vui lòng chờ duyệt.";

            return RedirectToAction(
                "DanhSachDangKy",
                new { maNguoiThamDu });
        }

        // =========================================================
        // 3. HỦY ĐĂNG KÝ
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(
            int maDangKy,
            int maNguoiThamDu,
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

        // =========================================================
        // 4. DANH SÁCH ĐĂNG KÝ CỦA NGƯỜI THAM DỰ
        // =========================================================
        public IActionResult DanhSachDangKy(
            int maNguoiThamDu,
            string trangThai,
            string maLoaiSuKien,
            DateTime? tuNgayDangKy,
            DateTime? denNgayDangKy,
            string sapXep)
        {
            var query = _context.DangKyThamDus
                .Where(d => d.MaNguoiThamDu == maNguoiThamDu)
                .AsQueryable();

            ViewBag.LoaiSuKien = _context.LoaiSuKiens
                .OrderBy(l => l.TenLoaiSuKien)
                .ToList();

            // -----------------------------------------------------
            // Lọc theo trạng thái
            // -----------------------------------------------------
            if (!string.IsNullOrEmpty(trangThai) &&
                Enum.TryParse<TrangThaiDangKy>(
                    trangThai,
                    out var tt))
            {
                query = query.Where(
                    d => d.TrangThai == tt);
            }

            // -----------------------------------------------------
            // Lọc theo loại sự kiện
            // -----------------------------------------------------
            if (!string.IsNullOrEmpty(maLoaiSuKien))
            {
                query = query.Where(
                    d => d.SuKien.MaLoaiSuKien == maLoaiSuKien);
            }

            // -----------------------------------------------------
            // Lọc theo ngày đăng ký
            // -----------------------------------------------------
            if (tuNgayDangKy.HasValue)
            {
                query = query.Where(
                    d => d.NgayDangKy >= tuNgayDangKy.Value.Date);
            }

            if (denNgayDangKy.HasValue)
            {
                query = query.Where(
                    d => d.NgayDangKy < denNgayDangKy.Value.Date.AddDays(1));
            }

            // -----------------------------------------------------
            // Sắp xếp
            // -----------------------------------------------------
            query = sapXep switch
            {
                "ngaytochuc" =>
                    query.OrderBy(
                        d => d.SuKien.ThoiGianBatDau),

                _ =>
                    query.OrderByDescending(
                        d => d.NgayDangKy)
            };

            // -----------------------------------------------------
            // Lấy danh sách
            // -----------------------------------------------------
            var danhSach = query
                .Select(d => new
                {
                    d.MaDangKy,
                    d.SuKien.TenSuKien,
                    d.NgayDangKy,
                    d.TrangThai,
                    d.SuKien.ThoiGianBatDau,
                    d.SuKien.ThoiGianKetThuc
                })
                .ToList();

            // -----------------------------------------------------
            // Truyền mã người tham dự xuống View
            // -----------------------------------------------------
            ViewBag.MaNguoiThamDu = maNguoiThamDu;

            return View(danhSach);
        }

        // =========================================================
        // 5. TÌM KIẾM ĐĂNG KÝ
        // =========================================================
        public IActionResult TimKiem(
            string tenNguoiThamDu,
            string maSuKien,
            string trangThai,
            string maLoaiSuKien,
            DateTime? tuNgayDangKy,
            DateTime? denNgayDangKy,
            string sapXep)
        {
            var query =
                _context.DangKyThamDus.AsQueryable();

            // -----------------------------------------------------
            // Tìm theo tên người tham dự
            // -----------------------------------------------------
            if (!string.IsNullOrEmpty(tenNguoiThamDu))
            {
                query = query.Where(
                    d => d.NguoiThamDu.HoTen
                        .Contains(tenNguoiThamDu));
            }

            // -----------------------------------------------------
            // Tìm theo mã sự kiện
            // -----------------------------------------------------
            if (!string.IsNullOrEmpty(maSuKien))
            {
                query = query.Where(
                    d => d.MaSuKien == maSuKien);
            }

            // -----------------------------------------------------
            // Lọc theo trạng thái
            // -----------------------------------------------------
            if (!string.IsNullOrEmpty(trangThai) &&
                Enum.TryParse<TrangThaiDangKy>(
                    trangThai,
                    out var tt))
            {
                query = query.Where(
                    d => d.TrangThai == tt);
            }

            // -----------------------------------------------------
            // Lọc theo loại sự kiện
            // -----------------------------------------------------
            if (!string.IsNullOrEmpty(maLoaiSuKien))
            {
                query = query.Where(
                    d => d.SuKien.MaLoaiSuKien == maLoaiSuKien);
            }

            // -----------------------------------------------------
            // Lọc theo ngày đăng ký
            // -----------------------------------------------------
            if (tuNgayDangKy.HasValue)
            {
                query = query.Where(
                    d => d.NgayDangKy >= tuNgayDangKy.Value.Date);
            }

            if (denNgayDangKy.HasValue)
            {
                query = query.Where(
                    d => d.NgayDangKy < denNgayDangKy.Value.Date.AddDays(1));
            }

            // -----------------------------------------------------
            // Sắp xếp
            // -----------------------------------------------------
            query = sapXep switch
            {
                "ngaytochuc" =>
                    query.OrderBy(
                        d => d.SuKien.ThoiGianBatDau),

                _ =>
                    query.OrderByDescending(
                        d => d.NgayDangKy)
            };

            // -----------------------------------------------------
            // Lấy danh sách
            // -----------------------------------------------------
            var danhSach = query
                .Select(d => new
                {
                    d.MaDangKy,
                    d.NguoiThamDu.HoTen,
                    d.SuKien.TenSuKien,
                    d.SuKien.MaLoaiSuKien,
                    d.NgayDangKy,
                    d.SuKien.ThoiGianBatDau,
                    d.TrangThai
                })
                .ToList();

            ViewBag.LoaiSuKien = _context.LoaiSuKiens
                .OrderBy(l => l.TenLoaiSuKien)
                .ToList();

            return View(danhSach);
        }
    }
}