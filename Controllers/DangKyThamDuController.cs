// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 4 - Xét duyệt đăng ký, mã tham dự, check-in, điểm danh.

using HTquanlyhoinghi_UNETI2_TI17A3HN.Helper;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module4;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Services;
using Microsoft.AspNetCore.Mvc;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers;

public class DangKyThamDuController : Controller
{
    private readonly IDichVuModule4 _dichVuModule4;

    public DangKyThamDuController(IDichVuModule4 dichVuModule4)
    {
        _dichVuModule4 = dichVuModule4;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var ketQuaPhanQuyen = KiemTraQuyenQuanTri();
        if (ketQuaPhanQuyen is not null)
        {
            return ketQuaPhanQuyen;
        }

        return View(new DuyetDangKyViewModel
        {
            ThongBaoTichHop = "Chưa tích hợp Module 3."
        });
    }

    [HttpGet]
    public IActionResult ChiTiet(string? maDangKy)
    {
        var ketQuaPhanQuyen = KiemTraQuyenQuanTri();
        if (ketQuaPhanQuyen is not null)
        {
            return ketQuaPhanQuyen;
        }

        return View(new ChiTietDangKyViewModel
        {
            MaDangKy = maDangKy,
            ThongBao = "Chưa tích hợp Module 3."
        });
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

        var ketQua = await _dichVuModule4.DuyetDangKyAsync(model.MaDangKy);
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

        var ketQua = await _dichVuModule4.TuChoiDangKyAsync(model.MaDangKy, model.LyDoTuChoi);
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