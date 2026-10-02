using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // === DbSet chính (Module 1, 2) ===
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<LoaiSuKien> LoaiSuKiens { get; set; }
        public DbSet<DiaDiem> DiaDiems { get; set; }
        public DbSet<SuKien> SuKiens { get; set; }
        public DbSet<PhienSuKien> PhienSuKiens { get; set; }

        // === DbSet Module 3, 4, 5 ===
        public DbSet<NguoiThamDu> NguoiThamDus { get; set; }
        public DbSet<DangKyThamDu> DangKyThamDus { get; set; }
        public DbSet<DangKyPhien> DangKyPhiens { get; set; }
        public DbSet<CheckIn> CheckIns { get; set; }

        // === Alias không "s" - Module 3 sử dụng tên này ===
        public DbSet<NguoiThamDu> NguoiThamDu => NguoiThamDus;
        public DbSet<DangKyThamDu> DangKyThamDu => DangKyThamDus;
        public DbSet<DangKyPhien> DangKyPhien => DangKyPhiens;
        public DbSet<DiaDiem> DiaDiem => DiaDiems;
        public DbSet<SuKien> SuKien => SuKiens;
        public DbSet<PhienSuKien> PhienSuKien => PhienSuKiens;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoaiSuKien>()
                .HasIndex(l => l.TenLoaiSuKien)
                .IsUnique();
            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(t => t.TenDangNhap)
                .IsUnique();

            modelBuilder.Entity<SuKien>()
                .HasOne(s => s.LoaiSuKien)
                .WithMany()
                .HasForeignKey(s => s.MaLoaiSuKien)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SuKien>()
                .HasOne(s => s.DiaDiem)
                .WithMany()
                .HasForeignKey(s => s.MaDiaDiem)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PhienSuKien>()
                .HasOne(p => p.SuKien)
                .WithMany(s => s.PhienSuKiens)
                .HasForeignKey(p => p.MaSuKien)
                .OnDelete(DeleteBehavior.Cascade);

            // === Quan hệ cho NguoiThamDu, DangKyThamDu, CheckIn ===

            // TaiKhoan (1) - (0..1) NguoiThamDu
            modelBuilder.Entity<NguoiThamDu>()
                .HasOne(n => n.TaiKhoan)
                .WithMany()
                .HasForeignKey(n => n.MaTaiKhoan)
                .OnDelete(DeleteBehavior.Restrict);

            // NguoiThamDu (1) - (n) DangKyThamDu
            modelBuilder.Entity<DangKyThamDu>()
                .HasOne(d => d.NguoiThamDu)
                .WithMany(n => n.DangKyThamDus)
                .HasForeignKey(d => d.MaNguoiThamDu)
                .OnDelete(DeleteBehavior.Restrict);

            // SuKien (1) - (n) DangKyThamDu
            modelBuilder.Entity<DangKyThamDu>()
                .HasOne(d => d.SuKien)
                .WithMany()
                .HasForeignKey(d => d.MaSuKien)
                .OnDelete(DeleteBehavior.Restrict);

            // DangKyThamDu (1) - (0..1) CheckIn
            modelBuilder.Entity<CheckIn>()
                .HasOne(c => c.DangKyThamDu)
                .WithOne(d => d.CheckIn)
                .HasForeignKey<CheckIn>(c => c.MaDangKy)
                .OnDelete(DeleteBehavior.Restrict);

            // === Quan hệ cho DangKyPhien ===

            // NguoiThamDu (1) - (n) DangKyPhien
            modelBuilder.Entity<DangKyPhien>()
                .HasOne(dp => dp.NguoiThamDu)
                .WithMany()
                .HasForeignKey(dp => dp.MaNguoiThamDu)
                .OnDelete(DeleteBehavior.Restrict);

            // PhienSuKien (1) - (n) DangKyPhien
            modelBuilder.Entity<DangKyPhien>()
                .HasOne(dp => dp.Phien)
                .WithMany()
                .HasForeignKey(dp => dp.MaPhien)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}