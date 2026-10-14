using EncontreProfessor.Core;
using Xunit;

namespace EncontreProfessor.Tests;

public class BuscaProfessoresTests
{
    private readonly BuscaProfessores _busca = new();

    private static int Min(string horario)
    {
        Horario.TryParse(horario, out var minutos);
        return minutos;
    }

    private static ConsultaBusca Consulta(string idioma, string[] dias, string inicio, string fim, int duracao) =>
        new(idioma, dias, Min(inicio), Min(fim), duracao);

    private static Professor Prof(
        int id, string nome, string[] idiomas, BlocoDisponibilidade[] disponibilidade,
        AulaMarcada[]? aulas = null, bool bloqueada = false) =>
        new(id, nome, idiomas, bloqueada, disponibilidade, aulas ?? Array.Empty<AulaMarcada>());


    [Fact]
    public void Aula_pode_comecar_exatamente_quando_outra_termina()
    {
        var professor = Prof(1, "Ana", new[] { "Inglês" },
            new[] { new BlocoDisponibilidade("segunda", "18:00", "22:00") },
            new[] { new AulaMarcada("segunda", "18:00", 60) });

        var resultado = _busca.Buscar(new[] { professor }, Consulta("Inglês", new[] { "segunda" }, "19:00", "20:00", 60));

        Assert.Equal(new[] { "19:00" }, Assert.Single(resultado.Professores).Horarios.ToArray());
    }



    [Fact]
    public void Aula_que_apenas_encosta_em_outra_e_permitida()
    {
        var ana = Prof(1, "Ana", new[] { "Espanhol" },
            new[] { new BlocoDisponibilidade("segunda", "18:00", "22:00") },
            new[] { new AulaMarcada("segunda", "19:00", 60) });

        var resultado = _busca.Buscar(new[] { ana }, Consulta("Espanhol", new[] { "segunda" }, "18:00", "20:00", 60));

        var encontrada = Assert.Single(resultado.Professores);
        Assert.Equal(new[] { "18:00" }, encontrada.Horarios.ToArray());
    }

    [Fact]
    public void Gera_horarios_de_meia_em_meia_hora_em_ordem_crescente()
    {
        var diego = Prof(4, "Diego", new[] { "Espanhol" },
            new[] { new BlocoDisponibilidade("segunda", "18:00", "22:00") });

        var resultado = _busca.Buscar(new[] { diego }, Consulta("Espanhol", new[] { "segunda" }, "18:00", "20:00", 60));

        Assert.Equal(new[] { "18:00", "18:30", "19:00" }, Assert.Single(resultado.Professores).Horarios.ToArray());
    }

    [Fact]
    public void Horario_fixo_exige_o_mesmo_inicio_em_todos_os_dias()
    {
        var professor = Prof(1, "Ana", new[] { "Inglês" }, new[]
        {
            new BlocoDisponibilidade("segunda", "18:00", "20:00"),
            new BlocoDisponibilidade("quarta", "19:00", "20:00"),
        });

        var resultado = _busca.Buscar(new[] { professor },
            Consulta("Inglês", new[] { "segunda", "quarta" }, "18:00", "20:00", 60));

        Assert.Equal(new[] { "19:00" }, Assert.Single(resultado.Professores).Horarios.ToArray());
    }

    [Fact]
    public void Aula_precisa_caber_inteira_em_um_unico_bloco_de_disponibilidade()
    {
        var bruno = Prof(2, "Bruno", new[] { "Inglês" }, new[]
        {
            new BlocoDisponibilidade("quarta", "18:00", "19:00"),
            new BlocoDisponibilidade("quarta", "20:00", "22:00"),
        });

        var resultado = _busca.Buscar(new[] { bruno }, Consulta("Inglês", new[] { "quarta" }, "18:00", "22:00", 60));

        Assert.Equal(new[] { "18:00", "20:00", "20:30", "21:00" }, Assert.Single(resultado.Professores).Horarios.ToArray());
    }

    [Fact]
    public void Faixa_que_comeca_fora_da_grade_so_aceita_inicio_em_hora_cheia_ou_meia_hora()
    {
        var professor = Prof(1, "Ana", new[] { "Inglês" },
            new[] { new BlocoDisponibilidade("segunda", "18:00", "22:00") });

        var resultado = _busca.Buscar(new[] { professor }, Consulta("Inglês", new[] { "segunda" }, "18:15", "19:45", 60));

        Assert.Equal(new[] { "18:30" }, Assert.Single(resultado.Professores).Horarios.ToArray());
    }

    [Fact]
    public void Idioma_e_comparado_sem_diferenciar_acento_nem_maiuscula()
    {
        var professor = Prof(1, "Ana", new[] { "Inglês" },
            new[] { new BlocoDisponibilidade("segunda", "18:00", "22:00") });

        var resultado = _busca.Buscar(new[] { professor }, Consulta("INGLES", new[] { "segunda" }, "18:00", "19:00", 60));

        Assert.Single(resultado.Professores);
    }

    [Fact]
    public void Ordena_por_menos_aulas_e_desempata_pelo_nome()
    {
        var disponibilidade = new[] { new BlocoDisponibilidade("segunda", "18:00", "22:00") };
        var duasAulas = new[] { new AulaMarcada("sexta", "10:00", 60), new AulaMarcada("sexta", "11:00", 60) };

        var zeca = Prof(1, "Zeca", new[] { "Inglês" }, disponibilidade);
        var ana = Prof(2, "Ana", new[] { "Inglês" }, disponibilidade, duasAulas);
        var bia = Prof(3, "Bia", new[] { "Inglês" }, disponibilidade);

        var resultado = _busca.Buscar(new[] { zeca, ana, bia }, Consulta("Inglês", new[] { "segunda" }, "18:00", "19:00", 60));

        Assert.Equal(new[] { "Bia", "Zeca", "Ana" }, resultado.Professores.Select(p => p.Nome).ToArray());
        Assert.Equal(new[] { 0, 0, 2 }, resultado.Professores.Select(p => p.AulasNaSemana).ToArray());
    }

    [Fact]
    public void Informa_o_primeiro_motivo_na_ordem_de_prioridade_do_enunciado()
    {
        var disponibilidade = new[] { new BlocoDisponibilidade("segunda", "18:00", "22:00") };

        var outroIdiomaEBloqueado = Prof(1, "Carlos", new[] { "Francês" }, disponibilidade, bloqueada: true);
        var soBloqueado = Prof(2, "Carla", new[] { "Inglês" }, disponibilidade, bloqueada: true);
        var semHorario = Prof(3, "Cesar", new[] { "Inglês" },
            new[] { new BlocoDisponibilidade("terca", "18:00", "22:00") });

        var resultado = _busca.Buscar(new[] { outroIdiomaEBloqueado, soBloqueado, semHorario },
            Consulta("Inglês", new[] { "segunda" }, "18:00", "20:00", 60));

        Assert.Empty(resultado.Professores);
        Assert.Equal(
            new[] { "Não ensina o idioma", "Agenda bloqueada", "Sem horário em comum nos dias pedidos" },
            resultado.NaoElegiveis.Select(n => n.Motivo).ToArray());
    }
}