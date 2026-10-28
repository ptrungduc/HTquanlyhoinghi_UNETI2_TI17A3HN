using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Helper;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly AppDbContext _context;

        public TaiKhoanController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // ĐĂNG NHẬP
        // =====================================================
        [HttpGet]
        public IActionResult DangNhap()
        {
            // Nếu đã đăng nhập thì tự động chuyển hướng theo vai trò
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (!string.IsNullOrWhiteSpace(vaiTro))
            {
                return RedirectByVaiTro(vaiTro);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(DangNhapViewModels model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!DangNhapViewModels.IsValidEmail(model.Email))
            {
                ModelState.AddModelError("Email", "Định dạng email không hợp lệ");
                return View(model);
            }

            var matKhauMaHoa = MatKhauHelper.MaHoa(model.MatKhau);
            var tenDangNhap = model.TenDangNhap.Trim();
            var email = model.Email.Trim().ToLower();

            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap
                                       && t.Email.ToLower() == email
                                       && t.MatKhau == matKhauMaHoa);

            if (taiKhoan == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập, email hoặc mật khẩu không đúng");
                return View(model);
            }

            if (!taiKhoan.TrangThai)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
                return View(model);
            }

            // Lưu thông tin tối thiểu vào Session theo đặc tả Module 1 (Mục 5.2)
            HttpContext.Session.SetString("MaTaiKhoan", taiKhoan.MaTaiKhoan);
            HttpContext.Session.SetString("HoTen", taiKhoan.HoTen);
            HttpContext.Session.SetString("VaiTro", taiKhoan.VaiTro);

            // Phân quyền điều hướng theo vai trò tại Controller (Mục 5.3)
            return RedirectByVaiTro(taiKhoan.VaiTro);
        }

        // =====================================================
        // ĐĂNG KÝ TÀI KHOẢN NGƯỜI THAM DỰ
        // =====================================================
        [HttpGet]
        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(DangKyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra tên đăng nhập bắt buộc không trùng (Mục 5.1)
            var tenDangNhapTrung = await _context.TaiKhoans
                .AnyAsync(t => t.TenDangNhap.ToLower() == model.TenDangNhap.Trim().ToLower());
            if (tenDangNhapTrung)
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại, vui lòng chọn tên khác");
                return View(model);
            }

            // Kiểm tra email không trùng
            var emailTrung = await _context.TaiKhoans
                .AnyAsync(t => t.Email.ToLower() == model.Email.Trim().ToLower());
            if (emailTrung)
            {
                ModelState.AddModelError("Email", "Địa chỉ email này đã được sử dụng");
                return View(model);
            }

            // Tạo mã duy nhất
            var suffix = Guid.NewGuid().ToString("N")[..4];
            var maTaiKhoan = "NTD_TK" + DateTime.Now.ToString("yyyyMMddHHmmss") + suffix;
            var maNguoiThamDu = "NTD_" + DateTime.Now.ToString("yyyyMMddHHmmss") + suffix;

            // 1. Tạo entity TaiKhoan (Module 1)
            var taiKhoan = new TaiKhoan
            {
                MaTaiKhoan = maTaiKhoan,
                TenDangNhap = model.TenDangNhap.Trim(),
                MatKhau = MatKhauHelper.MaHoa(model.MatKhau),
                HoTen = model.HoTen.Trim(),
                Email = model.Email.Trim(),
                VaiTro = "Nguoi Tham Du",
                TrangThai = true
            };

            _context.TaiKhoans.Add(taiKhoan);

            // 2. Tạo entity NguoiThamDu liên kết với TaiKhoan (Module 3)
            var nguoiThamDu = new NguoiThamDu
            {
                MaNguoiThamDu = maNguoiThamDu,
                MaTaiKhoan = maTaiKhoan,
                HoTen = model.HoTen.Trim(),
                SoDienThoai = model.SoDienThoai.Trim(),
                Email = model.Email.Trim(),
                DonViCongTac = model.DonViCongTac?.Trim() ?? string.Empty,
                TrangThai = true
            };

            _context.NguoiThamDus.Add(nguoiThamDu);

            await _context.SaveChangesAsync();

            TempData["ThongBao"] = "Đăng ký tài khoản thành công! Bạn có thể đăng nhập ngay bây giờ.";
            return RedirectToAction("DangNhap");
        }

        // =====================================================
        // ĐĂNG XUẤT (XÓA SESSION)
        // =====================================================
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("DangNhap");
        }

        // =====================================================
        // PHÂN QUYỀN ĐIỀU HƯỚNG THEO VAI TRÒ (Mục 5.3)
        // =====================================================
        private IActionResult RedirectByVaiTro(string? vaiTro)
        {
            if (string.IsNullOrWhiteSpace(vaiTro))
            {
                return RedirectToAction("DangNhap");
            }

            var vt = vaiTro.Trim();

            // 1. Admin: chuyển hướng đến trang tổng quan Admin
            if (string.Equals(vt, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Admin");
            }

            // 2. Ban tổ chức: quản lý sự kiện, phiên, đăng ký
            if (string.Equals(vt, "Ban To Chuc", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(vt, "Ban tổ chức", StringComparison.OrdinalIgnoreCase) ||
                vt.ToLower().Contains("to chuc") || vt.ToLower().Contains("tổ chức"))
            {
                return RedirectToAction("Index", "SuKien");
            }

            // 3. Nhân viên tiếp đón / Lễ tân: kiểm tra mã tham dự, check-in
            if (string.Equals(vt, "Le Tan", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(vt, "Lễ tân", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(vt, "Receptionist", StringComparison.OrdinalIgnoreCase) ||
                vt.ToLower().Contains("tan"))
            {
                return RedirectToAction("Index", "Receptionist");
            }

            // 4. Người tham dự: xem sự kiện, đăng ký tham gia và xem dữ liệu cá nhân
            if (string.Equals(vt, "Nguoi Tham Du", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(vt, "Người tham dự", StringComparison.OrdinalIgnoreCase) ||
                vt.ToLower().Contains("tham du") || vt.ToLower().Contains("tham dự"))
            {
                return RedirectToAction("Index", "DangKy");
            }

            // Mặc định fallback
            return RedirectToAction("Index", "Home");
        }
    }
}
