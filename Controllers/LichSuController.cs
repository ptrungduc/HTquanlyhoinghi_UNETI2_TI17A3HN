// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 5 - Lịch sử tham dự, Dashboard, thống kê LINQ, báo cáo.

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
        // -------------------------------------------------------
        // Bước 1: Kiểm tra đăng nhập
        // -------------------------------------------------------
        var maTaiKhoan = HttpContext.Session.GetString("MaTaiKhoan");
        if (string.IsNullOrWhiteSpace(maTaiKhoan))
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        // -------------------------------------------------------
        // Bước 2: Tìm NguoiThamDu liên kết với tài khoản đang đăng nhập
        //         Dùng MaTaiKhoan từ Session → chỉ lấy đúng người này
        // -------------------------------------------------------
        var nguoiThamDu = await _context.NguoiThamDus
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.MaTaiKhoan == maTaiKhoan);

        // Nếu chưa có hồ sơ NguoiThamDu → hiển thị trang trống
        if (nguoiThamDu == null)
        {
            return View(new LichSuThamDuViewModel
            {
                HoTenNguoiThamDu = HttpContext.Session.GetString("HoTen") ?? "",
                DanhSachDangKy = []
            });
        }

        // -------------------------------------------------------
        // Bước 3: LINQ - Lấy toàn bộ đăng ký của chính người này
        //
        //   Where:              Lọc theo MaNguoiThamDu (bảo mật)
        //   OrderByDescending:  Đăng ký mới nhất hiện đầu tiên
        //   Select:             Projection - chỉ lấy các trường cần thiết
        //                       EF Core tự sinh LEFT JOIN cho SuKien,
        //                       DiaDiem và CheckIn
        // -------------------------------------------------------
        var danhSachDangKy = await _context.DangKyThamDus
            .AsNoTracking()
            .Where(d => d.MaNguoiThamDu == nguoiThamDu.MaNguoiThamDu)
            .OrderByDescending(d => d.NgayDangKy)
            .Select(d => new LichSuDangKyItem
            {
                MaDangKy = d.MaDangKy,
                TenSuKien = d.SuKien != null
                    ? d.SuKien.TenSuKien : "",
                ThoiGianBatDau = d.SuKien != null
                    ? d.SuKien.ThoiGianBatDau : DateTime.MinValue,
                ThoiGianKetThuc = d.SuKien != null
                    ? d.SuKien.ThoiGianKetThuc : DateTime.MinValue,
                TenDiaDiem = d.SuKien != null && d.SuKien.DiaDiem != null
                    ? d.SuKien.DiaDiem.TenDiaDiem : "",
                DiaChi = d.SuKien != null && d.SuKien.DiaDiem != null
                    ? d.SuKien.DiaDiem.DiaChi : "",
                NgayDangKy = d.NgayDangKy,
                TrangThai = d.TrangThai.ToString(),
                ThoiGianCheckIn = d.CheckIn != null
                    ? d.CheckIn.ThoiGianCheckIn : (DateTime?)null,
                MaThamDu = d.MaThamDu
            })
            .ToListAsync();

        // -------------------------------------------------------
        // Bước 4: Đưa ViewModel sang View
        // -------------------------------------------------------
        var viewModel = new LichSuThamDuViewModel
        {
            HoTenNguoiThamDu = nguoiThamDu.HoTen,
            DanhSachDangKy = danhSachDangKy
        };

        return View(viewModel);
    }


    // =================================================================
    // CHI TIẾT SỰ KIỆN - Admin xem lịch sử đăng ký của một sự kiện
    // =================================================================
    [HttpGet]
    public async Task<IActionResult> ChiTietSuKien(string id)
    {
        // -------------------------------------------------------
        // Bước 1: Kiểm tra quyền Admin
        // -------------------------------------------------------
        var vaiTro = HttpContext.Session.GetString("VaiTro");
        if (vaiTro != "Admin")
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        // -------------------------------------------------------
        // Bước 2: Lấy thông tin sự kiện kèm loại, địa điểm, phiên
        //         Include: Eager loading - tải dữ liệu liên quan
        // -------------------------------------------------------
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

        // -------------------------------------------------------
        // Bước 3: LINQ - Lấy toàn bộ đăng ký cho sự kiện này
        //
        //   Where:              Lọc theo MaSuKien
        //   OrderByDescending:  Đăng ký mới nhất hiện đầu tiên
        //   Select:             Projection sang ViewModel
        // -------------------------------------------------------
        var danhSachDangKy = await _context.DangKyThamDus
            .AsNoTracking()
            .Where(d => d.MaSuKien == id)
            .OrderByDescending(d => d.NgayDangKy)
            .Select(d => new DangKyThamDuItem
            {
                MaDangKy = d.MaDangKy,
                HoTenNguoiThamDu = d.NguoiThamDu != null
                    ? d.NguoiThamDu.HoTen : "",
                Email = d.NguoiThamDu != null
                    ? d.NguoiThamDu.Email : "",
                NgayDangKy = d.NgayDangKy,
                TrangThai = d.TrangThai.ToString(),
                NgayDuyet = d.NgayDuyet,
                ThoiGianCheckIn = d.CheckIn != null
                    ? d.CheckIn.ThoiGianCheckIn : (DateTime?)null,
                MaThamDu = d.MaThamDu,
                KetQuaThamGia = "" // Sẽ tính sau (không dịch được sang SQL)
            })
            .ToListAsync();

        // -------------------------------------------------------
        // Bước 4: Tính KetQuaThamGia trên C# (sau khi lấy dữ liệu)
        //         Hàm XacDinhKetQua dùng switch expression → không
        //         thể dịch sang SQL nên phải chạy trên C#
        // -------------------------------------------------------
        foreach (var item in danhSachDangKy)
        {
            item.KetQuaThamGia = XacDinhKetQua(
                item.TrangThai,
                item.ThoiGianCheckIn.HasValue);
        }

        // -------------------------------------------------------
        // Bước 5: LINQ Count - Đếm số lượng theo trạng thái
        //         Tính sẵn trong Controller → View chỉ hiển thị
        // -------------------------------------------------------
        var viewModel = new ChiTietSuKienLichSuViewModel
        {
            MaSuKien = suKien.MaSuKien,
            TenSuKien = suKien.TenSuKien,
            TenLoaiSuKien = suKien.LoaiSuKien?.TenLoaiSuKien ?? "",
            TenDiaDiem = suKien.DiaDiem?.TenDiaDiem ?? "",
            ThoiGianBatDau = suKien.ThoiGianBatDau,
            ThoiGianKetThuc = suKien.ThoiGianKetThuc,
            TrangThaiSuKien = suKien.TrangThai,

            // LINQ Count với predicate
            TongDangKy = danhSachDangKy.Count,
            SoDaDuyet = danhSachDangKy.Count(d =>
                d.TrangThai == nameof(TrangThaiDangKy.DaDuyet)
                || d.TrangThai == nameof(TrangThaiDangKy.DaCheckIn)
                || d.TrangThai == nameof(TrangThaiDangKy.HoanThanh)),
            SoDaCheckIn = danhSachDangKy.Count(d =>
                d.ThoiGianCheckIn.HasValue),
            SoTuChoi = danhSachDangKy.Count(d =>
                d.TrangThai == nameof(TrangThaiDangKy.TuChoi)),
            SoDaHuy = danhSachDangKy.Count(d =>
                d.TrangThai == nameof(TrangThaiDangKy.DaHuy)),

            DanhSachDangKy = danhSachDangKy,

            // LINQ Select: Chuyển PhienSuKien entity sang ViewModel
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
    /// Xác định kết quả tham gia dựa trên trạng thái đăng ký và check-in.
    /// Hàm này chạy trên C# (không dịch sang SQL).
    /// So sánh bằng nameof(enum) vì TrangThai đã được .ToString() khi Select.
    /// </summary>
    private static string XacDinhKetQua(string trangThai, bool daCheckIn)
    {
        return trangThai switch
        {
            nameof(TrangThaiDangKy.HoanThanh)  => "Đã tham dự",
            nameof(TrangThaiDangKy.DaCheckIn)  => "Đã check-in",
            nameof(TrangThaiDangKy.DaDuyet)    => daCheckIn ? "Đã check-in" : "Chưa check-in",
            nameof(TrangThaiDangKy.ChoDuyet)   => "Đang chờ duyệt",
            nameof(TrangThaiDangKy.TuChoi)     => "Bị từ chối",
            nameof(TrangThaiDangKy.DaHuy)      => "Đã hủy",
            _                                   => trangThai
        };
    }
}
