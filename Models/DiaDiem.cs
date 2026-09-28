// Entity DiaDiem gồm tối thiểu:
// •	MaDiaDiem.
// •	TenDiaDiem.
// •	DiaChi.
// •	SucChuaToiDa.
// •	MoTa.
// •	TrangThai.
// •	Tên địa điểm bắt buộc.
// •	Sức chứa tối đa > 0.
// •	Địa điểm ngừng hoạt động không được dùng cho sự kiện mới.

using System.ComponentModel.DataAnnotations;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models
{
    public class DiaDiem
    {
        [Key]
        public string MaDiaDiem { get; set; } = string.Empty;
        [Required(ErrorMessage = "Tên địa điểm không được để trống")]
        public string TenDiaDiem { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        [Required(ErrorMessage = "Sức chứa tối đa không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Sức chứa tối đa phải là một số dương")]
        public int SucChuaToiDa { get; set; }
        public string MoTa { get; set; } = string.Empty;
        public bool TrangThai { get; set; } = true;
    }
}