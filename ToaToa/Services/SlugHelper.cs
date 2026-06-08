using System.Text;

namespace ToaToa.Services;

public static class SlugHelper
{
    public static string Gerar(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        var lower = texto.Trim().ToLowerInvariant();
        var sb = new StringBuilder(lower.Length);
        char anterior = '-';
        foreach (var c in lower)
        {
            char mapped = c switch
            {
                'á' or 'à' or 'â' or 'ã' or 'ä' => 'a',
                'é' or 'è' or 'ê' or 'ë' => 'e',
                'í' or 'ì' or 'î' or 'ï' => 'i',
                'ó' or 'ò' or 'ô' or 'õ' or 'ö' => 'o',
                'ú' or 'ù' or 'û' or 'ü' => 'u',
                'ç' => 'c',
                _ => c
            };

            if ((mapped >= 'a' && mapped <= 'z') || (mapped >= '0' && mapped <= '9'))
            {
                sb.Append(mapped);
                anterior = mapped;
            }
            else if (anterior != '-')
            {
                sb.Append('-');
                anterior = '-';
            }
        }

        return sb.ToString().Trim('-');
    }
}
