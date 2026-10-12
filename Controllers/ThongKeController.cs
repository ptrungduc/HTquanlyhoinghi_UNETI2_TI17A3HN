// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 5 - Lịch sử tham dự, Dashboard, thống kê LINQ, báo cáo.

using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module5;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers;

public class ThongKeController : Controller
{
    private readonly AppDbContext _context;

    public ThongKeController(AppDbContext context)
    {
        _context = context;
    }

    // =================================================================
    // THỐNG KÊ - Admin xem thống kê tổng hợp
    // =================================================================
    [HttpGet]
    public async Task<IActionResult> Index(int? thang, int? nam, DateTime? tuNgay, DateTime? denNgay)
    {
        // -------------------------------------------------------
        // Bước 1: Kiểm tra quyền Admin
        // -------------------------------------------------------
        var vaiTro = HttpContext.Session.GetString("VaiTro");
        if (vaiTro != "Admin")
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        // -------------------------------------------------------
        // Bước 2: Lấy toàn bộ dữ liệu gốc (IQueryable)
        //         IQueryable cho phép thêm điều kiện Where()
        //         trước khi thực thi SQL → tối ưu hiệu năng.
        // -------------------------------------------------------
        IQueryable<DangKyThamDu> queryDangKy = _context.DangKyThamDus.AsNoTracking();
        IQueryable<CheckIn> queryCheckIn = _context.CheckIns.AsNoTracking();
        IQueryable<SuKien> querySuKien = _context.SuKiens.AsNoTracking();

        // -------------------------------------------------------
        // Bước 3: ÁP DỤNG BỘ LỌC (Where)
        //         Lọc TRƯỚC GroupBy → giảm dữ liệu cần xử lý.
        // -------------------------------------------------------
        if (thang.HasValue && nam.HasValue)
        {
            // Where() — Lọc đăng ký theo tháng/năm
            queryDangKy = queryDangKy
                .Where(d => d.NgayDangKy.Month == thang.Value
                         && d.NgayDangKy.Year == nam.Value);

            queryCheckIn = queryCheckIn
                .Where(c => c.ThoiGianCheckIn.Month == thang.Value
                          && c.ThoiGianCheckIn.Year == nam.Value);

            querySuKien = querySuKien
                .Where(s => s.ThoiGianBatDau.Month == thang.Value
                          && s.ThoiGianBatDau.Year == nam.Value);
        }
        else if (tuNgay.HasValue && denNgay.HasValue)
        {
            var denNgayCuoi = denNgay.Value.Date.AddDays(1);

            // Where() — Lọc theo khoảng ngày
            queryDangKy = queryDangKy
                .Where(d => d.NgayDangKy >= tuNgay.Value.Date
                         && d.NgayDangKy < denNgayCuoi);

            queryCheckIn = queryCheckIn
                .Where(c => c.ThoiGianCheckIn >= tuNgay.Value.Date
                          && c.ThoiGianCheckIn < denNgayCuoi);

            querySuKien = querySuKien
                .Where(s => s.ThoiGianBatDau >= tuNgay.Value.Date
                          && s.ThoiGianBatDau < denNgayCuoi);
        }

        // Materialize data (ToList) - chuyển sang C# để xử lý GroupBy phức tạp
        var dsDangKy = await queryDangKy
            .Include(d => d.SuKien).ThenInclude(s => s!.LoaiSuKien)
            .Include(d => d.SuKien).ThenInclude(s => s!.DiaDiem)
            .ToListAsync();

        var dsCheckIn = await queryCheckIn
            .Include(c => c.DangKyThamDu).ThenInclude(d => d!.SuKien)
            .ToListAsync();

        var dsSuKien = await querySuKien
            .Include(s => s.DiaDiem)
            .Include(s => s.LoaiSuKien)
            .ToListAsync();

        var dsDangKyPhien = await _context.DangKyPhiens
            .AsNoTracking()
            .Include(dp => dp.Phien).ThenInclude(p => p!.SuKien)
            .ToListAsync();

        // -------------------------------------------------------
        // Bước 4: TÍNH TOÁN THỐNG KÊ BẰNG LINQ
        // -------------------------------------------------------
        var viewModel = new ThongKeViewModel
        {
            ThangLoc = thang,
            NamLoc = nam,
            TuNgay = tuNgay,
            DenNgay = denNgay
        };

        // === TỔNG HỢP ===
        // Count() — Đếm tổng số
        viewModel.TongSuKien = dsSuKien.Count;
        viewModel.TongDangKy = dsDangKy.Count;
        viewModel.TongCheckIn = dsCheckIn.Count;

        // =====================================================
        // 1. THỐNG KÊ ĐĂNG KÝ
        // =====================================================

        // --- 1a. Số đăng ký theo sự kiện ---
        // GroupBy()  — Nhóm đăng ký theo MaSuKien
        // Select()  — Chuyển mỗi nhóm thành ViewModel
        // Count()   — Đếm số phần tử trong nhóm
        // OrderByDescending() — Sự kiện nhiều đăng ký nhất lên đầu
        viewModel.DangKyTheoSuKien = dsDangKy
            .GroupBy(d => d.MaSuKien)
            .Select(g => new ThongKeDangKyTheoSuKien
            {
                MaSuKien = g.Key,
                TenSuKien = g.First().SuKien?.TenSuKien ?? "",
                SoDangKy = g.Count()
            })
            .OrderByDescending(x => x.SoDangKy)
            .ToList();

        // --- 1b. Số đăng ký theo loại sự kiện ---
        // GroupBy()  — Nhóm theo TenLoaiSuKien
        // Sum()     — Tính tổng đăng ký qua các sự kiện cùng loại
        // Average() — Tính trung bình đăng ký/sự kiện
        viewModel.DangKyTheoLoaiSuKien = dsDangKy
            .Where(d => d.SuKien?.LoaiSuKien != null)
            .GroupBy(d => d.SuKien!.LoaiSuKien!.TenLoaiSuKien)
            .Select(g =>
            {
                var danhSachSuKien = g.Select(d => d.MaSuKien).Distinct().ToList();
                return new ThongKeDangKyTheoLoai
                {
                    TenLoaiSuKien = g.Key,
                    SoDangKy = g.Count(),
                    SoSuKien = danhSachSuKien.Count,
                    // Average() — Trung bình đăng ký trên mỗi sự kiện
                    // Xử lý mẫu số = 0: danhSachSuKien.Count luôn >= 1 (vì GroupBy)
                    TrungBinhDangKy = Math.Round(
                        (double)g.Count() / danhSachSuKien.Count, 1)
                };
            })
            .OrderByDescending(x => x.SoDangKy)
            .ToList();

        // --- 1c. Số đăng ký theo địa điểm ---
        // GroupBy() — Nhóm theo tên địa điểm
        viewModel.DangKyTheoDiaDiem = dsDangKy
            .Where(d => d.SuKien?.DiaDiem != null)
            .GroupBy(d => d.SuKien!.DiaDiem!.TenDiaDiem)
            .Select(g => new ThongKeDangKyTheoDiaDiem
            {
                TenDiaDiem = g.Key,
                SoDangKy = g.Count(),
                SoSuKien = g.Select(d => d.MaSuKien).Distinct().Count()
            })
            .OrderByDescending(x => x.SoDangKy)
            .ToList();

        // =====================================================
        // 2. THỐNG KÊ DUYỆT
        // =====================================================

        // GroupBy() — Nhóm đăng ký theo sự kiện
        // Count() với predicate — Đếm theo điều kiện (trạng thái đã duyệt)
        var trangThaiDuyet = new[]
        {
            TrangThaiDangKy.DaDuyet,
            TrangThaiDangKy.DaCheckIn,
            TrangThaiDangKy.HoanThanh
        };

        viewModel.DuyetTheoSuKien = dsDangKy
            .GroupBy(d => d.SuKien?.TenSuKien ?? "")
            .Select(g =>
            {
                int tongDK = g.Count();
                int soDuyet = g.Count(d => trangThaiDuyet.Contains(d.TrangThai));
                return new ThongKeDuyetTheoSuKien
                {
                    TenSuKien = g.Key,
                    TongDangKy = tongDK,
                    SoDaDuyet = soDuyet,
                    // Xử lý mẫu số = 0: nếu tongDK = 0 → tỷ lệ = 0
                    TyLeDuyet = tongDK > 0
                        ? Math.Round((double)soDuyet / tongDK * 100, 1)
                        : 0
                };
            })
            .OrderByDescending(x => x.SoDaDuyet)
            .ToList();

        // =====================================================
        // 3. THỐNG KÊ CHECK-IN
        // =====================================================

        // --- 3a. Check-in thực tế theo sự kiện ---
        // GroupBy() — Nhóm check-in theo sự kiện
        viewModel.CheckInTheoSuKien = dsCheckIn
            .Where(c => c.DangKyThamDu?.SuKien != null)
            .GroupBy(c => c.DangKyThamDu!.SuKien!.TenSuKien)
            .Select(g =>
            {
                int soCI = g.Count();
                // Đếm số người được duyệt của sự kiện này
                string maSK = g.First().DangKyThamDu!.MaSuKien;
                int soDuyet = dsDangKy
                    .Count(d => d.MaSuKien == maSK
                             && trangThaiDuyet.Contains(d.TrangThai));
                return new ThongKeCheckInTheoSuKien
                {
                    TenSuKien = g.Key,
                    SoDaDuyet = soDuyet,
                    SoCheckIn = soCI,
                    // Attendance Rate = CheckIn / Approved * 100
                    // Xử lý mẫu số = 0
                    TyLeCheckIn = soDuyet > 0
                        ? Math.Round((double)soCI / soDuyet * 100, 1)
                        : 0
                };
            })
            .OrderByDescending(x => x.SoCheckIn)
            .ToList();

        // --- 3b. Số người tham gia theo phiên ---
        // GroupBy() — Nhóm đăng ký phiên theo phiên
        viewModel.ThamGiaTheoPhien = dsDangKyPhien
            .Where(dp => dp.Phien != null)
            .GroupBy(dp => dp.Phien!.TenPhien)
            .Select(g => new ThongKeThamGiaPhien
            {
                TenPhien = g.Key,
                TenSuKien = g.First().Phien?.SuKien?.TenSuKien ?? "",
                SoDangKy = g.Count()
            })
            .OrderByDescending(x => x.SoDangKy)
            .ToList();

        // =====================================================
        // 4. THỐNG KÊ THEO THỜI GIAN
        // =====================================================

        // --- 4a. Số sự kiện theo tháng ---
        // GroupBy() — Nhóm theo (Year, Month)
        // OrderBy() — Sắp xếp tăng dần theo thời gian
        viewModel.SuKienTheoThang = dsSuKien
            .GroupBy(s => new { s.ThoiGianBatDau.Year, s.ThoiGianBatDau.Month })
            .Select(g => new ThongKeTheoThang
            {
                Nam = g.Key.Year,
                Thang = g.Key.Month,
                SoLuong = g.Count()
            })
            .OrderBy(t => t.Nam)
            .ThenBy(t => t.Thang)
            .ToList();

        // --- 4b. Số đăng ký theo tháng ---
        viewModel.DangKyTheoThang = dsDangKy
            .GroupBy(d => new { d.NgayDangKy.Year, d.NgayDangKy.Month })
            .Select(g => new ThongKeTheoThang
            {
                Nam = g.Key.Year,
                Thang = g.Key.Month,
                SoLuong = g.Count()
            })
            .OrderBy(t => t.Nam)
            .ThenBy(t => t.Thang)
            .ToList();

        // --- 4c. Số check-in theo tháng ---
        viewModel.CheckInTheoThang = dsCheckIn
            .GroupBy(c => new { c.ThoiGianCheckIn.Year, c.ThoiGianCheckIn.Month })
            .Select(g => new ThongKeTheoThang
            {
                Nam = g.Key.Year,
                Thang = g.Key.Month,
                SoLuong = g.Count()
            })
            .OrderBy(t => t.Nam)
            .ThenBy(t => t.Thang)
            .ToList();

        // =====================================================
        // 6. TỶ LỆ
        // =====================================================

        // Tính tỷ lệ cho từng sự kiện:
        // - Attendance Rate = CheckIn / Approved * 100
        // - Cancellation Rate = Canceled / Total * 100
        // - Fill Rate = Approved / MaxCapacity * 100
        viewModel.TyLeTheoSuKien = dsSuKien
            .Select(s =>
            {
                var dkSuKien = dsDangKy.Where(d => d.MaSuKien == s.MaSuKien).ToList();
                int tongDK = dkSuKien.Count;
                int soDuyet = dkSuKien.Count(d => trangThaiDuyet.Contains(d.TrangThai));
                int soCI = dsCheckIn.Count(c => c.DangKyThamDu?.MaSuKien == s.MaSuKien);
                int soHuy = dkSuKien.Count(d => d.TrangThai == TrangThaiDangKy.DaHuy);
                int sucChua = s.DiaDiem?.SucChuaToiDa ?? 0;

                return new ThongKeTyLeSuKien
                {
                    TenSuKien = s.TenSuKien,
                    TongDangKy = tongDK,
                    SoDaDuyet = soDuyet,
                    SoCheckIn = soCI,
                    SoDaHuy = soHuy,
                    SucChua = sucChua,

                    // BẮT BUỘC xử lý mẫu số = 0
                    // Nếu mẫu số = 0 → kết quả = 0 (không Divide by zero, NaN, Infinity)
                    TyLeCheckIn = soDuyet > 0
                        ? Math.Round((double)soCI / soDuyet * 100, 1) : 0,
                    TyLeHuy = tongDK > 0
                        ? Math.Round((double)soHuy / tongDK * 100, 1) : 0,
                    TyLeLapDay = sucChua > 0
                        ? Math.Round((double)soDuyet / sucChua * 100, 1) : 0
                };
            })
            .OrderByDescending(x => x.TongDangKy)
            .ToList();

        // === TỶ LỆ TRUNG BÌNH ===
        // Average() — Tính trung bình tỷ lệ check-in và hủy qua tất cả sự kiện
        if (viewModel.TyLeTheoSuKien.Count > 0)
        {
            viewModel.TyLeCheckInTrungBinh = Math.Round(
                viewModel.TyLeTheoSuKien.Average(t => t.TyLeCheckIn), 1);
            viewModel.TyLeHuyTrungBinh = Math.Round(
                viewModel.TyLeTheoSuKien.Average(t => t.TyLeHuy), 1);
        }

        // =====================================================
        // THỐNG KÊ NỔI BẬT (Bước 4)
        // =====================================================

        // --- NB1. Sự kiện có nhiều đăng ký nhất ---
        // OrderByDescending() — Sắp xếp giảm dần theo SoDangKy
        // FirstOrDefault()   — Lấy phần tử đầu tiên (nhiều nhất)
        //                       Trả về null nếu danh sách rỗng
        var skNhieuDK = viewModel.DangKyTheoSuKien
            .OrderByDescending(x => x.SoDangKy)
            .FirstOrDefault();

        viewModel.SuKienNhieuDangKyNhat = skNhieuDK != null
            ? new NoiBatItem
            {
                Ten = skNhieuDK.TenSuKien,
                GiaTri = skNhieuDK.SoDangKy,
                ChiTiet = $"{skNhieuDK.SoDangKy} đăng ký"
            }
            : null; // Không có dữ liệu → null → View hiển thị "Chưa có"

        // --- NB2. Sự kiện có tỷ lệ tham dự cao nhất ---
        // Tỷ lệ tham dự = CheckIn / Approved * 100 (theo đúng đề bài)
        // Where()              — Chỉ xét sự kiện có người được duyệt (mẫu số > 0)
        // OrderByDescending()  — Sắp xếp theo TyLeCheckIn giảm dần
        // FirstOrDefault()     — Lấy sự kiện có tỷ lệ cao nhất
        var skTyLeCao = viewModel.TyLeTheoSuKien
            .Where(x => x.SoDaDuyet > 0)
            .OrderByDescending(x => x.TyLeCheckIn)
            .FirstOrDefault();

        viewModel.SuKienTyLeThamDuCaoNhat = skTyLeCao != null
            ? new NoiBatItem
            {
                Ten = skTyLeCao.TenSuKien,
                GiaTri = skTyLeCao.SoCheckIn,
                TyLe = skTyLeCao.TyLeCheckIn,
                ChiTiet = $"{skTyLeCao.SoCheckIn}/{skTyLeCao.SoDaDuyet} ({skTyLeCao.TyLeCheckIn}%)"
            }
            : null;

        // --- NB3. Loại sự kiện có nhiều lượt tham dự nhất ---
        // GroupBy()            — Nhóm check-in theo loại sự kiện
        // Select() + Count()   — Đếm check-in mỗi loại
        // OrderByDescending()  — Loại nhiều nhất lên đầu
        // FirstOrDefault()     — Lấy loại đầu tiên
        var loaiNhieuCI = dsCheckIn
            .Where(c => c.DangKyThamDu?.SuKien?.LoaiSuKien != null)
            .GroupBy(c => c.DangKyThamDu!.SuKien!.LoaiSuKien!.TenLoaiSuKien)
            .Select(g => new { TenLoai = g.Key, SoLuot = g.Count() })
            .OrderByDescending(x => x.SoLuot)
            .FirstOrDefault();

        viewModel.LoaiSuKienNhieuThamDuNhat = loaiNhieuCI != null
            ? new NoiBatItem
            {
                Ten = loaiNhieuCI.TenLoai,
                GiaTri = loaiNhieuCI.SoLuot,
                ChiTiet = $"{loaiNhieuCI.SoLuot} lượt check-in"
            }
            : null;

        // --- NB4. Địa điểm được sử dụng nhiều nhất ---
        // GroupBy()            — Nhóm sự kiện theo địa điểm
        // Select() + Count()   — Đếm số sự kiện mỗi địa điểm
        // OrderByDescending()  — Địa điểm nhiều SK nhất lên đầu
        // FirstOrDefault()     — Lấy địa điểm đầu tiên
        var ddNhieuNhat = dsSuKien
            .Where(s => s.DiaDiem != null)
            .GroupBy(s => s.DiaDiem!.TenDiaDiem)
            .Select(g => new { TenDD = g.Key, SoSK = g.Count() })
            .OrderByDescending(x => x.SoSK)
            .FirstOrDefault();

        viewModel.DiaDiemSuDungNhieuNhat = ddNhieuNhat != null
            ? new NoiBatItem
            {
                Ten = ddNhieuNhat.TenDD,
                GiaTri = ddNhieuNhat.SoSK,
                ChiTiet = $"{ddNhieuNhat.SoSK} sự kiện"
            }
            : null;

        // --- NB5. Phiên có nhiều người tham gia nhất ---
        // OrderByDescending()  — Phiên nhiều đăng ký nhất lên đầu
        // FirstOrDefault()     — Lấy phiên đầu tiên
        var phienNhieu = viewModel.ThamGiaTheoPhien
            .OrderByDescending(x => x.SoDangKy)
            .FirstOrDefault();

        viewModel.PhienNhieuNguoiNhat = phienNhieu != null
            ? new NoiBatItem
            {
                Ten = phienNhieu.TenPhien,
                GiaTri = phienNhieu.SoDangKy,
                ChiTiet = $"{phienNhieu.SoDangKy} người • {phienNhieu.TenSuKien}"
            }
            : null;

        // --- NB6. Người tham dự tham gia nhiều sự kiện nhất ---
        // GroupBy()            — Nhóm đăng ký theo MaNguoiThamDu
        // Select() + Count()   — Đếm số sự kiện (distinct) mỗi người
        // OrderByDescending()  — Người nhiều SK nhất lên đầu
        // FirstOrDefault()     — Lấy người đầu tiên
        var nguoiNhieu = dsDangKy
            .Where(d => d.NguoiThamDu != null)
            .GroupBy(d => new { d.MaNguoiThamDu, d.NguoiThamDu!.HoTen })
            .Select(g => new
            {
                HoTen = g.Key.HoTen,
                SoSuKien = g.Select(d => d.MaSuKien).Distinct().Count()
            })
            .OrderByDescending(x => x.SoSuKien)
            .FirstOrDefault();

        viewModel.NguoiThamDuNhieuNhat = nguoiNhieu != null
            ? new NoiBatItem
            {
                Ten = nguoiNhieu.HoTen,
                GiaTri = nguoiNhieu.SoSuKien,
                ChiTiet = $"{nguoiNhieu.SoSuKien} sự kiện"
            }
            : null;

        // -------------------------------------------------------
        // Bước 5: Đưa ViewModel sang View
        // -------------------------------------------------------
        return View(viewModel);
    }
}
