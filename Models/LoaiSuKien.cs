// Entity LoaiSuKien gồm tối thiểu:
// •	MaLoaiSuKien.
// •	TenLoaiSuKien.
// •	MoTa.
// •	TrangThai.
// •	Tên loại sự kiện bắt buộc và không trùng.
// •	Loại sự kiện ngừng hoạt động không được dùng để tạo sự kiện mới.
// •	Dữ liệu đã phát sinh lịch sử không xóa nếu làm mất tính toàn vẹn.

using System.ComponentModel.DataAnnotations;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models
{
    public class LoaiSuKien
    {
        [Key]
        public string MaLoaiSuKien { get; set; } = string.Empty;
        [Required(ErrorMessage = "Tên loại sự kiện không được để trống")]
        public string TenLoaiSuKien { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public bool TrangThai { get; set; } = true;
    }
}