import type { ProfessorDisponivel, ProfessorNaoElegivel, ResultadoApi } from './api'

type Props = {
  carregando: boolean
  resultado: ResultadoApi | null
}

export function Resultado({ carregando, resultado }: Props) {
  if (carregando) {
    return (
      <p role="status" className="mensagem">
        Buscando professores...
      </p>
    )
  }

  if (resultado === null) {
    return null
  }

  if (resultado.tipo === 'falha') {
    return (
      <p role="alert" className="mensagem erro">
        {resultado.mensagem}
      </p>
    )
  }

  if (resultado.tipo === 'validacao') {
    return (
      <div role="alert" className="mensagem erro">
        <p>Corrija os itens abaixo e tente novamente:</p>
        <ul>
          {resultado.erros.map((erro) => (
            <li key={erro}>{erro}</li>
          ))}
        </ul>
      </div>
    )
  }

  const { professores, naoElegiveis } = resultado.dados

  return (
    <section className="resultado">
      {professores.length === 0 ? (
        <p className="mensagem">Nenhum professor disponível para esse pedido.</p>
      ) : (
        <>
          <h2>Professores disponíveis</h2>
          <ul className="professores">
            {professores.map((professor) => (
              <CartaoProfessor key={professor.id} professor={professor} />
            ))}
          </ul>
        </>
      )}

      {naoElegiveis.length > 0 && <ListaNaoElegiveis professores={naoElegiveis} />}
    </section>
  )
}

function CartaoProfessor({ professor }: { professor: ProfessorDisponivel }) {
  const aulas = professor.aulasNaSemana

  return (
    <li className="cartao">
      <h3>{professor.nome}</h3>
      <p>
        {aulas} {aulas === 1 ? 'aula' : 'aulas'} na semana
      </p>
      <ul className="horarios">
        {professor.horarios.map((horario) => (
          <li key={horario}>{horario}</li>
        ))}
      </ul>
    </li>
  )
}

function ListaNaoElegiveis({ professores }: { professores: ProfessorNaoElegivel[] }) {
  return (
    <div className="nao-elegiveis">
      <h2>Professores que ficaram de fora</h2>
      <ul>
        {professores.map((professor) => (
          <li key={professor.id}>
            <strong>{professor.nome}</strong>: {professor.motivo}
          </li>
        ))}
      </ul>
    </div>
  )
}