using Microsoft.EntityFrameworkCore;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using System.Data;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Services
{
    public class DangKyService
    {
        private readonly AppDbContext _context;

        private static readonly TrangThaiDangKy[] TrangThaiHieuLuc =
        {
            TrangThaiDangKy.ChoDuyet,
            TrangThaiDangKy.DaDuyet,
            TrangThaiDangKy.DaCheckIn,
            TrangThaiDangKy.HoanThanh
        };

        public const int SoNgayToiThieuTruocKhiHuy = 1;

        public DangKyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool HopLe, string? LyDo)> KiemTraDieuKienDangKyAsync(
            string maNguoiThamDu,
            string maSuKien)
        {
            var nguoiThamDu = await _context.NguoiThamDus
                .FirstOrDefaultAsync(n => n.MaNguoiThamDu == maNguoiThamDu);
            var suKien = await _context.SuKiens
                .Include(s => s.DiaDiem)
                .FirstOrDefaultAsync(s => s.MaSuKien == maSuKien);

            return await KiemTraDieuKienDangKyAsync(nguoiThamDu, suKien, maSuKien);
        }

        public async Task<(bool ThanhCong, string? LyDo)> DangKySuKienAsync(
            string maNguoiThamDu,
            string maSuKien,
            string? ghiChu = null)
        {
            if (string.IsNullOrWhiteSpace(maNguoiThamDu) || string.IsNullOrWhiteSpace(maSuKien))
                return (false, "Thông tin người tham dự hoặc sự kiện không hợp lệ.");

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            var suKien = await _context.SuKiens
                .FromSqlInterpolated(
                    $"SELECT * FROM [SuKiens] WITH (UPDLOCK, HOLDLOCK) WHERE [MaSuKien] = {maSuKien}")
                .FirstOrDefaultAsync();
            if (suKien != null)
            {
                await _context.Entry(suKien).Reference(s => s.DiaDiem).LoadAsync();
            }

            var nguoiThamDu = await _context.NguoiThamDus
                .FirstOrDefaultAsync(n => n.MaNguoiThamDu == maNguoiThamDu);

            var (hopLe, lyDo) = await KiemTraDieuKienDangKyAsync(nguoiThamDu, suKien, maSuKien);
            if (!hopLe)
                return (false, lyDo);

            _context.DangKyThamDus.Add(new DangKyThamDu
            {
                MaDangKy = Guid.NewGuid().ToString("N"),
                MaNguoiThamDu = maNguoiThamDu,
                MaSuKien = maSuKien,
                NgayDangKy = DateTime.Now,
                TrangThai = TrangThaiDangKy.ChoDuyet,
                GhiChu = ghiChu?.Trim() ?? string.Empty
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, null);
        }

        private async Task<(bool HopLe, string? LyDo)> KiemTraDieuKienDangKyAsync(
            NguoiThamDu? nguoiThamDu,
            SuKien? suKien,
            string maSuKien)
        {
            if (nguoiThamDu == null)
                return (false, "Người tham dự không tồn tại.");

            if (!nguoiThamDu.TrangThai)
                return (false, "Tài khoản đã bị khóa/ngừng hoạt động, không thể đăng ký.");

            if (suKien == null)
                return (false, "Sự kiện không tồn tại.");

            if (suKien.TrangThai != nameof(TrangThaiSuKien.MoDangKy))
                return (false, "Sự kiện hiện không ở trạng thái mở đăng ký.");

            if (suKien.ThoiGianKetThuc <= suKien.ThoiGianBatDau)
                return (false, "Thời gian sự kiện không hợp lệ.");

            if (DateTime.Now >= suKien.ThoiGianBatDau)
                return (false, "Sự kiện đã bắt đầu, không thể đăng ký.");

            if (DateTime.Now >= suKien.HanDangKy)
                return (false, "Đã hết hạn đăng ký sự kiện.");

            bool daDangKyHieuLuc = await _context.DangKyThamDus
                .AnyAsync(d =>
                    d.MaNguoiThamDu == nguoiThamDu.MaNguoiThamDu &&
                    d.MaSuKien == maSuKien &&
                    TrangThaiHieuLuc.Contains(d.TrangThai));

            if (daDangKyHieuLuc)
                return (false, "Bạn đã có đăng ký đang hiệu lực cho sự kiện này.");

            int soLuongHieuLuc =
                await DemSoDangKyHieuLucAsync(maSuKien);

            if (suKien.DiaDiem == null)
                return (false, "Địa điểm của sự kiện không tồn tại.");

            var sucChuaDangKy = Math.Min(suKien.SoLuongToiDa, suKien.DiaDiem.SucChuaToiDa);
            if (soLuongHieuLuc >= sucChuaDangKy)
                return (false, "Sự kiện đã đủ số lượng đăng ký, không thể đăng ký thêm.");

            return (true, null);
        }

        public async Task<int> DemSoDangKyHieuLucAsync(string maSuKien)
        {
            return await _context.DangKyThamDus
                .Where(d =>
                    d.MaSuKien == maSuKien &&
                    TrangThaiHieuLuc.Contains(d.TrangThai))
                .CountAsync();
        }

        public async Task<(bool HopLe, string? LyDo)>
            KiemTraSoLuongToiDaTheoDiaDiemAsync(
            string maDiaDiem,
                int soLuongToiDa)
        {
            var diaDiem = await _context.DiaDiems
                .FirstOrDefaultAsync(d => d.MaDiaDiem == maDiaDiem);

            if (diaDiem == null)
                return (false, "Địa điểm không tồn tại.");

            if (soLuongToiDa > diaDiem.SucChuaToiDa)
                return (false,
                    $"Số lượng tối đa ({soLuongToiDa}) vượt quá sức chứa địa điểm ({diaDiem.SucChuaToiDa}).");

            return (true, null);
        }

        public async Task<(bool HopLe, string? LyDo)>
            KiemTraTrungLichPhienAsync(
            string maNguoiThamDu,
            string maPhienMoi,
                DateTime batDauMoi,
                DateTime ketThucMoi)
        {
            if (ketThucMoi <= batDauMoi)
                return (false, "Thời gian phiên không hợp lệ.");

            var cacPhienDaDangKy = await _context.DangKyPhiens
                .Where(dp =>
                    dp.MaNguoiThamDu == maNguoiThamDu &&
                    dp.MaPhien != maPhienMoi &&
                    dp.TrangThai != TrangThaiDangKy.DaHuy &&
                    dp.TrangThai != TrangThaiDangKy.TuChoi &&
                    dp.Phien != null)
                .Include(dp => dp.Phien)
                .Select(dp => new
                {
                    BatDau = dp.Phien!.ThoiGianBatDau,
                    KetThuc = dp.Phien.ThoiGianKetThuc,
                    dp.Phien.TenPhien
                })
                .ToListAsync();

            foreach (var phienCu in cacPhienDaDangKy)
            {
                bool giaoNhau =
                    batDauMoi < phienCu.KetThuc &&
                    ketThucMoi > phienCu.BatDau;

                if (giaoNhau)
                {
                    return (
                        false,
                        $"Phiên bạn chọn trùng thời gian với phiên \"{phienCu.TenPhien}\" " +
                        $"({phienCu.BatDau:HH:mm dd/MM} - {phienCu.KetThuc:HH:mm dd/MM}) " +
                        $"mà bạn đã đăng ký."
                    );
                }
            }

            return (true, null);
        }

        public async Task<(bool ThanhCong, string? LyDo)>
            HuyDangKyAsync(
            string maDangKy,
            string maNguoiThamDu,
                string lyDoHuy)
        {
            var dangKy = await _context.DangKyThamDus
                .Include(d => d.SuKien)
                .FirstOrDefaultAsync(d => d.MaDangKy == maDangKy);

            if (dangKy == null)
                return (false, "Đăng ký không tồn tại.");

            if (dangKy.SuKien == null)
                return (false, "Sự kiện của đăng ký không tồn tại.");

            if (dangKy.MaNguoiThamDu != maNguoiThamDu)
                return (false, "Bạn không có quyền hủy đăng ký này.");

            if (dangKy.TrangThai is not TrangThaiDangKy.ChoDuyet
                and not TrangThaiDangKy.DaDuyet)
                return (false, "Chỉ đăng ký đang chờ duyệt hoặc đã duyệt mới được hủy.");

            var hanHuyCuoiCung =
                dangKy.SuKien.ThoiGianBatDau
                    .AddDays(-SoNgayToiThieuTruocKhiHuy);

            if (DateTime.Now >= hanHuyCuoiCung)
                return (
                    false,
                    $"Đã quá hạn hủy đăng ký " +
                    $"(phải hủy trước {hanHuyCuoiCung:dd/MM/yyyy HH:mm})."
                );

            dangKy.TrangThai = TrangThaiDangKy.DaHuy;
            dangKy.LyDoTuChoiHuy = lyDoHuy;

            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool ThanhCong, string? LyDo)>
            DangKyPhienAsync(
                string maNguoiThamDu,
                string maPhien)
        {
            var phien = await _context.PhienSuKiens
                .Include(p => p.SuKien)
                .FirstOrDefaultAsync(p => p.MaPhienSuKien == maPhien);

            if (phien == null)
                return (false, "Phiên sự kiện không tồn tại.");

            if (phien.ThoiGianKetThuc <= phien.ThoiGianBatDau)
                return (false, "Thời gian phiên không hợp lệ.");

            if (phien.TrangThai is "Ngung" or "DaHuy" or "DaKetThuc")
                return (false, "Phiên đã ngừng, bị hủy hoặc kết thúc nên không nhận đăng ký mới.");

            if (phien.SuKien == null
                || phien.SuKien.TrangThai is nameof(TrangThaiSuKien.DaHuy) or nameof(TrangThaiSuKien.HoanThanh))
            {
                return (false, "Sự kiện không còn nhận đăng ký phiên.");
            }

            if (phien.SuKien.TrangThai != nameof(TrangThaiSuKien.MoDangKy))
                return (false, "Sự kiện hiện không ở trạng thái mở đăng ký.");

            if (DateTime.Now >= phien.SuKien.HanDangKy)
                return (false, "Đã hết hạn đăng ký sự kiện.");

            if (DateTime.Now >= phien.SuKien.ThoiGianBatDau)
                return (false, "Sự kiện đã bắt đầu, không thể đăng ký phiên.");

            if (DateTime.Now >= phien.ThoiGianBatDau)
                return (false, "Phiên đã bắt đầu, không thể đăng ký mới.");

            var nguoiThamDu = await _context.NguoiThamDus
                .FirstOrDefaultAsync(n => n.MaNguoiThamDu == maNguoiThamDu);
            if (nguoiThamDu == null || !nguoiThamDu.TrangThai)
                return (false, "Người tham dự không tồn tại hoặc đã ngừng hoạt động.");

            bool coDangKySuKienHieuLuc = await _context.DangKyThamDus
                .AnyAsync(d =>
                    d.MaNguoiThamDu == maNguoiThamDu &&
                    d.MaSuKien == phien.MaSuKien &&
                    TrangThaiHieuLuc.Contains(d.TrangThai));

            if (!coDangKySuKienHieuLuc)
                return (false, "Bạn cần có đăng ký hợp lệ cho sự kiện trước khi đăng ký phiên.");

            bool daDangKy = await _context.DangKyPhiens
                .AnyAsync(dp =>
                    dp.MaNguoiThamDu == maNguoiThamDu &&
                    dp.MaPhien == maPhien &&
                    dp.TrangThai != TrangThaiDangKy.DaHuy &&
                    dp.TrangThai != TrangThaiDangKy.TuChoi);

            if (daDangKy)
                return (false, "Bạn đã đăng ký phiên này rồi.");

            var soDangKyPhienHieuLuc = await _context.DangKyPhiens
                .CountAsync(dp =>
                    dp.MaPhien == maPhien
                    && dp.TrangThai != TrangThaiDangKy.DaHuy
                    && dp.TrangThai != TrangThaiDangKy.TuChoi);
            if (soDangKyPhienHieuLuc >= phien.SoLuongToiDa)
                return (false, "Phiên đã đủ số lượng đăng ký.");

            var (hopLe, lyDo) =
                await KiemTraTrungLichPhienAsync(
                    maNguoiThamDu,
                    maPhien,
                    phien.ThoiGianBatDau,
                    phien.ThoiGianKetThuc);

            if (!hopLe)
                return (false, lyDo);

            var dangKyPhien = new DangKyPhien
            {
                MaDangKyPhien = Guid.NewGuid().ToString("N"),
                MaNguoiThamDu = maNguoiThamDu,
                MaPhien = maPhien,
                NgayDangKy = DateTime.Now,
                TrangThai = TrangThaiDangKy.ChoDuyet
            };

            _context.DangKyPhiens.Add(dangKyPhien);

            await _context.SaveChangesAsync();

            return (true, null);
        }
    }
}
