// Họ và tên: PHAM TRUNG DUC
// Mã sinh viên: 23103100134
// Nội dung thực hiện: Module 5 - Dashboard Organizer/Admin (Bước 2).

using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Helper;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module5;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers;

public class DashboardController : Controller
{
    // Số sự kiện tối đa hiển thị trong mỗi danh sách
    private const int SoSuKienHienThi = 5;

    // Số ngày trước khi bắt đầu được coi là "gần hạn đăng ký"
    private const int SoNgayGanHanDangKy = 7;

    // Đăng ký còn hiệu lực (không tính đã từ chối / đã hủy)
    private static readonly TrangThaiDangKy[] TrangThaiHieuLuc =
    {
        TrangThaiDangKy.ChoDuyet,
        TrangThaiDangKy.DaDuyet,
        TrangThaiDangKy.DaCheckIn,
        TrangThaiDangKy.HoanThanh
    };

    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // 1. KIỂM TRA QUYỀN Ở SERVER
        if (!KiemTraQuyenModule4.CoQuyenQuanTri(HttpContext.Session.GetString("VaiTro")))
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var now = DateTime.Now;
        var homNay = now.Date;
        var ngayMai = homNay.AddDays(1);

        var viewModel = new DashboardViewModel
        {
            ThoiDiemTinh = now
        };

        // 2. TỔNG SỐ SỰ KIỆN - CountAsync -> SELECT COUNT(*)
        viewModel.TongSuKien = await _context.SuKiens
            .AsNoTracking()
            .CountAsync();

        // 3. SỐ SỰ KIỆN THEO TRẠNG THÁI - GroupBy(TrangThai) -> GROUP BY trên SQL
        var suKienNhom = await _context.SuKiens
            .AsNoTracking()
            .GroupBy(s => s.TrangThai)
            .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
            .ToListAsync();

        viewModel.SuKienTheoTrangThai = DanhSachTrangThaiSuKien()
            .Select(t => new DashboardThongKeItem
            {
                MaTrangThai = t.Ma,
                TenHienThi = t.Ten,
                CssClass = t.Css,
                SoLuong = suKienNhom
                    .Where(x => x.TrangThai == t.Ma)
                    .Select(x => x.SoLuong)
                    .FirstOrDefault()
            })
            .ToList();

        // 4. SỐ ĐĂNG KÝ THEO TRẠNG THÁI - GroupBy trên enum (lưu dạng số trong SQL)
        var dangKyNhom = await _context.DangKyThamDus
            .AsNoTracking()
            .GroupBy(d => d.TrangThai)
            .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
            .ToListAsync();

        viewModel.DangKyTheoTrangThai = DanhSachTrangThaiDangKy()
            .Select(t => new DashboardThongKeItem
            {
                MaTrangThai = t.Ma,
                TenHienThi = t.Ten,
                CssClass = t.Css,
                SoLuong = dangKyNhom
                    .Where(x => x.TrangThai.ToString() == t.Ma)
                    .Select(x => x.SoLuong)
                    .FirstOrDefault()
            })
            .ToList();

        // 5. CHECK-IN HÔM NAY - Where theo khoảng [hôm nay, ngày mai) + CountAsync
        viewModel.CheckInHomNay = await _context.CheckIns
            .AsNoTracking()
            .Where(c => c.ThoiGianCheckIn >= homNay && c.ThoiGianCheckIn < ngayMai)
            .CountAsync();

        // 6. SỰ KIỆN SẮP DIỄN RA - Where, OrderBy, Take, Select
        // Chưa bắt đầu, không bị hủy, không hoàn thành. Gần nhất lên đầu.
        var daHuy = nameof(TrangThaiSuKien.DaHuy);
        var hoanThanh = nameof(TrangThaiSuKien.HoanThanh);

        viewModel.SuKienSapDienRa = await _context.SuKiens
            .AsNoTracking()
            .Where(s => s.ThoiGianBatDau > now
                     && s.TrangThai != daHuy
                     && s.TrangThai != hoanThanh)
            .OrderBy(s => s.ThoiGianBatDau)
            .Take(SoSuKienHienThi)
            .Select(s => new DashboardSuKienItem
            {
                MaSuKien = s.MaSuKien,
                TenSuKien = s.TenSuKien,
                TenDiaDiem = s.DiaDiem != null ? s.DiaDiem.TenDiaDiem : "",
                ThoiGianBatDau = s.ThoiGianBatDau,
                TrangThai = s.TrangThai,
                // Đếm đăng ký còn hiệu lực qua DbSet, vì SuKien không có navigation DangKyThamDus
                SoDangKyHieuLuc = _context.DangKyThamDus.Count(d =>
                    d.MaSuKien == s.MaSuKien && TrangThaiHieuLuc.Contains(d.TrangThai)),
                SucChua = s.DiaDiem != null ? s.DiaDiem.SucChuaToiDa : 0
            })
            .ToListAsync();

        // 7. NGƯỜI THAM DỰ THỰC TẾ THEO THÁNG - 12 tháng gần nhất, tính theo check-in
        var thangHienTai = new DateTime(now.Year, now.Month, 1);
        var tuThang = thangHienTai.AddMonths(-11);
        var denThang = thangHienTai.AddMonths(1);

        var checkInTheoThang = await _context.CheckIns
            .AsNoTracking()
            .Where(c => c.ThoiGianCheckIn >= tuThang && c.ThoiGianCheckIn < denThang)
            .GroupBy(c => new { c.ThoiGianCheckIn.Year, c.ThoiGianCheckIn.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, SoNguoi = g.Count() })
            .ToListAsync();

        // Tạo đủ 12 tháng, tháng không có dữ liệu = 0
        viewModel.NguoiThamDuTheoThang = Enumerable.Range(0, 12)
            .Select(i => tuThang.AddMonths(i))
            .Select(ngay => new DashboardThangItem
            {
                Nam = ngay.Year,
                Thang = ngay.Month,
                SoNguoi = checkInTheoThang
                    .Where(x => x.Year == ngay.Year && x.Month == ngay.Month)
                    .Select(x => x.SoNguoi)
                    .FirstOrDefault()
            })
            .ToList();

        // 8. SỰ KIỆN GẦN HẠN ĐĂNG KÝ
        // Hệ thống không có trường hạn đăng ký. Quy ước tạm:
        // đang MoDangKy và bắt đầu trong vòng 7 ngày tới.
        var mocGanHan = now.AddDays(SoNgayGanHanDangKy);
        var moDangKy = nameof(TrangThaiSuKien.MoDangKy);

        viewModel.SuKienGanHanDangKy = await _context.SuKiens
            .AsNoTracking()
            .Where(s => s.TrangThai == moDangKy
                     && s.ThoiGianBatDau > now
                     && s.ThoiGianBatDau <= mocGanHan)
            .OrderBy(s => s.ThoiGianBatDau)
            .Take(SoSuKienHienThi)
            .Select(s => new DashboardSuKienItem
            {
                MaSuKien = s.MaSuKien,
                TenSuKien = s.TenSuKien,
                TenDiaDiem = s.DiaDiem != null ? s.DiaDiem.TenDiaDiem : "",
                ThoiGianBatDau = s.ThoiGianBatDau,
                TrangThai = s.TrangThai,
                SoDangKyHieuLuc = _context.DangKyThamDus.Count(d =>
                    d.MaSuKien == s.MaSuKien && TrangThaiHieuLuc.Contains(d.TrangThai)),
                SucChua = s.DiaDiem != null ? s.DiaDiem.SucChuaToiDa : 0
            })
            .ToListAsync();

        return View(viewModel);
    }

    // =================================================================
    // HÀM HỖ TRỢ - Danh sách trạng thái hiển thị (thứ tự cố định)
    // =================================================================

    private static List<(string Ma, string Ten, string Css)> DanhSachTrangThaiSuKien() =>
    [
        (nameof(TrangThaiSuKien.Nhap), "Nháp", "bg-secondary"),
        (nameof(TrangThaiSuKien.MoDangKy), "Mở đăng ký", "bg-primary"),
        (nameof(TrangThaiSuKien.DongDangKy), "Đóng đăng ký", "bg-warning text-dark"),
        (nameof(TrangThaiSuKien.DangDienRa), "Đang diễn ra", "bg-info"),
        (nameof(TrangThaiSuKien.HoanThanh), "Hoàn thành", "bg-success"),
        (nameof(TrangThaiSuKien.DaHuy), "Đã hủy", "bg-dark")
    ];

    private static List<(string Ma, string Ten, string Css)> DanhSachTrangThaiDangKy() =>
    [
        (nameof(TrangThaiDangKy.ChoDuyet), "Chờ duyệt", "bg-warning text-dark"),
        (nameof(TrangThaiDangKy.DaDuyet), "Đã duyệt", "bg-primary"),
        (nameof(TrangThaiDangKy.DaCheckIn), "Đã check-in", "bg-info"),
        (nameof(TrangThaiDangKy.HoanThanh), "Hoàn thành", "bg-success"),
        (nameof(TrangThaiDangKy.TuChoi), "Từ chối", "bg-danger"),
        (nameof(TrangThaiDangKy.DaHuy), "Đã hủy", "bg-dark")
    ];
}
