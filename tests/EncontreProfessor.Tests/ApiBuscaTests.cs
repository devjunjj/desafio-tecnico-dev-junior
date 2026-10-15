using System.Net;
using System.Net.Http.Json;
using System.Text;
using EncontreProfessor.Core;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EncontreProfessor.Tests;

public class ApiBuscaTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Rota = "/api/professores/busca";
    private readonly HttpClient _cliente;

    public ApiBuscaTests(WebApplicationFactory<Program> fabrica)
    {
        _cliente = fabrica.CreateClient();
    }

    [Fact]
    public async Task Pedido_valido_responde_200_com_o_exemplo_do_enunciado()
    {
        var resposta = await _cliente.PostAsJsonAsync(Rota, new
        {
            idioma = "Espanhol",
            dias = new[] { "segunda" },
            inicioFaixa = "18:00",
            fimFaixa = "20:00",
            duracaoMinutos = 60,
        });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<ResultadoBusca>();
        Assert.NotNull(corpo);
        var professores = corpo!.Professores;

        Assert.Equal(new[] { "Ana", "Diego" }, professores.Select(p => p.Nome).ToArray());
        Assert.Equal(new[] { "18:00" }, professores[0].Horarios.ToArray());
        Assert.Equal(new[] { "18:00", "18:30", "19:00" }, professores[1].Horarios.ToArray());
        Assert.Contains(corpo.NaoElegiveis, n => n.Nome == "Bruno" && n.Motivo == "Não ensina o idioma");
    }

    [Fact]
    public async Task Pedido_invalido_responde_400_com_todos_os_erros()
    {
        var resposta = await _cliente.PostAsJsonAsync(Rota, new
        {
            idioma = "",
            dias = Array.Empty<string>(),
            inicioFaixa = "25:00",
            fimFaixa = "18:00",
            duracaoMinutos = 45,
        });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<RespostaErro>();
        Assert.NotNull(corpo);
        Assert.Equal(4, corpo!.Erros.Count);
        Assert.Contains("O idioma é obrigatório.", corpo.Erros);
    }

    [Fact]
    public async Task Corpo_vazio_responde_400_com_mensagem_clara()
    {
        var resposta = await _cliente.PostAsync(Rota, new StringContent("", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<RespostaErro>();
        Assert.Contains("O corpo da requisição é obrigatório.", corpo!.Erros);
    }

    [Fact]
    public async Task Json_malformado_responde_400()
    {
        var resposta = await _cliente.PostAsync(Rota, new StringContent("{\"idioma\": ", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Rota_de_busca_so_aceita_post()
    {
        var resposta = await _cliente.GetAsync(Rota);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, resposta.StatusCode);
    }

    [Fact]
    public async Task Cors_libera_apenas_a_origem_da_tela()
    {
        var daTela = await PreflightDe("http://localhost:5173");
        var deOutroSite = await PreflightDe("http://outro-site.com");

        Assert.True(daTela.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.False(deOutroSite.Headers.Contains("Access-Control-Allow-Origin"));
    }

    private Task<HttpResponseMessage> PreflightDe(string origem)
    {
        var pedido = new HttpRequestMessage(HttpMethod.Options, Rota);
        pedido.Headers.Add("Origin", origem);
        pedido.Headers.Add("Access-Control-Request-Method", "POST");
        return _cliente.SendAsync(pedido);
    }

    private sealed record RespostaErro(List<string> Erros);
}