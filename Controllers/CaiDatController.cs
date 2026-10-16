// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Cài đặt giao diện - ngôn ngữ và theme sáng/tối.

using Microsoft.AspNetCore.Mvc;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers;

public class CaiDatController : Controller
{
    private const string CookieNgonNgu = "ngon_ngu";
    private const string CookieTheme = "theme";
    private static readonly string[] NgonNguHoTro = { "vi", "en" };
    private static readonly string[] ThemeHoTro = { "light", "dark" };

    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.NgonNgu = LayNgonNguHienTai();
        ViewBag.Theme = LayThemeHienTai();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult LuuCaiDat(string ngonNgu, string theme)
    {
        // Chỉ chấp nhận giá trị trong danh sách hỗ trợ
        if (!NgonNguHoTro.Contains(ngonNgu))
        {
            ngonNgu = "vi";
        }
        if (!ThemeHoTro.Contains(theme))
        {
            theme = "light";
        }

        var tuyChon = new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            HttpOnly = false,
            SameSite = SameSiteMode.Lax,
            IsEssential = true
        };

        Response.Cookies.Append(CookieNgonNgu, ngonNgu, tuyChon);
        Response.Cookies.Append(CookieTheme, theme, tuyChon);

        TempData["ThongBao"] = "Đã lưu cài đặt.";
        return RedirectToAction(nameof(Index));
    }

    private string LayNgonNguHienTai()
    {
        var gia = Request.Cookies[CookieNgonNgu];
        return NgonNguHoTro.Contains(gia) ? gia! : "vi";
    }

    private string LayThemeHienTai()
    {
        var gia = Request.Cookies[CookieTheme];
        return ThemeHoTro.Contains(gia) ? gia! : "light";
    }
}
