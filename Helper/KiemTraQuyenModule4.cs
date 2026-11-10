// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 4 - Xét duyệt đăng ký, mã tham dự, check-in, điểm danh.

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Helper;

public static class KiemTraQuyenModule4
{
    /// <summary>
    /// Kiểm tra quyền quản trị chung cho Admin và Ban tổ chức (2 vai trò có cùng quyền hạn).
    /// </summary>
    public static bool LaAdminHoacBanToChuc(string? vaiTro)
    {
        if (string.IsNullOrWhiteSpace(vaiTro)) return false;
        var vt = vaiTro.Trim();
        return string.Equals(vt, "Admin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(vt, "Ban To Chuc", StringComparison.OrdinalIgnoreCase)
            || string.Equals(vt, "BanToChuc", StringComparison.OrdinalIgnoreCase)
            || string.Equals(vt, "Ban tổ chức", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Quyền thao tác tại Module 4 (tiếp đón, check-in, điểm danh) bao gồm Admin, Ban tổ chức và Lễ tân.
    /// </summary>
    public static bool CoQuyenQuanTri(string? vaiTro)
    {
        if (string.IsNullOrWhiteSpace(vaiTro)) return false;
        var vt = vaiTro.Trim();
        return LaAdminHoacBanToChuc(vt)
            || string.Equals(vt, "Le Tan", StringComparison.OrdinalIgnoreCase)
            || string.Equals(vt, "Lễ tân", StringComparison.OrdinalIgnoreCase)
            || string.Equals(vt, "Receptionist", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Trả về trang điều hướng mặc định phù hợp cho từng vai trò.
    /// </summary>
    public static (string controller, string action) LayTrangDieuHuongTheoVaiTro(string? vaiTro)
    {
        if (string.IsNullOrWhiteSpace(vaiTro)) return ("TaiKhoan", "DangNhap");
        if (LaAdminHoacBanToChuc(vaiTro)) return ("Admin", "Index");
        if (string.Equals(vaiTro, "Le Tan", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(vaiTro, "Lễ tân", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(vaiTro, "Receptionist", StringComparison.OrdinalIgnoreCase))
        {
            return ("Receptionist", "Index");
        }
        return ("DangKy", "Index");
    }
}
