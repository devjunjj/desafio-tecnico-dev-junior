using EncontreProfessor.Core;
using Xunit;

namespace EncontreProfessor.Tests;

public class ValidadorBuscaTests
{
    private static RequisicaoBusca Requisicao(
        string? idioma = "Inglês",
        string?[]? dias = null,
        string? inicio = "18:00",
        string? fim = "20:00",
        int? duracao = 60) =>
        new(idioma, dias ?? new string?[] { "segunda" }, inicio, fim, duracao);

    [Fact]
    public void Requisicao_valida_vira_consulta_com_horarios_em_minutos()
    {
        var resultado = ValidadorBusca.Validar(Requisicao());

        Assert.True(resultado.Valido);
        var consulta = resultado.Consulta!;
        Assert.Equal("Inglês", consulta.Idioma);
        Assert.Equal(1080, consulta.InicioFaixaMinutos);
        Assert.Equal(1200, consulta.FimFaixaMinutos);
        Assert.Equal(60, consulta.DuracaoMinutos);
    }

    [Fact]
    public void Dias_sao_normalizados_e_repetidos_contam_uma_vez()
    {
        var resultado = ValidadorBusca.Validar(Requisicao(dias: new string?[] { "Terça", "terca", "SEGUNDA" }));

        Assert.True(resultado.Valido);
        Assert.Equal(new[] { "terca", "segunda" }, resultado.Consulta!.Dias.ToArray());
    }

    [Fact]
    public void Idioma_vazio_gera_erro()
    {
        var erros = ErrosDe(Requisicao(idioma: "   "));

        Assert.Contains("idioma", erros);
    }

    [Fact]
    public void Nenhum_dia_gera_erro()
    {
        Assert.Contains("ao menos um dia", ErrosDe(Requisicao(dias: Array.Empty<string?>())));
        Assert.Contains("ao menos um dia", ErrosDe(new RequisicaoBusca("Inglês", null, "18:00", "20:00", 60)));
    }

    [Fact]
    public void Dia_invalido_gera_erro_citando_o_dia()
    {
        Assert.Contains("domingao", ErrosDe(Requisicao(dias: new string?[] { "segunda", "domingao" })));
    }

    [Fact]
    public void Horario_fora_do_formato_gera_erro()
    {
        Assert.Contains("inicioFaixa", ErrosDe(Requisicao(inicio: "25:00")));
        Assert.Contains("fimFaixa", ErrosDe(Requisicao(fim: "20h")));
        Assert.Contains("inicioFaixa", ErrosDe(Requisicao(inicio: null)));
    }

    [Fact]
    public void Fim_igual_ou_anterior_ao_inicio_gera_erro()
    {
        Assert.Contains("posterior", ErrosDe(Requisicao(inicio: "20:00", fim: "20:00")));
        Assert.Contains("posterior", ErrosDe(Requisicao(inicio: "20:00", fim: "18:00")));
    }

    [Fact]
    public void Duracao_diferente_de_30_60_ou_90_gera_erro()
    {
        Assert.Contains("30, 60 ou 90", ErrosDe(Requisicao(duracao: 45)));
        Assert.Contains("30, 60 ou 90", ErrosDe(Requisicao(duracao: null)));
    }

    [Fact]
    public void Reune_todos_os_erros_de_uma_vez()
    {
        var resultado = ValidadorBusca.Validar(Requisicao(idioma: "", dias: Array.Empty<string?>(), duracao: 45));

        Assert.False(resultado.Valido);
        Assert.Equal(3, resultado.Erros.Count);
        Assert.True(resultado.Consulta is null);
    }

    private static string ErrosDe(RequisicaoBusca requisicao)
    {
        var resultado = ValidadorBusca.Validar(requisicao);
        Assert.False(resultado.Valido);
        return string.Join(" | ", resultado.Erros);
    }
}