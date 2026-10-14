import { useState } from 'react'
import { buscarProfessores, type ResultadoApi } from './api'

function App() {
  const [resultado, setResultado] = useState<ResultadoApi | null>(null)

  async function testar() {
    const resposta = await buscarProfessores({
      idioma: 'Espanhol',
      dias: ['segunda'],
      inicioFaixa: '18:00',
      fimFaixa: '20:00',
      duracaoMinutos: 60,
    })
    setResultado(resposta)
  }

  return (
    <main>
      <button onClick={testar}>Testar API</button>
      <pre>{JSON.stringify(resultado, null, 2)}</pre>
    </main>
  )
}

export default App