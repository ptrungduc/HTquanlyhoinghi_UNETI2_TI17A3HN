// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 4 - Xét duyệt đăng ký, mã tham dự, check-in, điểm danh.

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Helper;

public static class KiemTraQuyenModule4
{
    public static bool CoQuyenQuanTri(string? vaiTro)
    {
        return string.Equals(vaiTro, "Admin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(vaiTro, "Ban To Chuc", StringComparison.OrdinalIgnoreCase)
            || string.Equals(vaiTro, "Ban tổ chức", StringComparison.OrdinalIgnoreCase);
    }

    public static bool CoQuyenTiepDon(string? vaiTro)
    {
        return CoQuyenQuanTri(vaiTro)
            || string.Equals(vaiTro, "Le Tan", StringComparison.OrdinalIgnoreCase)
            || string.Equals(vaiTro, "Lễ tân", StringComparison.OrdinalIgnoreCase)
            || string.Equals(vaiTro, "Receptionist", StringComparison.OrdinalIgnoreCase);
    }
}