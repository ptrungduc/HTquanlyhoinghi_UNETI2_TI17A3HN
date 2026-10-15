using System.ComponentModel.DataAnnotations;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models
{
    public class PhienSuKien
    {
        [Key]
        public string MaPhienSuKien { get; set; } = string.Empty;

        [Required]
        public string MaSuKien { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string TenPhien { get; set; } = string.Empty;

        [Required]
        public DateTime ThoiGianBatDau { get; set; }

        [Required]
        public DateTime ThoiGianKetThuc { get; set; }

        [Required]
        [StringLength(100)]
        public string PhongKhuVuc { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng tối đa phải lớn hơn 0.")]
        public int SoLuongToiDa { get; set; }

        public string DienGia { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string TrangThai { get; set; } = "SapDienRa";

        public SuKien? SuKien { get; set; }
    }
}
