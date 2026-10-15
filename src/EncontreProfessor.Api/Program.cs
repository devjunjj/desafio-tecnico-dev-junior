using EncontreProfessor.Api;
using EncontreProfessor.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IProfessorRepository>(_ =>
    new ProfessorJsonRepository(Path.Combine(AppContext.BaseDirectory, "professores.json")));
builder.Services.AddSingleton<BuscaProfessores>();

builder.Services.AddCors(opcoes =>
    opcoes.AddDefaultPolicy(politica => politica
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

app.UseCors();

app.MapPost("/api/professores/busca", (
    RequisicaoBusca? requisicao,
    IProfessorRepository repositorio,
    BuscaProfessores busca) =>
{
    var validacao = ValidadorBusca.Validar(requisicao);
    if (!validacao.Valido)
        return Results.BadRequest(new { erros = validacao.Erros });

    var resultado = busca.Buscar(repositorio.ObterTodos(), validacao.Consulta!);
    return Results.Ok(resultado);
});

app.Run();

public partial class Program { }