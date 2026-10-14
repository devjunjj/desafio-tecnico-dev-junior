namespace EncontreProfessor.Core;

public sealed class BuscaProfessores
{
    public const int PassoMinutos = 30;

    public ResultadoBusca Buscar(IEnumerable<Professor> professores, ConsultaBusca consulta)
    {
        var disponiveis = new List<ProfessorDisponivel>();
        var naoElegiveis = new List<ProfessorNaoElegivel>();
        var idiomaPedido = Texto.Normalizar(consulta.Idioma);

        foreach (var professor in professores)
        {
            if (!professor.Idiomas.Any(i => Texto.Normalizar(i) == idiomaPedido))
            {
                naoElegiveis.Add(new(professor.Id, professor.Nome, MotivosNaoElegivel.NaoEnsinaIdioma));
                continue;
            }

            if (professor.AgendaBloqueada)
            {
                naoElegiveis.Add(new(professor.Id, professor.Nome, MotivosNaoElegivel.AgendaBloqueada));
                continue;
            }

            var inicios = IniciosEmComum(professor, consulta);
            if (inicios.Count == 0)
            {
                naoElegiveis.Add(new(professor.Id, professor.Nome, MotivosNaoElegivel.SemHorarioEmComum));
                continue;
            }

            disponiveis.Add(new(
                professor.Id,
                professor.Nome,
                professor.AulasMarcadas.Count,
                inicios.Select(Horario.Formatar).ToList()));
        }

        var ordenados = disponiveis
            .OrderBy(p => p.AulasNaSemana)
            .ThenBy(p => Texto.Normalizar(p.Nome), StringComparer.Ordinal)
            .ThenBy(p => p.Nome, StringComparer.Ordinal)
            .ThenBy(p => p.Id)
            .ToList();

        return new ResultadoBusca(ordenados, naoElegiveis);
    }

    private static List<int> IniciosEmComum(Professor professor, ConsultaBusca consulta)
    {
        HashSet<int>? emComum = null;

        foreach (var dia in consulta.Dias)
        {
            var doDia = IniciosValidosNoDia(professor, dia, consulta);

            if (emComum is null)
                emComum = new HashSet<int>(doDia);
            else
                emComum.IntersectWith(doDia);

            if (emComum.Count == 0)
                return new List<int>();
        }

        return emComum is null ? new List<int>() : emComum.OrderBy(m => m).ToList();
    }

    private static List<int> IniciosValidosNoDia(Professor professor, string dia, ConsultaBusca consulta)
    {
        var blocos = professor.Disponibilidade
            .Where(b => Texto.Normalizar(b.Dia) == dia)
            .Select(b => (Ok: Intervalo(b.Inicio, b.Fim, out var ini, out var fim), Ini: ini, Fim: fim))
            .Where(x => x.Ok)
            .ToList();

        var aulas = professor.AulasMarcadas
            .Where(a => Texto.Normalizar(a.Dia) == dia && Horario.TryParse(a.Inicio, out _))
            .Select(a =>
            {
                Horario.TryParse(a.Inicio, out var ini);
                return (Ini: ini, Fim: ini + a.DuracaoMinutos);
            })
            .ToList();

        var validos = new List<int>();

        var primeiro = (consulta.InicioFaixaMinutos + PassoMinutos - 1) / PassoMinutos * PassoMinutos;

        for (var inicio = primeiro; inicio + consulta.DuracaoMinutos <= consulta.FimFaixaMinutos; inicio += PassoMinutos)
        {
            var fim = inicio + consulta.DuracaoMinutos;

            var cabeNoBloco = blocos.Any(b => b.Ini <= inicio && fim <= b.Fim);
            if (!cabeNoBloco)
                continue;

            var sobrepoe = aulas.Any(a => inicio < a.Fim && a.Ini < fim);
            if (sobrepoe)
                continue;

            validos.Add(inicio);
        }

        return validos;
    }

    private static bool Intervalo(string inicio, string fim, out int ini, out int f)
    {
        f = 0;
        return Horario.TryParse(inicio, out ini) & Horario.TryParse(fim, out f);
    }
}