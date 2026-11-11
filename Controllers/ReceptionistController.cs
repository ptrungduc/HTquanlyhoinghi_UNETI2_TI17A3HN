// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Bàn tiếp đón Lễ tân - Dashboard thống kê, tiếp đón người tham dự, check-in nhanh.

using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Helper;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module4;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers
{
    public class ReceptionistController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IDichVuModule4 _dichVuModule4;
        private readonly ILogger<ReceptionistController> _logger;

        public ReceptionistController(
            AppDbContext context,
            IDichVuModule4 dichVuModule4,
            ILogger<ReceptionistController> logger)
        {
            _context = context;
            _dichVuModule4 = dichVuModule4;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? tuKhoa, string? maSuKien)
        {
            var phanQuyen = KiemTraQuyenLeTan();
            if (phanQuyen != null) return phanQuyen;

            var viewModel = await TaiDuLieuDashboardAsync(tuKhoa, maSuKien);

            if (TempData["ThongBaoThanhCong"] is string thongBaoOk)
            {
                viewModel.KetQua = new KetQuaThaoTacViewModel { ThanhCong = true, ThongBao = thongBaoOk };
            }
            else if (TempData["ThongBaoLoi"] is string thongBaoErr)
            {
                viewModel.KetQua = new KetQuaThaoTacViewModel { ThanhCong = false, ThongBao = thongBaoErr };
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckInNhanh(string maThamDu, string? ghiChu, string? tuKhoa, string? maSuKien)
        {
            var phanQuyen = KiemTraQuyenLeTan();
            if (phanQuyen != null) return phanQuyen;

            if (string.IsNullOrWhiteSpace(maThamDu))
            {
                TempData["ThongBaoLoi"] = "Mã tham dự không hợp lệ.";
                return RedirectToAction(nameof(Index), new { tuKhoa, maSuKien });
            }

            try
            {
                var nguoiThucHien = HttpContext.Session.GetString("MaTaiKhoan") ?? "LT_01";
                var ketQua = await _dichVuModule4.CheckInAsync(maThamDu, nguoiThucHien, ghiChu ?? "Tiếp đón tại bàn lễ tân");

                if (ketQua.ThanhCong)
                {
                    // Lấy thêm tên người tham dự để hiển thị lời chào nồng nhiệt
                    var dangKy = await _context.DangKyThamDus
                        .Include(d => d.NguoiThamDu)
                        .Include(d => d.SuKien)
                        .FirstOrDefaultAsync(d => d.MaThamDu == maThamDu.Trim());

                    var tenNguoi = dangKy?.NguoiThamDu?.HoTen ?? "Người tham dự";
                    var tenSK = dangKy?.SuKien?.TenSuKien ?? "Sự kiện";
                    TempData["ThongBaoThanhCong"] = $"✅ Đã check-in thành công cho {tenNguoi} ({maThamDu}) vào {tenSK}!";
                }
                else
                {
                    TempData["ThongBaoLoi"] = ketQua.ThongBao;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thực hiện check-in nhanh tại bàn lễ tân");
                TempData["ThongBaoLoi"] = "Đã xảy ra lỗi hệ thống khi check-in. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index), new { tuKhoa, maSuKien });
        }

        [HttpGet]
        public async Task<IActionResult> TraCuu(string? tuKhoa, string? maSuKien)
        {
            var phanQuyen = KiemTraQuyenLeTan();
            if (phanQuyen != null) return Unauthorized();

            var danhSach = await TimKiemNguoiThamDuAsync(tuKhoa, maSuKien, 20);
            return Json(danhSach);
        }

        private async Task<ReceptionistDashboardViewModel> TaiDuLieuDashboardAsync(string? tuKhoa, string? maSuKien)
        {
            var homNay = DateTime.Today;
            var ngayMai = homNay.AddDays(1);
            var bayGio = DateTime.Now;

            // 1. Danh sách sự kiện hôm nay và sự kiện đang diễn ra
            var querySuKien = _context.SuKiens
                .AsNoTracking()
                .Include(s => s.LoaiSuKien)
                .Include(s => s.DiaDiem)
                .Include(s => s.PhienSuKiens)
                .Where(s => s.TrangThai != nameof(TrangThaiSuKien.DaHuy));

            // Ưu tiên sự kiện hôm nay / đang diễn ra
            var suKiensDb = await querySuKien
                .OrderBy(s => s.ThoiGianBatDau)
                .ToListAsync();

            var suKienHomNayList = suKiensDb
                .Where(s => s.ThoiGianBatDau < ngayMai && s.ThoiGianKetThuc >= homNay)
                .ToList();

            // Nếu hôm nay không có sự kiện, lấy các sự kiện sắp tới gần nhất để bàn tiếp đón vẫn có dữ liệu làm việc
            var danhSachSuKienHienThi = suKienHomNayList.Any()
                ? suKienHomNayList
                : suKiensDb.Where(s => s.ThoiGianKetThuc >= homNay).Take(5).ToList();

            var maSuKienIds = danhSachSuKienHienThi.Select(s => s.MaSuKien).ToList();

            // Thống kê đăng ký cho các sự kiện hiển thị
            var thongKeDangKy = await _context.DangKyThamDus
                .AsNoTracking()
                .Where(d => maSuKienIds.Contains(d.MaSuKien))
                .GroupBy(d => d.MaSuKien)
                .Select(g => new
                {
                    MaSuKien = g.Key,
                    Tong = g.Count(d => d.TrangThai != TrangThaiDangKy.DaHuy && d.TrangThai != TrangThaiDangKy.TuChoi),
                    DaDuyet = g.Count(d => d.TrangThai == TrangThaiDangKy.DaDuyet),
                    DaCheckIn = g.Count(d => d.TrangThai == TrangThaiDangKy.DaCheckIn || d.TrangThai == TrangThaiDangKy.HoanThanh)
                })
                .ToDictionaryAsync(x => x.MaSuKien);

            var danhSachSuKienVm = danhSachSuKienHienThi.Select(s =>
            {
                thongKeDangKy.TryGetValue(s.MaSuKien, out var tk);
                bool dangDienRa = bayGio >= s.ThoiGianBatDau && bayGio <= s.ThoiGianKetThuc;

                return new SuKienTiepDonViewModel
                {
                    MaSuKien = s.MaSuKien,
                    TenSuKien = s.TenSuKien,
                    TenLoaiSuKien = s.LoaiSuKien?.TenLoaiSuKien ?? "Sự kiện",
                    TenDiaDiem = s.DiaDiem?.TenDiaDiem ?? "Chưa rõ",
                    ThoiGianBatDau = s.ThoiGianBatDau,
                    ThoiGianKetThuc = s.ThoiGianKetThuc,
                    TrangThai = s.TrangThai,
                    DangDienRa = dangDienRa,
                    TongDangKy = tk?.Tong ?? 0,
                    DaDuyet = tk?.DaDuyet ?? 0,
                    DaCheckIn = tk?.DaCheckIn ?? 0,
                    DanhSachPhien = s.PhienSuKiens
                        .Where(p => p.TrangThai != "DaHuy")
                        .OrderBy(p => p.ThoiGianBatDau)
                        .Select(p => new PhienTiepDonViewModel
                        {
                            MaPhienSuKien = p.MaPhienSuKien,
                            TenPhien = p.TenPhien,
                            ThoiGianBatDau = p.ThoiGianBatDau,
                            ThoiGianKetThuc = p.ThoiGianKetThuc,
                            PhongKhuVuc = p.PhongKhuVuc,
                            TrangThai = p.TrangThai
                        }).ToList()
                };
            }).ToList();

            // 2. Thống kê nhanh toàn hệ thống cho ngày hôm nay
            int tongSuKienHomNay = suKienHomNayList.Count;

            int tongDaCheckInHomNay = await _context.CheckIns
                .AsNoTracking()
                .CountAsync(c => c.ThoiGianCheckIn >= homNay && c.ThoiGianCheckIn < ngayMai);

            int tongDangKyHomNay = suKienHomNayList.Any()
                ? await _context.DangKyThamDus
                    .AsNoTracking()
                    .CountAsync(d => suKienHomNayList.Select(x => x.MaSuKien).Contains(d.MaSuKien)
                        && d.TrangThai != TrangThaiDangKy.DaHuy && d.TrangThai != TrangThaiDangKy.TuChoi)
                : 0;

            int tongChoCheckInHomNay = suKienHomNayList.Any()
                ? await _context.DangKyThamDus
                    .AsNoTracking()
                    .CountAsync(d => suKienHomNayList.Select(x => x.MaSuKien).Contains(d.MaSuKien)
                        && d.TrangThai == TrangThaiDangKy.DaDuyet)
                : 0;

            // 3. Danh sách người tham dự cho bảng tra cứu & tiếp đón
            var danhSachNguoiThamDu = await TimKiemNguoiThamDuAsync(tuKhoa, maSuKien, 30);

            return new ReceptionistDashboardViewModel
            {
                TongSuKienHomNay = tongSuKienHomNay,
                TongDaCheckInHomNay = tongDaCheckInHomNay,
                TongChoCheckInHomNay = tongChoCheckInHomNay,
                TongDangKyHomNay = tongDangKyHomNay,
                DanhSachSuKien = danhSachSuKienVm,
                DanhSachNguoiThamDu = danhSachNguoiThamDu,
                TuKhoa = tuKhoa,
                MaSuKienChon = maSuKien
            };
        }

        private async Task<List<NguoiThamDuTiepDonViewModel>> TimKiemNguoiThamDuAsync(string? tuKhoa, string? maSuKien, int soLuongToiDa)
        {
            var query = _context.DangKyThamDus
                .AsNoTracking()
                .Include(d => d.NguoiThamDu)
                .Include(d => d.SuKien)
                .Include(d => d.CheckIn)
                .Where(d => d.TrangThai != TrangThaiDangKy.DaHuy && d.TrangThai != TrangThaiDangKy.TuChoi);

            if (!string.IsNullOrWhiteSpace(maSuKien))
            {
                query = query.Where(d => d.MaSuKien == maSuKien);
            }

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var kw = tuKhoa.Trim();
                query = query.Where(d =>
                    (d.MaThamDu != null && d.MaThamDu.Contains(kw)) ||
                    d.MaDangKy.Contains(kw) ||
                    (d.NguoiThamDu != null && (
                        d.NguoiThamDu.HoTen.Contains(kw) ||
                        d.NguoiThamDu.SoDienThoai.Contains(kw) ||
                        d.NguoiThamDu.Email.Contains(kw) ||
                        d.NguoiThamDu.MaNguoiThamDu.Contains(kw)
                    )) ||
                    (d.SuKien != null && d.SuKien.TenSuKien.Contains(kw))
                );
            }
            else if (string.IsNullOrWhiteSpace(maSuKien))
            {
                // Mặc định ưu tiên người tham dự của sự kiện đang diễn ra hoặc đã duyệt chờ check-in
                query = query.OrderByDescending(d => d.TrangThai == TrangThaiDangKy.DaDuyet)
                             .ThenByDescending(d => d.NgayDangKy);
            }

            var items = await query.Take(soLuongToiDa).ToListAsync();
            var bayGio = DateTime.Now;

            return items.Select(d =>
            {
                bool suKienHopLe = d.SuKien != null &&
                    bayGio >= d.SuKien.ThoiGianBatDau &&
                    bayGio <= d.SuKien.ThoiGianKetThuc;

                // Có thể check-in nếu trạng thái là Đã duyệt, có mã tham dự và đang trong thời gian sự kiện
                bool coTheCheckIn = d.TrangThai == TrangThaiDangKy.DaDuyet &&
                                    !string.IsNullOrWhiteSpace(d.MaThamDu) &&
                                    suKienHopLe;

                return new NguoiThamDuTiepDonViewModel
                {
                    MaDangKy = d.MaDangKy,
                    MaThamDu = d.MaThamDu ?? "-",
                    MaNguoiThamDu = d.NguoiThamDu?.MaNguoiThamDu ?? string.Empty,
                    HoTen = d.NguoiThamDu?.HoTen ?? "Chưa rõ",
                    SoDienThoai = d.NguoiThamDu?.SoDienThoai ?? string.Empty,
                    Email = d.NguoiThamDu?.Email ?? string.Empty,
                    DonViCongTac = d.NguoiThamDu?.DonViCongTac ?? string.Empty,
                    MaSuKien = d.MaSuKien,
                    TenSuKien = d.SuKien?.TenSuKien ?? "Chưa rõ",
                    TrangThaiDangKy = d.TrangThai,
                    ThoiGianCheckIn = d.CheckIn?.ThoiGianCheckIn,
                    NguoiCheckIn = d.CheckIn?.NguoiThucHien,
                    CoTheCheckIn = coTheCheckIn
                };
            }).ToList();
        }

        private IActionResult? KiemTraQuyenLeTan()
        {
            var maTaiKhoan = HttpContext.Session.GetString("MaTaiKhoan");
            if (string.IsNullOrWhiteSpace(maTaiKhoan))
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (!Helper.KiemTraQuyenModule4.CoQuyenTiepDon(vaiTro))
            {
                TempData["ThongBaoSaiVaiTro"] = "Bạn không có quyền truy cập vào mục Lễ tân do sai vai trò!";
                var (ctrl, act) = Helper.KiemTraQuyenModule4.LayTrangDieuHuongTheoVaiTro(vaiTro);
                return RedirectToAction(act, ctrl);
            }

            return null;
        }
    }
}
