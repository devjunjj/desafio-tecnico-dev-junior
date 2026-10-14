using System.Globalization;
using System.Text.RegularExpressions;

namespace EncontreProfessor.Core;

public static class Horario
{
    private static readonly Regex Formato = new(
        @"^([01][0-9]|2[0-3]):[0-5][0-9]\z",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static bool TryParse(string? texto, out int minutos)
    {
        minutos = 0;
        if (texto is null || !Formato.IsMatch(texto))
            return false;

        minutos = int.Parse(texto[..2], CultureInfo.InvariantCulture) * 60
                + int.Parse(texto[3..], CultureInfo.InvariantCulture);
        return true;
    }

    public static string Formatar(int minutos) =>
        $"{minutos / 60:00}:{minutos % 60:00}";
}