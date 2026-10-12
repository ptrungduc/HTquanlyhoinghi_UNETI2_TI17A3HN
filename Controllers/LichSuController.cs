// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 5 - Lịch sử tham dự (Bước 1).

using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module5;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers;

public class LichSuController : Controller
{
    private readonly AppDbContext _context;

    public LichSuController(AppDbContext context)
    {
        _context = context;
    }

    // =================================================================
    // LỊCH SỬ CÁ NHÂN - Người tham dự xem lịch sử của chính mình
    // =================================================================
    [HttpGet]
    public async Task<IActionResult> CaNhan()
    {
        // Bước 1: Kiểm tra đăng nhập
        var maTaiKhoan = HttpContext.Session.GetString("MaTaiKhoan");
        if (string.IsNullOrWhiteSpace(maTaiKhoan))
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        // Bước 2: Tìm NguoiThamDu theo tài khoản trong Session
        // Không nhận id từ URL nên không thể xem lịch sử người khác.
        var nguoiThamDu = await _context.NguoiThamDus
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.MaTaiKhoan == maTaiKhoan);

        if (nguoiThamDu == null)
        {
            return View(new LichSuThamDuViewModel
            {
                HoTenNguoiThamDu = HttpContext.Session.GetString("HoTen") ?? "",
                DanhSachDangKy = []
            });
        }

        var maNguoiThamDu = nguoiThamDu.MaNguoiThamDu;

        // Bước 3: Đăng ký của chính người này
        //   Where:             lọc theo MaNguoiThamDu
        //   OrderByDescending: đăng ký mới nhất lên đầu
        //   Select:            projection, EF Core dịch thành LEFT JOIN
        //   Enum giữ nguyên ở SQL, chuyển sang chuỗi ở C# (Bước 4)
        var dsDangKy = await _context.DangKyThamDus
            .AsNoTracking()
            .Where(d => d.MaNguoiThamDu == maNguoiThamDu)
            .OrderByDescending(d => d.NgayDangKy)
            .Select(d => new
            {
                d.MaDangKy,
                d.MaSuKien,
                TenSuKien = d.SuKien != null ? d.SuKien.TenSuKien : "",
                ThoiGianBatDau = d.SuKien != null ? d.SuKien.ThoiGianBatDau : DateTime.MinValue,
                ThoiGianKetThuc = d.SuKien != null ? d.SuKien.ThoiGianKetThuc : DateTime.MinValue,
                TenDiaDiem = d.SuKien != null && d.SuKien.DiaDiem != null ? d.SuKien.DiaDiem.TenDiaDiem : "",
                DiaChi = d.SuKien != null && d.SuKien.DiaDiem != null ? d.SuKien.DiaDiem.DiaChi : "",
                d.NgayDangKy,
                d.TrangThai,
                ThoiGianCheckIn = d.CheckIn != null ? d.CheckIn.ThoiGianCheckIn : (DateTime?)null,
                d.MaThamDu
            })
            .ToListAsync();

        // Bước 4: Chuyển sang ViewModel, enum -> chuỗi trong C#
        var danhSachDangKy = dsDangKy
            .Select(x => new LichSuDangKyItem
            {
                MaDangKy = x.MaDangKy,
                MaSuKien = x.MaSuKien,
                TenSuKien = x.TenSuKien,
                ThoiGianBatDau = x.ThoiGianBatDau,
                ThoiGianKetThuc = x.ThoiGianKetThuc,
                TenDiaDiem = x.TenDiaDiem,
                DiaChi = x.DiaChi,
                NgayDangKy = x.NgayDangKy,
                TrangThai = x.TrangThai.ToString(),
                ThoiGianCheckIn = x.ThoiGianCheckIn,
                MaThamDu = x.MaThamDu
            })
            .ToList();

        // Bước 5: Session đã đăng ký, nhóm theo MaSuKien
        var phienTheoSuKien = await LayPhienCuaNguoiThamDuTheoSuKienAsync(maNguoiThamDu);

        // Bước 6: Gắn session vào từng đăng ký theo MaSuKien
        foreach (var item in danhSachDangKy)
        {
            item.DanhSachPhien = phienTheoSuKien.GetValueOrDefault(item.MaSuKien) ?? [];
        }

        return View(new LichSuThamDuViewModel
        {
            HoTenNguoiThamDu = nguoiThamDu.HoTen,
            DanhSachDangKy = danhSachDangKy
        });
    }


    // =================================================================
    // CHI TIẾT SỰ KIỆN - Admin xem lịch sử đăng ký của một sự kiện
    // =================================================================
    [HttpGet]
    public async Task<IActionResult> ChiTietSuKien(string id)
    {
        // Bước 1: Kiểm tra quyền Admin ở server
        var vaiTro = HttpContext.Session.GetString("VaiTro");
        if (vaiTro != "Admin")
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        // Bước 2: Sự kiện kèm loại, địa điểm, phiên
        var suKien = await _context.SuKiens
            .AsNoTracking()
            .Include(s => s.LoaiSuKien)
            .Include(s => s.DiaDiem)
            .Include(s => s.PhienSuKiens.OrderBy(p => p.ThoiGianBatDau))
            .FirstOrDefaultAsync(s => s.MaSuKien == id);

        if (suKien == null)
        {
            return NotFound();
        }

        // Bước 3: Đăng ký của sự kiện (enum giữ nguyên ở SQL)
        var dsDangKy = await _context.DangKyThamDus
            .AsNoTracking()
            .Where(d => d.MaSuKien == id)
            .OrderByDescending(d => d.NgayDangKy)
            .Select(d => new
            {
                d.MaDangKy,
                d.MaNguoiThamDu,
                HoTenNguoiThamDu = d.NguoiThamDu != null ? d.NguoiThamDu.HoTen : "",
                Email = d.NguoiThamDu != null ? d.NguoiThamDu.Email : "",
                d.NgayDangKy,
                d.TrangThai,
                d.NgayDuyet,
                ThoiGianCheckIn = d.CheckIn != null ? d.CheckIn.ThoiGianCheckIn : (DateTime?)null,
                d.MaThamDu
            })
            .ToListAsync();

        // Bước 4: Chuyển sang ViewModel, tính KetQuaThamGia trên C#
        //         (switch expression không dịch được sang SQL)
        var danhSachDangKy = dsDangKy
            .Select(x => new DangKyThamDuItem
            {
                MaDangKy = x.MaDangKy,
                MaNguoiThamDu = x.MaNguoiThamDu,
                HoTenNguoiThamDu = x.HoTenNguoiThamDu,
                Email = x.Email,
                NgayDangKy = x.NgayDangKy,
                TrangThai = x.TrangThai.ToString(),
                NgayDuyet = x.NgayDuyet,
                ThoiGianCheckIn = x.ThoiGianCheckIn,
                MaThamDu = x.MaThamDu,
                KetQuaThamGia = XacDinhKetQua(x.TrangThai.ToString(), x.ThoiGianCheckIn.HasValue)
            })
            .ToList();

        // Bước 5: Gắn session của từng người trong sự kiện
        var phienTheoNguoi = await LayPhienCuaSuKienTheoNguoiThamDuAsync(id);
        foreach (var item in danhSachDangKy)
        {
            item.DanhSachPhien = phienTheoNguoi.GetValueOrDefault(item.MaNguoiThamDu) ?? [];
        }

        // Bước 6: Thống kê tổng hợp bằng LINQ Count
        var viewModel = new ChiTietSuKienLichSuViewModel
        {
            MaSuKien = suKien.MaSuKien,
            TenSuKien = suKien.TenSuKien,
            TenLoaiSuKien = suKien.LoaiSuKien?.TenLoaiSuKien ?? "",
            TenDiaDiem = suKien.DiaDiem?.TenDiaDiem ?? "",
            ThoiGianBatDau = suKien.ThoiGianBatDau,
            ThoiGianKetThuc = suKien.ThoiGianKetThuc,
            TrangThaiSuKien = suKien.TrangThai,

            TongDangKy = danhSachDangKy.Count,
            SoDaDuyet = danhSachDangKy.Count(d =>
                d.TrangThai == nameof(TrangThaiDangKy.DaDuyet)
                || d.TrangThai == nameof(TrangThaiDangKy.DaCheckIn)
                || d.TrangThai == nameof(TrangThaiDangKy.HoanThanh)),
            SoDaCheckIn = danhSachDangKy.Count(d => d.ThoiGianCheckIn.HasValue),
            SoTuChoi = danhSachDangKy.Count(d => d.TrangThai == nameof(TrangThaiDangKy.TuChoi)),
            SoDaHuy = danhSachDangKy.Count(d => d.TrangThai == nameof(TrangThaiDangKy.DaHuy)),

            DanhSachDangKy = danhSachDangKy,

            // Select: PhienSuKien entity -> ViewModel
            DanhSachPhien = suKien.PhienSuKiens.Select(p => new PhienSuKienItem
            {
                MaPhienSuKien = p.MaPhienSuKien,
                TenPhien = p.TenPhien,
                ThoiGianBatDau = p.ThoiGianBatDau,
                ThoiGianKetThuc = p.ThoiGianKetThuc,
                DienGia = p.DienGia,
                TrangThai = p.TrangThai
            }).ToList()
        };

        return View(viewModel);
    }


    // =================================================================
    // HÀM HỖ TRỢ
    // =================================================================

    /// <summary>
    /// Session người tham dự đã đăng ký, nhóm theo MaSuKien.
    /// Phiên thuộc sự kiện nào được xác định qua PhienSuKien.MaSuKien.
    /// </summary>
    private async Task<Dictionary<string, List<PhienDangKyItem>>> LayPhienCuaNguoiThamDuTheoSuKienAsync(
        string maNguoiThamDu)
    {
        var dsPhien = await _context.DangKyPhiens
            .AsNoTracking()
            .Where(dp => dp.MaNguoiThamDu == maNguoiThamDu && dp.Phien != null)
            .OrderBy(dp => dp.Phien!.ThoiGianBatDau)
            .Select(dp => new
            {
                MaSuKien = dp.Phien!.MaSuKien,
                TenPhien = dp.Phien!.TenPhien,
                ThoiGianBatDau = dp.Phien!.ThoiGianBatDau,
                ThoiGianKetThuc = dp.Phien!.ThoiGianKetThuc,
                dp.TrangThai
            })
            .ToListAsync();

        return dsPhien
            .GroupBy(p => p.MaSuKien)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p => new PhienDangKyItem
                {
                    TenPhien = p.TenPhien,
                    ThoiGianBatDau = p.ThoiGianBatDau,
                    ThoiGianKetThuc = p.ThoiGianKetThuc,
                    TrangThai = p.TrangThai.ToString()
                }).ToList());
    }

    /// <summary>
    /// Session đã đăng ký của mọi người trong một sự kiện, nhóm theo MaNguoiThamDu.
    /// </summary>
    private async Task<Dictionary<string, List<PhienDangKyItem>>> LayPhienCuaSuKienTheoNguoiThamDuAsync(
        string maSuKien)
    {
        var dsPhien = await _context.DangKyPhiens
            .AsNoTracking()
            .Where(dp => dp.Phien != null && dp.Phien.MaSuKien == maSuKien)
            .OrderBy(dp => dp.Phien!.ThoiGianBatDau)
            .Select(dp => new
            {
                dp.MaNguoiThamDu,
                TenPhien = dp.Phien!.TenPhien,
                ThoiGianBatDau = dp.Phien!.ThoiGianBatDau,
                ThoiGianKetThuc = dp.Phien!.ThoiGianKetThuc,
                dp.TrangThai
            })
            .ToListAsync();

        return dsPhien
            .GroupBy(p => p.MaNguoiThamDu)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p => new PhienDangKyItem
                {
                    TenPhien = p.TenPhien,
                    ThoiGianBatDau = p.ThoiGianBatDau,
                    ThoiGianKetThuc = p.ThoiGianKetThuc,
                    TrangThai = p.TrangThai.ToString()
                }).ToList());
    }

    /// <summary>
    /// Kết quả tham gia dựa trên trạng thái đăng ký và check-in. Chạy trên C#.
    /// </summary>
    private static string XacDinhKetQua(string trangThai, bool daCheckIn)
    {
        return trangThai switch
        {
            nameof(TrangThaiDangKy.HoanThanh) => "Đã tham dự",
            nameof(TrangThaiDangKy.DaCheckIn) => "Đã check-in",
            nameof(TrangThaiDangKy.DaDuyet) => daCheckIn ? "Đã check-in" : "Chưa check-in",
            nameof(TrangThaiDangKy.ChoDuyet) => "Đang chờ duyệt",
            nameof(TrangThaiDangKy.TuChoi) => "Bị từ chối",
            nameof(TrangThaiDangKy.DaHuy) => "Đã hủy",
            _ => trangThai
        };
    }
}
