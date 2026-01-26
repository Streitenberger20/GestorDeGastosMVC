using Microsoft.EntityFrameworkCore;
using GestorDeGastos.Models;

namespace GestorDeGastos.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<Rubro> Rubros { get; set; }
        public DbSet<Detalle> Detalles { get; set; }
        public DbSet<RolRubro> RolRubros { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // RolRubro
            modelBuilder.Entity<RolRubro>()
                .HasKey(rr => new { rr.RolId, rr.RubroId });

            modelBuilder.Entity<RolRubro>()
                .HasOne(rr => rr.Rol)
                .WithMany(r => r.RolRubros)
                .HasForeignKey(rr => rr.RolId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolRubro>()
                .HasOne(rr => rr.Rubro)
                .WithMany(r => r.RolRubros)
                .HasForeignKey(rr => rr.RubroId)
                .OnDelete(DeleteBehavior.Cascade);

            // Rubro → Descripciones
            modelBuilder.Entity<Rubro>()
                .HasMany(r => r.Detalles)
                .WithOne(d => d.Rubro)
                .HasForeignKey(d => d.RubroId)
                .OnDelete(DeleteBehavior.Restrict);

            // Usuario → Gastos
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Gastos)
                .WithOne(g => g.Usuario)
                .HasForeignKey(g => g.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Gasto → Rubro
            modelBuilder.Entity<Gasto>()
                .HasOne(g => g.Rubro)
                .WithMany(r => r.Gastos) 
                .HasForeignKey(g => g.RubroId)
                .OnDelete(DeleteBehavior.Restrict);

            // Gasto → Descripcion
            modelBuilder.Entity<Gasto>()
                .HasOne(g => g.Detalle)
                .WithMany()
                .HasForeignKey(g => g.DetalleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Precisión del campo decimal
            modelBuilder.Entity<Gasto>()
                .Property(g => g.Importe)
                .HasPrecision(18, 2);

            // Longitud de Moneda (opcional)
            modelBuilder.Entity<Gasto>()
                .Property(g => g.Moneda)
                .HasMaxLength(3);
        }
    }
}
