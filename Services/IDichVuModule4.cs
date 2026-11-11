// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 4 - Xét duyệt đăng ký, mã tham dự, check-in, điểm danh.

using HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module4;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Services;

public interface IDichVuModule4
{
    Task<DuyetDangKyViewModel> LayDanhSachChoDuyetAsync(
        string? maSuKien = null,
        TrangThaiDangKy? trangThai = null,
        string? tuKhoa = null);
    Task<ChiTietDangKyViewModel> LayChiTietDangKyAsync(string maDangKy);
    Task<ChiTietDangKyViewModel> TraCuuMaThamDuAsync(string? maThamDu);
    Task<DiemDanhPhienViewModel> LayDiemDanhPhienAsync(
        string? maPhienSuKien,
        string? tuKhoa = null);
    Task<DangKyPhienChoDuyetViewModel> LayDangKyPhienChoDuyetAsync(string? maPhienSuKien);
    Task<KetQuaThaoTacViewModel> DuyetDangKyAsync(string maDangKy);
    Task<KetQuaThaoTacViewModel> TuChoiDangKyAsync(string maDangKy, string lyDoTuChoi);
    Task<KetQuaThaoTacViewModel> DuyetDangKyPhienAsync(string maDangKyPhien);
    Task<KetQuaThaoTacViewModel> CheckInAsync(string maThamDu, string nguoiThucHien, string? ghiChu);
    Task<KetQuaThaoTacViewModel> DiemDanhPhienAsync(
        string maPhienSuKien,
        string maNguoiThamDu,
        KetQuaDiemDanhPhien ketQua,
        string nguoiThucHien);
}