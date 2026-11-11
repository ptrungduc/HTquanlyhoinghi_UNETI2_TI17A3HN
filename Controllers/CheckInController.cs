// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 4 - Xét duyệt đăng ký, mã tham dự, check-in, điểm danh.

using System.Data.Common;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Helper;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module4;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers;

public class CheckInController : Controller
{
    private readonly AppDbContext _context;
    private readonly IDichVuModule4 _dichVuModule4;
    private readonly ILogger<CheckInController> _logger;

    public CheckInController(
        AppDbContext context,
        IDichVuModule4 dichVuModule4,
        ILogger<CheckInController> logger)
    {
        _context = context;
        _dichVuModule4 = dichVuModule4;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? maThamDu)
    {
        var ketQuaPhanQuyen = KiemTraQuyenTruyCap();
        if (ketQuaPhanQuyen is not null)
        {
            return ketQuaPhanQuyen;
        }

        var model = new CheckInViewModel { MaThamDu = maThamDu ?? string.Empty };
        if (!string.IsNullOrWhiteSpace(maThamDu))
        {
            try
            {
                model.DangKy = await _dichVuModule4.TraCuuMaThamDuAsync(maThamDu);
            }
            catch (Exception ex) when (ex is DbUpdateException or DbException)
            {
                _logger.LogError(ex, "Module 4 failed to look up an attendance code.");
                model.KetQua = new KetQuaThaoTacViewModel
                {
                    ThanhCong = false,
                    ThongBao = "Không thể tra cứu mã tham dự. Vui lòng thử lại."
                };
            }
        }

        await TaiDuLieuCheckInViewAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckInViewModel model)
    {
        var ketQuaPhanQuyen = KiemTraQuyenTruyCap();
        if (ketQuaPhanQuyen is not null)
        {
            return ketQuaPhanQuyen;
        }

        if (!ModelState.IsValid)
        {
            await TaiDuLieuCheckInViewAsync(model);
            return View(model);
        }

        try
        {
            model.DangKy = await _dichVuModule4.TraCuuMaThamDuAsync(model.MaThamDu);
            model.KetQua = string.IsNullOrWhiteSpace(model.DangKy?.MaDangKy)
                ? new KetQuaThaoTacViewModel
                {
                    ThanhCong = false,
                    ThongBao = model.DangKy?.ThongBao ?? "Mã tham dự không hợp lệ."
                }
                : await _dichVuModule4.CheckInAsync(
                    model.MaThamDu,
                    HttpContext.Session.GetString("MaTaiKhoan") ?? string.Empty,
                    model.GhiChu);

            if (model.KetQua?.ThanhCong == true)
            {
                model.DangKy = await _dichVuModule4.TraCuuMaThamDuAsync(model.MaThamDu);
            }
        }
        catch (Exception ex) when (ex is DbUpdateException or DbException)
        {
            _logger.LogError(ex, "Module 4 failed to confirm check-in.");
            model.KetQua = new KetQuaThaoTacViewModel
            {
                ThanhCong = false,
                ThongBao = "Không thể xác nhận kết quả check-in do lỗi dữ liệu. Vui lòng kiểm tra trạng thái đăng ký trước khi thử lại."
            };
        }

        await TaiDuLieuCheckInViewAsync(model);
        return View(model);
    }

    private async Task TaiDuLieuCheckInViewAsync(CheckInViewModel model)
    {
        var homNay = DateTime.Today;
        var ngayMai = homNay.AddDays(1);
        var bayGio = DateTime.Now;

        // 1. Danh sách người tham dự đã duyệt của các sự kiện đang diễn ra hôm nay
        model.DanhSachSanSang = await _context.DangKyThamDus
            .AsNoTracking()
            .Include(d => d.NguoiThamDu)
            .Include(d => d.SuKien)
            .Where(d => d.TrangThai == TrangThaiDangKy.DaDuyet &&
                        d.MaThamDu != null &&
                        d.SuKien != null &&
                        d.SuKien.TrangThai != nameof(TrangThaiSuKien.DaHuy) &&
                        bayGio >= d.SuKien.ThoiGianBatDau &&
                        bayGio <= d.SuKien.ThoiGianKetThuc)
            .OrderBy(d => d.SuKien!.TenSuKien)
            .ThenBy(d => d.NguoiThamDu!.HoTen)
            .Take(30)
            .Select(d => new NguoiThamDuCheckInItemViewModel
            {
                MaDangKy = d.MaDangKy,
                MaThamDu = d.MaThamDu!,
                HoTen = d.NguoiThamDu != null ? d.NguoiThamDu.HoTen : "Chưa rõ",
                SoDienThoai = d.NguoiThamDu != null ? d.NguoiThamDu.SoDienThoai : "",
                Email = d.NguoiThamDu != null ? d.NguoiThamDu.Email : "",
                TenSuKien = d.SuKien != null ? d.SuKien.TenSuKien : "Chưa rõ",
                MaSuKien = d.MaSuKien
            })
            .ToListAsync();

        // 2. Lịch sử check-in hôm nay
        model.DanhSachVuaCheckIn = await _context.CheckIns
            .AsNoTracking()
            .Include(c => c.DangKyThamDu)
                .ThenInclude(d => d!.NguoiThamDu)
            .Include(c => c.DangKyThamDu)
                .ThenInclude(d => d!.SuKien)
            .Where(c => c.ThoiGianCheckIn >= homNay && c.ThoiGianCheckIn < ngayMai)
            .OrderByDescending(c => c.ThoiGianCheckIn)
            .Take(15)
            .Select(c => new NguoiThamDuCheckInItemViewModel
            {
                MaDangKy = c.MaDangKy,
                MaThamDu = c.DangKyThamDu != null && c.DangKyThamDu.MaThamDu != null ? c.DangKyThamDu.MaThamDu : "-",
                HoTen = c.DangKyThamDu != null && c.DangKyThamDu.NguoiThamDu != null ? c.DangKyThamDu.NguoiThamDu.HoTen : "Chưa rõ",
                SoDienThoai = c.DangKyThamDu != null && c.DangKyThamDu.NguoiThamDu != null ? c.DangKyThamDu.NguoiThamDu.SoDienThoai : "",
                Email = c.DangKyThamDu != null && c.DangKyThamDu.NguoiThamDu != null ? c.DangKyThamDu.NguoiThamDu.Email : "",
                TenSuKien = c.DangKyThamDu != null && c.DangKyThamDu.SuKien != null ? c.DangKyThamDu.SuKien.TenSuKien : "Chưa rõ",
                MaSuKien = c.DangKyThamDu != null ? c.DangKyThamDu.MaSuKien : "",
                ThoiGianCheckIn = c.ThoiGianCheckIn,
                GhiChu = c.GhiChu
            })
            .ToListAsync();
    }

    private IActionResult? KiemTraQuyenTruyCap()
    {
        if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("MaTaiKhoan")))
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var vaiTro = HttpContext.Session.GetString("VaiTro");
        if (!KiemTraQuyenModule4.CoQuyenTiepDon(vaiTro))
        {
            TempData["ThongBaoSaiVaiTro"] = "Bạn không có quyền truy cập vào mục Check-in do sai vai trò!";
            var (ctrl, act) = KiemTraQuyenModule4.LayTrangDieuHuongTheoVaiTro(vaiTro);
            return RedirectToAction(act, ctrl);
        }

        return null;
    }
}
