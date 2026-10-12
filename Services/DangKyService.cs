using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Services
{
    public class DangKyService
    {
        private static readonly TrangThaiDangKy[] TrangThaiHieuLuc =
        {
            TrangThaiDangKy.ChoDuyet,
            TrangThaiDangKy.DaDuyet,
            TrangThaiDangKy.DaCheckIn,
            TrangThaiDangKy.HoanThanh
        };

        private const int SoNgayToiThieuTruocKhiHuy = 1;
        private readonly AppDbContext _context;

        public DangKyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool HopLe, string? LyDo)> KiemTraDieuKienDangKyAsync(
            string maNguoiThamDu,
            string maSuKien)
        {
            var nguoiThamDu = await _context.NguoiThamDus
                .FirstOrDefaultAsync(nguoi => nguoi.MaNguoiThamDu == maNguoiThamDu);

            if (nguoiThamDu == null)
            {
                return (false, "Người tham dự không tồn tại.");
            }

            if (!nguoiThamDu.TrangThai)
            {
                return (false, "Tài khoản đã bị khóa/ngừng hoạt động, không thể đăng ký.");
            }

            var suKien = await _context.SuKiens
                .FirstOrDefaultAsync(suKien => suKien.MaSuKien == maSuKien);

            if (suKien == null)
            {
                return (false, "Sự kiện không tồn tại.");
            }

            if (suKien.TrangThai != nameof(TrangThaiSuKien.MoDangKy))
            {
                return (false, "Sự kiện hiện không ở trạng thái mở đăng ký.");
            }

            if (DateTime.Now >= suKien.ThoiGianBatDau)
            {
                return (false, "Sự kiện đã bắt đầu, không thể đăng ký.");
            }

            var daDangKyHieuLuc = await _context.DangKyThamDus
                .AnyAsync(dangKy =>
                    dangKy.MaNguoiThamDu == maNguoiThamDu &&
                    dangKy.MaSuKien == maSuKien &&
                    TrangThaiHieuLuc.Contains(dangKy.TrangThai));

            if (daDangKyHieuLuc)
            {
                return (false, "Bạn đã có đăng ký đang hiệu lực cho sự kiện này.");
            }

            var diaDiem = await _context.DiaDiems
                .FirstOrDefaultAsync(diaDiem => diaDiem.MaDiaDiem == suKien.MaDiaDiem);

            if (diaDiem == null)
            {
                return (false, "Địa điểm của sự kiện không tồn tại.");
            }

            var soNguoiDangKy = await DemSoDangKyHieuLucAsync(maSuKien);
            if (soNguoiDangKy >= diaDiem.SucChuaToiDa)
            {
                return (
                    false,
                    $"Sự kiện đã đủ số lượng người tham dự. Sức chứa tối đa: {diaDiem.SucChuaToiDa} người.");
            }

            return (true, null);
        }

        public Task<int> DemSoDangKyHieuLucAsync(string maSuKien)
        {
            return _context.DangKyThamDus
                .Where(dangKy =>
                    dangKy.MaSuKien == maSuKien &&
                    TrangThaiHieuLuc.Contains(dangKy.TrangThai))
                .CountAsync();
        }

        public async Task<(bool HopLe, string? LyDo)> KiemTraSoLuongToiDaTheoDiaDiemAsync(
            string maDiaDiem,
            int soLuongToiDa)
        {
            var diaDiem = await _context.DiaDiems
                .FirstOrDefaultAsync(item => item.MaDiaDiem == maDiaDiem);

            if (diaDiem == null)
            {
                return (false, "Địa điểm không tồn tại.");
            }

            if (soLuongToiDa > diaDiem.SucChuaToiDa)
            {
                return (
                    false,
                    $"Số lượng tối đa ({soLuongToiDa}) vượt quá sức chứa địa điểm ({diaDiem.SucChuaToiDa}).");
            }

            return (true, null);
        }

        public async Task<(bool HopLe, string? LyDo)> KiemTraTrungLichPhienAsync(
            string maNguoiThamDu,
            string maPhienMoi,
            DateTime batDauMoi,
            DateTime ketThucMoi)
        {
            var cacPhienDaDangKy = await _context.DangKyPhiens
                .Where(dangKy =>
                    dangKy.MaNguoiThamDu == maNguoiThamDu &&
                    dangKy.MaPhien != maPhienMoi &&
                    dangKy.TrangThai != TrangThaiDangKy.DaHuy &&
                    dangKy.TrangThai != TrangThaiDangKy.TuChoi &&
                    dangKy.Phien != null)
                .Select(dangKy => new
                {
                    dangKy.Phien!.ThoiGianBatDau,
                    dangKy.Phien.ThoiGianKetThuc,
                    dangKy.Phien.TenPhien
                })
                .ToListAsync();

            foreach (var phienCu in cacPhienDaDangKy)
            {
                var giaoNhau =
                    batDauMoi < phienCu.ThoiGianKetThuc &&
                    ketThucMoi > phienCu.ThoiGianBatDau;

                if (giaoNhau)
                {
                    return (
                        false,
                        $"Phiên bạn chọn trùng thời gian với phiên \"{phienCu.TenPhien}\" " +
                        $"({phienCu.ThoiGianBatDau:HH:mm dd/MM} - {phienCu.ThoiGianKetThuc:HH:mm dd/MM}) " +
                        "mà bạn đã đăng ký.");
                }
            }

            return (true, null);
        }

        public async Task<(bool ThanhCong, string? LyDo)> HuyDangKyAsync(
            string maDangKy,
            string maNguoiThamDu,
            string lyDoHuy)
        {
            var dangKy = await _context.DangKyThamDus
                .Include(item => item.SuKien)
                .FirstOrDefaultAsync(item => item.MaDangKy == maDangKy);

            if (dangKy == null)
            {
                return (false, "Đăng ký không tồn tại.");
            }

            if (dangKy.SuKien == null)
            {
                return (false, "Sự kiện của đăng ký không tồn tại.");
            }

            if (dangKy.MaNguoiThamDu != maNguoiThamDu)
            {
                return (false, "Bạn không có quyền hủy đăng ký này.");
            }

            if (dangKy.TrangThai == TrangThaiDangKy.DaCheckIn ||
                dangKy.TrangThai == TrangThaiDangKy.HoanThanh)
            {
                return (false, "Đăng ký đã check-in/hoàn thành, không thể hủy.");
            }

            if (dangKy.TrangThai == TrangThaiDangKy.DaHuy)
            {
                return (false, "Đăng ký này đã được hủy trước đó.");
            }

            if (string.IsNullOrWhiteSpace(lyDoHuy))
            {
                return (false, "Vui lòng nhập lý do hủy đăng ký.");
            }

            var hanHuyCuoiCung = dangKy.SuKien.ThoiGianBatDau
                .AddDays(-SoNgayToiThieuTruocKhiHuy);

            if (DateTime.Now > hanHuyCuoiCung)
            {
                return (
                    false,
                    $"Đã quá hạn hủy đăng ký (phải hủy trước {hanHuyCuoiCung:dd/MM/yyyy HH:mm}).");
            }

            dangKy.TrangThai = TrangThaiDangKy.DaHuy;
            dangKy.LyDoTuChoiHuy = lyDoHuy;
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool ThanhCong, string? LyDo)> DangKyPhienAsync(
            string maNguoiThamDu,
            string maPhien)
        {
            var nguoiThamDu = await _context.NguoiThamDus
                .FirstOrDefaultAsync(nguoi => nguoi.MaNguoiThamDu == maNguoiThamDu);

            if (nguoiThamDu == null)
            {
                return (false, "Người tham dự không tồn tại.");
            }

            if (!nguoiThamDu.TrangThai)
            {
                return (false, "Tài khoản đã bị khóa/ngừng hoạt động, không thể đăng ký phiên.");
            }

            var phien = await _context.PhienSuKiens
                .FirstOrDefaultAsync(item => item.MaPhienSuKien == maPhien);

            if (phien == null)
            {
                return (false, "Phiên sự kiện không tồn tại.");
            }

            if (DateTime.Now >= phien.ThoiGianBatDau)
            {
                return (false, "Phiên sự kiện đã bắt đầu, không thể đăng ký.");
            }

            var coDangKySuKienHieuLuc = await _context.DangKyThamDus
                .AnyAsync(dangKy =>
                    dangKy.MaNguoiThamDu == maNguoiThamDu &&
                    dangKy.MaSuKien == phien.MaSuKien &&
                    TrangThaiHieuLuc.Contains(dangKy.TrangThai));

            if (!coDangKySuKienHieuLuc)
            {
                return (false, "Bạn cần có đăng ký hợp lệ cho sự kiện trước khi đăng ký phiên.");
            }

            var daDangKy = await _context.DangKyPhiens
                .AnyAsync(dangKy =>
                    dangKy.MaNguoiThamDu == maNguoiThamDu &&
                    dangKy.MaPhien == maPhien &&
                    dangKy.TrangThai != TrangThaiDangKy.DaHuy &&
                    dangKy.TrangThai != TrangThaiDangKy.TuChoi);

            if (daDangKy)
            {
                return (false, "Bạn đã đăng ký phiên này rồi.");
            }

            var (hopLe, lyDo) = await KiemTraTrungLichPhienAsync(
                maNguoiThamDu,
                maPhien,
                phien.ThoiGianBatDau,
                phien.ThoiGianKetThuc);

            if (!hopLe)
            {
                return (false, lyDo);
            }

            _context.DangKyPhiens.Add(new DangKyPhien
            {
                MaDangKyPhien = Guid.NewGuid().ToString("N"),
                MaNguoiThamDu = maNguoiThamDu,
                MaPhien = maPhien,
                NgayDangKy = DateTime.Now,
                TrangThai = TrangThaiDangKy.ChoDuyet
            });

            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}
