
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers
{
    public class NguoiThamDuController : Controller
    {
        private readonly AppDbContext _context;

        public NguoiThamDuController(AppDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // 1. DANH SÁCH NGƯỜI THAM DỰ + TÌM KIẾM
        // =========================================================
        public async Task<IActionResult> Index(string timKiem)
        {
            var query = _context.NguoiThamDus
                .AsQueryable();

            // Tìm theo họ tên, email hoặc số điện thoại
            if (!string.IsNullOrWhiteSpace(timKiem))
            {
                query = query.Where(n =>
                    n.HoTen.Contains(timKiem) ||
                    n.Email.Contains(timKiem) ||
                    n.SoDienThoai.Contains(timKiem));
            }

            var danhSach = await query
                .OrderBy(n => n.HoTen)
                .ToListAsync();

            return View(danhSach);
        }

        // =========================================================
        // 2. HIỂN THỊ FORM THÊM NGƯỜI THAM DỰ
        // =========================================================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // =========================================================
        // 3. XỬ LÝ THÊM NGƯỜI THAM DỰ
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            NguoiThamDu nguoiThamDu)
        {
            // -----------------------------------------------------
            // Kiểm tra tài khoản có tồn tại không
            // -----------------------------------------------------
       var taiKhoanTonTai = await _context.TaiKhoans
    .AnyAsync(t =>
        t.MaTaiKhoan == nguoiThamDu.MaTaiKhoan.ToString());

            if (!taiKhoanTonTai)
            {
                ModelState.AddModelError(
                    "MaTaiKhoan",
                    "Tài khoản không tồn tại.");
            }

            // -----------------------------------------------------
            // Kiểm tra Email đã tồn tại chưa
            // -----------------------------------------------------
            bool trungEmail = await _context.NguoiThamDus
                .AnyAsync(n =>
                    n.Email == nguoiThamDu.Email);

            if (trungEmail)
            {
                ModelState.AddModelError(
                    "Email",
                    "Email này đã được đăng ký cho người tham dự khác.");
            }

            // -----------------------------------------------------
            // Kiểm tra dữ liệu Model
            // -----------------------------------------------------
            if (!ModelState.IsValid)
            {
                return View(nguoiThamDu);
            }

            // Người mới mặc định đang hoạt động
            nguoiThamDu.TrangThai = true;

            _context.NguoiThamDus.Add(nguoiThamDu);

            await _context.SaveChangesAsync();

            TempData["ThongBao"] =
                "Thêm người tham dự thành công.";

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // 4. HIỂN THỊ FORM SỬA
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var nguoiThamDu =
                await _context.NguoiThamDus
                    .FindAsync(id);

            if (nguoiThamDu == null)
            {
                return NotFound();
            }

            return View(nguoiThamDu);
        }

        // =========================================================
        // 5. XỬ LÝ SỬA NGƯỜI THAM DỰ
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            NguoiThamDu nguoiThamDu)
        {
            // Kiểm tra ID trên URL và ID trong Model
            if (id != nguoiThamDu.MaNguoiThamDu)
            {
                return NotFound();
            }

            // -----------------------------------------------------
            // Kiểm tra Email trùng
            // -----------------------------------------------------
            bool trungEmail = await _context.NguoiThamDus
                .AnyAsync(n =>
                    n.Email == nguoiThamDu.Email &&
                    n.MaNguoiThamDu != id);

            if (trungEmail)
            {
                ModelState.AddModelError(
                    "Email",
                    "Email này đã được dùng bởi người tham dự khác.");
            }

            // -----------------------------------------------------
            // Kiểm tra Model
            // -----------------------------------------------------
            if (!ModelState.IsValid)
            {
                return View(nguoiThamDu);
            }

            // -----------------------------------------------------
            // Lấy dữ liệu hiện tại trong DB
            // để không vô tình thay đổi trạng thái
            // -----------------------------------------------------
            var nguoiHienTai =
                await _context.NguoiThamDus
                    .FindAsync(id);

            if (nguoiHienTai == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin
            nguoiHienTai.MaTaiKhoan =
                nguoiThamDu.MaTaiKhoan;

            nguoiHienTai.HoTen =
                nguoiThamDu.HoTen;

            nguoiHienTai.SoDienThoai =
                nguoiThamDu.SoDienThoai;

            nguoiHienTai.Email =
                nguoiThamDu.Email;

            nguoiHienTai.DonViCongTac =
                nguoiThamDu.DonViCongTac;

            // Không cập nhật TrangThai ở đây.
            // Khóa/mở khóa được thực hiện riêng bằng KhoaMoKhoa.

            await _context.SaveChangesAsync();

            TempData["ThongBao"] =
                "Cập nhật thông tin thành công.";

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // 6. KHÓA / MỞ KHÓA NGƯỜI THAM DỰ
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KhoaMoKhoa(int id)
        {
            var nguoiThamDu =
                await _context.NguoiThamDus
                    .FindAsync(id);

            if (nguoiThamDu == null)
            {
                return NotFound();
            }

            // Đảo trạng thái
            nguoiThamDu.TrangThai =
                !nguoiThamDu.TrangThai;

            await _context.SaveChangesAsync();

            if (nguoiThamDu.TrangThai)
            {
                TempData["ThongBao"] =
                    "Đã mở khóa người tham dự.";
            }
            else
            {
                TempData["ThongBao"] =
                    "Đã khóa người tham dự.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // 7. XÓA NGƯỜI THAM DỰ
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var nguoiThamDu =
                await _context.NguoiThamDus
                    .FindAsync(id);

            if (nguoiThamDu == null)
            {
                return NotFound();
            }

            // Kiểm tra người tham dự đã có đăng ký hay chưa
            bool daDangKySuKien =
                await _context.DangKyThamDus
                    .AnyAsync(d =>
                        d.MaNguoiThamDu == id);

            bool daDangKyPhien =
                await _context.DangKyPhiens
                    .AnyAsync(d =>
                        d.MaNguoiThamDu == id);

            if (daDangKySuKien || daDangKyPhien)
            {
                TempData["Loi"] =
                    "Không thể xóa người tham dự đã có lịch sử đăng ký sự kiện hoặc phiên. " +
                    "Bạn có thể khóa người tham dự thay vì xóa.";

                return RedirectToAction(nameof(Index));
            }

            _context.NguoiThamDus.Remove(nguoiThamDu);

            await _context.SaveChangesAsync();

            TempData["ThongBao"] =
                "Xóa người tham dự thành công.";

            return RedirectToAction(nameof(Index));
        }
    }
}
