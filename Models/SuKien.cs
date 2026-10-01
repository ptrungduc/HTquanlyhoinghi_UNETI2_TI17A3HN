using System.ComponentModel.DataAnnotations;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models
{
    public class SuKien
    {
        [Key]
        public string MaSuKien { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string TenSuKien { get; set; } = string.Empty;

        [Required]
        public string MaLoaiSuKien { get; set; } = string.Empty;

        [Required]
        public string MaDiaDiem { get; set; } = string.Empty;

        [Required]
        public DateTime ThoiGianBatDau { get; set; }

        [Required]
        public DateTime ThoiGianKetThuc { get; set; }

        public string MoTa { get; set; } = string.Empty;
        public string TrangThai { get; set; } = "SapDienRa";

        public LoaiSuKien? LoaiSuKien { get; set; }
        public DiaDiem? DiaDiem { get; set; }
        public ICollection<PhienSuKien> PhienSuKiens { get; set; } = new List<PhienSuKien>();
    }
}
