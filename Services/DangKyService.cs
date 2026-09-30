using Microsoft.EntityFrameworkCore;
using QuanLyHoiNghi.Models;

namespace QuanLyHoiNghi.Services
{
    public class DangKyService
    {
        private readonly ApplicationDbContext _context;

        // QUY ƯỚC CHUNG CỦA NHÓM (phải giải thích được khi bảo vệ):
        // - "Đang hiệu lực" = các trạng thái vẫn chiếm 1 suất đăng ký:
        //   ChoDuyet, DaDuyet, DaCheckIn, HoanThanh
        //   (TuChoi và DaHuy KHÔNG tính vào sức chứa)
        // - Thời điểm GIỮ CHỖ: ngay khi tạo đăng ký ở trạng thái Chờ duyệt
        private static readonly TrangThaiDangKy[] TrangThaiHieuLuc = new[]
        {
            TrangThaiDangKy.ChoDuyet,
            TrangThaiDangKy.DaDuyet,
            TrangThaiDangKy.DaCheckIn,
            TrangThaiDangKy.HoanThanh
        };

        private const int SoNgayToiThieuTruocKhiHuy = 1;

        public DangKyService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================== 7.3 + 7.4: KIỂM TRA ĐIỀU KIỆN ĐĂNG KÝ ==================
        public async Task<(bool HopLe, string LyDo)> KiemTraDieuKienDangKyAsync(int maNguoiThamDu, int maSuKien)
        {
            var nguoiThamDu = await _context.NguoiThamDu
                .FirstOrDefaultAsync(n => n.MaNguoiThamDu == maNguoiThamDu);

            if (nguoiThamDu == null)
                return (false, "Người tham dự không tồn tại.");

            if (!nguoiThamDu.TrangThai)
                return (false, "Tài khoản đã bị khóa/ngừng hoạt động, không thể đăng ký.");

            var suKien = await _context.SuKien
                .FirstOrDefaultAsync(s => s.MaSuKien == maSuKien);

            if (suKien == null)
                return (false, "Sự kiện không tồn tại.");

            if (suKien.TrangThai != TrangThaiSuKien.MoDangKy)
                return (false, "Sự kiện hiện không ở trạng thái mở đăng ký.");

            if (DateTime.Now > suKien.HanDangKy)
                return (false, $"Đã quá hạn đăng ký (hạn: {suKien.HanDangKy:dd/MM/yyyy HH:mm}).");

            if (DateTime.Now >= suKien.ThoiGianBatDau)
                return (false, "Sự kiện đã bắt đầu, không thể đăng ký.");

            bool daDangKyHieuLuc = await _context.DangKyThamDu
                .AnyAsync(d => d.MaNguoiThamDu == maNguoiThamDu
                            && d.MaSuKien == maSuKien
                            && TrangThaiHieuLuc.Contains(d.TrangThai));

            if (daDangKyHieuLuc)
                return (false, "Bạn đã có đăng ký đang hiệu lực cho sự kiện này.");

            int soLuongHieuLuc = await DemSoDangKyHieuLucAsync(maSuKien);

            if (soLuongHieuLuc >= suKien.SoLuongToiDa)
                return (false, "Sự kiện đã đủ số lượng đăng ký, không thể đăng ký thêm.");

            return (true, null);
        }

        public async Task<int> DemSoDangKyHieuLucAsync(int maSuKien)
        {
            return await _context.DangKyThamDu
                .Where(d => d.MaSuKien == maSuKien && TrangThaiHieuLuc.Contains(d.TrangThai))
                .CountAsync();
        }

        public async Task<(bool HopLe, string LyDo)> KiemTraSoLuongToiDaTheoDiaDiemAsync(int maDiaDiem, int soLuongToiDa)
        {
            var diaDiem = await _context.DiaDiem.FirstOrDefaultAsync(d => d.MaDiaDiem == maDiaDiem);
            if (diaDiem == null)
                return (false, "Địa điểm không tồn tại.");

            if (soLuongToiDa > diaDiem.SucChuaToiDa)
                return (false, $"Số lượng tối đa ({soLuongToiDa}) vượt quá sức chứa địa điểm ({diaDiem.SucChuaToiDa}).");

            return (true, null);
        }

        // ================== 7.6: ĐĂNG KÝ PHIÊN - KIỂM TRA TRÙNG LỊCH ==================
        // Công thức: BatDauMoi < KetThucCu && KetThucMoi > BatDauCu
        // Bao phủ: trùng hoàn toàn, trùng đầu, trùng cuối, nằm bên trong, bao phủ,
        // và loại trừ trường hợp tiếp giáp (chạm nhau đúng 1 điểm -> vẫn hợp lệ)
        public async Task<(bool HopLe, string LyDo)> KiemTraTrungLichPhienAsync(
            int maNguoiThamDu, int maPhienMoi, DateTime batDauMoi, DateTime ketThucMoi)
        {
            var cacPhienDaDangKy = await _context.DangKyPhien
                .Where(dp => dp.MaNguoiThamDu == maNguoiThamDu
                          && dp.MaPhien != maPhienMoi
                          && dp.TrangThai != TrangThaiDangKy.DaHuy
                          && dp.TrangThai != TrangThaiDangKy.TuChoi)
                .Include(dp => dp.Phien)
                .Select(dp => new { dp.Phien.BatDau, dp.Phien.KetThuc, dp.Phien.TenPhien })
                .ToListAsync();

            foreach (var phienCu in cacPhienDaDangKy)
            {
                bool giaoNhau = batDauMoi < phienCu.KetThuc && ketThucMoi > phienCu.BatDau;
                if (giaoNhau)
                {
                    return (false, $"Phiên bạn chọn trùng thời gian với phiên \"{phienCu.TenPhien}\" " +
                                   $"({phienCu.BatDau:HH:mm dd/MM} - {phienCu.KetThuc:HH:mm dd/MM}) mà bạn đã đăng ký.");
                }
            }

            return (true, null);
        }

        // ================== 7.7: HỦY ĐĂNG KÝ ==================
        public async Task<(bool ThanhCong, string LyDo)> HuyDangKyAsync(int maDangKy, int maNguoiThamDu, string lyDoHuy)
        {
            var dangKy = await _context.DangKyThamDu
                .Include(d => d.SuKien)
                .FirstOrDefaultAsync(d => d.MaDangKy == maDangKy);

            if (dangKy == null)
                return (false, "Đăng ký không tồn tại.");

            if (dangKy.MaNguoiThamDu != maNguoiThamDu)
                return (false, "Bạn không có quyền hủy đăng ký này.");

            if (dangKy.TrangThai == TrangThaiDangKy.DaCheckIn || dangKy.TrangThai == TrangThaiDangKy.HoanThanh)
                return (false, "Đăng ký đã check-in/hoàn thành, không thể hủy.");

            if (dangKy.TrangThai == TrangThaiDangKy.DaHuy)
                return (false, "Đăng ký này đã được hủy trước đó.");

            var hanHuyCuoiCung = dangKy.SuKien.ThoiGianBatDau.AddDays(-SoNgayToiThieuTruocKhiHuy);
            if (DateTime.Now > hanHuyCuoiCung)
                return (false, $"Đã quá hạn hủy đăng ký (phải hủy trước {hanHuyCuoiCung:dd/MM/yyyy HH:mm}).");

            dangKy.TrangThai = TrangThaiDangKy.DaHuy;
            dangKy.LyDoTuChoiHuy = lyDoHuy;

            await _context.SaveChangesAsync();
            return (true, null);
        }

        // ================== ĐĂNG KÝ PHIÊN - LƯU ĐĂNG KÝ SAU KHI KIỂM TRA ==================
        public async Task<(bool ThanhCong, string LyDo)> DangKyPhienAsync(int maNguoiThamDu, int maPhien)
        {
            var phien = await _context.PhienSuKien.FirstOrDefaultAsync(p => p.MaPhien == maPhien);
            if (phien == null)
                return (false, "Phiên sự kiện không tồn tại.");

            // Không cho đăng ký trùng phiên đã đăng ký
            bool daDangKy = await _context.DangKyPhien
                .AnyAsync(dp => dp.MaNguoiThamDu == maNguoiThamDu
                             && dp.MaPhien == maPhien
                             && dp.TrangThai != TrangThaiDangKy.DaHuy
                             && dp.TrangThai != TrangThaiDangKy.TuChoi);

            if (daDangKy)
                return (false, "Bạn đã đăng ký phiên này rồi.");

            // Kiểm tra trùng lịch với các phiên khác đã đăng ký (mục 7.6)
            var (hopLe, lyDo) = await KiemTraTrungLichPhienAsync(maNguoiThamDu, maPhien, phien.BatDau, phien.KetThuc);
            if (!hopLe)
                return (false, lyDo);

            var dangKyPhien = new DangKyPhien
            {
                MaNguoiThamDu = maNguoiThamDu,
                MaPhien = maPhien,
                NgayDangKy = DateTime.Now,
                TrangThai = TrangThaiDangKy.ChoDuyet
            };

            _context.DangKyPhien.Add(dangKyPhien);
            await _context.SaveChangesAsync();

            return (true, null);
        }
    }
}