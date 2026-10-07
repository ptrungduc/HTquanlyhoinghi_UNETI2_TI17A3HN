
using Microsoft.EntityFrameworkCore;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Services
{
    public class DangKyService
    {
        private readonly AppDbContext _context;

        // Các trạng thái được tính là đăng ký đang có hiệu lực
        private static readonly TrangThaiDangKy[] TrangThaiHieuLuc =
        {
            TrangThaiDangKy.ChoDuyet,
            TrangThaiDangKy.DaDuyet,
            TrangThaiDangKy.DaCheckIn,
            TrangThaiDangKy.HoanThanh
        };

        // Phải hủy trước sự kiện ít nhất 1 ngày
        private const int SoNgayToiThieuTruocKhiHuy = 1;

        public DangKyService(AppDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // 1. KIỂM TRA ĐIỀU KIỆN ĐĂNG KÝ SỰ KIỆN
        // =========================================================
        public async Task<(bool HopLe, string LyDo)> KiemTraDieuKienDangKyAsync(
            int maNguoiThamDu,
            string maSuKien)
        {
            // Kiểm tra người tham dự
            var nguoiThamDu = await _context.NguoiThamDus
                .FirstOrDefaultAsync(n =>
                    n.MaNguoiThamDu == maNguoiThamDu);

            if (nguoiThamDu == null)
            {
                return (
                    false,
                    "Người tham dự không tồn tại."
                );
            }

            // Người tham dự phải đang hoạt động
            if (!nguoiThamDu.TrangThai)
            {
                return (
                    false,
                    "Tài khoản đã bị khóa/ngừng hoạt động, không thể đăng ký."
                );
            }

            // Kiểm tra sự kiện
            var suKien = await _context.SuKiens
                .FirstOrDefaultAsync(s =>
                    s.MaSuKien == maSuKien);

            if (suKien == null)
            {
                return (
                    false,
                    "Sự kiện không tồn tại."
                );
            }

            // Kiểm tra trạng thái mở đăng ký
            if (suKien.TrangThai != "MoDangKy")
            {
                return (
                    false,
                    "Sự kiện hiện không ở trạng thái mở đăng ký."
                );
            }

            // Không cho đăng ký khi sự kiện đã bắt đầu
            if (DateTime.Now >= suKien.ThoiGianBatDau)
            {
                return (
                    false,
                    "Sự kiện đã bắt đầu, không thể đăng ký."
                );
            }

            // Kiểm tra người này đã đăng ký sự kiện chưa
            bool daDangKyHieuLuc =
                await _context.DangKyThamDus
                    .AnyAsync(d =>
                        d.MaNguoiThamDu == maNguoiThamDu &&
                        d.MaSuKien == maSuKien &&
                        TrangThaiHieuLuc.Contains(d.TrangThai));

            if (daDangKyHieuLuc)
            {
                return (
                    false,
                    "Bạn đã có đăng ký đang hiệu lực cho sự kiện này."
                );
            }

            // =====================================================
            // KIỂM TRA SỨC CHỨA ĐỊA ĐIỂM
            // =====================================================

            // Tìm địa điểm của sự kiện
            var diaDiem = await _context.DiaDiems
                .FirstOrDefaultAsync(d =>
                    d.MaDiaDiem == suKien.MaDiaDiem);

            if (diaDiem == null)
            {
                return (
                    false,
                    "Địa điểm của sự kiện không tồn tại."
                );
            }

            // Đếm số người đã đăng ký hiệu lực
            int soNguoiDangKy =
                await DemSoDangKyHieuLucAsync(maSuKien);

            // Nếu đã đủ sức chứa thì không cho đăng ký
            if (soNguoiDangKy >= diaDiem.SucChuaToiDa)
            {
                return (
                    false,
                    $"Sự kiện đã đủ số lượng người tham dự. " +
                    $"Sức chứa tối đa: {diaDiem.SucChuaToiDa} người."
                );
            }

            return (true, null);
        }

        // =========================================================
        // 2. ĐẾM SỐ LƯỢNG ĐĂNG KÝ ĐANG HIỆU LỰC
        // =========================================================
        public async Task<int> DemSoDangKyHieuLucAsync(
            string maSuKien)
        {
            return await _context.DangKyThamDus
                .Where(d =>
                    d.MaSuKien == maSuKien &&
                    TrangThaiHieuLuc.Contains(d.TrangThai))
                .CountAsync();
        }

        // =========================================================
        // 3. KIỂM TRA SỨC CHỨA ĐỊA ĐIỂM
        // =========================================================
        public async Task<(bool HopLe, string LyDo)>
            KiemTraSoLuongToiDaTheoDiaDiemAsync(
                string maDiaDiem,
                int soLuongToiDa)
        {
            var diaDiem = await _context.DiaDiems
                .FirstOrDefaultAsync(d =>
                    d.MaDiaDiem == maDiaDiem);

            if (diaDiem == null)
            {
                return (
                    false,
                    "Địa điểm không tồn tại."
                );
            }

            if (soLuongToiDa > diaDiem.SucChuaToiDa)
            {
                return (
                    false,
                    $"Số lượng tối đa ({soLuongToiDa}) " +
                    $"vượt quá sức chứa địa điểm " +
                    $"({diaDiem.SucChuaToiDa})."
                );
            }

            return (true, null);
        }

        // =========================================================
        // 4. KIỂM TRA TRÙNG LỊCH PHIÊN
        // =========================================================
        public async Task<(bool HopLe, string LyDo)>
            KiemTraTrungLichPhienAsync(
                int maNguoiThamDu,
                string maPhienMoi,
                DateTime batDauMoi,
                DateTime ketThucMoi)
        {
            var cacPhienDaDangKy =
                await _context.DangKyPhiens
                    .Where(dp =>
                        dp.MaNguoiThamDu == maNguoiThamDu &&
                        dp.MaPhien != maPhienMoi &&
                        dp.TrangThai != TrangThaiDangKy.DaHuy &&
                        dp.TrangThai != TrangThaiDangKy.TuChoi)
                    .Include(dp => dp.Phien)
                    .Select(dp => new
                    {
                        BatDau = dp.Phien.ThoiGianBatDau,
                        KetThuc = dp.Phien.ThoiGianKetThuc,
                        TenPhien = dp.Phien.TenPhien
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
                        $"Phiên bạn chọn trùng thời gian với phiên " +
                        $"\"{phienCu.TenPhien}\" " +
                        $"({phienCu.BatDau:HH:mm dd/MM} - " +
                        $"{phienCu.KetThuc:HH:mm dd/MM}) " +
                        $"mà bạn đã đăng ký."
                    );
                }
            }

            return (true, null);
        }

        // =========================================================
        // 5. HỦY ĐĂNG KÝ SỰ KIỆN
        // =========================================================
        public async Task<(bool ThanhCong, string LyDo)>
            HuyDangKyAsync(
                int maDangKy,
                int maNguoiThamDu,
                string lyDoHuy)
        {
            var dangKy = await _context.DangKyThamDus
                .Include(d => d.SuKien)
                .FirstOrDefaultAsync(d =>
                    d.MaDangKy == maDangKy);

            if (dangKy == null)
            {
                return (
                    false,
                    "Đăng ký không tồn tại."
                );
            }

            // Kiểm tra quyền sở hữu đăng ký
            if (dangKy.MaNguoiThamDu != maNguoiThamDu)
            {
                return (
                    false,
                    "Bạn không có quyền hủy đăng ký này."
                );
            }

            // Không cho hủy nếu đã check-in hoặc hoàn thành
            if (dangKy.TrangThai == TrangThaiDangKy.DaCheckIn ||
                dangKy.TrangThai == TrangThaiDangKy.HoanThanh)
            {
                return (
                    false,
                    "Đăng ký đã check-in/hoàn thành, không thể hủy."
                );
            }

            // Kiểm tra đã hủy trước đó
            if (dangKy.TrangThai == TrangThaiDangKy.DaHuy)
            {
                return (
                    false,
                    "Đăng ký này đã được hủy trước đó."
                );
            }

            if (string.IsNullOrWhiteSpace(lyDoHuy))
            {
                return (
                    false,
                    "Vui lòng nhập lý do hủy đăng ký."
                );
            }

            if (dangKy.SuKien == null)
            {
                return (
                    false,
                    "Không tìm thấy thông tin sự kiện."
                );
            }

            // Hạn hủy: trước thời gian diễn ra ít nhất 1 ngày
            var hanHuyCuoiCung =
                dangKy.SuKien.ThoiGianBatDau
                    .AddDays(-SoNgayToiThieuTruocKhiHuy);

            if (DateTime.Now > hanHuyCuoiCung)
            {
                return (
                    false,
                    $"Đã quá hạn hủy đăng ký " +
                    $"(phải hủy trước " +
                    $"{hanHuyCuoiCung:dd/MM/yyyy HH:mm})."
                );
            }

            // Cập nhật trạng thái
            dangKy.TrangThai = TrangThaiDangKy.DaHuy;
            dangKy.LyDoTuChoiHuy = lyDoHuy;

            await _context.SaveChangesAsync();

            return (true, null);
        }

        // =========================================================
        // 6. ĐĂNG KÝ PHIÊN
        // =========================================================
        public async Task<(bool ThanhCong, string LyDo)>
            DangKyPhienAsync(
                int maNguoiThamDu,
                string maPhien)
        {
            // Kiểm tra người tham dự
            var nguoiThamDu = await _context.NguoiThamDus
                .FirstOrDefaultAsync(n =>
                    n.MaNguoiThamDu == maNguoiThamDu);

            if (nguoiThamDu == null)
            {
                return (
                    false,
                    "Người tham dự không tồn tại."
                );
            }

            // Không cho người bị khóa đăng ký phiên
            if (!nguoiThamDu.TrangThai)
            {
                return (
                    false,
                    "Tài khoản đã bị khóa/ngừng hoạt động, không thể đăng ký phiên."
                );
            }

            // Tìm phiên
            var phien = await _context.PhienSuKiens
                .FirstOrDefaultAsync(p =>
                    p.MaPhienSuKien == maPhien);

            if (phien == null)
            {
                return (
                    false,
                    "Phiên sự kiện không tồn tại."
                );
            }

            // Không cho đăng ký phiên đã bắt đầu
            if (DateTime.Now >= phien.ThoiGianBatDau)
            {
                return (
                    false,
                    "Phiên sự kiện đã bắt đầu, không thể đăng ký."
                );
            }

            // Kiểm tra đã đăng ký phiên này chưa
            bool daDangKy =
                await _context.DangKyPhiens
                    .AnyAsync(dp =>
                        dp.MaNguoiThamDu == maNguoiThamDu &&
                        dp.MaPhien == maPhien &&
                        dp.TrangThai != TrangThaiDangKy.DaHuy &&
                        dp.TrangThai != TrangThaiDangKy.TuChoi);

            if (daDangKy)
            {
                return (
                    false,
                    "Bạn đã đăng ký phiên này rồi."
                );
            }

            // Kiểm tra trùng lịch
            var (hopLe, lyDo) =
                await KiemTraTrungLichPhienAsync(
                    maNguoiThamDu,
                    maPhien,
                    phien.ThoiGianBatDau,
                    phien.ThoiGianKetThuc);

            if (!hopLe)
            {
                return (
                    false,
                    lyDo
                );
            }

            // Tạo đăng ký phiên
            var dangKyPhien = new DangKyPhien
            {
                MaNguoiThamDu = maNguoiThamDu,
                MaPhien = maPhien,
                NgayDangKy = DateTime.Now,
                TrangThai = TrangThaiDangKy.ChoDuyet
            };

            _context.DangKyPhiens.Add(dangKyPhien);

            await _context.SaveChangesAsync();

            return (
                true,
                null
            );
        }
    }
}