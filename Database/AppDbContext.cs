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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoaiSuKien>()
                .HasIndex(l => l.TenLoaiSuKien)
                .IsUnique();
            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(t => t.TenDangNhap)
                .IsUnique();
        }
        
    }
}