using Microsoft.EntityFrameworkCore;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;

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

        private const int SoNgayToiThieuTruocKhiHuy = 1;

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

            if (nguoiThamDu == null)
                return (false, "Người tham dự không tồn tại.");

            if (!nguoiThamDu.TrangThai)
                return (false, "Tài khoản đã bị khóa/ngừng hoạt động, không thể đăng ký.");

            var suKien = await _context.SuKiens
                .Include(s => s.DiaDiem)
                .FirstOrDefaultAsync(s => s.MaSuKien == maSuKien);

            if (suKien == null)
                return (false, "Sự kiện không tồn tại.");

            if (suKien.TrangThai != nameof(TrangThaiSuKien.MoDangKy))
                return (false, "Sự kiện hiện không ở trạng thái mở đăng ký.");

            if (DateTime.Now >= suKien.ThoiGianBatDau)
                return (false, "Sự kiện đã bắt đầu, không thể đăng ký.");

            bool daDangKyHieuLuc = await _context.DangKyThamDus
                .AnyAsync(d =>
                    d.MaNguoiThamDu == maNguoiThamDu &&
                    d.MaSuKien == maSuKien &&
                    TrangThaiHieuLuc.Contains(d.TrangThai));

            if (daDangKyHieuLuc)
                return (false, "Bạn đã có đăng ký đang hiệu lực cho sự kiện này.");

            int soLuongHieuLuc =
                await DemSoDangKyHieuLucAsync(maSuKien);

            if (suKien.DiaDiem == null)
                return (false, "Địa điểm của sự kiện không tồn tại.");

            if (soLuongHieuLuc >= suKien.DiaDiem.SucChuaToiDa)
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

            if (dangKy.TrangThai == TrangThaiDangKy.DaCheckIn ||
                dangKy.TrangThai == TrangThaiDangKy.HoanThanh)
                return (false, "Đăng ký đã check-in/hoàn thành, không thể hủy.");

            if (dangKy.TrangThai == TrangThaiDangKy.DaHuy)
                return (false, "Đăng ký này đã được hủy trước đó.");

            var hanHuyCuoiCung =
                dangKy.SuKien.ThoiGianBatDau
                    .AddDays(-SoNgayToiThieuTruocKhiHuy);

            if (DateTime.Now > hanHuyCuoiCung)
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
                .FirstOrDefaultAsync(p => p.MaPhienSuKien == maPhien);

            if (phien == null)
                return (false, "Phiên sự kiện không tồn tại.");

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
