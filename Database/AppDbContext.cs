using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using Microsoft.EntityFrameworkCore;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<LoaiSuKien> LoaiSuKiens { get; set; }
        public DbSet<DiaDiem> DiaDiems { get; set; }
        public DbSet<SuKien> SuKiens { get; set; }
        public DbSet<PhienSuKien> PhienSuKiens { get; set; }
        public DbSet<NguoiThamDu> NguoiThamDus { get; set; }
        public DbSet<DangKyThamDu> DangKyThamDus { get; set; }
        public DbSet<CheckIn> CheckIns { get; set; }
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
        }

    }
}