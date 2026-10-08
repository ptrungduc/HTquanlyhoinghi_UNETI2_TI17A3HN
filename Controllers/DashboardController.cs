// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 5 - Lịch sử tham dự, Dashboard, thống kê LINQ, báo cáo.

using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module5;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers;

public class DashboardController : Controller
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    // =================================================================
    // DASHBOARD ADMIN / ORGANIZER
    // =================================================================
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Bước 1: Kiểm tra quyền Admin
        var vaiTro = HttpContext.Session.GetString("VaiTro");
        if (vaiTro != "Admin")
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var now = DateTime.Now;
        var today = now.Date;

        // Bước 2: Lấy dữ liệu cơ bản từ database
        var tatCaSuKien = await _context.SuKiens.AsNoTracking().ToListAsync();
        var tatCaDangKy = await _context.DangKyThamDus.AsNoTracking().ToListAsync();
        var tatCaCheckIn = await _context.CheckIns.AsNoTracking().ToListAsync();

        // Bước 3: LINQ - Tính toán Dashboard
        var viewModel = new DashboardViewModel();

        // === 1. Tổng số sự kiện — Count() ===
        viewModel.TongSuKien = tatCaSuKien.Count;

        // === 2. Số sự kiện theo trạng thái — Count() với predicate ===
        viewModel.SuKienNhap = tatCaSuKien
            .Count(s => s.TrangThai == "Nhap" || s.TrangThai == "SapDienRa");
        viewModel.SuKienMoDangKy = tatCaSuKien.Count(s => s.TrangThai == "MoDangKy");
        viewModel.SuKienDongDangKy = tatCaSuKien.Count(s => s.TrangThai == "DongDangKy");
        viewModel.SuKienDangDienRa = tatCaSuKien.Count(s => s.TrangThai == "DangDienRa");
        viewModel.SuKienHoanThanh = tatCaSuKien.Count(s => s.TrangThai == "HoanThanh");
        viewModel.SuKienDaHuy = tatCaSuKien.Count(s => s.TrangThai == "DaHuy");

        // === 3. Số đăng ký theo trạng thái — Count() với predicate trên enum ===
        viewModel.TongDangKy = tatCaDangKy.Count;
        viewModel.DangKyChoDuyet = tatCaDangKy.Count(d => d.TrangThai == TrangThaiDangKy.ChoDuyet);
        viewModel.DangKyDaDuyet = tatCaDangKy.Count(d => d.TrangThai == TrangThaiDangKy.DaDuyet);
        viewModel.DangKyDaCheckIn = tatCaDangKy.Count(d => d.TrangThai == TrangThaiDangKy.DaCheckIn);
        viewModel.DangKyHoanThanh = tatCaDangKy.Count(d => d.TrangThai == TrangThaiDangKy.HoanThanh);
        viewModel.DangKyTuChoi = tatCaDangKy.Count(d => d.TrangThai == TrangThaiDangKy.TuChoi);
        viewModel.DangKyDaHuy = tatCaDangKy.Count(d => d.TrangThai == TrangThaiDangKy.DaHuy);

        // === 4. Số lượt check-in hôm nay — Count() lọc theo ngày ===
        viewModel.CheckInHomNay = tatCaCheckIn.Count(c => c.ThoiGianCheckIn.Date == today);

        // === 5. Sự kiện sắp diễn ra — Where() + OrderBy() + Take() + Select() ===
        viewModel.SuKienSapDienRa = await _context.SuKiens
            .AsNoTracking()
            .Where(s => s.ThoiGianBatDau > now)
            .OrderBy(s => s.ThoiGianBatDau)
            .Take(5)
            .Select(s => new SuKienSapDienRaItem
            {
                MaSuKien = s.MaSuKien,
                TenSuKien = s.TenSuKien,
                ThoiGianBatDau = s.ThoiGianBatDau,
                TenDiaDiem = s.DiaDiem != null ? s.DiaDiem.TenDiaDiem : "",
                TrangThai = s.TrangThai,
                SoDangKy = _context.DangKyThamDus.Count(d => d.MaSuKien == s.MaSuKien)
            })
            .ToListAsync();

        // === 6. Tham dự thực tế theo tháng (12 tháng) — Where() + GroupBy() + Select() + OrderBy() ===
        var dauKy = now.AddMonths(-11);
        var ngayBatDauKy = new DateTime(dauKy.Year, dauKy.Month, 1);

        viewModel.ThamDuTheoThang = tatCaCheckIn
            .Where(c => c.ThoiGianCheckIn >= ngayBatDauKy)
            .GroupBy(c => new { c.ThoiGianCheckIn.Year, c.ThoiGianCheckIn.Month })
            .Select(g => new ThongKeThamDuTheoThang
            {
                Nam = g.Key.Year,
                Thang = g.Key.Month,
                SoLuotCheckIn = g.Count()
            })
            .OrderBy(t => t.Nam)
            .ThenBy(t => t.Thang)
            .ToList();

        // === 7. Sự kiện gần hạn (bắt đầu trong 7 ngày tới) — Where() + OrderBy() + Select() ===
        var hanCuoi = now.AddDays(7);
        var dsGanHan = await _context.SuKiens
            .AsNoTracking()
            .Where(s => s.ThoiGianBatDau > now && s.ThoiGianBatDau <= hanCuoi)
            .OrderBy(s => s.ThoiGianBatDau)
            .Select(s => new
            {
                s.MaSuKien,
                s.TenSuKien,
                s.ThoiGianBatDau,
                SoDangKy = _context.DangKyThamDus.Count(d => d.MaSuKien == s.MaSuKien)
            })
            .ToListAsync();

        // Tính số ngày còn lại trên C# (phép trừ DateTime)
        viewModel.SuKienGanHan = dsGanHan
            .Select(s => new SuKienGanHanItem
            {
                MaSuKien = s.MaSuKien,
                TenSuKien = s.TenSuKien,
                ThoiGianBatDau = s.ThoiGianBatDau,
                SoNgayConLai = (s.ThoiGianBatDau - now).Days,
                SoDangKy = s.SoDangKy
            })
            .ToList();

        // Bước 4: Đưa ViewModel sang View
        return View(viewModel);
    }
}

