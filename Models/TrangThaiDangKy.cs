// Enum trạng thái đăng ký - Dùng chung cho Module 3, 4, 5.
// Định nghĩa theo tài liệu yêu cầu (Mục 7.2):
// Chờ duyệt, Đã duyệt, Đã check-in, Hoàn thành, Từ chối, Đã hủy.

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models
{
    public enum TrangThaiDangKy
    {
        ChoDuyet,
        DaDuyet,
        DaCheckIn,
        HoanThanh,
        TuChoi,
        DaHuy
    }
}
