using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ToaToa.Domain;

namespace ToaToa.Data;

public static class DbToaToa
{
    public const string AdminRole = "Admin";
    public const string AdminEmail = "admin@toatoa.local";
    public const string AdminPassword = "Admin@123";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        // Aplica migrations do catálogo
        var catalogo = sp.GetRequiredService<CatalogoDbContext>();
        await catalogo.Database.MigrateAsync();

        // Garante schema do Identity
        var identity = sp.GetRequiredService<ApplicationDbContext>();
        await identity.Database.MigrateAsync();

        await SeedAdminAsync(sp);
        await SeedCatalogoAsync(catalogo);
    }

    private static async Task SeedAdminAsync(IServiceProvider sp)
    {
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync(AdminRole))
            await roleManager.CreateAsync(new IdentityRole(AdminRole));

        var admin = await userManager.FindByEmailAsync(AdminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, AdminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, AdminRole);
        }
        else if (!await userManager.IsInRoleAsync(admin, AdminRole))
        {
            await userManager.AddToRoleAsync(admin, AdminRole);
        }
    }

    private static async Task SeedCatalogoAsync(CatalogoDbContext db)
    {
        if (await db.Categorias.AnyAsync())
            return;

        var festa = new Categoria { Nome = "Festa", Slug = "festa" };
        var casual = new Categoria { Nome = "Casual", Slug = "casual" };
        var praia = new Categoria { Nome = "Praia", Slug = "praia" };
        db.Categorias.AddRange(festa, casual, praia);

        db.Vestidos.AddRange(
            new Vestido
            {
                Nome = "Vestido Longo Floral",
                Slug = "vestido-longo-floral",
                Descricao = "Vestido longo estampado, tecido leve e fluido.",
                Preco = 199.90m,
                Categoria = casual,
                ImagemUrl = "https://picsum.photos/seed/floral/600/800",
                Variantes =
                {
                    new VarianteVestido { Tamanho = "P", Cor = "Azul", EstoqueQtd = 5, Sku = "FLOR-P-AZ" },
                    new VarianteVestido { Tamanho = "M", Cor = "Azul", EstoqueQtd = 8, Sku = "FLOR-M-AZ" },
                    new VarianteVestido { Tamanho = "G", Cor = "Vermelho", EstoqueQtd = 3, Sku = "FLOR-G-VM" }
                }
            },
            new Vestido
            {
                Nome = "Vestido de Festa Sereia",
                Slug = "vestido-festa-sereia",
                Descricao = "Modelo sereia com brilho, ideal para festas.",
                Preco = 459.90m,
                Categoria = festa,
                ImagemUrl = "https://picsum.photos/seed/sereia/600/800",
                Variantes =
                {
                    new VarianteVestido { Tamanho = "M", Cor = "Preto", EstoqueQtd = 4, Sku = "SER-M-PT" },
                    new VarianteVestido { Tamanho = "G", Cor = "Dourado", EstoqueQtd = 2, Sku = "SER-G-DR" }
                }
            },
            new Vestido
            {
                Nome = "Vestido Chemise Praia",
                Slug = "vestido-chemise-praia",
                Descricao = "Leve e arejado, perfeito para o verão.",
                Preco = 129.90m,
                Categoria = praia,
                ImagemUrl = "https://picsum.photos/seed/praia/600/800",
                Variantes =
                {
                    new VarianteVestido { Tamanho = "P", Cor = "Branco", EstoqueQtd = 10, Sku = "CHE-P-BR" },
                    new VarianteVestido { Tamanho = "M", Cor = "Branco", EstoqueQtd = 7, Sku = "CHE-M-BR" }
                }
            }
        );

        await db.SaveChangesAsync();
    }
}
