using System.ComponentModel.DataAnnotations;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models
{
    public class NguoiThamDu
    {
        [Key]
        public int MaNguoiThamDu { get; set; }

        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại không hợp lệ (10 số, bắt đầu bằng 0)")]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [StringLength(150)]
        public string DonViCongTac { get; set; }

        public bool TrangThai { get; set; } = true;

        public virtual ICollection<DangKyThamDu> DanhSachDangKy { get; set; }
    }
}