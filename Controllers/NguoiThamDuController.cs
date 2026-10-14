using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers;

public class NguoiThamDuController : Controller
{
    private readonly AppDbContext _context;

    public NguoiThamDuController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? timKiem)
    {
        if (!LaQuanTri())
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Chỉ ban tổ chức mới được quản lý danh sách người tham dự.");
        }

        var query = _context.NguoiThamDus.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(timKiem))
        {
            var tuKhoa = timKiem.Trim();
            query = query.Where(n =>
                n.HoTen.Contains(tuKhoa) ||
                n.Email.Contains(tuKhoa) ||
                n.SoDienThoai.Contains(tuKhoa));
        }

        ViewBag.TimKiem = timKiem;
        return View(await query.OrderBy(n => n.HoTen).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> HoSo()
    {
        var maTaiKhoan = HttpContext.Session.GetString("MaTaiKhoan");
        if (string.IsNullOrWhiteSpace(maTaiKhoan))
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var nguoiThamDu = await _context.NguoiThamDus
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.MaTaiKhoan == maTaiKhoan);

        if (nguoiThamDu == null)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(maTaiKhoan);
            if (taiKhoan == null || !taiKhoan.TrangThai)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Tài khoản không tồn tại hoặc đã bị khóa.");
            }

            return View(new NguoiThamDuHoSoViewModel
            {
                HoTen = taiKhoan.HoTen,
                Email = taiKhoan.Email
            });
        }

        return View(new NguoiThamDuHoSoViewModel
        {
            HoTen = nguoiThamDu.HoTen,
            SoDienThoai = nguoiThamDu.SoDienThoai,
            Email = nguoiThamDu.Email,
            DonViCongTac = nguoiThamDu.DonViCongTac
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HoSo(NguoiThamDuHoSoViewModel model)
    {
        var maTaiKhoan = HttpContext.Session.GetString("MaTaiKhoan");
        if (string.IsNullOrWhiteSpace(maTaiKhoan))
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var taiKhoan = await _context.TaiKhoans
            .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan && t.TrangThai);
        if (taiKhoan == null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Tài khoản không tồn tại hoặc đã bị khóa.");
        }

        var nguoiThamDu = await _context.NguoiThamDus
            .FirstOrDefaultAsync(n => n.MaTaiKhoan == maTaiKhoan);
        var email = model.Email.Trim();

        var emailNguoiKhac = _context.NguoiThamDus
            .Where(n => n.Email == email);
        if (nguoiThamDu != null)
        {
            emailNguoiKhac = emailNguoiKhac
                .Where(n => n.MaNguoiThamDu != nguoiThamDu.MaNguoiThamDu);
        }

        if (await emailNguoiKhac.AnyAsync())
        {
            ModelState.AddModelError(nameof(model.Email), "Email này đã được dùng bởi người tham dự khác.");
            return View(model);
        }

        if (nguoiThamDu == null)
        {
            nguoiThamDu = new NguoiThamDu
            {
                MaNguoiThamDu = Guid.NewGuid().ToString("N"),
                MaTaiKhoan = maTaiKhoan,
                TrangThai = true
            };
            _context.NguoiThamDus.Add(nguoiThamDu);
        }

        nguoiThamDu.HoTen = model.HoTen.Trim();
        nguoiThamDu.SoDienThoai = model.SoDienThoai.Trim();
        nguoiThamDu.Email = email;
        nguoiThamDu.DonViCongTac = model.DonViCongTac.Trim();

        await _context.SaveChangesAsync();

        TempData["ThongBao"] = "Cập nhật hồ sơ thành công.";
        return RedirectToAction(nameof(HoSo));
    }

    // =========================================================
    // 4. HIỂN THỊ FORM SỬA
    // =========================================================
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (!LaQuanTri())
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Chỉ ban tổ chức mới được sửa người tham dự.");
        }

        var nguoiThamDu = await _context.NguoiThamDus
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.MaNguoiThamDu == id);

        if (nguoiThamDu == null)
        {
            return NotFound();
        }

        return View(nguoiThamDu);
    }

    // =========================================================
    // 5. XỬ LÝ SỬA NGƯỜI THAM DỰ
    // =========================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, NguoiThamDu nguoiThamDu)
    {
        if (!LaQuanTri())
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Chỉ ban tổ chức mới được sửa người tham dự.");
        }

        if (id != nguoiThamDu.MaNguoiThamDu)
        {
            return NotFound();
        }

        var banGhi = await _context.NguoiThamDus
            .FirstOrDefaultAsync(n => n.MaNguoiThamDu == id);

        if (banGhi == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(nguoiThamDu);
        }

        var email = nguoiThamDu.Email.Trim();
        bool trungEmail = await _context.NguoiThamDus
            .AnyAsync(n => n.Email == email && n.MaNguoiThamDu != id);

        if (trungEmail)
        {
            ModelState.AddModelError(nameof(nguoiThamDu.Email),
                "Email này đã được dùng bởi người tham dự khác.");
            return View(nguoiThamDu);
        }

        banGhi.HoTen = nguoiThamDu.HoTen.Trim();
        banGhi.SoDienThoai = nguoiThamDu.SoDienThoai.Trim();
        banGhi.Email = email;
        banGhi.DonViCongTac = nguoiThamDu.DonViCongTac.Trim();

        await _context.SaveChangesAsync();

        TempData["ThongBao"] = "Đã cập nhật người tham dự.";
        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // 6. KHÓA / MỞ KHÓA NGƯỜI THAM DỰ
    // =========================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KhoaMoKhoa(string id)
    {
        if (!LaQuanTri())
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Chỉ ban tổ chức mới được quản lý danh sách người tham dự.");
        }

        var nguoiThamDu = await _context.NguoiThamDus
            .FirstOrDefaultAsync(n => n.MaNguoiThamDu == id);

        if (nguoiThamDu == null)
        {
            return NotFound();
        }

        nguoiThamDu.TrangThai = !nguoiThamDu.TrangThai;
        await _context.SaveChangesAsync();

        TempData["ThongBao"] = nguoiThamDu.TrangThai
            ? "Đã mở khóa người tham dự."
            : "Đã khóa người tham dự.";
        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // 7. XÓA NGƯỜI THAM DỰ
    // =========================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (!LaQuanTri())
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Chỉ ban tổ chức mới được xóa người tham dự.");
        }

        var nguoiThamDu = await _context.NguoiThamDus
            .FirstOrDefaultAsync(n => n.MaNguoiThamDu == id);

        if (nguoiThamDu == null)
        {
            return NotFound();
        }

        var daCoDangKySuKien = await _context.DangKyThamDus
            .AnyAsync(d => d.MaNguoiThamDu == id);
        var daCoDangKyPhien = await _context.DangKyPhiens
            .AnyAsync(d => d.MaNguoiThamDu == id);

        if (daCoDangKySuKien || daCoDangKyPhien)
        {
            TempData["Loi"] = "Không thể xóa người tham dự đã có lịch sử đăng ký. Bạn có thể khóa người tham dự thay vì xóa.";
            return RedirectToAction(nameof(Index));
        }

        _context.NguoiThamDus.Remove(nguoiThamDu);
        await _context.SaveChangesAsync();

        TempData["ThongBao"] = "Đã xóa người tham dự.";
        return RedirectToAction(nameof(Index));
    }

    private bool LaQuanTri() =>
        string.Equals(
            HttpContext.Session.GetString("VaiTro"),
            "Admin",
            StringComparison.OrdinalIgnoreCase);

    private async Task NapTaiKhoanChuaCoNguoiThamDu(string? selected = null)
    {
        var daCoHoSo = _context.NguoiThamDus.Select(n => n.MaTaiKhoan);
        var taiKhoans = await _context.TaiKhoans
            .Where(t => t.TrangThai && !daCoHoSo.Contains(t.MaTaiKhoan))
            .OrderBy(t => t.HoTen)
            .Select(t => new { t.MaTaiKhoan, t.HoTen, t.Email })
            .ToListAsync();

        ViewBag.TaiKhoans = new SelectList(
            taiKhoans.Select(t => new
            {
                t.MaTaiKhoan,
                HienThi = $"{t.HoTen} ({t.Email})"
            }),
            "MaTaiKhoan",
            "HienThi",
            selected);
    }
}
