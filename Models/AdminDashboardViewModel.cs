namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models
{
    public class AdminDashboardViewModel
    {
        // Stat Cards
        public int TongSuKien { get; set; }
        public int TongNguoiThamDu { get; set; }
        public int TongDiaDiem { get; set; }
        public int TongDangKy { get; set; }

        // Sự kiện sắp tới (Top 5)
        public List<SuKienSapToiItem> SuKienSapTois { get; set; } = new();

        // Sự kiện gần đây (Top 5)
        public List<SuKienGanDayItem> SuKienGanDays { get; set; } = new();
    }

    public class SuKienSapToiItem
    {
        public string MaSuKien { get; set; } = string.Empty;
        public string TenSuKien { get; set; } = string.Empty;
        public DateTime ThoiGianBatDau { get; set; }
        public string TenDiaDiem { get; set; } = string.Empty;
        public int SoNgayConLai => (ThoiGianBatDau.Date - DateTime.Now.Date).Days;
    }

    public class SuKienGanDayItem
    {
        public string MaSuKien { get; set; } = string.Empty;
        public string TenSuKien { get; set; } = string.Empty;
        public DateTime ThoiGianBatDau { get; set; }
        public string TenDiaDiem { get; set; } = string.Empty;
        public int SoNguoiDangKy { get; set; }
        public string TrangThai { get; set; } = string.Empty;
    }
}

