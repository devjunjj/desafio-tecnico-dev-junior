# Uso de IA

## Ferramentas

- **Claude (Anthropic)**, usado como par de programação durante todo o desenvolvimento.

## Como usei

Trabalhei de forma incremental, uma parte do sistema por vez (domínio, validação, regra de busca, API, testes e tela), com um commit ao final de cada etapa.

- **Análise do problema:** usei a IA para discutir o enunciado, decompor as regras e levantar os casos que ele não cobre. As decisões foram tomadas em conjunto e estão registradas no README.
- **Implementação:** a IA propunha a implementação de cada parte e explicava as escolhas técnicas envolvidas (conversão de horários para minutos, interseção entre dias, separação entre `Core` e API, *type guards* no front). Eu revisava, executava e validava na minha máquina antes de avançar.
- **Validação contínua:** a cada etapa, conferi o comportamento com build, testes automatizados e uso real da aplicação. Também usei mutação manual (alterar uma regra de propósito e verificar se algum teste falha) para checar se os testes protegiam de fato as regras de fronteira.
- **Avaliação das sugestões:** não apliquei tudo o que a IA propôs. Alguns exemplos:
  - A IA sugeriu deixar os testes de integração da API, os testes da tela e a validação da resposta como pendências. Avaliei que eram relevantes para os critérios do desafio e decidi implementá-los antes de fechar a entrega.
  - Para os campos de hora, que no meu ambiente apareciam com AM/PM, a IA sugeriu substituí-los por uma lista de horários. Avaliei que era apenas uma questão de exibição ligada ao idioma do sistema operacional, já que o valor enviado à API sempre segue o formato `HH:mm`, e mantive o campo nativo.
  - Na interface, recusei uma proposta visual que não estava coerente com o restante da tela e pedi outra abordagem.
- **Diagnóstico de problemas:** quando algo falhava, compartilhava a saída do terminal e os erros do editor para investigar a causa com a IA, em vez de aplicar correções às cegas.
- **Revisão da solução:** ao final, pedi à IA um guia de estudo explicando cada parte do código e as decisões tomadas, e usei esse material para revisar toda a solução antes da entrevista.

## Casos em que a IA errou

### 1. Uma afirmação errada sobre os testes, que revelou uma lacuna de cobertura

A IA afirmou que, se a verificação de sobreposição fosse alterada de `inicio < a.Fim` para `inicio <= a.Fim`, o teste da aula que "encosta" em outra falharia.

**Como percebi:** apliquei a alteração e rodei `dotnet test`. Todos os testes passaram, ou seja, a afirmação estava errada.

**Causa e correção:** a sobreposição tem duas comparações, uma para cada lado do "encostar". O teste existente cobria apenas a aula nova terminando quando a outra começa; nenhum teste cobria a aula nova começando quando a outra termina. Criei o teste `Aula_pode_comecar_exatamente_quando_outra_termina` e confirmei que ele falha com `<=` e passa com `<`. A lição: testes passando não garantem que a regra está coberta.

### 2. Validação nativa do navegador interceptando o formulário

A IA propôs um cenário de teste manual (apagar o horário de início para ver o erro de validação da API) sem considerar que o navegador valida campos `type="time"` incompletos. O envio foi bloqueado antes de chegar à API.

**Como percebi:** apareceu o aviso nativo do navegador em vez da mensagem devolvida pela API.

**Correção:** adicionei `noValidate` ao formulário, mantendo a API como fonte única das regras de validação. A decisão está no README.

### 3. Dependências que não foram instaladas

O comando de instalação sugerido não colocava entre aspas os pacotes com escopo (`@testing-library/...`). No PowerShell, o `@` tem significado especial, e parte dos pacotes foi descartada sem nenhuma mensagem de erro.

**Como percebi:** o editor acusou `Cannot find module` nos testes, e o `npm ls` confirmou que só 2 dos 5 pacotes estavam instalados.

**Correção:** reinstalei os pacotes com os nomes entre aspas.

## Conclusão

A IA acelerou o desenvolvimento e me ajudou a entender conceitos novos, mas cometeu erros em afirmações que pareciam corretas. Por isso, tratei cada sugestão como algo a ser verificado, e não como resposta final: tudo o que está na entrega foi executado e validado com testes.