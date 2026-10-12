using HTquanlyhoinghi_UNETI2_TI17A3HN.Helper;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Services;
using Microsoft.AspNetCore.Mvc;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers;

public class DiemDanhPhienController : Controller
{
    private readonly IDichVuModule4 _dichVuModule4;

    public DiemDanhPhienController(IDichVuModule4 dichVuModule4)
    {
        _dichVuModule4 = dichVuModule4;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? maPhienSuKien)
    {
        var ketQuaPhanQuyen = KiemTraQuyenQuanTri();
        if (ketQuaPhanQuyen is not null)
            return ketQuaPhanQuyen;

        return View(await _dichVuModule4.LayDiemDanhPhienAsync(maPhienSuKien));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DiemDanh(string? maPhienSuKien, string? maNguoiThamDu)
    {
        var ketQuaPhanQuyen = KiemTraQuyenQuanTri();
        if (ketQuaPhanQuyen is not null)
            return ketQuaPhanQuyen;

        var ketQua = string.IsNullOrWhiteSpace(maPhienSuKien) ||
            string.IsNullOrWhiteSpace(maNguoiThamDu)
            ? new Models.Module4.KetQuaThaoTacViewModel
            {
                ThanhCong = false,
                ThongBao = "Phiên hoặc người tham dự không hợp lệ."
            }
            : await _dichVuModule4.DiemDanhPhienAsync(maPhienSuKien, maNguoiThamDu);

        var viewModel = await _dichVuModule4.LayDiemDanhPhienAsync(maPhienSuKien);
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