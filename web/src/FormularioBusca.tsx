import { useState, type FormEvent } from 'react'
import type { BuscaRequest } from './api'

const IDIOMAS = ['Inglês', 'Espanhol', 'Francês']

const DIAS = [
  { valor: 'segunda', rotulo: 'Segunda' },
  { valor: 'terca', rotulo: 'Terça' },
  { valor: 'quarta', rotulo: 'Quarta' },
  { valor: 'quinta', rotulo: 'Quinta' },
  { valor: 'sexta', rotulo: 'Sexta' },
  { valor: 'sabado', rotulo: 'Sábado' },
  { valor: 'domingo', rotulo: 'Domingo' },
]

const DURACOES = [30, 60, 90]

type Props = {
  carregando: boolean
  onBuscar: (pedido: BuscaRequest) => void
}

export function FormularioBusca({ carregando, onBuscar }: Props) {
  const [idioma, setIdioma] = useState(IDIOMAS[0])
  const [dias, setDias] = useState<string[]>([])
  const [inicioFaixa, setInicioFaixa] = useState('18:00')
  const [fimFaixa, setFimFaixa] = useState('20:00')
  const [duracaoMinutos, setDuracaoMinutos] = useState(60)

  function alternarDia(dia: string) {
    setDias((atuais) =>
      atuais.includes(dia) ? atuais.filter((d) => d !== dia) : [...atuais, dia],
    )
  }

  function enviar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault()
    onBuscar({ idioma, dias, inicioFaixa, fimFaixa, duracaoMinutos })
  }

  return (
    <form className="formulario" onSubmit={enviar} noValidate>
      <label>
        Idioma
        <select value={idioma} onChange={(e) => setIdioma(e.target.value)}>
          {IDIOMAS.map((i) => (
            <option key={i} value={i}>
              {i}
            </option>
          ))}
        </select>
      </label>

      <fieldset>
        <legend>Dias da semana</legend>
        {DIAS.map((dia) => (
          <label key={dia.valor} className="opcao">
            <input
              type="checkbox"
              checked={dias.includes(dia.valor)}
              onChange={() => alternarDia(dia.valor)}
            />
            {dia.rotulo}
          </label>
        ))}
      </fieldset>

      <div className="linha">
        <label>
          Início da faixa
          <input type="time" value={inicioFaixa} onChange={(e) => setInicioFaixa(e.target.value)} />
        </label>
        <label>
          Fim da faixa
          <input type="time" value={fimFaixa} onChange={(e) => setFimFaixa(e.target.value)} />
        </label>
      </div>

      <label>
        Duração da aula
        <select
          value={duracaoMinutos}
          onChange={(e) => setDuracaoMinutos(Number(e.target.value))}
        >
          {DURACOES.map((d) => (
            <option key={d} value={d}>
              {d} minutos
            </option>
          ))}
        </select>
      </label>

      <button type="submit" disabled={carregando}>
        {carregando ? 'Buscando...' : 'Buscar professores'}
      </button>
    </form>
  )
}