// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 4 - Xét duyệt đăng ký, mã tham dự, check-in, điểm danh.

using System.Data;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module4;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Services;

public class DichVuModule4 : IDichVuModule4
{
    private static readonly TrangThaiDangKy[] TrangThaiDangKyHieuLuc =
    [
        TrangThaiDangKy.ChoDuyet,
        TrangThaiDangKy.DaDuyet,
        TrangThaiDangKy.DaCheckIn,
        TrangThaiDangKy.HoanThanh
    ];

    private static readonly TrangThaiDangKy[] TrangThaiDangKySuKienDaDuyet =
    [
        TrangThaiDangKy.DaDuyet,
        TrangThaiDangKy.DaCheckIn,
        TrangThaiDangKy.HoanThanh
    ];

    private readonly AppDbContext _context;

    public DichVuModule4(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DuyetDangKyViewModel> LayDanhSachChoDuyetAsync()
    {
        var danhSach = await _context.DangKyThamDus
            .AsNoTracking()
            .Where(d => d.TrangThai == TrangThaiDangKy.ChoDuyet)
            .OrderBy(d => d.NgayDangKy)
            .Select(d => new DangKyChoDuyetViewModel
            {
                MaDangKy = d.MaDangKy,
                TenNguoiThamDu = d.NguoiThamDu == null ? string.Empty : d.NguoiThamDu.HoTen,
                TenSuKien = d.SuKien == null ? string.Empty : d.SuKien.TenSuKien,
                TrangThai = d.TrangThai.ToString(),
                MaThamDu = d.MaThamDu
            })
            .ToListAsync();

        return new DuyetDangKyViewModel { DanhSachDangKy = danhSach };
    }

    public async Task<ChiTietDangKyViewModel> LayChiTietDangKyAsync(string maDangKy)
    {
        var dangKy = await _context.DangKyThamDus
            .AsNoTracking()
            .Include(d => d.NguoiThamDu)
            .Include(d => d.SuKien)
            .FirstOrDefaultAsync(d => d.MaDangKy == maDangKy);

        if (dangKy == null)
        {
            return new ChiTietDangKyViewModel
            {
                ThongBao = "Không tìm thấy đăng ký."
            };
        }

        return new ChiTietDangKyViewModel
        {
            MaDangKy = dangKy.MaDangKy,
            TenNguoiThamDu = dangKy.NguoiThamDu?.HoTen ?? "Không có thông tin",
            Email = dangKy.NguoiThamDu?.Email ?? string.Empty,
            SoDienThoai = dangKy.NguoiThamDu?.SoDienThoai ?? string.Empty,
            TenSuKien = dangKy.SuKien?.TenSuKien ?? "Không có thông tin",
            TrangThai = dangKy.TrangThai.ToString(),
            NgayDangKy = dangKy.NgayDangKy,
            NgayDuyet = dangKy.NgayDuyet,
            MaThamDu = dangKy.MaThamDu
        };
    }

    public async Task<DiemDanhPhienViewModel> LayDiemDanhPhienAsync(string? maPhienSuKien)
    {
        var viewModel = new DiemDanhPhienViewModel
        {
            DanhSachPhien = await _context.PhienSuKiens
                .AsNoTracking()
                .Where(p => p.TrangThai != "DaHuy" && p.SuKien != null &&
                    p.SuKien.TrangThai != nameof(TrangThaiSuKien.DaHuy))
                .OrderBy(p => p.ThoiGianBatDau)
                .Select(p => new PhienDiemDanhLuaChonViewModel
                {
                    MaPhienSuKien = p.MaPhienSuKien,
                    TenHienThi = p.SuKien == null
                        ? p.TenPhien
                        : $"{p.SuKien.TenSuKien} - {p.TenPhien}"
                })
                .ToListAsync(),
            MaPhienSuKien = maPhienSuKien
        };

        if (string.IsNullOrWhiteSpace(maPhienSuKien))
            return viewModel;

        var phien = await _context.PhienSuKiens
            .AsNoTracking()
            .Include(p => p.SuKien)
            .FirstOrDefaultAsync(p => p.MaPhienSuKien == maPhienSuKien);

        if (phien == null || phien.SuKien == null)
        {
            viewModel.ThongBao = "Phiên sự kiện không tồn tại.";
            return viewModel;
        }

        if (phien.TrangThai == "DaHuy" ||
            phien.SuKien.TrangThai == nameof(TrangThaiSuKien.DaHuy))
        {
            viewModel.ThongBao = "Phiên hoặc sự kiện đã bị hủy.";
            return viewModel;
        }

        viewModel.TenPhien = phien.TenPhien;
        viewModel.TenSuKien = phien.SuKien.TenSuKien;
        viewModel.DanhSachNguoiThamDu = await (
            from dangKyPhien in _context.DangKyPhiens.AsNoTracking()
            join dangKySuKien in _context.DangKyThamDus.AsNoTracking()
                on dangKyPhien.MaNguoiThamDu equals dangKySuKien.MaNguoiThamDu
            join nguoiThamDu in _context.NguoiThamDus.AsNoTracking()
                on dangKyPhien.MaNguoiThamDu equals nguoiThamDu.MaNguoiThamDu
            where dangKyPhien.MaPhien == phien.MaPhienSuKien &&
                TrangThaiDangKyHieuLuc.Contains(dangKyPhien.TrangThai) &&
                dangKySuKien.MaSuKien == phien.MaSuKien &&
                TrangThaiDangKySuKienDaDuyet.Contains(dangKySuKien.TrangThai)
            orderby nguoiThamDu.HoTen
            select new NguoiDiemDanhPhienViewModel
            {
                MaNguoiThamDu = nguoiThamDu.MaNguoiThamDu,
                HoTen = nguoiThamDu.HoTen,
                TrangThaiDangKyPhien = dangKyPhien.TrangThai,
                DaDiemDanh = dangKyPhien.TrangThai == TrangThaiDangKy.DaCheckIn
            })
            .ToListAsync();

        return viewModel;
    }

    public async Task<KetQuaThaoTacViewModel> DuyetDangKyAsync(string maDangKy)
    {
        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        var dangKy = await _context.DangKyThamDus
            .Include(d => d.NguoiThamDu)
            .Include(d => d.SuKien)
                .ThenInclude(s => s!.DiaDiem)
            .FirstOrDefaultAsync(d => d.MaDangKy == maDangKy);

        if (dangKy == null)
            return KetQuaThatBai("Đăng ký không tồn tại.");

        if (dangKy.TrangThai != TrangThaiDangKy.ChoDuyet)
            return KetQuaThatBai("Chỉ đăng ký đang chờ duyệt mới được duyệt.");

        if (dangKy.NguoiThamDu == null)
            return KetQuaThatBai("Người tham dự của đăng ký không tồn tại.");

        if (dangKy.SuKien == null)
            return KetQuaThatBai("Sự kiện của đăng ký không tồn tại.");

        if (dangKy.SuKien.TrangThai == nameof(TrangThaiSuKien.DaHuy))
            return KetQuaThatBai("Không thể duyệt đăng ký vì sự kiện đã bị hủy.");

        if (dangKy.SuKien.DiaDiem == null)
            return KetQuaThatBai("Địa điểm của sự kiện không tồn tại.");

        int soLuongDangKyHieuLuc = await _context.DangKyThamDus
            .CountAsync(d =>
                d.MaDangKy != dangKy.MaDangKy &&
                d.MaSuKien == dangKy.MaSuKien &&
                TrangThaiDangKyHieuLuc.Contains(d.TrangThai));

        if (soLuongDangKyHieuLuc >= dangKy.SuKien.DiaDiem.SucChuaToiDa)
            return KetQuaThatBai("Sự kiện đã đủ sức chứa, không thể duyệt thêm.");

        dangKy.MaThamDu = await TaoMaThamDuChuaSuDungAsync();
        dangKy.TrangThai = TrangThaiDangKy.DaDuyet;
        dangKy.NgayDuyet = DateTime.Now;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return KetQuaThanhCong("Đã duyệt đăng ký và cấp mã tham dự.");
    }

    public async Task<KetQuaThaoTacViewModel> TuChoiDangKyAsync(string maDangKy, string lyDoTuChoi)
    {
        if (string.IsNullOrWhiteSpace(lyDoTuChoi))
            return KetQuaThatBai("Vui lòng nhập lý do từ chối.");

        var dangKy = await _context.DangKyThamDus
            .FirstOrDefaultAsync(d => d.MaDangKy == maDangKy);

        if (dangKy == null)
            return KetQuaThatBai("Đăng ký không tồn tại.");

        if (dangKy.TrangThai != TrangThaiDangKy.ChoDuyet)
            return KetQuaThatBai("Chỉ đăng ký đang chờ duyệt mới được từ chối.");

        var ghiChuTuChoi = $"Lý do từ chối: {lyDoTuChoi.Trim()}";
        dangKy.GhiChu = string.IsNullOrWhiteSpace(dangKy.GhiChu)
            ? ghiChuTuChoi
            : $"{dangKy.GhiChu}{Environment.NewLine}{ghiChuTuChoi}";
        dangKy.TrangThai = TrangThaiDangKy.TuChoi;

        await _context.SaveChangesAsync();
        return KetQuaThanhCong("Đã từ chối đăng ký.");
    }

    public async Task<KetQuaThaoTacViewModel> DiemDanhPhienAsync(
        string maPhienSuKien,
        string maNguoiThamDu)
    {
        var phien = await _context.PhienSuKiens
            .Include(p => p.SuKien)
            .FirstOrDefaultAsync(p => p.MaPhienSuKien == maPhienSuKien);

        if (phien == null || phien.SuKien == null)
            return KetQuaThatBai("Phiên sự kiện không tồn tại.");

        if (phien.TrangThai == "DaHuy" ||
            phien.SuKien.TrangThai == nameof(TrangThaiSuKien.DaHuy))
        {
            return KetQuaThatBai("Phiên hoặc sự kiện đã bị hủy.");
        }

        DateTime thoiGianHienTai = DateTime.Now;
        if (thoiGianHienTai < phien.ThoiGianBatDau ||
            thoiGianHienTai > phien.ThoiGianKetThuc)
        {
            return KetQuaThatBai("Chỉ được điểm danh trong thời gian diễn ra phiên.");
        }

        bool coDangKySuKienDaDuyet = await _context.DangKyThamDus
            .AnyAsync(d =>
                d.MaNguoiThamDu == maNguoiThamDu &&
                d.MaSuKien == phien.MaSuKien &&
                TrangThaiDangKySuKienDaDuyet.Contains(d.TrangThai));

        if (!coDangKySuKienDaDuyet)
            return KetQuaThatBai("Người tham dự chưa có đăng ký sự kiện được duyệt.");

        var cacDangKyPhien = await _context.DangKyPhiens
            .Where(d =>
                d.MaNguoiThamDu == maNguoiThamDu &&
                d.MaPhien == phien.MaPhienSuKien)
            .ToListAsync();

        if (cacDangKyPhien.Any(d => d.TrangThai == TrangThaiDangKy.DaCheckIn))
            return KetQuaThatBai("Người tham dự đã được điểm danh phiên này.");

        var dangKyPhienHopLe = cacDangKyPhien
            .Where(d => TrangThaiDangKyHieuLuc.Contains(d.TrangThai))
            .ToList();

        if (dangKyPhienHopLe.Count == 0)
            return KetQuaThatBai("Người tham dự chưa đăng ký hợp lệ phiên này.");

        if (dangKyPhienHopLe.Count > 1)
            return KetQuaThatBai("Có nhiều đăng ký phiên trùng; cần kiểm tra dữ liệu trước khi điểm danh.");

        string maDangKyPhien = dangKyPhienHopLe[0].MaDangKyPhien;
        int soDongCapNhat = await _context.DangKyPhiens
            .Where(d =>
                d.MaDangKyPhien == maDangKyPhien &&
                d.TrangThai != TrangThaiDangKy.DaCheckIn &&
                TrangThaiDangKyHieuLuc.Contains(d.TrangThai))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(d => d.TrangThai, TrangThaiDangKy.DaCheckIn));

        if (soDongCapNhat == 0)
            return KetQuaThatBai("Người tham dự đã được điểm danh phiên này.");

        return KetQuaThanhCong("Điểm danh phiên thành công.");
    }

    public async Task<KetQuaThaoTacViewModel> CheckInAsync(
        string maThamDu,
        string nguoiThucHien,
        string? ghiChu)
    {
        if (string.IsNullOrWhiteSpace(maThamDu))
            return KetQuaThatBai("Mã tham dự không hợp lệ.");

        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        string maThamDuNhap = maThamDu.Trim();
        var cacDangKy = await _context.DangKyThamDus
            .Where(d => d.MaThamDu == maThamDuNhap)
            .Include(d => d.NguoiThamDu)
            .Include(d => d.SuKien)
            .Take(2)
            .ToListAsync();

        if (cacDangKy.Count != 1 ||
            !string.Equals(cacDangKy[0].MaThamDu, maThamDuNhap, StringComparison.Ordinal))
            return KetQuaThatBai("Mã tham dự không tồn tại hoặc không duy nhất.");

        var dangKy = cacDangKy[0];
        bool daCheckIn = dangKy.TrangThai == TrangThaiDangKy.DaCheckIn ||
            await _context.CheckIns.AnyAsync(c => c.MaDangKy == dangKy.MaDangKy);

        if (daCheckIn)
            return KetQuaThatBai("Đăng ký này đã check-in trước đó.");

        if (dangKy.TrangThai != TrangThaiDangKy.DaDuyet)
            return KetQuaThatBai("Đăng ký chưa được duyệt nên không thể check-in.");

        if (dangKy.NguoiThamDu == null)
            return KetQuaThatBai("Người tham dự của đăng ký không tồn tại.");

        if (dangKy.SuKien == null)
            return KetQuaThatBai("Sự kiện của đăng ký không tồn tại.");

        if (dangKy.SuKien.TrangThai == nameof(TrangThaiSuKien.DaHuy))
            return KetQuaThatBai("Sự kiện đã bị hủy, không thể check-in.");

        DateTime thoiGianHienTai = DateTime.Now;
        if (thoiGianHienTai < dangKy.SuKien.ThoiGianBatDau ||
            thoiGianHienTai > dangKy.SuKien.ThoiGianKetThuc)
        {
            return KetQuaThatBai("Check-in chỉ được thực hiện trong thời gian diễn ra sự kiện.");
        }

        var checkIn = new CheckIn
        {
            MaCheckIn = $"CI-{Guid.NewGuid():N}".ToUpperInvariant(),
            MaDangKy = dangKy.MaDangKy,
            ThoiGianCheckIn = thoiGianHienTai,
            NguoiThucHien = nguoiThucHien,
            GhiChu = ghiChu?.Trim() ?? string.Empty
        };

        dangKy.TrangThai = TrangThaiDangKy.DaCheckIn;
        _context.CheckIns.Add(checkIn);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return KetQuaThanhCong("Check-in thành công.");
    }

    private async Task<string> TaoMaThamDuChuaSuDungAsync()
    {
        string maThamDu;
        do
        {
            maThamDu = $"UN-{Guid.NewGuid():N}".ToUpperInvariant();
        }
        while (await _context.DangKyThamDus.AnyAsync(d => d.MaThamDu == maThamDu));

        return maThamDu;
    }

    private static KetQuaThaoTacViewModel KetQuaThanhCong(string thongBao) =>
        new() { ThanhCong = true, ThongBao = thongBao };

    private static KetQuaThaoTacViewModel KetQuaThatBai(string thongBao) =>
        new() { ThanhCong = false, ThongBao = thongBao };
}