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

        [HttpGet]
        public IActionResult DangNhap()
        {
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

            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.TenDangNhap == model.TenDangNhap
                                       && t.Email == model.Email
                                       && t.MatKhau == matKhauMaHoa);

            if (taiKhoan == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng");
                return View(model);
            }

            if (!taiKhoan.TrangThai)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa");
                return View(model);
            }

            HttpContext.Session.SetString("MaTaiKhoan", taiKhoan.MaTaiKhoan);
            HttpContext.Session.SetString("HoTen", taiKhoan.HoTen);
            HttpContext.Session.SetString("VaiTro", taiKhoan.VaiTro);
            
            return RedirectToAction("Index", taiKhoan.VaiTro);
        }

        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("DangNhap");
        }
    }
}