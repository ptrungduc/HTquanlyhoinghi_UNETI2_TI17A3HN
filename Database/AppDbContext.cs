using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // =========================
        // CÁC BẢNG CÓ SẴN
        // =========================

        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<LoaiSuKien> LoaiSuKiens { get; set; }
        public DbSet<DiaDiem> DiaDiems { get; set; }
        public DbSet<SuKien> SuKiens { get; set; }
        public DbSet<PhienSuKien> PhienSuKiens { get; set; }

        // =========================
        // MODULE 3
        // =========================

        public DbSet<NguoiThamDu> NguoiThamDus { get; set; }
        public DbSet<DangKyThamDu> DangKyThamDus { get; set; }
        public DbSet<DangKyPhien> DangKyPhiens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // LOAI SU KIEN
            // =========================

            modelBuilder.Entity<LoaiSuKien>()
                .HasIndex(l => l.TenLoaiSuKien)
                .IsUnique();

            // =========================
            // TAI KHOAN
            // =========================

            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(t => t.TenDangNhap)
                .IsUnique();

            // =========================
            // SU KIEN - LOAI SU KIEN
            // =========================

            modelBuilder.Entity<SuKien>()
                .HasOne(s => s.LoaiSuKien)
                .WithMany()
                .HasForeignKey(s => s.MaLoaiSuKien)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // SU KIEN - DIA DIEM
            // =========================

            modelBuilder.Entity<SuKien>()
                .HasOne(s => s.DiaDiem)
                .WithMany()
                .HasForeignKey(s => s.MaDiaDiem)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // PHIEN SU KIEN - SU KIEN
            // =========================

            modelBuilder.Entity<PhienSuKien>()
                .HasOne(p => p.SuKien)
                .WithMany(s => s.PhienSuKiens)
                .HasForeignKey(p => p.MaSuKien)
                .OnDelete(DeleteBehavior.Cascade);

            // ==================================================
            // MODULE 3
            // NGUOI THAM DU - DANG KY THAM DU
            // ==================================================

            modelBuilder.Entity<DangKyThamDu>()
                .HasOne(d => d.NguoiThamDu)
                .WithMany(n => n.DanhSachDangKy)
                .HasForeignKey(d => d.MaNguoiThamDu)
                .OnDelete(DeleteBehavior.Cascade);

            // ==================================================
            // MODULE 3
            // DANG KY THAM DU - SU KIEN
            // ==================================================

            modelBuilder.Entity<DangKyThamDu>()
                .HasOne(d => d.SuKien)
                .WithMany()
                .HasForeignKey(d => d.MaSuKien)
                .OnDelete(DeleteBehavior.Cascade);

            // ==================================================
            // MODULE 3
            // NGUOI THAM DU - DANG KY PHIEN
            // ==================================================

            modelBuilder.Entity<DangKyPhien>()
                .HasOne(d => d.NguoiThamDu)
                .WithMany()
                .HasForeignKey(d => d.MaNguoiThamDu)
                .OnDelete(DeleteBehavior.Cascade);

            // ==================================================
            // MODULE 3
            // DANG KY PHIEN - PHIEN SU KIEN
            // ==================================================

            modelBuilder.Entity<DangKyPhien>()
                .HasOne(d => d.Phien)
                .WithMany()
                .HasForeignKey(d => d.MaPhien)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}