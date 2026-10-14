using System.Text.Json;
using EncontreProfessor.Core;

namespace EncontreProfessor.Api;

public sealed class ProfessorJsonRepository : IProfessorRepository
{
    private readonly IReadOnlyList<Professor> _professores;

    public ProfessorJsonRepository(string caminhoArquivo)
    {
        var json = File.ReadAllText(caminhoArquivo);
        var opcoes = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var arquivo = JsonSerializer.Deserialize<ArquivoProfessores>(json, opcoes)
            ?? throw new InvalidOperationException("professores.json está vazio ou inválido.");

        _professores = arquivo.Professores;
    }

    public IReadOnlyList<Professor> ObterTodos() => _professores;
}

internal sealed record ArquivoProfessores(List<Professor> Professores);