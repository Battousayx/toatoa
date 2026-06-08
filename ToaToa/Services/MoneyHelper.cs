using System.Globalization;
using System.Text;

namespace ToaToa.Services;

/// <summary>
/// Formatação de moeda em pt-BR (R$ 1.234,56) que funciona mesmo em invariant globalization mode
/// (sem depender de CultureInfo "pt-BR", que exige ICU/libicu).
/// </summary>
public static class MoneyHelper
{
    public static string Brl(this decimal valor)
    {
        var negativo = valor < 0;
        var abs = Math.Abs(valor);

        long inteiro = (long)decimal.Truncate(abs);
        int centavos = (int)decimal.Round((abs - inteiro) * 100m, 0, MidpointRounding.AwayFromZero);
        if (centavos >= 100) { inteiro += centavos / 100; centavos %= 100; }

        var intStr = inteiro.ToString(CultureInfo.InvariantCulture);
        var sb = new StringBuilder();
        int count = 0;
        for (int i = intStr.Length - 1; i >= 0; i--)
        {
            sb.Insert(0, intStr[i]);
            if (++count % 3 == 0 && i != 0)
                sb.Insert(0, '.');
        }

        return $"R$ {(negativo ? "-" : "")}{sb},{centavos:D2}";
    }

    public static string Brl(this decimal? valor) => (valor ?? 0m).Brl();
}
