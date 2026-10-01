// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 4 - Xét duyệt đăng ký, mã tham dự, check-in, điểm danh.

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Helper;

public static class KiemTraQuyenModule4
{
    public static bool CoQuyenQuanTri(string? vaiTro)
    {
        return string.Equals(vaiTro, "Admin", StringComparison.OrdinalIgnoreCase);
    }
}