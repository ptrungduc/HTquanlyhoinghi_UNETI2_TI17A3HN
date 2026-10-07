// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 4 - Xét duyệt đăng ký, mã tham dự, check-in, điểm danh.

using System.ComponentModel.DataAnnotations;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module4;

public class TuChoiDangKyViewModel
{
    [Required(ErrorMessage = "Mã đăng ký không được để trống.")]
    [StringLength(100, ErrorMessage = "Mã đăng ký không được vượt quá 100 ký tự.")]
    [Display(Name = "Mã đăng ký")]
    public string MaDangKy { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập lý do từ chối.")]
    [StringLength(500, ErrorMessage = "Lý do từ chối không được vượt quá 500 ký tự.")]
    [Display(Name = "Lý do từ chối")]
    public string LyDoTuChoi { get; set; } = string.Empty;
}