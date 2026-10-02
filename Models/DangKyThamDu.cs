using System.ComponentModel.DataAnnotations;

namespace QuanLyHoiNghi.Models
{
    public enum TrangThaiDangKy
    {
        ChoDuyet = 0,
        DaDuyet = 1,
        DaCheckIn = 2,
        HoanThanh = 3,
        TuChoi = 4,
        DaHuy = 5
    }

    public class DangKyThamDu
    {
        [Key]
        public int MaDangKy { get; set; }

        public int MaNguoiThamDu { get; set; }
        public virtual NguoiThamDu NguoiThamDu { get; set; }

        public int MaSuKien { get; set; }
        public virtual SuKien SuKien { get; set; }

        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        public TrangThaiDangKy TrangThai { get; set; } = TrangThaiDangKy.ChoDuyet;

        [StringLength(500)]
        public string GhiChu { get; set; }

        public DateTime? NgayDuyet { get; set; }

        [StringLength(500)]
        public string LyDoTuChoiHuy { get; set; }
    }
}