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

public class CheckInController : Controller
{
    private readonly IDichVuModule4 _dichVuModule4;
    private readonly ILogger<CheckInController> _logger;

    public CheckInController(
        IDichVuModule4 dichVuModule4,
        ILogger<CheckInController> logger)
    {
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
            return View(model);
        }

        try
        {
            model.DangKy = await _dichVuModule4.TraCuuMaThamDuAsync(model.MaThamDu);
            model.KetQua = string.IsNullOrWhiteSpace(model.DangKy.MaDangKy)
                ? new KetQuaThaoTacViewModel
                {
                    ThanhCong = false,
                    ThongBao = model.DangKy.ThongBao
                }
                : await _dichVuModule4.CheckInAsync(
                    model.MaThamDu,
                    HttpContext.Session.GetString("MaTaiKhoan") ?? string.Empty,
                    model.GhiChu);

            if (model.KetQua?.ThanhCong == true)
                model.DangKy = await _dichVuModule4.TraCuuMaThamDuAsync(model.MaThamDu);
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

        return View(model);
    }

    private IActionResult? KiemTraQuyenTruyCap()
    {
        if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("MaTaiKhoan")))
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        if (!KiemTraQuyenModule4.CoQuyenTiepDon(HttpContext.Session.GetString("VaiTro")))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Bạn không có quyền truy cập chức năng Module 4.");
        }

        return null;
    }
}