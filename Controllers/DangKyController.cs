using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module3;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers;

public class DangKyController : Controller
{
    private readonly AppDbContext _context;
    private readonly DangKyService _dangKyService;

    public DangKyController(AppDbContext context, DangKyService dangKyService)
    {
        _context = context;
        _dangKyService = dangKyService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? tab,
        string? tuKhoaSuKien,
        string? maLoaiSuKienSuKien,
        string? tenNguoiThamDu,
        string? maSuKien,
        string? trangThai,
        string? maLoaiSuKien,
        DateTime? tuNgay,
        DateTime? denNgay,
        string? sapXep)
    {
        var taiKhoan = await LayTaiKhoanHienTai();
        if (taiKhoan == null)
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var laQuanTri = LaQuanTri();
        var nguoiThamDu = await _context.NguoiThamDus
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.MaTaiKhoan == taiKhoan.MaTaiKhoan);

        if (!laQuanTri && nguoiThamDu == null)
        {
            TempData["Loi"] = "Hãy hoàn thiện hồ sơ người tham dự trước khi đăng ký sự kiện.";
            return RedirectToAction("HoSo", "NguoiThamDu");
        }

        if (!laQuanTri && nguoiThamDu is { TrangThai: false })
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Người tham dự đã ngừng hoạt động.");
        }

        var tabHienTai = tab switch
        {
            "ca-nhan" => "ca-nhan",
            "tim-kiem" when laQuanTri => "tim-kiem",
            "tim-kiem" => "su-kien",
            _ => "su-kien"
        };

        var model = new DangKyModule3ViewModel
        {
            Tab = tabHienTai,
            LaQuanTri = laQuanTri,
            CoHoSoNguoiThamDu = nguoiThamDu != null,
            TuKhoaSuKien = tuKhoaSuKien,
            MaLoaiSuKienSuKien = maLoaiSuKienSuKien,
            TenNguoiThamDu = tenNguoiThamDu,
            MaSuKien = maSuKien,
            TrangThai = trangThai,
            MaLoaiSuKien = maLoaiSuKien,
            TuNgay = tuNgay,
            DenNgay = denNgay,
            SapXep = sapXep == "ngaytochuc" ? "ngaytochuc" : "ngaydangky",
            LoaiSuKiens = await _context.LoaiSuKiens.AsNoTracking()
                .OrderBy(l => l.TenLoaiSuKien)
                .Select(l => new LoaiSuKienLuaChonViewModel
                {
                    MaLoaiSuKien = l.MaLoaiSuKien,
                    TenLoaiSuKien = l.TenLoaiSuKien
                })
                .ToListAsync()
        };

        var trangThaiHieuLuc = new[]
        {
            TrangThaiDangKy.ChoDuyet,
            TrangThaiDangKy.DaDuyet,
            TrangThaiDangKy.DaCheckIn,
            TrangThaiDangKy.HoanThanh
        };
        var batDauTu = DateTime.Now;
        var querySuKien = _context.SuKiens.AsNoTracking()
            .Where(s => s.TrangThai == nameof(TrangThaiSuKien.MoDangKy)
                && s.HanDangKy > batDauTu
                && s.ThoiGianBatDau > batDauTu
                && s.DiaDiem != null);

        if (!string.IsNullOrWhiteSpace(tuKhoaSuKien))
        {
            var tuKhoa = tuKhoaSuKien.Trim();
            querySuKien = querySuKien.Where(s => s.TenSuKien.Contains(tuKhoa));
        }

        if (!string.IsNullOrWhiteSpace(maLoaiSuKienSuKien))
        {
            querySuKien = querySuKien.Where(s => s.MaLoaiSuKien == maLoaiSuKienSuKien);
        }

        model.SuKiens = await querySuKien
            .Select(s => new SuKienMoDangKyDongViewModel
            {
                MaSuKien = s.MaSuKien,
                TenSuKien = s.TenSuKien,
                TenLoaiSuKien = s.LoaiSuKien == null ? string.Empty : s.LoaiSuKien.TenLoaiSuKien,
                TenDiaDiem = s.DiaDiem == null ? string.Empty : s.DiaDiem.TenDiaDiem,
                SucChua = s.DiaDiem == null
                    ? 0
                    : s.SoLuongToiDa < s.DiaDiem.SucChuaToiDa
                        ? s.SoLuongToiDa
                        : s.DiaDiem.SucChuaToiDa,
                BatDau = s.ThoiGianBatDau,
                SoLuongHieuLuc = _context.DangKyThamDus.Count(d =>
                    d.MaSuKien == s.MaSuKien && trangThaiHieuLuc.Contains(d.TrangThai))
            })
            .Where(s => s.SoLuongHieuLuc < s.SucChua)
            .OrderBy(s => s.BatDau)
            .ToListAsync();

        if (nguoiThamDu != null)
        {
            var queryCuaToi = _context.DangKyThamDus.AsNoTracking()
                .Where(d => d.MaNguoiThamDu == nguoiThamDu.MaNguoiThamDu);
            if (KiemTraBoLoc(model))
            {
                queryCuaToi = ApDungBoLoc(queryCuaToi, trangThai, maLoaiSuKien, tuNgay, denNgay);
                model.DangKysCuaToi = await TaoQueryDong(queryCuaToi, model.SapXep).ToListAsync();
            }
        }

        if (laQuanTri && tabHienTai == "tim-kiem" && KiemTraBoLoc(model))
        {
            var queryTimKiem = _context.DangKyThamDus.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(tenNguoiThamDu))
            {
                var tuKhoa = tenNguoiThamDu.Trim();
                queryTimKiem = queryTimKiem.Where(d => d.NguoiThamDu != null
                    && d.NguoiThamDu.HoTen.Contains(tuKhoa));
            }

            if (!string.IsNullOrWhiteSpace(maSuKien))
            {
                queryTimKiem = queryTimKiem.Where(d => d.MaSuKien == maSuKien);
            }

            queryTimKiem = ApDungBoLoc(queryTimKiem, trangThai, maLoaiSuKien, tuNgay, denNgay);
            model.KetQuaTimKiem = await TaoQueryDong(queryTimKiem, model.SapXep).ToListAsync();
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult SuKienMoDangKy(string? tuKhoa, string? maLoaiSuKien) =>
        RedirectToAction(nameof(Index), new
        {
            tab = "su-kien",
            tuKhoaSuKien = tuKhoa,
            maLoaiSuKienSuKien = maLoaiSuKien
        });

    [HttpGet]
    public async Task<IActionResult> DangKy(string maSuKien)
    {
        if (string.IsNullOrWhiteSpace(maSuKien))
        {
            return BadRequest();
        }

        var taiKhoan = await LayTaiKhoanHienTai();
        if (taiKhoan == null)
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var nguoiThamDu = await _context.NguoiThamDus
            .FirstOrDefaultAsync(n => n.MaTaiKhoan == taiKhoan.MaTaiKhoan);
        if (nguoiThamDu == null)
        {
            TempData["Loi"] = "Hãy hoàn thiện hồ sơ người tham dự trước khi đăng ký sự kiện.";
            return RedirectToAction("HoSo", "NguoiThamDu");
        }

        if (!nguoiThamDu.TrangThai)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Người tham dự đã ngừng hoạt động.");
        }

        var suKien = await _context.SuKiens
            .AsNoTracking()
            .Where(s => s.MaSuKien == maSuKien)
            .Select(s => new
            {
                s.MaSuKien,
                s.TenSuKien,
                s.ThoiGianBatDau,
                s.ThoiGianKetThuc,
                s.TrangThai,
                s.SoLuongToiDa,
                TenLoaiSuKien = s.LoaiSuKien == null ? string.Empty : s.LoaiSuKien.TenLoaiSuKien,
                TenDiaDiem = s.DiaDiem == null ? string.Empty : s.DiaDiem.TenDiaDiem,
                SucChua = s.DiaDiem == null ? 0 : s.DiaDiem.SucChuaToiDa
            })
            .FirstOrDefaultAsync();
        if (suKien == null)
        {
            return NotFound();
        }

        var (coTheDangKy, lyDoKhongTheDangKy) =
            await _dangKyService.KiemTraDieuKienDangKyAsync(
                nguoiThamDu.MaNguoiThamDu,
                maSuKien);

        var model = new DangKyFormViewModel
        {
            MaSuKien = suKien.MaSuKien,
            TenSuKien = suKien.TenSuKien,
            TenLoaiSuKien = suKien.TenLoaiSuKien,
            TenDiaDiem = suKien.TenDiaDiem,
            SucChuaDiaDiem = suKien.SucChua,
            SoLuongToiDaSuKien = suKien.SoLuongToiDa,
            SucChuaDangKy = suKien.SucChua == 0
                ? 0
                : Math.Min(suKien.SoLuongToiDa, suKien.SucChua),
            ThoiGianBatDau = suKien.ThoiGianBatDau,
            ThoiGianKetThuc = suKien.ThoiGianKetThuc,
            TrangThaiSuKien = suKien.TrangThai,
            SoLuongDangKyHieuLuc = await _dangKyService.DemSoDangKyHieuLucAsync(maSuKien),
            CoTheDangKy = coTheDangKy,
            LyDoKhongTheDangKy = lyDoKhongTheDangKy,
            HoTenNguoiThamDu = nguoiThamDu.HoTen,
            SoDienThoaiNguoiThamDu = nguoiThamDu.SoDienThoai,
            EmailNguoiThamDu = nguoiThamDu.Email,
            DonViCongTacNguoiThamDu = nguoiThamDu.DonViCongTac
        };
        return View(model);
    }

    [HttpPost]
    [ActionName("DangKy")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XacNhanDangKy(string maSuKien, string? ghiChu)
    {
        var taiKhoan = await LayTaiKhoanHienTai();
        if (taiKhoan == null)
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var nguoiThamDu = await _context.NguoiThamDus
            .FirstOrDefaultAsync(n => n.MaTaiKhoan == taiKhoan.MaTaiKhoan);
        if (nguoiThamDu == null)
        {
            TempData["Loi"] = "Hãy hoàn thiện hồ sơ người tham dự trước khi đăng ký sự kiện.";
            return RedirectToAction("HoSo", "NguoiThamDu");
        }

        var (thanhCong, lyDo) = await _dangKyService.DangKySuKienAsync(
            nguoiThamDu.MaNguoiThamDu,
            maSuKien,
            ghiChu);
        if (!thanhCong)
        {
            TempData["Loi"] = lyDo;
            return RedirectToAction(nameof(DangKy), new { maSuKien });
        }

        TempData["ThongBao"] = "Đăng ký thành công, vui lòng chờ duyệt.";
        return RedirectToAction(nameof(Index), new { tab = "ca-nhan" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Huy(string maDangKy, string? lyDoHuy)
    {
        var taiKhoan = await LayTaiKhoanHienTai();
        if (taiKhoan == null)
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var nguoiThamDu = await _context.NguoiThamDus
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.MaTaiKhoan == taiKhoan.MaTaiKhoan);
        if (nguoiThamDu == null || !nguoiThamDu.TrangThai)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Người tham dự không hoạt động hoặc chưa có hồ sơ.");
        }

        var (thanhCong, lyDo) = await _dangKyService.HuyDangKyAsync(
            maDangKy,
            nguoiThamDu.MaNguoiThamDu,
            lyDoHuy?.Trim() ?? string.Empty);
        TempData[thanhCong ? "ThongBao" : "Loi"] = thanhCong
            ? "Hủy đăng ký thành công."
            : lyDo;
        return RedirectToAction(nameof(Index), new { tab = "ca-nhan" });
    }

    [HttpGet]
    public IActionResult DanhSachDangKy(
        string? trangThai,
        string? maLoaiSuKien,
        DateTime? tuNgay,
        DateTime? denNgay,
        string? sapXep) =>
        RedirectToAction(nameof(Index), new
        {
            tab = "ca-nhan",
            trangThai,
            maLoaiSuKien,
            tuNgay,
            denNgay,
            sapXep
        });

    [HttpGet]
    public IActionResult TimKiem(
        string? tenNguoiThamDu,
        string? maSuKien,
        string? trangThai,
        string? maLoaiSuKien,
        DateTime? tuNgay,
        DateTime? denNgay,
        string? sapXep) =>
        LaQuanTri()
            ? RedirectToAction(nameof(Index), new
            {
                tab = "tim-kiem",
                tenNguoiThamDu,
                maSuKien,
                trangThai,
                maLoaiSuKien,
                tuNgay,
                denNgay,
                sapXep
            })
            : StatusCode(StatusCodes.Status403Forbidden, "Chỉ ban tổ chức mới được tìm kiếm danh sách đăng ký.");

    private bool KiemTraBoLoc(DangKyModule3ViewModel model)
    {
        var hopLe = KiemTraTrangThai(model.TrangThai);
        if (model.TuNgay.HasValue && model.DenNgay.HasValue
            && model.TuNgay.Value.Date > model.DenNgay.Value.Date)
        {
            ModelState.AddModelError(string.Empty, "Từ ngày không được sau đến ngày.");
            hopLe = false;
        }

        return hopLe;
    }

    private IQueryable<DangKyThamDu> ApDungBoLoc(
        IQueryable<DangKyThamDu> query,
        string? trangThai,
        string? maLoaiSuKien,
        DateTime? tuNgay,
        DateTime? denNgay)
    {
        if (Enum.TryParse<TrangThaiDangKy>(trangThai, out var trangThaiEnum))
        {
            query = query.Where(d => d.TrangThai == trangThaiEnum);
        }

        if (!string.IsNullOrWhiteSpace(maLoaiSuKien))
        {
            query = query.Where(d => d.SuKien != null &&
                d.SuKien.MaLoaiSuKien == maLoaiSuKien);
        }

        if (tuNgay.HasValue)
        {
            query = query.Where(d => d.NgayDangKy >= tuNgay.Value.Date);
        }

        if (denNgay.HasValue)
        {
            var ngaySauDenNgay = denNgay.Value.Date.AddDays(1);
            query = query.Where(d => d.NgayDangKy < ngaySauDenNgay);
        }

        return query;
    }

    private bool KiemTraTrangThai(string? trangThai)
    {
        if (string.IsNullOrWhiteSpace(trangThai))
        {
            return true;
        }

        if (Enum.TryParse<TrangThaiDangKy>(trangThai, out var trangThaiEnum)
            && Enum.IsDefined(trangThaiEnum))
        {
            return true;
        }

        ModelState.AddModelError(string.Empty, "Trạng thái đăng ký không hợp lệ.");
        return false;
    }

    private static IQueryable<DangKyDongViewModel> TaoQueryDong(
        IQueryable<DangKyThamDu> query,
        string sapXep)
    {
        var ordered = sapXep == "ngaytochuc"
            ? query.OrderBy(d => d.SuKien == null ? DateTime.MaxValue : d.SuKien.ThoiGianBatDau)
            : query.OrderByDescending(d => d.NgayDangKy);

        return ordered.Select(d => new DangKyDongViewModel
        {
            MaDangKy = d.MaDangKy,
            MaNguoiThamDu = d.MaNguoiThamDu,
            MaSuKien = d.MaSuKien,
            TenNguoiThamDu = d.NguoiThamDu == null ? string.Empty : d.NguoiThamDu.HoTen,
            TenSuKien = d.SuKien == null ? string.Empty : d.SuKien.TenSuKien,
            MaLoaiSuKien = d.SuKien == null ? string.Empty : d.SuKien.MaLoaiSuKien,
            NgayDangKy = d.NgayDangKy,
            ThoiGianBatDau = d.SuKien == null ? DateTime.MaxValue : d.SuKien.ThoiGianBatDau,
            TrangThai = d.TrangThai,
            HanHuy = d.SuKien == null
                ? DateTime.MinValue
                : d.SuKien.ThoiGianBatDau.AddDays(-DangKyService.SoNgayToiThieuTruocKhiHuy)
        });
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

    private bool LaQuanTri() =>
        string.Equals(
            HttpContext.Session.GetString("VaiTro"),
            "Admin",
            StringComparison.OrdinalIgnoreCase);
}
