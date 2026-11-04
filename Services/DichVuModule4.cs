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

    private static readonly TrangThaiDangKy[] TrangThaiDangKyPhienDuocDiemDanh =
    [
        TrangThaiDangKy.DaDuyet,
        TrangThaiDangKy.DaCheckIn
    ];

    private static readonly TrangThaiDangKy[] TrangThaiDangKyPhienHieuLuc =
    [
        TrangThaiDangKy.ChoDuyet,
        TrangThaiDangKy.DaDuyet,
        TrangThaiDangKy.DaCheckIn,
        TrangThaiDangKy.HoanThanh
    ];

    private const int SoLanThuTaoMaThamDuToiDa = 10;

    private readonly AppDbContext _context;

    public DichVuModule4(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DuyetDangKyViewModel> LayDanhSachChoDuyetAsync(
        string? maSuKien = null,
        TrangThaiDangKy? trangThai = null,
        string? tuKhoa = null)
    {
        var truyVanDangKy = _context.DangKyThamDus.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(maSuKien))
            truyVanDangKy = truyVanDangKy.Where(d => d.MaSuKien == maSuKien.Trim());

        if (trangThai.HasValue)
            truyVanDangKy = truyVanDangKy.Where(d => d.TrangThai == trangThai.Value);

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            string tuKhoaDaChuanHoa = tuKhoa.Trim();
            truyVanDangKy = truyVanDangKy.Where(d =>
                d.MaDangKy.Contains(tuKhoaDaChuanHoa) ||
                (d.MaThamDu != null && d.MaThamDu.Contains(tuKhoaDaChuanHoa)) ||
                (d.NguoiThamDu != null &&
                    (d.NguoiThamDu.HoTen.Contains(tuKhoaDaChuanHoa) ||
                     d.NguoiThamDu.Email.Contains(tuKhoaDaChuanHoa))));
        }

        var danhSach = await truyVanDangKy
            .OrderByDescending(d => d.NgayDangKy)
            .Select(d => new DangKyChoDuyetViewModel
            {
                MaDangKy = d.MaDangKy,
                MaSuKien = d.MaSuKien,
                TenNguoiThamDu = d.NguoiThamDu == null ? string.Empty : d.NguoiThamDu.HoTen,
                Email = d.NguoiThamDu == null ? string.Empty : d.NguoiThamDu.Email,
                TenSuKien = d.SuKien == null ? string.Empty : d.SuKien.TenSuKien,
                TrangThai = d.TrangThai.ToString(),
                MaThamDu = d.MaThamDu
            })
            .ToListAsync();

        var danhSachSuKien = await _context.SuKiens
            .AsNoTracking()
            .OrderBy(s => s.TenSuKien)
            .Select(s => new SuKienLuaChonViewModel
            {
                MaSuKien = s.MaSuKien,
                TenSuKien = s.TenSuKien
            })
            .ToListAsync();

        return new DuyetDangKyViewModel
        {
            DanhSachDangKy = danhSach,
            DanhSachSuKien = danhSachSuKien,
            MaSuKien = maSuKien,
            TrangThai = trangThai,
            TuKhoa = tuKhoa
        };
    }

    public async Task<ChiTietDangKyViewModel> LayChiTietDangKyAsync(string maDangKy)
    {
        var dangKy = await _context.DangKyThamDus
            .AsNoTracking()
            .Include(d => d.NguoiThamDu)
            .Include(d => d.SuKien)
            .Include(d => d.CheckIn)
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
            NgayCheckIn = dangKy.CheckIn?.ThoiGianCheckIn,
            NguoiCheckIn = dangKy.CheckIn?.NguoiThucHien,
            MaThamDu = dangKy.MaThamDu,
            GhiChu = dangKy.TrangThai == TrangThaiDangKy.TuChoi &&
                !string.IsNullOrWhiteSpace(dangKy.LyDoTuChoiHuy)
                    ? dangKy.LyDoTuChoiHuy
                    : dangKy.GhiChu
        };
    }

    public async Task<ChiTietDangKyViewModel> TraCuuMaThamDuAsync(string? maThamDu)
    {
        if (string.IsNullOrWhiteSpace(maThamDu))
            return new ChiTietDangKyViewModel { ThongBao = "Vui lòng nhập mã tham dự." };

        string maThamDuDaChuanHoa = maThamDu.Trim();
        var cacDangKy = await _context.DangKyThamDus
            .AsNoTracking()
            .Where(d => d.MaThamDu == maThamDuDaChuanHoa)
            .Include(d => d.NguoiThamDu)
            .Include(d => d.SuKien)
            .Include(d => d.CheckIn)
            .Take(2)
            .ToListAsync();

        if (cacDangKy.Count > 1)
            return new ChiTietDangKyViewModel { ThongBao = "Mã tham dự bị trùng; cần kiểm tra dữ liệu trước khi check-in." };

        if (cacDangKy.Count == 0 ||
            !string.Equals(cacDangKy[0].MaThamDu, maThamDuDaChuanHoa, StringComparison.Ordinal))
        {
            return new ChiTietDangKyViewModel { ThongBao = "Không tìm thấy mã tham dự hợp lệ." };
        }

        var dangKy = cacDangKy[0];
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
            NgayCheckIn = dangKy.CheckIn?.ThoiGianCheckIn,
            NguoiCheckIn = dangKy.CheckIn?.NguoiThucHien,
            MaThamDu = dangKy.MaThamDu,
            GhiChu = dangKy.TrangThai == TrangThaiDangKy.TuChoi &&
                !string.IsNullOrWhiteSpace(dangKy.LyDoTuChoiHuy)
                    ? dangKy.LyDoTuChoiHuy
                    : dangKy.GhiChu
        };
    }

    public async Task<DiemDanhPhienViewModel> LayDiemDanhPhienAsync(
        string? maPhienSuKien,
        string? tuKhoa = null)
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
                MaPhienSuKien = maPhienSuKien,
                TuKhoa = tuKhoa
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
        int soDangKySuKienDaDuyet = await _context.DangKyThamDus
            .CountAsync(d =>
                d.MaSuKien == phien.MaSuKien &&
                TrangThaiDangKySuKienDaDuyet.Contains(d.TrangThai));

        var thongKeDangKyPhien = await _context.DangKyPhiens
            .Where(d => d.MaPhien == phien.MaPhienSuKien)
            .GroupBy(d => d.TrangThai)
            .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
            .ToListAsync();

        var truyVanDangKyDuDieuKien = (
            from dangKyPhien in _context.DangKyPhiens.AsNoTracking()
            join nguoiThamDu in _context.NguoiThamDus.AsNoTracking()
                on dangKyPhien.MaNguoiThamDu equals nguoiThamDu.MaNguoiThamDu
            where dangKyPhien.MaPhien == phien.MaPhienSuKien &&
                TrangThaiDangKyPhienDuocDiemDanh.Contains(dangKyPhien.TrangThai) &&
                nguoiThamDu.TrangThai &&
                _context.DangKyThamDus.Any(d =>
                    d.MaNguoiThamDu == dangKyPhien.MaNguoiThamDu &&
                    d.MaSuKien == phien.MaSuKien &&
                    TrangThaiDangKySuKienDaDuyet.Contains(d.TrangThai))
            select new
            {
                MaNguoiThamDu = nguoiThamDu.MaNguoiThamDu,
                HoTen = nguoiThamDu.HoTen,
                Email = nguoiThamDu.Email,
                TrangThaiDangKyPhien = dangKyPhien.TrangThai,
                dangKyPhien.KetQuaDiemDanh,
                dangKyPhien.ThoiGianDiemDanh,
                dangKyPhien.NguoiDiemDanh
            });

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            string tuKhoaDaChuanHoa = tuKhoa.Trim();
            truyVanDangKyDuDieuKien = truyVanDangKyDuDieuKien.Where(d =>
                d.MaNguoiThamDu.Contains(tuKhoaDaChuanHoa) ||
                d.HoTen.Contains(tuKhoaDaChuanHoa) ||
                d.Email.Contains(tuKhoaDaChuanHoa));
        }

        var dangKyDuDieuKienDiemDanh = await truyVanDangKyDuDieuKien
            .OrderBy(d => d.HoTen)
            .ToListAsync();

        DateTime thoiGianHienTai = DateTime.Now;
        var dangKyTheoNguoiThamDu = dangKyDuDieuKienDiemDanh
            .GroupBy(d => d.MaNguoiThamDu)
            .ToList();

        viewModel.SoNguoiCoDangKyPhienTrung =
            dangKyTheoNguoiThamDu.Count(g => g.Count() > 1);
        viewModel.DanhSachNguoiThamDu = dangKyTheoNguoiThamDu
            .Where(g => g.Count() == 1)
            .Select(g =>
            {
                var dangKy = g.Single();
                return new NguoiDiemDanhPhienViewModel
                {
                    MaNguoiThamDu = dangKy.MaNguoiThamDu,
                    HoTen = dangKy.HoTen,
                    TrangThaiDangKyPhien = dangKy.TrangThaiDangKyPhien,
                    KetQuaDiemDanh = dangKy.KetQuaDiemDanh,
                    ThoiGianDiemDanh = dangKy.ThoiGianDiemDanh,
                    NguoiDiemDanh = dangKy.NguoiDiemDanh,
                    DaDiemDanh = dangKy.KetQuaDiemDanh.HasValue ||
                        dangKy.TrangThaiDangKyPhien == TrangThaiDangKy.DaCheckIn,
                    CoTheDanhDauCoMat = thoiGianHienTai >= phien.ThoiGianBatDau &&
                        thoiGianHienTai <= phien.ThoiGianKetThuc,
                    CoTheDanhDauVangMat = thoiGianHienTai > phien.ThoiGianKetThuc
                };
            })
            .OrderBy(n => n.HoTen)
            .ToList();

        if (viewModel.DanhSachNguoiThamDu.Count == 0)
        {
            int soDangKyPhien = thongKeDangKyPhien.Sum(g => g.SoLuong);
            int soDangKyPhienChoDuyet = thongKeDangKyPhien
                .Where(g => g.TrangThai == TrangThaiDangKy.ChoDuyet)
                .Sum(g => g.SoLuong);

            viewModel.ThongBaoDanhSachRong = soDangKySuKienDaDuyet == 0
                ? "Chưa có đăng ký sự kiện được duyệt cho phiên này."
                : soDangKyPhien == 0
                    ? "Đã có đăng ký sự kiện được duyệt, nhưng chưa có người đăng ký phiên này."
                    : viewModel.SoNguoiCoDangKyPhienTrung > 0
                        ? $"Có {viewModel.SoNguoiCoDangKyPhienTrung} người có nhiều đăng ký phiên đủ điều kiện bị trùng; các trường hợp này đã bị loại khỏi danh sách điểm danh."
                        : soDangKyPhienChoDuyet > 0
                            ? $"Có {soDangKyPhienChoDuyet} đăng ký phiên đang chờ duyệt. Hãy duyệt đăng ký phiên trước khi điểm danh."
                            : "Có đăng ký phiên, nhưng không có đăng ký nào vừa được duyệt phiên vừa thuộc đăng ký sự kiện đã duyệt.";
        }

        return viewModel;
    }

    public async Task<DangKyPhienChoDuyetViewModel> LayDangKyPhienChoDuyetAsync(
        string? maPhienSuKien)
    {
        var viewModel = new DangKyPhienChoDuyetViewModel
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

        viewModel.DanhSachDangKy = await _context.DangKyPhiens
            .AsNoTracking()
            .Where(d => d.MaPhien == maPhienSuKien &&
                d.TrangThai == TrangThaiDangKy.ChoDuyet)
            .OrderBy(d => d.NgayDangKy)
            .Select(d => new DangKyPhienChoDuyetItemViewModel
            {
                MaDangKyPhien = d.MaDangKyPhien,
                MaNguoiThamDu = d.MaNguoiThamDu,
                TenNguoiThamDu = d.NguoiThamDu == null ? string.Empty : d.NguoiThamDu.HoTen,
                Email = d.NguoiThamDu == null ? string.Empty : d.NguoiThamDu.Email,
                TenSuKien = d.Phien == null || d.Phien.SuKien == null
                    ? string.Empty
                    : d.Phien.SuKien.TenSuKien,
                TenPhien = d.Phien == null ? string.Empty : d.Phien.TenPhien,
                NgayDangKy = d.NgayDangKy,
                TrangThai = d.TrangThai.ToString()
            })
            .ToListAsync();

        return viewModel;
    }

    public async Task<KetQuaThaoTacViewModel> DuyetDangKyAsync(string maDangKy)
    {
        if (string.IsNullOrWhiteSpace(maDangKy))
            return KetQuaThatBai("Mã đăng ký không hợp lệ.");

        maDangKy = maDangKy.Trim();

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

        string? maThamDu = dangKy.MaThamDu;
        if (!string.IsNullOrWhiteSpace(maThamDu))
        {
            bool maDaDuocCapChoDangKyKhac = await _context.DangKyThamDus
                .AnyAsync(d => d.MaDangKy != dangKy.MaDangKy && d.MaThamDu == maThamDu);
            if (maDaDuocCapChoDangKyKhac)
                return KetQuaThatBai("Mã tham dự hiện tại bị trùng; cần kiểm tra dữ liệu trước khi duyệt.");
        }
        else
        {
            maThamDu = await TaoMaThamDuChuaSuDungAsync();
            if (maThamDu == null)
                return KetQuaThatBai("Không thể tạo mã tham dự. Vui lòng thử lại.");
        }

        dangKy.MaThamDu = maThamDu;
        dangKy.TrangThai = TrangThaiDangKy.DaDuyet;
        dangKy.NgayDuyet = DateTime.Now;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return KetQuaThanhCong("Đã duyệt đăng ký và cấp mã tham dự.");
    }

    public async Task<KetQuaThaoTacViewModel> DuyetDangKyPhienAsync(string maDangKyPhien)
    {
        if (string.IsNullOrWhiteSpace(maDangKyPhien))
            return KetQuaThatBai("Mã đăng ký phiên không hợp lệ.");

        maDangKyPhien = maDangKyPhien.Trim();
        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        var dangKyPhien = await _context.DangKyPhiens
            .Include(d => d.NguoiThamDu)
            .Include(d => d.Phien)
                .ThenInclude(p => p!.SuKien)
            .FirstOrDefaultAsync(d => d.MaDangKyPhien == maDangKyPhien);

        if (dangKyPhien == null)
            return KetQuaThatBai("Đăng ký phiên không tồn tại.");

        if (dangKyPhien.TrangThai != TrangThaiDangKy.ChoDuyet)
            return KetQuaThatBai("Chỉ đăng ký phiên đang chờ duyệt mới được duyệt.");

        if (dangKyPhien.NguoiThamDu == null || !dangKyPhien.NguoiThamDu.TrangThai)
            return KetQuaThatBai("Người tham dự không tồn tại hoặc đã ngừng hoạt động.");

        var phien = dangKyPhien.Phien;
        var suKien = phien?.SuKien;
        if (phien == null || suKien == null)
            return KetQuaThatBai("Phiên hoặc sự kiện của đăng ký không tồn tại.");

        if (phien.TrangThai == "DaHuy" ||
            suKien.TrangThai == nameof(TrangThaiSuKien.DaHuy))
        {
            return KetQuaThatBai("Không thể duyệt đăng ký phiên đã bị hủy.");
        }

        bool coDangKySuKienDaDuyet = await _context.DangKyThamDus
            .AnyAsync(d =>
                d.MaNguoiThamDu == dangKyPhien.MaNguoiThamDu &&
                d.MaSuKien == phien.MaSuKien &&
                TrangThaiDangKySuKienDaDuyet.Contains(d.TrangThai));
        if (!coDangKySuKienDaDuyet)
            return KetQuaThatBai("Người tham dự chưa có đăng ký sự kiện được duyệt.");

        if (phien.SoLuongToiDa <= 0)
            return KetQuaThatBai("Sức chứa phiên chưa được cấu hình hợp lệ.");

        int soDangKyPhienHieuLucKhac = await _context.DangKyPhiens
            .CountAsync(d =>
                d.MaPhien == phien.MaPhienSuKien &&
                d.MaDangKyPhien != dangKyPhien.MaDangKyPhien &&
                TrangThaiDangKyPhienHieuLuc.Contains(d.TrangThai));
        if (soDangKyPhienHieuLucKhac >= phien.SoLuongToiDa)
            return KetQuaThatBai("Phiên đã đủ số lượng đăng ký.");

        int soDongCapNhat = await _context.DangKyPhiens
            .Where(d =>
                d.MaDangKyPhien == dangKyPhien.MaDangKyPhien &&
                d.TrangThai == TrangThaiDangKy.ChoDuyet)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(d => d.TrangThai, TrangThaiDangKy.DaDuyet));

        if (soDongCapNhat == 0)
            return KetQuaThatBai("Đăng ký phiên đã được xử lý; không thể duyệt.");

        await transaction.CommitAsync();
        return KetQuaThanhCong("Đã duyệt đăng ký phiên.");
    }

    public async Task<KetQuaThaoTacViewModel> TuChoiDangKyAsync(string maDangKy, string lyDoTuChoi)
    {
        if (string.IsNullOrWhiteSpace(maDangKy))
            return KetQuaThatBai("Mã đăng ký không hợp lệ.");

        if (string.IsNullOrWhiteSpace(lyDoTuChoi))
            return KetQuaThatBai("Vui lòng nhập lý do từ chối.");

        maDangKy = maDangKy.Trim();
        lyDoTuChoi = lyDoTuChoi.Trim();

        if (lyDoTuChoi.Length > 500)
            return KetQuaThatBai("Lý do từ chối không được vượt quá 500 ký tự.");

        var dangKy = await _context.DangKyThamDus
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.MaDangKy == maDangKy);

        if (dangKy == null)
            return KetQuaThatBai("Đăng ký không tồn tại.");

        if (dangKy.TrangThai != TrangThaiDangKy.ChoDuyet)
            return KetQuaThatBai("Chỉ đăng ký đang chờ duyệt mới được từ chối.");

        var ghiChuTuChoi = $"Lý do từ chối: {lyDoTuChoi}";
        string ghiChuMoi = string.IsNullOrWhiteSpace(dangKy.GhiChu)
            ? ghiChuTuChoi
            : $"{dangKy.GhiChu}{Environment.NewLine}{ghiChuTuChoi}";

        int soDongCapNhat = await _context.DangKyThamDus
            .Where(d =>
                d.MaDangKy == maDangKy &&
                d.TrangThai == TrangThaiDangKy.ChoDuyet)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(d => d.GhiChu, ghiChuMoi)
                .SetProperty(d => d.MaThamDu, (string?)null)
                .SetProperty(d => d.TrangThai, TrangThaiDangKy.TuChoi));

        if (soDongCapNhat == 0)
            return KetQuaThatBai("Đăng ký đã được xử lý; không thể từ chối.");

        return KetQuaThanhCong("Đã từ chối đăng ký.");
    }

    public async Task<KetQuaThaoTacViewModel> DiemDanhPhienAsync(
        string maPhienSuKien,
        string maNguoiThamDu,
        KetQuaDiemDanhPhien ketQua,
        string nguoiThucHien)
    {
        if (string.IsNullOrWhiteSpace(maPhienSuKien) ||
            string.IsNullOrWhiteSpace(maNguoiThamDu) ||
            string.IsNullOrWhiteSpace(nguoiThucHien) ||
            nguoiThucHien.Trim().Length > 100 ||
            !Enum.IsDefined(ketQua))
        {
            return KetQuaThatBai("Thông tin điểm danh không hợp lệ.");
        }

        maPhienSuKien = maPhienSuKien.Trim();
        maNguoiThamDu = maNguoiThamDu.Trim();
        nguoiThucHien = nguoiThucHien.Trim();

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
        if (ketQua == KetQuaDiemDanhPhien.CoMat &&
            (thoiGianHienTai < phien.ThoiGianBatDau ||
             thoiGianHienTai > phien.ThoiGianKetThuc))
        {
            return KetQuaThatBai("Chỉ được điểm danh trong thời gian diễn ra phiên.");
        }

        if (ketQua == KetQuaDiemDanhPhien.VangMat &&
            thoiGianHienTai <= phien.ThoiGianKetThuc)
        {
            return KetQuaThatBai("Chỉ được ghi nhận vắng mặt sau khi phiên kết thúc.");
        }

        bool coDangKySuKienDaDuyet = await _context.DangKyThamDus
            .AnyAsync(d =>
                d.MaNguoiThamDu == maNguoiThamDu &&
                d.MaSuKien == phien.MaSuKien &&
                TrangThaiDangKySuKienDaDuyet.Contains(d.TrangThai));

        if (!coDangKySuKienDaDuyet)
            return KetQuaThatBai("Người tham dự chưa có đăng ký sự kiện được duyệt.");

        bool nguoiThamDuConHoatDong = await _context.NguoiThamDus
            .AnyAsync(n => n.MaNguoiThamDu == maNguoiThamDu && n.TrangThai);
        if (!nguoiThamDuConHoatDong)
            return KetQuaThatBai("Người tham dự không tồn tại hoặc đã ngừng hoạt động.");

        var cacDangKyPhien = await _context.DangKyPhiens
            .Where(d =>
                d.MaNguoiThamDu == maNguoiThamDu &&
                d.MaPhien == phien.MaPhienSuKien)
            .ToListAsync();

        if (cacDangKyPhien.Any(d =>
            d.TrangThai == TrangThaiDangKy.DaCheckIn ||
            d.KetQuaDiemDanh.HasValue))
            return KetQuaThatBai("Người tham dự đã được điểm danh phiên này.");

        var dangKyPhienHopLe = cacDangKyPhien
            .Where(d => TrangThaiDangKyPhienDuocDiemDanh.Contains(d.TrangThai))
            .ToList();

        if (dangKyPhienHopLe.Count == 0)
            return KetQuaThatBai("Người tham dự chưa đăng ký hợp lệ phiên này.");

        if (dangKyPhienHopLe.Count > 1)
            return KetQuaThatBai("Có nhiều đăng ký phiên trùng; cần kiểm tra dữ liệu trước khi điểm danh.");

        string maDangKyPhien = dangKyPhienHopLe[0].MaDangKyPhien;
        int soDongCapNhat = await _context.DangKyPhiens
            .Where(d =>
                d.MaDangKyPhien == maDangKyPhien &&
                d.TrangThai == TrangThaiDangKy.DaDuyet &&
                d.KetQuaDiemDanh == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(d => d.TrangThai, ketQua == KetQuaDiemDanhPhien.CoMat
                    ? TrangThaiDangKy.DaCheckIn
                    : TrangThaiDangKy.DaDuyet)
                .SetProperty(d => d.KetQuaDiemDanh, (KetQuaDiemDanhPhien?)ketQua)
                .SetProperty(d => d.ThoiGianDiemDanh, (DateTime?)thoiGianHienTai)
                .SetProperty(d => d.NguoiDiemDanh, nguoiThucHien));

        if (soDongCapNhat == 0)
            return KetQuaThatBai("Người tham dự đã được điểm danh phiên này.");

        return KetQuaThanhCong(ketQua == KetQuaDiemDanhPhien.CoMat
            ? "Đã ghi nhận có mặt."
            : "Đã ghi nhận vắng mặt.");
    }

    public async Task<KetQuaThaoTacViewModel> CheckInAsync(
        string maThamDu,
        string nguoiThucHien,
        string? ghiChu)
    {
        if (string.IsNullOrWhiteSpace(maThamDu) || maThamDu.Trim().Length > 100)
            return KetQuaThatBai("Mã tham dự không hợp lệ.");

        if (string.IsNullOrWhiteSpace(nguoiThucHien) || nguoiThucHien.Trim().Length > 100)
            return KetQuaThatBai("Tài khoản thực hiện check-in không hợp lệ.");

        if (ghiChu is { Length: > 500 })
            return KetQuaThatBai("Ghi chú check-in không được vượt quá 500 ký tự.");

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
            NguoiThucHien = nguoiThucHien.Trim(),
            GhiChu = ghiChu?.Trim() ?? string.Empty
        };

        dangKy.TrangThai = TrangThaiDangKy.DaCheckIn;
        _context.CheckIns.Add(checkIn);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return KetQuaThanhCong("Check-in thành công.");
    }

    private async Task<string?> TaoMaThamDuChuaSuDungAsync()
    {
        for (int lanThu = 0; lanThu < SoLanThuTaoMaThamDuToiDa; lanThu++)
        {
            string maThamDu = $"UN-{Guid.NewGuid():N}".ToUpperInvariant();
            if (!await _context.DangKyThamDus.AnyAsync(d => d.MaThamDu == maThamDu))
                return maThamDu;
        }

        return null;
    }

    private static KetQuaThaoTacViewModel KetQuaThanhCong(string thongBao) =>
        new() { ThanhCong = true, ThongBao = thongBao };

    private static KetQuaThaoTacViewModel KetQuaThatBai(string thongBao) =>
        new() { ThanhCong = false, ThongBao = thongBao };
}