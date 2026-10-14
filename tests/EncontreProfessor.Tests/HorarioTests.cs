using EncontreProfessor.Core;
using Xunit;

namespace EncontreProfessor.Tests;

public class HorarioTests
{
    [Fact]
    public void Converte_horario_valido_em_minutos()
    {
        Assert.True(Horario.TryParse("18:30", out var minutos));
        Assert.Equal(1110, minutos);
    }

    [Fact]
    public void Aceita_os_limites_do_dia()
    {
        Assert.True(Horario.TryParse("00:00", out var inicioDoDia));
        Assert.Equal(0, inicioDoDia);

        Assert.True(Horario.TryParse("23:59", out var fimDoDia));
        Assert.Equal(1439, fimDoDia);
    }

    [Fact]
    public void Recusa_formatos_invalidos()
    {
        Assert.False(Horario.TryParse(null, out _));
        Assert.False(Horario.TryParse("", out _));
        Assert.False(Horario.TryParse("abc", out _));
        Assert.False(Horario.TryParse("9:00", out _));
        Assert.False(Horario.TryParse("18h", out _));
        Assert.False(Horario.TryParse("24:00", out _));
        Assert.False(Horario.TryParse("18:60", out _));
        Assert.False(Horario.TryParse("18:00\n", out _));
    }

    [Fact]
    public void Formata_minutos_sempre_com_dois_digitos()
    {
        Assert.Equal("09:05", Horario.Formatar(545));
        Assert.Equal("18:30", Horario.Formatar(1110));
        Assert.Equal("00:00", Horario.Formatar(0));
    }
}