using HTquanlyhoinghi_UNETI2_TI17A3HN.Helper;
using Microsoft.AspNetCore.Mvc;
namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers
{
    public class ReceptionistController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("MaTaiKhoan")))
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            if (!KiemTraQuyenModule4.CoQuyenTiepDon(HttpContext.Session.GetString("VaiTro")))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            return View();
        }
    }
}