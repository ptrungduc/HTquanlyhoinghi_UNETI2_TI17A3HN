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
        nguoiThamDu.Email = model.Email.Trim();
        nguoiThamDu.DonViCongTac = model.DonViCongTac.Trim();

        await _context.SaveChangesAsync();
        TempData["ThongBao"] = "Đã lưu hồ sơ người tham dự.";
        return RedirectToAction("SuKienMoDangKy", "DangKy");
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!LaQuanTri())
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Chỉ ban tổ chức mới được quản lý danh sách người tham dự.");
        }

        await NapTaiKhoanChuaCoNguoiThamDu();
        return View(new NguoiThamDuQuanTriViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NguoiThamDuQuanTriViewModel model)
    {
        if (!LaQuanTri())
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Chỉ ban tổ chức mới được quản lý danh sách người tham dự.");
        }

        var taiKhoanHopLe = await _context.TaiKhoans.AnyAsync(t =>
            t.MaTaiKhoan == model.MaTaiKhoan && t.TrangThai);
        var daCoHoSo = await _context.NguoiThamDus
            .AnyAsync(n => n.MaTaiKhoan == model.MaTaiKhoan);
        if (!taiKhoanHopLe)
        {
            ModelState.AddModelError(nameof(model.MaTaiKhoan), "Tài khoản không tồn tại hoặc đã bị khóa.");
        }
        else if (daCoHoSo)
        {
            ModelState.AddModelError(nameof(model.MaTaiKhoan), "Tài khoản đã có hồ sơ người tham dự.");
        }

        if (!ModelState.IsValid)
        {
            await NapTaiKhoanChuaCoNguoiThamDu(model.MaTaiKhoan);
            return View(model);
        }

        var maNguoiThamDu = Guid.NewGuid().ToString("N");
        _context.NguoiThamDus.Add(new NguoiThamDu
        {
            MaNguoiThamDu = maNguoiThamDu,
            MaTaiKhoan = model.MaTaiKhoan,
            HoTen = model.HoTen.Trim(),
            SoDienThoai = model.SoDienThoai.Trim(),
            Email = model.Email.Trim(),
            DonViCongTac = model.DonViCongTac.Trim(),
            TrangThai = true
        });
        await _context.SaveChangesAsync();
        TempData["ThongBao"] = "Đã thêm người tham dự.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string? id)
    {
        if (!LaQuanTri())
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Chỉ ban tổ chức mới được quản lý danh sách người tham dự.");
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        var nguoiThamDu = await _context.NguoiThamDus
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.MaNguoiThamDu == id);
        if (nguoiThamDu == null)
        {
            return NotFound();
        }

        return View(new NguoiThamDuQuanTriViewModel
        {
            MaNguoiThamDu = nguoiThamDu.MaNguoiThamDu,
            MaTaiKhoan = nguoiThamDu.MaTaiKhoan,
            HoTen = nguoiThamDu.HoTen,
            SoDienThoai = nguoiThamDu.SoDienThoai,
            Email = nguoiThamDu.Email,
            DonViCongTac = nguoiThamDu.DonViCongTac
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, NguoiThamDuQuanTriViewModel model)
    {
        if (!LaQuanTri())
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Chỉ ban tổ chức mới được quản lý danh sách người tham dự.");
        }

        if (id != model.MaNguoiThamDu)
        {
            return BadRequest();
        }

        var nguoiThamDu = await _context.NguoiThamDus
            .FirstOrDefaultAsync(n => n.MaNguoiThamDu == id);
        if (nguoiThamDu == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.MaTaiKhoan = nguoiThamDu.MaTaiKhoan;
            return View(model);
        }

        nguoiThamDu.HoTen = model.HoTen.Trim();
        nguoiThamDu.SoDienThoai = model.SoDienThoai.Trim();
        nguoiThamDu.Email = model.Email.Trim();
        nguoiThamDu.DonViCongTac = model.DonViCongTac.Trim();
        await _context.SaveChangesAsync();
        TempData["ThongBao"] = "Đã cập nhật người tham dự.";
        return RedirectToAction(nameof(Index));
    }

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