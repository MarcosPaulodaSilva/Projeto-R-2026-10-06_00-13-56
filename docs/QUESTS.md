# QUESTS E HISTÓRIA DE GRÜNWALD

Referência de design das quests do vertical slice. Integração validada no Unity em 07/10/2026. O estado e os resultados recentes estão em `PROJECT_STATE.md`.

Cânone usado: `docs/lore/LORE_BIBLE.txt` §6 (civilização sem nome, língua indecifrável, relatos que se contradizem), §19 (histórias de escala humana), §36 (NPCs existem fora das quests), §43 (reputação como memória social). Nada aqui nomeia, explica ou traduz a civilização antiga, nem toca na morte de Heinrich ou na sucessão.

## Regras do sistema de quests (um só)

- Cada quest tem **ID estável** (`grunwald.<nome>`). Nunca renomear ou reaproveitar.
- Estado persistido em `AdventureProgress`: a quest das ervas continua em `quest/herbs/coins`; tudo novo usa `flags` (lista de IDs em `StoryFlags`). Versão do save continua 1: o campo novo é aditivo e saves antigos carregam com `flags` vazio.
- IDs desconhecidos são descartados ao carregar e recusados em `AddFlag`: um erro de digitação não chega ao save.
- Regras ficam em `Core/QuestJournal.cs` (puro, testável sem Unity); textos e posições em `World/GrunwaldStory.cs` (puro); a cena só chama (`VillageInteraction`).
- Recompensa única: a flag de conclusão é gravada na mesma operação que paga; recarregar não reabre.
- Ler um aviso ou ouvir uma conversa é conhecimento do mundo: fica lembrado mesmo antes de Konrad pedir (as pessoas já existiam antes do jogador).

## Quest 1 — `grunwald.ervas` (existente, preservada)

Konrad pede três porções de ervas; 25 moedas, uma vez. Comportamento e textos inalterados. Migrar para flags só se houver motivo (hoje funciona e tem testes).

## Quest 2 — `grunwald.vozes` “Vozes da vila” (nova)

**Por que existe:** dá a Grünwald uma história própria, de escala local, e prepara o terreno para a Cripta do Marco Partido (Master §6, item 6) sem resolver mistério algum. Não exige combate, interior nem NPC novo.

**Gancho:** depois do favor das ervas, Konrad conta que a Guilda quer relatos sobre uma pedra antiga, partida, a leste da vila, e que cada morador conta de um jeito.

| Relato | Onde | Flag |
|---|---|---|
| Mural de avisos (aviso prático, com a dúvida “sempre foi assim / rachou há pouco”) | perto do mural, praça norte | `vozes.mural` |
| Conversa na banca (“inverno passado” × “meu avô já falava dela”; “ninguém sabe ler aquilo”) | sul da banca leste | `vozes.banca` |
| Placa da guilda (a Guilda recolhe relatos, não afirma significado) | porta da guilda | `vozes.guilda` |

Fluxo: Konrad oferece (`vozes.aceita`) → três relatos → Konrad conclui (`vozes.concluida`), **15 moedas** (placeholder de balanceamento) → fala final de Konrad. Relatos já ouvidos antes contam.

**Não responde:** quem tem razão, o que as marcas dizem, se a pedra tem relação com algo maior. O jogo mostra evidência, não certeza.

## Verificação

Automática (sem Unity, `Tests/Regression.csproj`, e em `Vadronia > Verificar demo`): flags únicas/limpas, save antigo carrega vazio, quest só depois das ervas, 15 moedas exatamente uma vez (inclusive após salvar/recarregar), textos do diário, falas de Konrad, ids e flags dos pontos, pontos em chão livre e alcançável a pé.

Roteiro para teste manual (o fluxo e a persistência também têm verificações automatizadas em Play):
1. Sem save: falar com Konrad, colher 3 ervas, entregar (25 moedas). Voltar a falar: oferta da pedra; diário muda para “Vozes da vila 0 / 3”.
2. Mural, banca e placa da guilda: cada um mostra o texto e o aviso “Relato ouvido n / 3” só na primeira vez.
3. Konrad com 2 relatos: lista só o que falta. Com 3: 15 moedas, uma vez; falar de novo não paga.
4. Salvar (F5), reiniciar Play: diário e moedas iguais; falar com Konrad não paga de novo.
5. Ler os três pontos **antes** de falar com Konrad e só depois aceitar: a oferta menciona os relatos já ouvidos e a conclusão vem na conversa seguinte.
6. Rodar `Vadronia > Verificar exploração em Play` (36 verificações, incluindo as duas quests e preservação dos cinco NPCs).

## Como adicionar a próxima quest

1. Escolher o ID (`grunwald.<nome>`) e as flags; acrescentar em `StoryFlags` **e** em `StoryFlags.All`.
2. Regras puras (aceitar, concluir, recompensa única) numa classe como `Vozes`, com checks em `StoryChecks`.
3. Textos e posições em `GrunwaldStory`; interações novas usam ids de escolha entre 9 e 99; IDs a partir de 100 pertencem aos NPCs.
4. Texto do diário em `QuestJournal.Describe`.
5. Atualizar este documento e `PROJECT_STATE.md`.

## Propostas (precisam de decisão de Marcos; não implementadas)

- **Papel de Konrad.** O lore não tem entrada sobre ele. O jogo já o mostra fazendo ronda e orientando recém-chegados; é o que as falas assumem, sem título nem passado. Qualquer biografia é cânone novo.
- **Ligação da pedra com a Cripta do Marco Partido.** Nenhum texto afirma que a pedra partida é a entrada ou tem relação com a cripta. Ficou aberto de propósito.
- **Próximos ganchos locais (sem coroa):** uma família da vila (um desaparecimento ou objeto perdido), a Guilda pedindo um relato da estrada, a noite e as lanternas quando o relógio existir (Master §6, item 3). Cada um precisa de NPC ou sistema que ainda não existe.
- **Reputação.** Hoje só há flags. “Memória social” (Master §12) pede relações compactas depois do inventário; não antecipar.

## Pergunta para Marcos

Konrad deve continuar apenas como quem orienta e faz a ronda (como está), ou você quer definir um papel fixo para ele (guarda, curandeiro, algo da Guilda) antes de escrevermos mais falas dele?
