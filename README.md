# Encontre um Professor

Solução do desafio técnico para pessoa desenvolvedora júnior: uma API em C# (.NET 8) que encontra professores disponíveis para um novo aluno e uma tela em React (TypeScript) para usá-la.

## Como rodar

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org) LTS (testado com a versão 24)

### API

Na raiz do repositório:

```
dotnet run --project src/EncontreProfessor.Api --urls http://localhost:5000
```

Endpoint: `POST http://localhost:5000/api/professores/busca`. O arquivo `api.http` tem exemplos de requisição (pode ser usado com a extensão REST Client do VS Code).

### Tela

Com a API rodando, em outro terminal:

```
cd web
npm install
npm run dev
```

Acesse `http://localhost:5173`. A tela chama a API em `http://localhost:5000`; para usar outro endereço, defina a variável de ambiente `VITE_API_URL`.

> No Windows, se o PowerShell bloquear o `npm` com erro de "execução de scripts", rode uma vez: `Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned`.

## Testes

### API e regras de negócio (C#)

Na raiz do repositório:

```
dotnet test
```

São 28 testes (xUnit), e nenhum depende de subir a API manualmente:

- **22 testes de unidade** do `Core`: regras da busca, validação e conversão de horários, incluindo os casos de fronteira (aula que encosta em outra pelos dois lados, aula que não cabe no bloco, horário fixo em vários dias, faixa fora da grade de 30 minutos, ordenação e prioridade dos motivos).
- **6 testes de integração** da API: sobem a aplicação em memória com `WebApplicationFactory` e fazem requisições HTTP de verdade (200 com o exemplo do enunciado, 400 com todos os erros, corpo vazio, JSON malformado, método não permitido e CORS).

### Tela (React)

Dentro da pasta `web`:

```
npm test
```

São 8 testes (Vitest + Testing Library) que simulam o uso da tela: preenchem o formulário, clicam em buscar e conferem o que aparece. A API é simulada em cada teste, o que permite cobrir todos os estados: envio dos dados corretos, professores disponíveis e os que ficaram de fora, nenhum professor encontrado, erros de validação, falha de comunicação, carregando, resposta em formato inesperado e erro 500.

## Estrutura

```
src/
  EncontreProfessor.Core/   regras de negócio: modelos, validação e busca (sem dependência de web)
  EncontreProfessor.Api/    camada HTTP: endpoint, leitura do professores.json, CORS
tests/
  EncontreProfessor.Tests/  testes de unidade do Core e testes de integração da API
web/                        tela em React + TypeScript (Vite), com testes em src/App.test.tsx
professores.json            dados dos professores
api.http                    exemplos de requisição à API
```

A regra de negócio fica no `Core`, que não conhece HTTP, JSON nem arquivos. A API só valida a requisição, chama o `Core` e devolve a resposta.

## Como a busca funciona

1. Todos os horários são convertidos para minutos desde 00:00 (`18:30` vira `1110`), o que torna as comparações simples.
2. Para cada dia pedido, são testados os inícios possíveis de 30 em 30 minutos dentro da faixa do aluno. Um início é válido se a aula inteira cabe em um único bloco de disponibilidade e não se sobrepõe a nenhuma aula marcada.
3. A sobreposição usa comparação estrita (`inicio < fimMarcada && inicioMarcada < fimNova`), então aulas que apenas se encostam são permitidas.
4. Como o horário é fixo, os horários finais são a interseção dos inícios válidos em todos os dias pedidos.
5. Os motivos de não elegibilidade são testados na ordem do enunciado (idioma, agenda bloqueada, horário), e o primeiro que falhar é informado.

## Decisões tomadas

Situações que o enunciado não cobre, e o que decidi:

- **Idioma:** comparado sem diferenciar maiúsculas, acentos e espaços nas pontas. `ingles`, `Inglês` e `INGLÊS ` encontram o mesmo professor.
- **Dias da semana:** também aceitos sem diferenciar maiúsculas e acentos (`Terça` vira `terca`). Dias repetidos na requisição contam uma vez só.
- **Início da faixa fora da grade:** se a faixa começa em um horário quebrado, como `18:15`, o primeiro início considerado é o próximo `:00` ou `:30` (`18:30`). A aula também precisa terminar dentro da faixa.
- **`aulasNaSemana`:** conta todas as aulas marcadas do professor na semana, não só as dos dias pedidos, já que o campo se chama "na semana".
- **Desempate na ordenação:** depois da quantidade de aulas, ordena pelo nome sem diferenciar acentos e maiúsculas (para `Fábio` ficar junto do `F`). Se ainda houver empate, usa o nome exato e depois o `id`, para o resultado ser sempre o mesmo.
- **Horário `24:00`:** recusado. O formato aceito é `HH:mm` de `00:00` a `23:59`, já que nada atravessa a meia-noite.
- **Erros de validação:** a API reúne todos os erros da requisição e responde 400 uma única vez, no formato `{ "erros": ["...", "..."] }`, para a tela mostrar tudo de uma vez.
- **Corpo vazio:** responde 400 com "O corpo da requisição é obrigatório.". JSON malformado também gera 400, tratado pelo próprio ASP.NET; a tela mostra o código do erro nesse caso.
- **Professor sem disponibilidade em um dos dias pedidos:** entra em `naoElegiveis` com "Sem horário em comum nos dias pedidos".
- **Dados:** o `professores.json` é lido uma única vez na inicialização e mantido em memória. O arquivo é copiado para a pasta de saída da API no build, para funcionar de qualquer diretório.
- **Validação no formulário:** desliguei a validação nativa do navegador (`noValidate`). Assim, todas as regras ficam na API, que é a fonte única da verdade, e a tela apenas exibe as mensagens que ela devolve.
- **Validação da resposta na tela:** antes de exibir os dados, a tela confere o formato da resposta da API. Se algo vier diferente do esperado, mostra "A API respondeu em um formato inesperado." em vez de quebrar. Usei funções de verificação próprias em vez de uma biblioteca (como Zod), porque o formato é pequeno.
- **CORS:** liberado apenas para `http://localhost:5173`, a origem da tela em desenvolvimento.

## O que ficou faltando

- **Testes ponta a ponta:** os testes da tela usam uma API simulada, e os testes da API não passam pela tela. Um teste que abrisse o navegador com a API real (com Playwright, por exemplo) cobriria a integração entre as duas partes.
- **Configuração para produção:** porta, origem do CORS e endereço da API estão ajustados para desenvolvimento local.
- **Documentação interativa da API:** não há Swagger/OpenAPI; os exemplos estão no `api.http`.
- **Persistência:** os professores vêm de um arquivo e ficam em memória, como o enunciado permite. Não há como cadastrar ou marcar aulas.