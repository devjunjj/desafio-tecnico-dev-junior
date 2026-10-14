namespace EncontreProfessor.Core;

public sealed record RequisicaoBusca(
    string? Idioma,
    IReadOnlyList<string?>? Dias,
    string? InicioFaixa,
    string? FimFaixa,
    int? DuracaoMinutos);

public sealed record ConsultaBusca(
    string Idioma,
    IReadOnlyList<string> Dias,
    int InicioFaixaMinutos,
    int FimFaixaMinutos,
    int DuracaoMinutos);

public sealed record ResultadoValidacao(IReadOnlyList<string> Erros, ConsultaBusca? Consulta)
{
    public bool Valido => Erros.Count == 0;
}

public sealed record ProfessorDisponivel(int Id, string Nome, int AulasNaSemana, IReadOnlyList<string> Horarios);

public sealed record ProfessorNaoElegivel(int Id, string Nome, string Motivo);

public sealed record ResultadoBusca(
    IReadOnlyList<ProfessorDisponivel> Professores,
    IReadOnlyList<ProfessorNaoElegivel> NaoElegiveis);

public static class MotivosNaoElegivel
{
    public const string NaoEnsinaIdioma = "Não ensina o idioma";
    public const string AgendaBloqueada = "Agenda bloqueada";
    public const string SemHorarioEmComum = "Sem horário em comum nos dias pedidos";
}