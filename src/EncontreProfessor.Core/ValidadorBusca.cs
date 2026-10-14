namespace EncontreProfessor.Core;

public static class ValidadorBusca
{
    public static readonly IReadOnlyList<string> DiasValidos =
        new[] { "segunda", "terca", "quarta", "quinta", "sexta", "sabado", "domingo" };

    public static ResultadoValidacao Validar(RequisicaoBusca? requisicao)
    {
        if (requisicao is null)
            return new ResultadoValidacao(new[] { "O corpo da requisição é obrigatório." }, null);

        var erros = new List<string>();

        var idioma = requisicao.Idioma?.Trim() ?? string.Empty;
        if (idioma.Length == 0)
            erros.Add("O idioma é obrigatório.");

        var dias = new List<string>();
        if (requisicao.Dias is null || requisicao.Dias.Count == 0)
        {
            erros.Add("Informe ao menos um dia da semana.");
        }
        else
        {
            var invalidos = new List<string>();
            foreach (var dia in requisicao.Dias)
            {
                var normalizado = Texto.Normalizar(dia);
                if (DiasValidos.Contains(normalizado))
                {
                    if (!dias.Contains(normalizado))
                        dias.Add(normalizado);
                }
                else
                {
                    invalidos.Add(string.IsNullOrWhiteSpace(dia) ? "(vazio)" : dia);
                }
            }

            if (invalidos.Count > 0)
                erros.Add($"Dia(s) inválido(s): {string.Join(", ", invalidos)}. " +
                          $"Use: {string.Join(", ", DiasValidos)}.");
        }

        var inicioOk = Horario.TryParse(requisicao.InicioFaixa, out var inicio);
        if (!inicioOk)
            erros.Add("inicioFaixa deve estar no formato HH:mm (ex.: 18:00).");

        var fimOk = Horario.TryParse(requisicao.FimFaixa, out var fim);
        if (!fimOk)
            erros.Add("fimFaixa deve estar no formato HH:mm (ex.: 20:00).");

        if (inicioOk && fimOk && fim <= inicio)
            erros.Add("O fim da faixa deve ser posterior ao início.");

        if (requisicao.DuracaoMinutos is not (30 or 60 or 90))
            erros.Add("A duração deve ser de 30, 60 ou 90 minutos.");

        if (erros.Count > 0)
            return new ResultadoValidacao(erros, null);

        var consulta = new ConsultaBusca(idioma, dias, inicio, fim, requisicao.DuracaoMinutos!.Value);
        return new ResultadoValidacao(erros, consulta);
    }
}