using System.ComponentModel.DataAnnotations;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models
{
    public class DangKyPhien
    {
        [Key]
        public int MaDangKyPhien { get; set; }

        public int MaNguoiThamDu { get; set; }
        public virtual NguoiThamDu NguoiThamDu { get; set; }

        public string MaPhien { get; set; } = string.Empty;
        public virtual PhienSuKien Phien { get; set; }

        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        public TrangThaiDangKy TrangThai { get; set; } = TrangThaiDangKy.ChoDuyet;
    }
}