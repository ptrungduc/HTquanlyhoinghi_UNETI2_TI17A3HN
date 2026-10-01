using HTquanlyhoinghi_UNETI2_TI17A3HN.Database;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Controllers
{
    public class DiaDiemController : Controller
    {
        private readonly AppDbContext _context;

        public DiaDiemController(AppDbContext context)
        {
            _context = context;
        }

        // Kiểm tra tài khoản hiện tại có phải Admin hay không
        private bool LaAdmin()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");

            return vaiTro == "Admin";
        }


        // =========================================================
        // INDEX - DANH SÁCH ĐỊA ĐIỂM
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            int page = 1)
        {
            // Chỉ Admin được truy cập
            if (!LaAdmin())
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            // Số địa điểm hiển thị trên 1 trang
            int pageSize = 4;

            // Không cho page nhỏ hơn 1
            if (page < 1)
            {
                page = 1;
            }

            // Lấy dữ liệu
            var query = _context.DiaDiems
                .AsNoTracking()
                .AsQueryable();


            // ============================
            // TÌM KIẾM
            // ============================
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                query = query.Where(x =>
                    x.TenDiaDiem.Contains(tuKhoa)
                    || x.DiaChi.Contains(tuKhoa));
            }


            // ============================
            // TỔNG SỐ BẢN GHI
            // ============================
            int tongSoDiaDiem = await query.CountAsync();

            int tongSoTrang =
                (int)Math.Ceiling(tongSoDiaDiem / (double)pageSize);


            // Nếu nhập page quá lớn
            if (tongSoTrang > 0 && page > tongSoTrang)
            {
                page = tongSoTrang;
            }


            // ============================
            // PHÂN TRANG
            // ============================
            var danhSach = await query
                .OrderBy(x => x.MaDiaDiem)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            // Gửi dữ liệu sang View
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TongSoTrang = tongSoTrang;
            ViewBag.TongSoDiaDiem = tongSoDiaDiem;

            return View(danhSach);
        }


        // =========================================================
        // CREATE - HIỂN THỊ FORM THÊM
        // =========================================================
        [HttpGet]
        public IActionResult Create()
        {
            if (!LaAdmin())
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            return View();
        }


        // =========================================================
        // CREATE - XỬ LÝ THÊM
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiaDiem model)
        {
            if (!LaAdmin())
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }


            // ============================
            // KIỂM TRA MÃ ĐỊA ĐIỂM
            // ============================
            bool trungMa = await _context.DiaDiems
                .AnyAsync(x => x.MaDiaDiem == model.MaDiaDiem);

            if (trungMa)
            {
                ModelState.AddModelError(
                    "MaDiaDiem",
                    "Mã địa điểm đã tồn tại.");
            }


            // ============================
            // KIỂM TRA TÊN ĐỊA ĐIỂM
            // ============================
            bool trungTen = await _context.DiaDiems
                .AnyAsync(x => x.TenDiaDiem == model.TenDiaDiem);

            if (trungTen)
            {
                ModelState.AddModelError(
                    "TenDiaDiem",
                    "Tên địa điểm đã tồn tại.");
            }


            // ============================
            // KIỂM TRA SỨC CHỨA
            // ============================
            if (model.SucChuaToiDa <= 0)
            {
                ModelState.AddModelError(
                    "SucChuaToiDa",
                    "Sức chứa phải lớn hơn 0.");
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // Địa điểm mới mặc định hoạt động
            model.TrangThai = true;

            _context.DiaDiems.Add(model);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // EDIT - HIỂN THỊ FORM SỬA
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (!LaAdmin())
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }


            var diaDiem = await _context.DiaDiems
                .FirstOrDefaultAsync(x => x.MaDiaDiem == id);


            if (diaDiem == null)
            {
                return NotFound();
            }


            return View(diaDiem);
        }


        // =========================================================
        // EDIT - XỬ LÝ SỬA
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            DiaDiem model)
        {
            if (!LaAdmin())
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }


            // Kiểm tra ID
            if (id != model.MaDiaDiem)
            {
                return NotFound();
            }


            // ============================
            // KIỂM TRA TÊN TRÙNG
            // ============================
            bool trungTen = await _context.DiaDiems
                .AnyAsync(x =>
                    x.TenDiaDiem == model.TenDiaDiem
                    && x.MaDiaDiem != model.MaDiaDiem);


            if (trungTen)
            {
                ModelState.AddModelError(
                    "TenDiaDiem",
                    "Tên địa điểm đã tồn tại.");
            }


            // ============================
            // KIỂM TRA SỨC CHỨA
            // ============================
            if (model.SucChuaToiDa <= 0)
            {
                ModelState.AddModelError(
                    "SucChuaToiDa",
                    "Sức chứa phải lớn hơn 0.");
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // Lấy dữ liệu thật trong DB
            var diaDiem = await _context.DiaDiems
                .FirstOrDefaultAsync(x => x.MaDiaDiem == id);


            if (diaDiem == null)
            {
                return NotFound();
            }


            // Không cho sửa mã địa điểm
            // Chỉ sửa các thông tin cần thiết
            diaDiem.TenDiaDiem = model.TenDiaDiem;
            diaDiem.DiaChi = model.DiaChi;
            diaDiem.SucChuaToiDa = model.SucChuaToiDa;
            diaDiem.MoTa = model.MoTa;


            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // NGỪNG HOẠT ĐỘNG
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NgungHoatDong(string id)
        {
            if (!LaAdmin())
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }


            var diaDiem = await _context.DiaDiems
                .FirstOrDefaultAsync(x => x.MaDiaDiem == id);


            if (diaDiem == null)
            {
                return NotFound();
            }


            // Không xóa khỏi database
            diaDiem.TrangThai = false;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // KÍCH HOẠT LẠI
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KichHoat(string id)
        {
            if (!LaAdmin())
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }


            var diaDiem = await _context.DiaDiems
                .FirstOrDefaultAsync(x => x.MaDiaDiem == id);


            if (diaDiem == null)
            {
                return NotFound();
            }


            diaDiem.TrangThai = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}