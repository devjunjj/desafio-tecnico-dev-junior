import { Resultado } from './Resultado'
import { useState } from 'react'
import { buscarProfessores, type BuscaRequest, type ResultadoApi } from './api'
import { FormularioBusca } from './FormularioBusca'

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
    <main>
      <h1>Encontre um professor</h1>
      <FormularioBusca carregando={carregando} onBuscar={buscar} />
      <Resultado carregando={carregando} resultado={resultado} />
    </main>
  )
}

export default App