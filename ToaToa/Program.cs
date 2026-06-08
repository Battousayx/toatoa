using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Minio;
using MudBlazor.Services;
using ToaToa.Components;
using ToaToa.Components.Account;
using ToaToa.Data;
using ToaToa.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

var catalogoConnectionString = builder.Configuration.GetConnectionString("CatalogoConnection") ?? throw new InvalidOperationException("Connection string 'CatalogoConnection' not found.");
builder.Services.AddDbContextFactory<CatalogoDbContext>(options =>
    options.UseSqlite(catalogoConnectionString));

// Serviços de aplicação do catálogo
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IVestidoService, VestidoService>();
builder.Services.AddScoped<IVendaService, VendaService>();
builder.Services.AddScoped<ICaixaService, CaixaService>();

// MinIO (storage de fotos)
builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection(MinioOptions.SectionName));
builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var o = sp.GetRequiredService<IOptions<MinioOptions>>().Value;
    return new MinioClient()
        .WithEndpoint(o.Endpoint)
        .WithCredentials(o.AccessKey, o.SecretKey)
        .WithSSL(o.UseSSL)
        .Build();
});
builder.Services.AddScoped<IStorageService, StorageService>();

// Carrinho da loja (por circuito do usuário)
builder.Services.AddScoped<CartService>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Aplica migrations e popula dados iniciais (roles, admin, catálogo)
await DbToaToa.SeedAsync(app.Services);

// Garante o bucket do MinIO (best-effort; não derruba o app se o MinIO estiver offline)
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<IStorageService>().EnsureBucketAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ToaToa.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
