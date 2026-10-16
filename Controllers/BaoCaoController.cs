// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 5 - Báo cáo theo thời gian (Mục 9.6).

using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module5;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers;

public class BaoCaoController : Controller
{
    private readonly AppDbContext _context;

    public BaoCaoController(AppDbContext context)
    {
        _context = context;
    }

    // =================================================================
    // BÁO CÁO THEO THỜI GIAN - CHỈ ADMIN
    // =================================================================
    [HttpGet]
    public async Task<IActionResult> Index(
        int? thang,
        int? nam,
        DateTime? tuNgay,
        DateTime? denNgay)
    {
        // 1. KIỂM TRA QUYỀN
        if (HttpContext.Session.GetString("VaiTro") != "Admin")
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var vm = new BaoCaoViewModel
        {
            Thang = thang,
            Nam = nam,
            TuNgay = tuNgay,
            DenNgay = denNgay
        };

        bool coKhoangNgay = tuNgay.HasValue || denNgay.HasValue;
        bool coThangNam = thang.HasValue || nam.HasValue;

        // 2. CHƯA LỌC: hiển thị form trống
        if (!coKhoangNgay && !coThangNam)
        {
            return View(vm);
        }

        vm.DaLoc = true;

        // 3. VALIDATION: không chọn đồng thời hai kiểu lọc
        if (coKhoangNgay && coThangNam)
        {
            vm.LoiValidation = "Chỉ chọn một kiểu lọc: hoặc Từ ngày/Đến ngày, hoặc Tháng/Năm.";
            return View(vm);
        }

        DateTime tu;   // mốc bắt đầu (bao gồm)
        DateTime den;  // mốc kết thúc (không bao gồm)

        // 4. XÁC ĐỊNH KHOẢNG THỜI GIAN
        if (coKhoangNgay)
        {
            // VALIDATION: FromDate > ToDate -> báo lỗi, không query
            if (tuNgay.HasValue && denNgay.HasValue && tuNgay.Value.Date > denNgay.Value.Date)
            {
                vm.LoiValidation = "\"Từ ngày\" phải nhỏ hơn hoặc bằng \"Đến ngày\".";
                return View(vm);
            }

            tu = tuNgay?.Date ?? DateTime.MinValue;
            den = denNgay.HasValue ? denNgay.Value.Date.AddDays(1) : DateTime.MaxValue;

            if (tuNgay.HasValue && denNgay.HasValue)
                vm.MoTaKhoangThoiGian = $"Từ {tuNgay.Value:dd/MM/yyyy} đến {denNgay.Value:dd/MM/yyyy}";
            else if (tuNgay.HasValue)
                vm.MoTaKhoangThoiGian = $"Từ {tuNgay.Value:dd/MM/yyyy} đến nay";
            else
                vm.MoTaKhoangThoiGian = $"Đến {denNgay!.Value:dd/MM/yyyy}";
        }
        else
        {
            int y = nam ?? DateTime.Now.Year;

            // VALIDATION: tháng và năm hợp lệ
            if (thang.HasValue && (thang.Value < 1 || thang.Value > 12))
            {
                vm.LoiValidation = "Tháng phải nằm trong khoảng 1 đến 12.";
                return View(vm);
            }
            if (y < 1900 || y > 2100)
            {
                vm.LoiValidation = "Năm không hợp lệ (1900 - 2100).";
                return View(vm);
            }

            if (thang.HasValue)
            {
                tu = new DateTime(y, thang.Value, 1);
                den = tu.AddMonths(1);
                vm.MoTaKhoangThoiGian = $"Tháng {thang.Value:D2}/{y}";
            }
            else
            {
                tu = new DateTime(y, 1, 1);
                den = tu.AddYears(1);
                vm.MoTaKhoangThoiGian = $"Năm {y}";
            }
        }

        // 5. WHERE() - query gốc, dùng lại cho tổng hợp và nhóm
        // Chưa thực thi SQL ở đây; EF chỉ dịch thành WHERE khi thực thi.
        var suKienQuery = _context.SuKiens
            .AsNoTracking()
            .Where(s => s.ThoiGianBatDau >= tu && s.ThoiGianBatDau < den);

        var dangKyQuery = _context.DangKyThamDus
            .AsNoTracking()
            .Where(d => d.NgayDangKy >= tu && d.NgayDangKy < den);

        var checkInQuery = _context.CheckIns
            .AsNoTracking()
            .Where(c => c.ThoiGianCheckIn >= tu && c.ThoiGianCheckIn < den);

        // 6. COUNT() - tổng hợp, thực thi SELECT COUNT(*) phía SQL Server
        vm.TongSuKien = await suKienQuery.CountAsync();
        vm.TongDangKy = await dangKyQuery.CountAsync();
        vm.TongCheckIn = await checkInQuery.CountAsync();

        // 7. GROUPBY() + SELECT() + COUNT() - theo tháng, gom nhóm phía SQL Server
        vm.SuKienTheoThang = await suKienQuery
            .GroupBy(s => new { s.ThoiGianBatDau.Year, s.ThoiGianBatDau.Month })
            .Select(g => new BaoCaoTheoThangItem
            {
                Nam = g.Key.Year,
                Thang = g.Key.Month,
                SoLuong = g.Count()
            })
            .OrderBy(x => x.Nam)
            .ThenBy(x => x.Thang)
            .ToListAsync();

        vm.DangKyTheoThang = await dangKyQuery
            .GroupBy(d => new { d.NgayDangKy.Year, d.NgayDangKy.Month })
            .Select(g => new BaoCaoTheoThangItem
            {
                Nam = g.Key.Year,
                Thang = g.Key.Month,
                SoLuong = g.Count()
            })
            .OrderBy(x => x.Nam)
            .ThenBy(x => x.Thang)
            .ToListAsync();

        vm.CheckInTheoThang = await checkInQuery
            .GroupBy(c => new { c.ThoiGianCheckIn.Year, c.ThoiGianCheckIn.Month })
            .Select(g => new BaoCaoTheoThangItem
            {
                Nam = g.Key.Year,
                Thang = g.Key.Month,
                SoLuong = g.Count()
            })
            .OrderBy(x => x.Nam)
            .ThenBy(x => x.Thang)
            .ToListAsync();

        // 8. SELECT() + COUNT() - chi tiết từng sự kiện trong khoảng
        // Đăng ký và check-in được đếm theo cùng khoảng thời gian đã lọc.
        // CheckIn không có MaSuKien trực tiếp nên đi qua DangKyThamDu.MaSuKien.
        vm.DanhSachSuKien = await suKienQuery
            .Select(s => new BaoCaoSuKienItem
            {
                TenSuKien = s.TenSuKien,
                ThoiGianBatDau = s.ThoiGianBatDau,
                TrangThai = s.TrangThai,
                SoDangKy = _context.DangKyThamDus.Count(d =>
                    d.MaSuKien == s.MaSuKien &&
                    d.NgayDangKy >= tu && d.NgayDangKy < den),
                SoCheckIn = _context.CheckIns.Count(c =>
                    c.DangKyThamDu!.MaSuKien == s.MaSuKien &&
                    c.ThoiGianCheckIn >= tu && c.ThoiGianCheckIn < den)
            })
            .OrderByDescending(x => x.SoDangKy)
            .ThenBy(x => x.ThoiGianBatDau)
            .ToListAsync();

        // 9. TRẢ VIEW
        return View(vm);
    }
}
