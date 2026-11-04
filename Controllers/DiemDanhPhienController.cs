using System.Data.Common;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Helper;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
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
    public async Task<IActionResult> Index(string? maPhienSuKien, string? tuKhoa)
    {
        var ketQuaPhanQuyen = KiemTraQuyenTruyCap();
        if (ketQuaPhanQuyen is not null)
            return ketQuaPhanQuyen;

        try
        {
            return View(await _dichVuModule4.LayDiemDanhPhienAsync(maPhienSuKien, tuKhoa));
        }
        catch (Exception ex) when (ex is DbUpdateException or DbException)
        {
            _logger.LogError(ex, "Module 4 failed to load session attendance.");
            return View(new DiemDanhPhienViewModel
            {
                MaPhienSuKien = maPhienSuKien,
                TuKhoa = tuKhoa,
                ThongBao = "Không thể tải dữ liệu điểm danh. Vui lòng thử lại."
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DiemDanh(
        string? maPhienSuKien,
        string? maNguoiThamDu,
        string? ketQuaDiemDanh,
        string? tuKhoa)
    {
        var ketQuaPhanQuyen = KiemTraQuyenTruyCap();
        if (ketQuaPhanQuyen is not null)
            return ketQuaPhanQuyen;

        KetQuaThaoTacViewModel ketQua;
        try
        {
            ketQua = string.IsNullOrWhiteSpace(maPhienSuKien) ||
                string.IsNullOrWhiteSpace(maNguoiThamDu) ||
                !Enum.TryParse<KetQuaDiemDanhPhien>(ketQuaDiemDanh, true, out var ketQuaDiemDanhDaParse) ||
                !Enum.IsDefined(ketQuaDiemDanhDaParse)
                ? new KetQuaThaoTacViewModel
                {
                    ThanhCong = false,
                    ThongBao = "Thông tin điểm danh không hợp lệ."
                }
                : await _dichVuModule4.DiemDanhPhienAsync(
                    maPhienSuKien,
                    maNguoiThamDu,
                    ketQuaDiemDanhDaParse,
                    HttpContext.Session.GetString("MaTaiKhoan") ?? string.Empty);
        }
        catch (Exception ex) when (ex is DbUpdateException or DbException)
        {
            _logger.LogError(ex, "Module 4 failed to confirm session attendance.");
            return View("Index", new DiemDanhPhienViewModel
            {
                MaPhienSuKien = maPhienSuKien,
                TuKhoa = tuKhoa,
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
            viewModel = await _dichVuModule4.LayDiemDanhPhienAsync(maPhienSuKien, tuKhoa);
        }
        catch (Exception ex) when (ex is DbUpdateException or DbException)
        {
            _logger.LogError(ex, "Module 4 failed to reload session attendance.");
            return View("Index", new DiemDanhPhienViewModel
            {
                MaPhienSuKien = maPhienSuKien,
                TuKhoa = tuKhoa,
                KetQua = ketQua,
                ThongBao = "Không thể tải lại danh sách điểm danh. Vui lòng làm mới trang."
            });
        }

        viewModel.KetQua = ketQua;
        return View("Index", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> ChoDuyet(string? maPhienSuKien)
    {
        var ketQuaPhanQuyen = KiemTraQuyenTruyCap();
        if (ketQuaPhanQuyen is not null)
            return ketQuaPhanQuyen;

        try
        {
            var viewModel = await _dichVuModule4.LayDangKyPhienChoDuyetAsync(maPhienSuKien);
            viewModel.KetQua = TempData["KetQuaDuyetPhienThongBao"] is not string thongBao
                ? null
                : new KetQuaThaoTacViewModel
                {
                    ThanhCong = TempData["KetQuaDuyetPhienThanhCong"] is bool thanhCong && thanhCong,
                    ThongBao = thongBao
                };
            return View("DangKyChoDuyet", viewModel);
        }
        catch (Exception ex) when (ex is DbUpdateException or DbException)
        {
            _logger.LogError(ex, "Module 4 failed to load pending session registrations.");
            return View("DangKyChoDuyet", new DangKyPhienChoDuyetViewModel
            {
                MaPhienSuKien = maPhienSuKien,
                ThongBao = "Không thể tải đăng ký phiên. Vui lòng thử lại."
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DuyetDangKy(string? maDangKyPhien, string? maPhienSuKien)
    {
        var ketQuaPhanQuyen = KiemTraQuyenTruyCap();
        if (ketQuaPhanQuyen is not null)
            return ketQuaPhanQuyen;

        try
        {
            var ketQua = await _dichVuModule4.DuyetDangKyPhienAsync(maDangKyPhien ?? string.Empty);
            TempData["KetQuaDuyetPhienThongBao"] = ketQua.ThongBao;
            TempData["KetQuaDuyetPhienThanhCong"] = ketQua.ThanhCong;
            return RedirectToAction(nameof(ChoDuyet), new { maPhienSuKien });
        }
        catch (Exception ex) when (ex is DbUpdateException or DbException)
        {
            _logger.LogError(ex, "Module 4 failed to approve a session registration.");
            return View("DangKyChoDuyet", new DangKyPhienChoDuyetViewModel
            {
                MaPhienSuKien = maPhienSuKien,
                KetQua = new KetQuaThaoTacViewModel
                {
                    ThanhCong = false,
                    ThongBao = "Không thể duyệt đăng ký phiên do lỗi dữ liệu. Vui lòng thử lại."
                }
            });
        }
    }

    private IActionResult? KiemTraQuyenTruyCap()
    {
        if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("MaTaiKhoan")))
            return RedirectToAction("DangNhap", "TaiKhoan");

        if (!KiemTraQuyenModule4.CoQuyenTiepDon(HttpContext.Session.GetString("VaiTro")))
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền truy cập chức năng Module 4.");

        return null;
    }
}