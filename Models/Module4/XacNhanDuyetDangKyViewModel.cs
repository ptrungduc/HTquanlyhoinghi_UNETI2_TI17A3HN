// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 4 - Xét duyệt đăng ký, mã tham dự, check-in, điểm danh.

using System.ComponentModel.DataAnnotations;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module4;

public class XacNhanDuyetDangKyViewModel
{
    [Required(ErrorMessage = "Mã đăng ký không được để trống.")]
    [StringLength(100, ErrorMessage = "Mã đăng ký không được vượt quá 100 ký tự.")]
    [Display(Name = "Mã đăng ký")]
    public string MaDangKy { get; set; } = string.Empty;
}