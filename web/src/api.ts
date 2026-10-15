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
    if (ehBuscaResponse(corpo)) {
      return { tipo: 'sucesso', dados: corpo }
    }
    return { tipo: 'falha', mensagem: 'A API respondeu em um formato inesperado.' }
  }

  if (resposta.status === 400 && temErros(corpo)) {
    return { tipo: 'validacao', erros: corpo.erros }
  }

  return { tipo: 'falha', mensagem: `A API respondeu com erro (${resposta.status}).` }
}

async function lerJson(resposta: Response): Promise<unknown> {
  try {
    return await resposta.json()
  } catch {
    return null
  }
}

// Conferências do formato da resposta. A API é externa à tela,
// então os dados são checados antes de serem usados.

function ehObjeto(valor: unknown): valor is Record<string, unknown> {
  return typeof valor === 'object' && valor !== null
}

function ehListaDeTextos(valor: unknown): valor is string[] {
  return Array.isArray(valor) && valor.every((item) => typeof item === 'string')
}

function temErros(valor: unknown): valor is { erros: string[] } {
  return ehObjeto(valor) && ehListaDeTextos(valor.erros)
}

function ehProfessorDisponivel(valor: unknown): valor is ProfessorDisponivel {
  return (
    ehObjeto(valor) &&
    typeof valor.id === 'number' &&
    typeof valor.nome === 'string' &&
    typeof valor.aulasNaSemana === 'number' &&
    ehListaDeTextos(valor.horarios)
  )
}

function ehProfessorNaoElegivel(valor: unknown): valor is ProfessorNaoElegivel {
  return (
    ehObjeto(valor) &&
    typeof valor.id === 'number' &&
    typeof valor.nome === 'string' &&
    typeof valor.motivo === 'string'
  )
}

function ehBuscaResponse(valor: unknown): valor is BuscaResponse {
  return (
    ehObjeto(valor) &&
    Array.isArray(valor.professores) &&
    valor.professores.every(ehProfessorDisponivel) &&
    Array.isArray(valor.naoElegiveis) &&
    valor.naoElegiveis.every(ehProfessorNaoElegivel)
  )
}