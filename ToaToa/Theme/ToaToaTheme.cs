using MudBlazor;

namespace ToaToa.Theme;

/// <summary>
/// Tema da loja ToaToa — derivado da logo "Tôa Tôa Moda Festa" (verde + dourado + vermelho),
/// em versões suaves e sofisticadas: verde sálvia profundo, dourado discreto, terracota,
/// sobre fundo creme. Títulos serifados (Playfair) via CSS.
/// </summary>
public static class ToaToaTheme
{
    // Tokens reutilizados em estilos inline (footer, hero, etc.)
    public const string Verde = "#3E5E50";        // verde sálvia profundo (primária)
    public const string VerdeEscuro = "#2C4339";  // verde garrafa (footer)
    public const string Dourado = "#BFA15A";      // dourado suave (acento)
    public const string Terracota = "#BC5B4D";    // vermelho/terracota suave
    public const string Creme = "#FAF8F3";        // fundo quente

    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = Verde,
            Secondary = Dourado,
            Tertiary = Terracota,
            Background = Creme,
            BackgroundGray = "#F1ECE2",
            Surface = "#FFFFFF",
            AppbarBackground = Creme,
            AppbarText = VerdeEscuro,
            DrawerBackground = "#F3EFE7",
            DrawerText = "#2A2A2A",
            DrawerIcon = Verde,
            TextPrimary = "#2A2A2A",
            TextSecondary = "#6F6A60",
            ActionDefault = Verde,
            Divider = "#E7E1D6",
            DividerLight = "#EFE9DE",
            GrayLight = "#F1ECE2",
            GrayLighter = "#F7F3EC"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = new[] { "Jost", "Helvetica", "Arial", "sans-serif" }
            }
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "2px"
        }
    };
}
