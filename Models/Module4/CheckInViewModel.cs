// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 4 - Xét duyệt đăng ký, mã tham dự, check-in, điểm danh.

using System.ComponentModel.DataAnnotations;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module4;

public class CheckInViewModel
{
    [Required(ErrorMessage = "Mã tham dự không được để trống.")]
    [StringLength(100, ErrorMessage = "Mã tham dự không được vượt quá 100 ký tự.")]
    [Display(Name = "Mã tham dự")]
    public string MaThamDu { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    public KetQuaThaoTacViewModel? KetQua { get; set; }
}