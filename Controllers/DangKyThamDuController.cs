// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 4 - Xét duyệt đăng ký, mã tham dự, check-in, điểm danh.

using System.Data.Common;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Helper;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module4;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers;

public class DangKyThamDuController : Controller
{
    private readonly IDichVuModule4 _dichVuModule4;
    private readonly ILogger<DangKyThamDuController> _logger;

    public DangKyThamDuController(
        IDichVuModule4 dichVuModule4,
        ILogger<DangKyThamDuController> logger)
    {
        _dichVuModule4 = dichVuModule4;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var ketQuaPhanQuyen = KiemTraQuyenQuanTri();
        if (ketQuaPhanQuyen is not null)
        {
            return ketQuaPhanQuyen;
        }

        try
        {
            return View(await _dichVuModule4.LayDanhSachChoDuyetAsync());
        }
        catch (Exception ex) when (ex is DbUpdateException or DbException)
        {
            _logger.LogError(ex, "Module 4 failed to load pending registrations.");
            return View(new DuyetDangKyViewModel
            {
                ThongBao = "Không thể tải danh sách đăng ký. Vui lòng thử lại."
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ChiTiet(string? maDangKy)
    {
        var ketQuaPhanQuyen = KiemTraQuyenQuanTri();
        if (ketQuaPhanQuyen is not null)
        {
            return ketQuaPhanQuyen;
        }

        if (string.IsNullOrWhiteSpace(maDangKy))
        {
            return View(new ChiTietDangKyViewModel { ThongBao = "Mã đăng ký không hợp lệ." });
        }

        try
        {
            return View(await _dichVuModule4.LayChiTietDangKyAsync(maDangKy));
        }
        catch (Exception ex) when (ex is DbUpdateException or DbException)
        {
            _logger.LogError(ex, "Module 4 failed to load a registration detail.");
            return View(new ChiTietDangKyViewModel
            {
                ThongBao = "Không thể tải chi tiết đăng ký. Vui lòng thử lại."
            });
        }
    }

    [HttpGet]
    public IActionResult Duyet(string? maDangKy)
    {
        var ketQuaPhanQuyen = KiemTraQuyenQuanTri();
        if (ketQuaPhanQuyen is not null)
        {
            return ketQuaPhanQuyen;
        }

        return View(new XacNhanDuyetDangKyViewModel { MaDangKy = maDangKy ?? string.Empty });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duyet(XacNhanDuyetDangKyViewModel model)
    {
        var ketQuaPhanQuyen = KiemTraQuyenQuanTri();
        if (ketQuaPhanQuyen is not null)
        {
            return ketQuaPhanQuyen;
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        KetQuaThaoTacViewModel ketQua;
        try
        {
            ketQua = await _dichVuModule4.DuyetDangKyAsync(model.MaDangKy);
        }
        catch (Exception ex) when (ex is DbUpdateException or DbException)
        {
            _logger.LogError(ex, "Module 4 failed to confirm registration approval.");
            ViewBag.KetQuaThaoTac = new KetQuaThaoTacViewModel
            {
                ThanhCong = false,
                ThongBao = "Không thể xác nhận kết quả duyệt do lỗi dữ liệu. Vui lòng kiểm tra trạng thái đăng ký trước khi thử lại."
            };
            return View(model);
        }

        if (ketQua.ThanhCong)
        {
            TempData["ThongBaoModule4"] = ketQua.ThongBao;
            return RedirectToAction(nameof(Index));
        }

        ViewBag.KetQuaThaoTac = ketQua;
        return View(model);
    }

    [HttpGet]
    public IActionResult TuChoi(string? maDangKy)
    {
        var ketQuaPhanQuyen = KiemTraQuyenQuanTri();
        if (ketQuaPhanQuyen is not null)
        {
            return ketQuaPhanQuyen;
        }

        return View(new TuChoiDangKyViewModel { MaDangKy = maDangKy ?? string.Empty });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TuChoi(TuChoiDangKyViewModel model)
    {
        var ketQuaPhanQuyen = KiemTraQuyenQuanTri();
        if (ketQuaPhanQuyen is not null)
        {
            return ketQuaPhanQuyen;
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        KetQuaThaoTacViewModel ketQua;
        try
        {
            ketQua = await _dichVuModule4.TuChoiDangKyAsync(model.MaDangKy, model.LyDoTuChoi);
        }
        catch (Exception ex) when (ex is DbUpdateException or DbException)
        {
            _logger.LogError(ex, "Module 4 failed to confirm registration rejection.");
            ViewBag.KetQuaThaoTac = new KetQuaThaoTacViewModel
            {
                ThanhCong = false,
                ThongBao = "Không thể xác nhận kết quả từ chối do lỗi dữ liệu. Vui lòng kiểm tra trạng thái đăng ký trước khi thử lại."
            };
            return View(model);
        }

        if (ketQua.ThanhCong)
        {
            TempData["ThongBaoModule4"] = ketQua.ThongBao;
            return RedirectToAction(nameof(Index));
        }

        ViewBag.KetQuaThaoTac = ketQua;
        return View(model);
    }

    private IActionResult? KiemTraQuyenQuanTri()
    {
        if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("MaTaiKhoan")))
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        if (!KiemTraQuyenModule4.CoQuyenQuanTri(HttpContext.Session.GetString("VaiTro")))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền truy cập chức năng Module 4.");
        }

        return null;
    }
}