using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using Microsoft.AspNetCore.Mvc;
namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers
{
    public class ReceptionistController : Controller
    {
        private readonly AppDbContext _context;

        public ReceptionistController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");

            if (vaiTro != "Receptionist")
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            return View();
        }
    }
}