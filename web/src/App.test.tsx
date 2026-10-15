import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import App from './App'

const fetchFalso = vi.fn()

beforeEach(() => {
  fetchFalso.mockReset()
  vi.stubGlobal('fetch', fetchFalso)
})

afterEach(() => {
  vi.unstubAllGlobals()
})

function respostaJson(status: number, corpo: unknown) {
  return new Response(JSON.stringify(corpo), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })
}

async function buscarEspanholNaSegunda() {
  const usuario = userEvent.setup()
  render(<App />)
  await usuario.selectOptions(screen.getByLabelText(/idioma/i), 'Espanhol')
  await usuario.click(screen.getByLabelText('Segunda'))
  await usuario.click(screen.getByRole('button', { name: 'Buscar professores' }))
}

describe('Tela de busca de professores', () => {
  it('envia para a API os dados preenchidos no formulário', async () => {
    fetchFalso.mockResolvedValue(respostaJson(200, { professores: [], naoElegiveis: [] }))

    await buscarEspanholNaSegunda()

    expect(fetchFalso).toHaveBeenCalledTimes(1)
    const [url, opcoes] = fetchFalso.mock.calls[0]
    expect(url).toBe('http://localhost:5000/api/professores/busca')
    expect(opcoes.method).toBe('POST')
    expect(JSON.parse(opcoes.body)).toEqual({
      idioma: 'Espanhol',
      dias: ['segunda'],
      inicioFaixa: '18:00',
      fimFaixa: '20:00',
      duracaoMinutos: 60,
    })
  })

  it('mostra os professores disponíveis e os que ficaram de fora', async () => {
    fetchFalso.mockResolvedValue(
      respostaJson(200, {
        professores: [
          { id: 1, nome: 'Ana', aulasNaSemana: 1, horarios: ['18:00'] },
          { id: 4, nome: 'Diego', aulasNaSemana: 2, horarios: ['18:00', '18:30'] },
        ],
        naoElegiveis: [{ id: 2, nome: 'Bruno', motivo: 'Não ensina o idioma' }],
      }),
    )

    await buscarEspanholNaSegunda()

    const ana = (await screen.findByRole('heading', { name: 'Ana' })).closest('li')!
    expect(within(ana).getByText('1 aula na semana')).toBeInTheDocument()
    expect(within(ana).getByText('18:00')).toBeInTheDocument()

    const diego = screen.getByRole('heading', { name: 'Diego' }).closest('li')!
    expect(within(diego).getByText('2 aulas na semana')).toBeInTheDocument()
    expect(within(diego).getByText('18:30')).toBeInTheDocument()

    expect(screen.getByText('Bruno').closest('li')).toHaveTextContent('Bruno: Não ensina o idioma')
  })

  it('avisa quando nenhum professor está disponível', async () => {
    fetchFalso.mockResolvedValue(
      respostaJson(200, {
        professores: [],
        naoElegiveis: [{ id: 3, nome: 'Carla', motivo: 'Agenda bloqueada' }],
      }),
    )

    await buscarEspanholNaSegunda()

    expect(await screen.findByText('Nenhum professor disponível para esse pedido.')).toBeInTheDocument()
    expect(screen.getByText('Carla').closest('li')).toHaveTextContent('Carla: Agenda bloqueada')
  })

  it('mostra os erros de validação devolvidos pela API', async () => {
    fetchFalso.mockResolvedValue(
      respostaJson(400, { erros: ['Informe ao menos um dia da semana.', 'O idioma é obrigatório.'] }),
    )

    await buscarEspanholNaSegunda()

    const alerta = await screen.findByRole('alert')
    expect(alerta).toHaveTextContent('Informe ao menos um dia da semana.')
    expect(alerta).toHaveTextContent('O idioma é obrigatório.')
  })

  it('mostra falha de comunicação quando a API não responde', async () => {
    fetchFalso.mockRejectedValue(new TypeError('Failed to fetch'))

    await buscarEspanholNaSegunda()

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Não foi possível se comunicar com a API',
    )
  })

  it('mostra o carregando e trava o botão enquanto espera a resposta', async () => {
    let responder: (resposta: Response) => void = () => {}
    fetchFalso.mockReturnValue(
      new Promise<Response>((resolve) => {
        responder = resolve
      }),
    )

    await buscarEspanholNaSegunda()

    expect(screen.getByRole('status')).toHaveTextContent('Buscando professores...')
    expect(screen.getByRole('button', { name: 'Buscando...' })).toBeDisabled()

    responder(respostaJson(200, { professores: [], naoElegiveis: [] }))

    expect(await screen.findByText('Nenhum professor disponível para esse pedido.')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Buscar professores' })).toBeEnabled()
  })

  it('trata resposta em formato inesperado como falha', async () => {
    fetchFalso.mockResolvedValue(respostaJson(200, { foo: 1 }))

    await buscarEspanholNaSegunda()

    expect(await screen.findByRole('alert')).toHaveTextContent('formato inesperado')
  })

  it('mostra o código do erro quando a API falha', async () => {
    fetchFalso.mockResolvedValue(respostaJson(500, {}))

    await buscarEspanholNaSegunda()

    expect(await screen.findByRole('alert')).toHaveTextContent('(500)')
  })
})