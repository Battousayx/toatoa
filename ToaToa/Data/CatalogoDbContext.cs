using Microsoft.EntityFrameworkCore;
using ToaToa.Domain;

namespace ToaToa.Data;

public class CatalogoDbContext(DbContextOptions<CatalogoDbContext> options) : DbContext(options)
{
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Vestido> Vestidos => Set<Vestido>();
    public DbSet<VarianteVestido> Variantes => Set<VarianteVestido>();
    public DbSet<FotoVestido> Fotos => Set<FotoVestido>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();
    public DbSet<Caixa> Caixas => Set<Caixa>();
    public DbSet<MovimentoCaixa> MovimentosCaixa => Set<MovimentoCaixa>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Categoria>(e =>
        {
            e.HasIndex(c => c.Slug).IsUnique();
        });

        builder.Entity<Vestido>(e =>
        {
            e.HasIndex(p => p.Slug).IsUnique();
            e.Property(p => p.Preco).HasColumnType("decimal(18,2)");
            e.HasOne(p => p.Categoria)
                .WithMany(c => c.Vestidos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<VarianteVestido>(e =>
        {
            e.HasOne(v => v.Vestido)
                .WithMany(p => p.Variantes)
                .HasForeignKey(v => v.VestidoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<FotoVestido>(e =>
        {
            e.HasOne(f => f.Vestido)
                .WithMany(p => p.Fotos)
                .HasForeignKey(f => f.VestidoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Venda>(e =>
        {
            e.Property(v => v.Total).HasColumnType("decimal(18,2)");
            e.HasOne(v => v.Caixa)
                .WithMany(c => c.Vendas)
                .HasForeignKey(v => v.CaixaId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ItemVenda>(e =>
        {
            e.Ignore(i => i.Subtotal);
            e.Property(i => i.PrecoUnitario).HasColumnType("decimal(18,2)");
            e.HasOne(i => i.Venda)
                .WithMany(v => v.Itens)
                .HasForeignKey(i => i.VendaId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.VarianteVestido)
                .WithMany()
                .HasForeignKey(i => i.VarianteVestidoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Caixa>(e =>
        {
            e.Property(c => c.ValorAbertura).HasColumnType("decimal(18,2)");
            e.Property(c => c.ValorContado).HasColumnType("decimal(18,2)");
            e.Property(c => c.Diferenca).HasColumnType("decimal(18,2)");
            // Garante no banco que só existe UM caixa aberto (Status = 0) por vez.
            e.HasIndex(c => c.Status).IsUnique().HasFilter("Status = 0");
        });

        builder.Entity<MovimentoCaixa>(e =>
        {
            e.Property(m => m.Valor).HasColumnType("decimal(18,2)");
            e.HasOne(m => m.Caixa)
                .WithMany(c => c.Movimentos)
                .HasForeignKey(m => m.CaixaId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
