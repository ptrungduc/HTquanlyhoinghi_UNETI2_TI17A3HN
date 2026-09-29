using System.ComponentModel.DataAnnotations;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models
{
    public class SuKien
    {
        [Key]
        public string MaSuKien { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên sự kiện không được để trống")]
        [StringLength(200, ErrorMessage = "Tên sự kiện không được vượt quá 200 ký tự")]
        public string TenSuKien { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại sự kiện")]
        public string MaLoaiSuKien { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn địa điểm")]
        public string MaDiaDiem { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        public DateTime ThoiGianBatDau { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống")]
        public DateTime ThoiGianKetThuc { get; set; }

        public string MoTa { get; set; } = string.Empty;
        public string TrangThai { get; set; } = "SapDienRa";

        public LoaiSuKien? LoaiSuKien { get; set; }
        public DiaDiem? DiaDiem { get; set; }
        public ICollection<PhienSuKien> PhienSuKiens { get; set; } = new List<PhienSuKien>();
    }
}
