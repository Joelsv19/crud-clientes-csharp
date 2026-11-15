using CrudClientesCSharp.Models;
using Microsoft.EntityFrameworkCore;

namespace CrudClientesCSharp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Provincia> Provincias => Set<Provincia>();
    public DbSet<Canton> Cantones => Set<Canton>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(cliente =>
        {
            cliente.Property(c => c.Cedula).HasMaxLength(10);
            cliente.Property(c => c.Telefono).HasMaxLength(20);
            cliente.Property(c => c.FechaNacimiento).HasColumnType("date").IsRequired();
            cliente.HasOne(c => c.Provincia).WithMany()
                .HasForeignKey(c => c.ProvinciaId).OnDelete(DeleteBehavior.Restrict);
            cliente.HasOne(c => c.Canton).WithMany()
                .HasForeignKey(c => new { c.CantonId, c.ProvinciaId })
                .HasPrincipalKey(c => new { c.Id, c.ProvinciaId })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Canton>().HasOne(c => c.Provincia).WithMany(p => p.Cantones)
            .HasForeignKey(c => c.ProvinciaId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Provincia>().HasData(
            new Provincia { Id = 1, Nombre = "Pichincha" },
            new Provincia { Id = 2, Nombre = "Guayas" },
            new Provincia { Id = 3, Nombre = "Tungurahua" },
            new Provincia { Id = 4, Nombre = "Azuay" });

        modelBuilder.Entity<Canton>().HasData(
            new Canton { Id = 1, Nombre = "Quito", ProvinciaId = 1 },
            new Canton { Id = 2, Nombre = "Rumiñahui", ProvinciaId = 1 },
            new Canton { Id = 3, Nombre = "Mejía", ProvinciaId = 1 },
            new Canton { Id = 4, Nombre = "Guayaquil", ProvinciaId = 2 },
            new Canton { Id = 5, Nombre = "Durán", ProvinciaId = 2 },
            new Canton { Id = 6, Nombre = "Daule", ProvinciaId = 2 },
            new Canton { Id = 7, Nombre = "Ambato", ProvinciaId = 3 },
            new Canton { Id = 8, Nombre = "Baños de Agua Santa", ProvinciaId = 3 },
            new Canton { Id = 9, Nombre = "Pelileo", ProvinciaId = 3 },
            new Canton { Id = 10, Nombre = "Cuenca", ProvinciaId = 4 },
            new Canton { Id = 11, Nombre = "Gualaceo", ProvinciaId = 4 },
            new Canton { Id = 12, Nombre = "Paute", ProvinciaId = 4 });
    }
}
