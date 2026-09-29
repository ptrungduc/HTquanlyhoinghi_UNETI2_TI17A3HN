using System.ComponentModel.DataAnnotations;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models
{
    public class PhienSuKien
    {
        [Key]
        public string MaPhienSuKien { get; set; } = string.Empty;

        [Required]
        public string MaSuKien { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên phiên không được để trống")]
        [StringLength(200, ErrorMessage = "Tên phiên không được vượt quá 200 ký tự")]
        public string TenPhien { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        public DateTime ThoiGianBatDau { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống")]
        public DateTime ThoiGianKetThuc { get; set; }

        public string DienGia { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string TrangThai { get; set; } = "SapDienRa";

        public SuKien? SuKien { get; set; }
    }
}
