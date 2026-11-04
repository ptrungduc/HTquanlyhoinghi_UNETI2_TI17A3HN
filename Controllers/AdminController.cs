using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");

            if (vaiTro != "Admin")
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var now = DateTime.Now;

            // 1. Stat Cards
            var tongSuKien = await _context.SuKiens.CountAsync();
            var tongNguoiThamDu = await _context.TaiKhoans
                .Where(t => t.VaiTro == "Nguoi Tham Du")
                .CountAsync();
            var tongDiaDiem = await _context.DiaDiems.CountAsync();
            var tongDangKy = await _context.DangKyThamDus.CountAsync();

            // 2. Sự kiện sắp tới (Top 5, thời gian bắt đầu >= hiện tại)
            var suKienSapTois = await _context.SuKiens
                .Include(s => s.DiaDiem)
                .Where(s => s.ThoiGianBatDau >= now)
                .OrderBy(s => s.ThoiGianBatDau)
                .Take(5)
                .Select(s => new SuKienSapToiItem
                {
                    MaSuKien = s.MaSuKien,
                    TenSuKien = s.TenSuKien,
                    ThoiGianBatDau = s.ThoiGianBatDau,
                    TenDiaDiem = s.DiaDiem != null ? s.DiaDiem.TenDiaDiem : "Chưa xác định"
                })
                .ToListAsync();

            // 3. Sự kiện gần đây (Top 5, mới nhất trước)
            var suKienGanDays = await _context.SuKiens
                .Include(s => s.DiaDiem)
                .OrderByDescending(s => s.ThoiGianBatDau)
                .Take(5)
                .Select(s => new SuKienGanDayItem
                {
                    MaSuKien = s.MaSuKien,
                    TenSuKien = s.TenSuKien,
                    ThoiGianBatDau = s.ThoiGianBatDau,
                    TenDiaDiem = s.DiaDiem != null ? s.DiaDiem.TenDiaDiem : "Chưa xác định",
                    SoNguoiDangKy = _context.DangKyThamDus.Count(d => d.MaSuKien == s.MaSuKien),
                    TrangThai = s.TrangThai
                })
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TongSuKien = tongSuKien,
                TongNguoiThamDu = tongNguoiThamDu,
                TongDiaDiem = tongDiaDiem,
                TongDangKy = tongDangKy,
                SuKienSapTois = suKienSapTois,
                SuKienGanDays = suKienGanDays
            };

            return View(viewModel);
        }
    }
}
