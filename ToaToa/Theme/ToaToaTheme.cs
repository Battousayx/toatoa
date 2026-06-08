using MudBlazor;

namespace ToaToa.Theme;

/// <summary>
/// Tema da loja ToaToa — estética fashion premium minimalista (inspirada no tema DT Vogue):
/// paleta neutra preto/branco com acento rosé, títulos serifados (Playfair) via CSS.
/// </summary>
public static class ToaToaTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1A1A1A",
            Secondary = "#B76E79",      // rosé/gold accent
            Tertiary = "#8C7355",
            Background = "#FFFFFF",
            Surface = "#FFFFFF",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#1A1A1A",
            DrawerBackground = "#FAFAFA",
            TextPrimary = "#1A1A1A",
            TextSecondary = "#6B6B6B",
            ActionDefault = "#1A1A1A",
            Divider = "#ECECEC",
            GrayLight = "#F5F5F5"
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
            DefaultBorderRadius = "0px"   // visual fashion: cantos retos
        }
    };
}
