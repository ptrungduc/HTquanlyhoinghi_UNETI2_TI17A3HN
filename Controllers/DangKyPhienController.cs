using Microsoft.AspNetCore.Mvc;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Services;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers
{
    public class DangKyPhienController : Controller
    {
        private readonly DangKyService _dangKyService;

        public DangKyPhienController(
            DangKyService dangKyService)
        {
            _dangKyService = dangKyService;
        }

        // Hiển thị trang đăng ký phiên
        public IActionResult DangKy(string maPhien)
        {
            ViewBag.MaPhien = maPhien;
            return View();
        }

        // Xử lý đăng ký phiên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(
            int maNguoiThamDu,
            string maPhien)
        {
            var (thanhCong, lyDo) =
                await _dangKyService.DangKyPhienAsync(
                    maNguoiThamDu,
                    maPhien);

            TempData[thanhCong ? "ThongBao" : "Loi"] =
                thanhCong
                    ? "Đăng ký phiên thành công."
                    : lyDo;

            return RedirectToAction(
                "DanhSachDangKy",
                "DangKy",
                new { maNguoiThamDu });
        }
    }
}