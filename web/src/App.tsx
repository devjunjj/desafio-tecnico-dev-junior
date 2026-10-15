import { useState } from 'react'
import { buscarProfessores, type BuscaRequest, type ResultadoApi } from './api'
import { FormularioBusca } from './FormularioBusca'
import { Resultado } from './Resultado'

function App() {
  const [carregando, setCarregando] = useState(false)
  const [resultado, setResultado] = useState<ResultadoApi | null>(null)

  async function buscar(pedido: BuscaRequest) {
    setCarregando(true)
    try {
      setResultado(await buscarProfessores(pedido))
    } finally {
      setCarregando(false)
    }
  }

  return (
    <main className="pagina">
      <header className="cabecalho">
        <h1>Encontre um professor</h1>
        <p>Informe o idioma, os dias e o horário do aluno para ver quem pode dar a aula.</p>
      </header>

      <div className="conteudo">
        <FormularioBusca carregando={carregando} onBuscar={buscar} />
        <div className="resultados" aria-live="polite">
          <Resultado carregando={carregando} resultado={resultado} />
        </div>
      </div>
    </main>
  )
}

export default App