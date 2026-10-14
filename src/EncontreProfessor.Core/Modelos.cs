namespace EncontreProfessor.Core;

public sealed record BlocoDisponibilidade(string Dia, string Inicio, string Fim);

public sealed record AulaMarcada(string Dia, string Inicio, int DuracaoMinutos);

public sealed record Professor(
    int Id,
    string Nome,
    IReadOnlyList<string> Idiomas,
    bool AgendaBloqueada,
    IReadOnlyList<BlocoDisponibilidade> Disponibilidade,
    IReadOnlyList<AulaMarcada> AulasMarcadas);