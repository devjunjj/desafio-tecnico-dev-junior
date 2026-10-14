export type BuscaRequest = {
  idioma: string
  dias: string[]
  inicioFaixa: string
  fimFaixa: string
  duracaoMinutos: number
}

export type ProfessorDisponivel = {
  id: number
  nome: string
  aulasNaSemana: number
  horarios: string[]
}

export type ProfessorNaoElegivel = {
  id: number
  nome: string
  motivo: string
}

export type BuscaResponse = {
  professores: ProfessorDisponivel[]
  naoElegiveis: ProfessorNaoElegivel[]
}

export type ResultadoApi =
  | { tipo: 'sucesso'; dados: BuscaResponse }
  | { tipo: 'validacao'; erros: string[] }
  | { tipo: 'falha'; mensagem: string }

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000'

async function lerJson(resposta: Response): Promise<unknown> {
  try {
    return await resposta.json()
  } catch {
    return null
  }
}

function temErros(corpo: unknown): corpo is { erros: string[] } {
  return (
    typeof corpo === 'object' &&
    corpo !== null &&
    'erros' in corpo &&
    Array.isArray((corpo as { erros: unknown }).erros)
  )
}

export async function buscarProfessores(pedido: BuscaRequest): Promise<ResultadoApi> {
  let resposta: Response

  try {
    resposta = await fetch(`${API_URL}/api/professores/busca`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(pedido),
    })
  } catch {
    return {
      tipo: 'falha',
      mensagem: 'Não foi possível se comunicar com a API. Verifique se ela está rodando.',
    }
  }

  const corpo = await lerJson(resposta)

  if (resposta.ok) {
    return { tipo: 'sucesso', dados: corpo as BuscaResponse }
  }

  if (resposta.status === 400 && temErros(corpo)) {
    return { tipo: 'validacao', erros: corpo.erros }
  }

  return { tipo: 'falha', mensagem: `A API respondeu com erro (${resposta.status}).` }
}