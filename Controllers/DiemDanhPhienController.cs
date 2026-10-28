using System.Data.Common;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Helper;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module4;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers;

public class DiemDanhPhienController : Controller
{
    private readonly IDichVuModule4 _dichVuModule4;
    private readonly ILogger<DiemDanhPhienController> _logger;

    public DiemDanhPhienController(
        IDichVuModule4 dichVuModule4,
        ILogger<DiemDanhPhienController> logger)
    {
        _dichVuModule4 = dichVuModule4;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? maPhienSuKien)
    {
        var ketQuaPhanQuyen = KiemTraQuyenQuanTri();
        if (ketQuaPhanQuyen is not null)
            return ketQuaPhanQuyen;

        try
        {
            return View(await _dichVuModule4.LayDiemDanhPhienAsync(maPhienSuKien));
        }
        catch (Exception ex) when (ex is DbUpdateException or DbException)
        {
            _logger.LogError(ex, "Module 4 failed to load session attendance.");
            return View(new DiemDanhPhienViewModel
            {
                MaPhienSuKien = maPhienSuKien,
                ThongBao = "Không thể tải dữ liệu điểm danh. Vui lòng thử lại."
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DiemDanh(string? maPhienSuKien, string? maNguoiThamDu)
    {
        var ketQuaPhanQuyen = KiemTraQuyenQuanTri();
        if (ketQuaPhanQuyen is not null)
            return ketQuaPhanQuyen;

        KetQuaThaoTacViewModel ketQua;
        try
        {
            ketQua = string.IsNullOrWhiteSpace(maPhienSuKien) ||
                string.IsNullOrWhiteSpace(maNguoiThamDu)
                ? new KetQuaThaoTacViewModel
                {
                    ThanhCong = false,
                    ThongBao = "Phiên hoặc người tham dự không hợp lệ."
                }
                : await _dichVuModule4.DiemDanhPhienAsync(maPhienSuKien, maNguoiThamDu);
        }
        catch (Exception ex) when (ex is DbUpdateException or DbException)
        {
            _logger.LogError(ex, "Module 4 failed to confirm session attendance.");
            return View("Index", new DiemDanhPhienViewModel
            {
                MaPhienSuKien = maPhienSuKien,
                KetQua = new KetQuaThaoTacViewModel
                {
                    ThanhCong = false,
                    ThongBao = "Không thể xác nhận kết quả điểm danh do lỗi dữ liệu. Vui lòng kiểm tra trạng thái trước khi thử lại."
                }
            });
        }

        DiemDanhPhienViewModel viewModel;
        try
        {
            viewModel = await _dichVuModule4.LayDiemDanhPhienAsync(maPhienSuKien);
        }
        catch (Exception ex) when (ex is DbUpdateException or DbException)
        {
            _logger.LogError(ex, "Module 4 failed to reload session attendance.");
            return View("Index", new DiemDanhPhienViewModel
            {
                MaPhienSuKien = maPhienSuKien,
                KetQua = ketQua,
                ThongBao = "Không thể tải lại danh sách điểm danh. Vui lòng làm mới trang."
            });
        }

        viewModel.KetQua = ketQua;
        return View("Index", viewModel);
    }

    private IActionResult? KiemTraQuyenQuanTri()
    {
        if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("MaTaiKhoan")))
            return RedirectToAction("DangNhap", "TaiKhoan");

        if (!KiemTraQuyenModule4.CoQuyenQuanTri(HttpContext.Session.GetString("VaiTro")))
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền truy cập chức năng Module 4.");

        return null;
    }
}