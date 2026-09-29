using Microsoft.AspNetCore.Mvc;
using QuanLyHoiNghi.Models;
using QuanLyHoiNghi.Services;

namespace QuanLyHoiNghi.Controllers
{
    public class DangKyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly DangKyService _dangKyService;

        public DangKyController(ApplicationDbContext context, DangKyService dangKyService)
        {
            _context = context;
            _dangKyService = dangKyService;
        }

        public IActionResult DangKy(int maSuKien)
        {
            ViewBag.MaSuKien = maSuKien;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(int maNguoiThamDu, int maSuKien)
        {
            var (hopLe, lyDo) = await _dangKyService.KiemTraDieuKienDangKyAsync(maNguoiThamDu, maSuKien);

            if (!hopLe)
            {
                TempData["Loi"] = lyDo;
                return RedirectToAction("ChiTiet", "SuKien", new { id = maSuKien });
            }

            var dangKy = new DangKyThamDu
            {
                MaNguoiThamDu = maNguoiThamDu,
                MaSuKien = maSuKien,
                NgayDangKy = DateTime.Now,
                TrangThai = TrangThaiDangKy.ChoDuyet
            };

            _context.DangKyThamDu.Add(dangKy);
            await _context.SaveChangesAsync();

            TempData["ThongBao"] = "Đăng ký thành công, vui lòng chờ duyệt.";
            return RedirectToAction("DanhSachDangKy", new { maNguoiThamDu });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int maDangKy, int maNguoiThamDu, string lyDoHuy)
        {
            var (thanhCong, lyDo) = await _dangKyService.HuyDangKyAsync(maDangKy, maNguoiThamDu, lyDoHuy);

            TempData[thanhCong ? "ThongBao" : "Loi"] = thanhCong ? "Hủy đăng ký thành công." : lyDo;
            return RedirectToAction("DanhSachDangKy", new { maNguoiThamDu });
        }

        public IActionResult DanhSachDangKy(int maNguoiThamDu, string trangThai, string sapXep)
        {
            var query = _context.DangKyThamDu
                .Where(d => d.MaNguoiThamDu == maNguoiThamDu)
                .AsQueryable();

            if (!string.IsNullOrEmpty(trangThai) && Enum.TryParse<TrangThaiDangKy>(trangThai, out var tt))
            {
                query = query.Where(d => d.TrangThai == tt);
            }

            query = sapXep switch
            {
                "ngaytochuc" => query.OrderBy(d => d.SuKien.ThoiGianBatDau),
                _ => query.OrderByDescending(d => d.NgayDangKy)
            };

            var danhSach = query
                .Select(d => new
                {
                    d.MaDangKy,
                    d.SuKien.TenSuKien,
                    d.NgayDangKy,
                    d.TrangThai,
                    d.SuKien.HanDangKy
                })
                .ToList();

            return View(danhSach);
        }
    }
}