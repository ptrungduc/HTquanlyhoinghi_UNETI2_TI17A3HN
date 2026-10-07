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
                .WithOne(l => l.SuKien)
                .HasForeignKey<SuKien>(s => s.MaLoaiSuKien)
                .IsRequired()
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
        }

    }
}