using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Helper;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
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
        public IActionResult DangNhap(DangNhapViewModels model)
        {
            if (ModelState.IsValid)
            {
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == model.TenDangNhap);
                if (taiKhoan != null && taiKhoan.MatKhau == MatKhauHelper.MaHoa(model.MatKhau))
                {
                    HttpContext.Session.SetString("TenDangNhap", taiKhoan.TenDangNhap);
                    HttpContext.Session.SetString("VaiTro", taiKhoan.VaiTro);
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
            }
            return View(model);
        }

        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("DangNhap");
        }
    }
}