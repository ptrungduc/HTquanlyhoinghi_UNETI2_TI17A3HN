using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using Microsoft.AspNetCore.Mvc;
namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");

            if (vaiTro != "Admin")
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            return View();
        }
    }
}