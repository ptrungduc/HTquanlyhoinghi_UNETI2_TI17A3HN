// Họ và tên: PHAM TRUNG DUC
// Mã sinh viên: 23103100134
// Nội dung thực hiện: Module 5 - Thống kê LINQ và thống kê nổi bật (Bước 3 + Bước 4).

using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Helper;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module5;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers;

public class ThongKeController : Controller
{
    private readonly AppDbContext _context;

    // Approved theo quy tắc Module 5
    private static readonly TrangThaiDangKy[] TrangThaiDaDuyet =
    {
        TrangThaiDangKy.DaDuyet,
        TrangThaiDangKy.DaCheckIn,
        TrangThaiDangKy.HoanThanh
    };

    public ThongKeController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int? thang,
        int? nam,
        DateTime? tuNgay,
        DateTime? denNgay)
    {
        // 1. KIỂM TRA QUYỀN Ở SERVER
        if (!KiemTraQuyenModule4.CoQuyenQuanTri(HttpContext.Session.GetString("VaiTro")))
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var vm = new ThongKeViewModel
        {
            ThangLoc = thang,
            NamLoc = nam,
            TuNgay = tuNgay,
            DenNgay = denNgay
        };

        // 2. VALIDATION VÀ KHOẢNG LỌC
        bool coThangNam = thang.HasValue || nam.HasValue;
        bool coKhoangNgay = tuNgay.HasValue || denNgay.HasValue;

        if (coThangNam && coKhoangNgay)
        {
            vm.LoiValidation = "Chỉ chọn một kiểu lọc: hoặc Tháng/Năm, hoặc Từ ngày/Đến ngày.";
            return View(vm);
        }

        DateTime? tu = null;   // bao gồm
        DateTime? den = null;  // không bao gồm

        if (coThangNam)
        {
            int y = nam ?? DateTime.Now.Year;
            if (y < 1900 || y > 2100)
            {
                vm.LoiValidation = "Năm không hợp lệ (1900 - 2100).";
                return View(vm);
            }

            if (thang.HasValue)
            {
                if (thang.Value < 1 || thang.Value > 12)
                {
                    vm.LoiValidation = "Tháng phải nằm trong khoảng 1 đến 12.";
                    return View(vm);
                }
                tu = new DateTime(y, thang.Value, 1);
                den = tu.Value.AddMonths(1);
                vm.MoTaKhoangThoiGian = $"Tháng {thang.Value:D2}/{y}";
            }
            else
            {
                tu = new DateTime(y, 1, 1);
                den = tu.Value.AddYears(1);
                vm.MoTaKhoangThoiGian = $"Năm {y}";
            }
        }
        else if (coKhoangNgay)
        {
            if (tuNgay.HasValue && denNgay.HasValue && tuNgay.Value.Date > denNgay.Value.Date)
            {
                vm.LoiValidation = "\"Từ ngày\" phải nhỏ hơn hoặc bằng \"Đến ngày\".";
                return View(vm);
            }

            tu = tuNgay?.Date;
            den = denNgay?.Date.AddDays(1);

            vm.MoTaKhoangThoiGian = (tuNgay, denNgay) switch
            {
                (not null, not null) => $"Từ {tuNgay.Value:dd/MM/yyyy} đến {denNgay.Value:dd/MM/yyyy}",
                (not null, null) => $"Từ {tuNgay.Value:dd/MM/yyyy} đến nay",
                _ => $"Đến {denNgay!.Value:dd/MM/yyyy}"
            };
        }

        vm.DaLoc = coThangNam || coKhoangNgay;

        // 3. WHERE() - IQueryable, lọc từng điều kiện riêng (kể cả chỉ có một đầu mút)
        IQueryable<SuKien> querySuKien = _context.SuKiens.AsNoTracking();
        IQueryable<DangKyThamDu> queryDangKy = _context.DangKyThamDus.AsNoTracking();
        IQueryable<CheckIn> queryCheckIn = _context.CheckIns.AsNoTracking();
        IQueryable<DangKyPhien> queryDangKyPhien = _context.DangKyPhiens.AsNoTracking();

        if (tu.HasValue)
        {
            querySuKien = querySuKien.Where(s => s.ThoiGianBatDau >= tu.Value);
            queryDangKy = queryDangKy.Where(d => d.NgayDangKy >= tu.Value);
            queryCheckIn = queryCheckIn.Where(c => c.ThoiGianCheckIn >= tu.Value);
            queryDangKyPhien = queryDangKyPhien.Where(dp =>
                dp.Phien != null && dp.Phien.ThoiGianBatDau >= tu.Value);
        }

        if (den.HasValue)
        {
            querySuKien = querySuKien.Where(s => s.ThoiGianBatDau < den.Value);
            queryDangKy = queryDangKy.Where(d => d.NgayDangKy < den.Value);
            queryCheckIn = queryCheckIn.Where(c => c.ThoiGianCheckIn < den.Value);
            queryDangKyPhien = queryDangKyPhien.Where(dp =>
                dp.Phien != null && dp.Phien.ThoiGianBatDau < den.Value);
        }

        // 4. MATERIALIZE - Include lấy navigation property cần cho GroupBy
        var dsDangKy = await queryDangKy
            .Include(d => d.SuKien).ThenInclude(s => s!.LoaiSuKien)
            .Include(d => d.SuKien).ThenInclude(s => s!.DiaDiem)
            .Include(d => d.NguoiThamDu)
            .Include(d => d.CheckIn)
            .ToListAsync();

        var dsSuKien = await querySuKien
            .Include(s => s.DiaDiem)
            .ToListAsync();

        var dsDangKyPhien = await queryDangKyPhien
            .Include(dp => dp.Phien).ThenInclude(p => p!.SuKien)
            .ToListAsync();

        // Tập con trong bộ nhớ dùng cho nhiều thống kê
        var dangKyDaDuyet = dsDangKy
            .Where(d => TrangThaiDaDuyet.Contains(d.TrangThai))
            .ToList();

        var dangKyDaCheckIn = dangKyDaDuyet
            .Where(d => d.CheckIn != null)
            .ToList();

        // 5. COUNT() - tổng hợp
        vm.TongSuKien = dsSuKien.Count;
        vm.TongDangKy = dsDangKy.Count;
        vm.TongCheckIn = await queryCheckIn.CountAsync();

        // =============================================================
        // 1. THỐNG KÊ ĐĂNG KÝ
        // =============================================================

        // 1a. Đăng ký theo sự kiện: GroupBy(MaSuKien) -> Count -> OrderByDescending
        vm.DangKyTheoSuKien = dsDangKy
            .GroupBy(d => d.MaSuKien)
            .Select(g => new ThongKeDangKyTheoSuKien
            {
                MaSuKien = g.Key,
                TenSuKien = g.First().SuKien?.TenSuKien ?? "",
                SoDangKy = g.Count()
            })
            .OrderByDescending(x => x.SoDangKy)
            .ThenBy(x => x.TenSuKien)
            .ToList();

        // 1b. Đăng ký theo loại: GroupBy(MaLoaiSuKien) -> Sum, Average
        vm.DangKyTheoLoaiSuKien = dsDangKy
            .Where(d => d.SuKien?.LoaiSuKien != null)
            .GroupBy(d => d.SuKien!.LoaiSuKien!.MaLoaiSuKien)
            .Select(g =>
            {
                var soDkMoiSuKien = g
                    .GroupBy(d => d.MaSuKien)
                    .Select(x => x.Count())
                    .ToList();

                return new ThongKeDangKyTheoLoai
                {
                    MaLoaiSuKien = g.Key,
                    TenLoaiSuKien = g.First().SuKien!.LoaiSuKien!.TenLoaiSuKien,
                    SoSuKien = soDkMoiSuKien.Count,
                    SoDangKy = soDkMoiSuKien.Sum(),
                    TrungBinhDangKy = soDkMoiSuKien.Count == 0
                        ? 0
                        : Math.Round(soDkMoiSuKien.Average(), 1)
                };
            })
            .OrderByDescending(x => x.SoDangKy)
            .ThenBy(x => x.TenLoaiSuKien)
            .ToList();

        // 1c. Đăng ký theo địa điểm: GroupBy(MaDiaDiem) -> Count
        vm.DangKyTheoDiaDiem = dsDangKy
            .Where(d => d.SuKien?.DiaDiem != null)
            .GroupBy(d => d.SuKien!.DiaDiem!.MaDiaDiem)
            .Select(g => new ThongKeDangKyTheoDiaDiem
            {
                MaDiaDiem = g.Key,
                TenDiaDiem = g.First().SuKien!.DiaDiem!.TenDiaDiem,
                SoSuKien = g.Select(d => d.MaSuKien).Distinct().Count(),
                SoDangKy = g.Count()
            })
            .OrderByDescending(x => x.SoDangKy)
            .ThenBy(x => x.TenDiaDiem)
            .ToList();

        // =============================================================
        // 2. THỐNG KÊ DUYỆT - số người được duyệt theo sự kiện
        // =============================================================
        vm.DuyetTheoSuKien = dsDangKy
            .GroupBy(d => d.MaSuKien)
            .Select(g =>
            {
                int tongDK = g.Count();
                int soDuyet = g.Count(d => TrangThaiDaDuyet.Contains(d.TrangThai));

                return new ThongKeDuyetTheoSuKien
                {
                    MaSuKien = g.Key,
                    TenSuKien = g.First().SuKien?.TenSuKien ?? "",
                    TongDangKy = tongDK,
                    SoDaDuyet = soDuyet,
                    TyLeDuyet = TinhTyLe(soDuyet, tongDK)
                };
            })
            .OrderByDescending(x => x.SoDaDuyet)
            .ThenBy(x => x.TenSuKien)
            .ToList();

        // =============================================================
        // 3. THỐNG KÊ CHECK-IN
        // =============================================================
        var soCheckInTheoSuKien = dangKyDaCheckIn
            .GroupBy(d => d.MaSuKien)
            .ToDictionary(g => g.Key, g => g.Count());

        // 3a. Số người check-in thực tế theo sự kiện
        vm.CheckInTheoSuKien = vm.DuyetTheoSuKien
            .Select(x =>
            {
                int soCI = soCheckInTheoSuKien.GetValueOrDefault(x.MaSuKien, 0);
                return new ThongKeCheckInTheoSuKien
                {
                    MaSuKien = x.MaSuKien,
                    TenSuKien = x.TenSuKien,
                    SoDaDuyet = x.SoDaDuyet,
                    SoCheckIn = soCI,
                    TyLeCheckIn = TinhTyLe(soCI, x.SoDaDuyet)
                };
            })
            .OrderByDescending(x => x.SoCheckIn)
            .ThenBy(x => x.TenSuKien)
            .ToList();

        // 3b. Số người tham gia từng phiên: GroupBy(MaPhien) -> Count
        vm.ThamGiaTheoPhien = dsDangKyPhien
            .Where(dp => dp.Phien != null)
            .GroupBy(dp => dp.MaPhien)
            .Select(g => new ThongKeThamGiaPhien
            {
                MaPhienSuKien = g.Key,
                TenPhien = g.First().Phien!.TenPhien,
                TenSuKien = g.First().Phien!.SuKien?.TenSuKien ?? "",
                SoNguoiThamGia = g.Count()
            })
            .OrderByDescending(x => x.SoNguoiThamGia)
            .ThenBy(x => x.TenPhien)
            .ToList();

        // =============================================================
        // 4. THỐNG KÊ THEO THỜI GIAN
        // =============================================================
        vm.SuKienTheoThang = dsSuKien
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

        vm.DangKyTheoThang = dsDangKy
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

        // Check-in theo tháng: GROUP BY trên SQL Server
        vm.CheckInTheoThang = await queryCheckIn
            .GroupBy(c => new { c.ThoiGianCheckIn.Year, c.ThoiGianCheckIn.Month })
            .Select(g => new ThongKeTheoThang
            {
                Nam = g.Key.Year,
                Thang = g.Key.Month,
                SoLuong = g.Count()
            })
            .OrderBy(t => t.Nam)
            .ThenBy(t => t.Thang)
            .ToListAsync();

        // =============================================================
        // 5. TỶ LỆ - mọi phép chia đều qua TinhTyLe
        // =============================================================
        var tongDkTheoSuKien = vm.DangKyTheoSuKien
            .ToDictionary(x => x.MaSuKien, x => x.SoDangKy);

        var soDuyetTheoSuKien = vm.DuyetTheoSuKien
            .ToDictionary(x => x.MaSuKien, x => x.SoDaDuyet);

        var soHuyTheoSuKien = dsDangKy
            .Where(d => d.TrangThai == TrangThaiDangKy.DaHuy)
            .GroupBy(d => d.MaSuKien)
            .ToDictionary(g => g.Key, g => g.Count());

        // Sức chứa lấy từ sự kiện trong phạm vi lọc (kể cả sự kiện chưa có đăng ký)
        var sucChuaTheoSuKien = dsSuKien
            .ToDictionary(s => s.MaSuKien, s => s.DiaDiem?.SucChuaToiDa ?? 0);

        vm.TyLeTheoSuKien = dsSuKien
            .Select(s =>
            {
                int tongDK = tongDkTheoSuKien.GetValueOrDefault(s.MaSuKien, 0);
                int soDuyet = soDuyetTheoSuKien.GetValueOrDefault(s.MaSuKien, 0);
                int soCI = soCheckInTheoSuKien.GetValueOrDefault(s.MaSuKien, 0);
                int soHuy = soHuyTheoSuKien.GetValueOrDefault(s.MaSuKien, 0);
                int sucChua = sucChuaTheoSuKien.GetValueOrDefault(s.MaSuKien, 0);

                return new ThongKeTyLeSuKien
                {
                    MaSuKien = s.MaSuKien,
                    TenSuKien = s.TenSuKien,
                    TongDangKy = tongDK,
                    SoDaDuyet = soDuyet,
                    SoCheckIn = soCI,
                    SoDaHuy = soHuy,
                    SucChua = sucChua,
                    TyLeCheckIn = TinhTyLe(soCI, soDuyet),    // Attendance Rate
                    TyLeHuy = TinhTyLe(soHuy, tongDK),        // Cancellation Rate
                    TyLeLapDay = TinhTyLe(soDuyet, sucChua)   // Fill Rate
                };
            })
            .OrderByDescending(x => x.TongDangKy)
            .ThenBy(x => x.TenSuKien)
            .ToList();

        // Trung bình: chỉ tính trên sự kiện có mẫu số > 0
        var coDuyet = vm.TyLeTheoSuKien.Where(t => t.SoDaDuyet > 0).ToList();
        vm.TyLeCheckInTrungBinh = coDuyet.Count == 0
            ? 0
            : Math.Round(coDuyet.Average(t => t.TyLeCheckIn), 1);

        var coDangKy = vm.TyLeTheoSuKien.Where(t => t.TongDangKy > 0).ToList();
        vm.TyLeHuyTrungBinh = coDangKy.Count == 0
            ? 0
            : Math.Round(coDangKy.Average(t => t.TyLeHuy), 1);

        // =============================================================
        // 6. THỐNG KÊ NỔI BẬT (null nếu không có dữ liệu)
        // =============================================================

        // NB1. Sự kiện có nhiều đăng ký nhất
        var skNhieuDK = vm.DangKyTheoSuKien.FirstOrDefault();
        vm.SuKienNhieuDangKyNhat = skNhieuDK == null ? null : new NoiBatItem
        {
            Ten = skNhieuDK.TenSuKien,
            GiaTri = skNhieuDK.SoDangKy,
            ChiTiet = $"{skNhieuDK.SoDangKy} đăng ký"
        };

        // NB2. Sự kiện có tỷ lệ tham dự cao nhất: CheckIn / Approved * 100, chỉ xét Approved > 0
        var skTyLeCao = vm.TyLeTheoSuKien
            .Where(t => t.SoDaDuyet > 0)
            .OrderByDescending(t => t.TyLeCheckIn)
            .ThenByDescending(t => t.SoCheckIn)
            .FirstOrDefault();
        vm.SuKienTyLeThamDuCaoNhat = skTyLeCao == null ? null : new NoiBatItem
        {
            Ten = skTyLeCao.TenSuKien,
            GiaTri = skTyLeCao.SoCheckIn,
            TyLe = skTyLeCao.TyLeCheckIn,
            ChiTiet = $"{skTyLeCao.SoCheckIn}/{skTyLeCao.SoDaDuyet} ({skTyLeCao.TyLeCheckIn}%)"
        };

        // NB3. Loại sự kiện có nhiều lượt tham dự nhất (số check-in của đăng ký đã duyệt)
        var loaiNhieuCI = dangKyDaCheckIn
            .Where(d => d.SuKien?.LoaiSuKien != null)
            .GroupBy(d => d.SuKien!.LoaiSuKien!.MaLoaiSuKien)
            .Select(g => new
            {
                Ten = g.First().SuKien!.LoaiSuKien!.TenLoaiSuKien,
                SoLuot = g.Count()
            })
            .OrderByDescending(x => x.SoLuot)
            .ThenBy(x => x.Ten)
            .FirstOrDefault();
        vm.LoaiSuKienNhieuThamDuNhat = loaiNhieuCI == null ? null : new NoiBatItem
        {
            Ten = loaiNhieuCI.Ten,
            GiaTri = loaiNhieuCI.SoLuot,
            ChiTiet = $"{loaiNhieuCI.SoLuot} lượt check-in"
        };

        // NB4. Địa điểm được sử dụng nhiều nhất (theo số sự kiện)
        var ddNhieuNhat = dsSuKien
            .Where(s => s.DiaDiem != null)
            .GroupBy(s => s.MaDiaDiem)
            .Select(g => new
            {
                Ten = g.First().DiaDiem!.TenDiaDiem,
                SoSuKien = g.Count()
            })
            .OrderByDescending(x => x.SoSuKien)
            .ThenBy(x => x.Ten)
            .FirstOrDefault();
        vm.DiaDiemSuDungNhieuNhat = ddNhieuNhat == null ? null : new NoiBatItem
        {
            Ten = ddNhieuNhat.Ten,
            GiaTri = ddNhieuNhat.SoSuKien,
            ChiTiet = $"{ddNhieuNhat.SoSuKien} sự kiện"
        };

        // NB5. Phiên có nhiều người tham gia nhất
        var phienNhieu = vm.ThamGiaTheoPhien.FirstOrDefault();
        vm.PhienNhieuNguoiNhat = phienNhieu == null ? null : new NoiBatItem
        {
            Ten = phienNhieu.TenPhien,
            GiaTri = phienNhieu.SoNguoiThamGia,
            ChiTiet = $"{phienNhieu.SoNguoiThamGia} người • {phienNhieu.TenSuKien}"
        };

        // NB6. Người tham dự tham gia nhiều sự kiện nhất (sự kiện khác nhau, đăng ký đã duyệt)
        var nguoiNhieu = dangKyDaDuyet
            .Where(d => d.NguoiThamDu != null)
            .GroupBy(d => d.MaNguoiThamDu)
            .Select(g => new
            {
                HoTen = g.First().NguoiThamDu!.HoTen,
                SoSuKien = g.Select(d => d.MaSuKien).Distinct().Count()
            })
            .OrderByDescending(x => x.SoSuKien)
            .ThenBy(x => x.HoTen)
            .FirstOrDefault();
        vm.NguoiThamDuNhieuNhat = nguoiNhieu == null ? null : new NoiBatItem
        {
            Ten = nguoiNhieu.HoTen,
            GiaTri = nguoiNhieu.SoSuKien,
            ChiTiet = $"{nguoiNhieu.SoSuKien} sự kiện"
        };

        return View(vm);
    }

    /// <summary>
    /// Tỷ lệ phần trăm làm tròn 1 chữ số.
    /// Mẫu số <= 0 trả về 0. Kết quả không hữu hạn (NaN, Infinity) cũng trả về 0.
    /// </summary>
    private static double TinhTyLe(int tuSo, int mauSo)
    {
        if (mauSo <= 0)
        {
            return 0;
        }

        double ketQua = Math.Round((double)tuSo / mauSo * 100, 1);
        return double.IsFinite(ketQua) ? ketQua : 0;
    }
}
