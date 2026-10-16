using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers;

public class DangKyPhienController : Controller
{
    private readonly AppDbContext _context;
    private readonly DangKyService _dangKyService;

    public DangKyPhienController(AppDbContext context, DangKyService dangKyService)
    {
        _context = context;
        _dangKyService = dangKyService;
    }

    [HttpGet]
    public async Task<IActionResult> DangKy(string maPhien)
    {
        if (string.IsNullOrWhiteSpace(maPhien))
        {
            return BadRequest();
        }

        var taiKhoan = await LayTaiKhoanHienTai();
        if (taiKhoan == null)
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var nguoiThamDu = await _context.NguoiThamDus
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.MaTaiKhoan == taiKhoan.MaTaiKhoan);
        if (nguoiThamDu == null)
        {
            TempData["Loi"] = "Hãy hoàn thiện hồ sơ người tham dự trước khi đăng ký phiên.";
            return RedirectToAction("HoSo", "NguoiThamDu");
        }

        if (!nguoiThamDu.TrangThai)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Người tham dự đã ngừng hoạt động.");
        }

        var phien = await _context.PhienSuKiens
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.MaPhienSuKien == maPhien);
        if (phien == null)
        {
            return NotFound();
        }

        ViewBag.MaPhien = maPhien;
        ViewBag.TenPhien = phien.TenPhien;
        ViewBag.ThoiGianBatDau = phien.ThoiGianBatDau;
        ViewBag.ThoiGianKetThuc = phien.ThoiGianKetThuc;
        return View();
    }

    [HttpPost]
    [ActionName("DangKy")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XacNhanDangKy(string maPhien)
    {
        var taiKhoan = await LayTaiKhoanHienTai();
        if (taiKhoan == null)
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var nguoiThamDu = await _context.NguoiThamDus
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.MaTaiKhoan == taiKhoan.MaTaiKhoan);
        if (nguoiThamDu == null)
        {
            TempData["Loi"] = "Hãy hoàn thiện hồ sơ người tham dự trước khi đăng ký phiên.";
            return RedirectToAction("HoSo", "NguoiThamDu");
        }

        if (!nguoiThamDu.TrangThai)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Người tham dự đã ngừng hoạt động.");
        }

        var (thanhCong, lyDo) = await _dangKyService.DangKyPhienAsync(
            nguoiThamDu.MaNguoiThamDu,
            maPhien);
        TempData[thanhCong ? "ThongBao" : "Loi"] = thanhCong
            ? "Đăng ký phiên thành công."
            : lyDo;
        return RedirectToAction("DanhSachDangKy", "DangKy");
    }

    private async Task<TaiKhoan?> LayTaiKhoanHienTai()
    {
        var maTaiKhoan = HttpContext.Session.GetString("MaTaiKhoan");
        if (string.IsNullOrWhiteSpace(maTaiKhoan))
        {
            return null;
        }

        return await _context.TaiKhoans
            .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan && t.TrangThai);
    }
}
