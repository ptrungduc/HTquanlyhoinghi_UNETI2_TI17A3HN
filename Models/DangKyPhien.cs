namespace QuanLyHoiNghi.Models
{
    public class DangKyPhien
    {
        public int MaDangKyPhien { get; set; }

        public int MaNguoiThamDu { get; set; }
        public virtual NguoiThamDu NguoiThamDu { get; set; }

        public int MaPhien { get; set; }
        public virtual PhienSuKien Phien { get; set; }

        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        public TrangThaiDangKy TrangThai { get; set; } = TrangThaiDangKy.ChoDuyet;
    }
}