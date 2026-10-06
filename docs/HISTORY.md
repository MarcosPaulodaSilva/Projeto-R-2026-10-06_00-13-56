# Continuação local — 4 de outubro de 2026

## Contexto recuperado

Chat de origem: “Configurar Projeto-R”, ID `01a10395-7c1d-7741-b27c-6f825db6402f`, host `durable`. Histórico completo de seis turnos lido nesta sessão.

Repositório GitHub: https://github.com/MarcosPaulodaSilva/Projeto-R-2026-10-06_00-13-56
Base consultada: `85f2bb864a67d1f95b16f2c829f788d157ab94f0`, branch `main`.
Branch local de trabalho: `codex/unity-local-continuation`.

As instruções recentes consolidam a demo Unity 6.6 em 2D top-down, com corpos visíveis, Player e um único NPC (Conrad). A base inicial tem mapa verde, movimento WASD/setas e patrulha.

Na etapa seguinte foram preparados na nuvem: caminhada em quatro direções ligada à distância percorrida, colisões nos pés com subpassos, câmera de acompanhamento e Grünwald inicial (ruas, praça, casas, estalagem, guilda, ferraria, poço, banca e árvores). Prédios são apenas fachadas; sem novos sistemas de combate ou interiores.

O último pedido foi **aplicar apenas essa etapa no Editor local para teste e não iniciar a etapa 2**. O relato anterior de dez testes de lógica aprovados corresponde à execução na nuvem, não à validação desta máquina nem a Play Mode.

## Estado inicial constatado (antes da transferência)

- A pasta deste chat estava vazia, contendo somente um Git sem commits ou remoto. Agora contém a base do repositório GitHub.
- Naquela etapa, a demo aberta pelo Hub ficava em `C:/Users/Marcos/Downloads/VadroniaDemo` (caminho histórico anterior à reorganização), com Unity `6000.6.0f1`.
- A demo local contém somente a versão anterior do mapa verde. Não contém `MovementCore.cs`, `TownWorld.cs` nem os novos atlas.
- Os arquivos Unity da atualização não estão no commit do GitHub consultado.
- Não foi encontrado `Vadronia-Atualizacao-Assets.zip` em Downloads. No chat de origem, o arquivo foi entregue como `/workspace/Vadronia-Atualizacao-Assets.zip`.
- O histórico permite consultar os scripts e as decisões, mas não transfere os PNGs binários gerados na nuvem. Não reconstruir/regerar esses visuais e chamar o resultado de transferência da atualização original.
- Backup anterior à conexão do Editor: `.local-backups/vadronia-before-sync-2026-10-04/` (Assets, Packages, ProjectSettings).
- Instalado `com.unity.pipeline` `0.8.0-exp.1` na demo existente para permitir operação local do Editor via Unity CLI.
- Editor local aberto e conexão confirmada: `unity status` retornou `ready`, Unity `6000.6.0f1`. A cena antiga está em Play Mode. `console_status` confirmou `compilationFailed: false`, `compiling: false` e zero erros no Console. Isso valida somente a base antiga e a conexão; não a atualização ainda ausente.

## Transferência e aplicação concluídas

Marcos autorizou enviar mensagem ao chat de origem e transferir pelo GitHub. O chat publicou a branch `codex/unity-grunwald-step1-transfer`, commit `bbfb8b51ddcf5f344aed6175f63dc125eab8ca19`: 43 arquivos conferidos contra o ZIP original. A primeira tentativa foi interrompida pelo limite de uso; a retomada concluiu o envio. Não houve merge na main.

A branch local `codex/unity-local-continuation` recebeu esse commit por fast-forward. Naquela etapa, os 34 arquivos de Assets foram copiados para `C:/Users/Marcos/Downloads/VadroniaDemo/Assets` (caminho histórico anterior à reorganização), com igualdade SHA-256 verificada após a cópia e GUIDs existentes conferidos antes da substituição. A cena local, Packages (incluindo Pipeline) e ProjectSettings foram preservados. Tests, Tools e documentação também foram copiados. Backup completo anterior à substituição: `.local-backups/before-grunwald-20261004-215410/`.

## Validação realizada no Unity local

- Unity `6000.6.0f1`, Pipeline `0.8.0-exp.1`, estado `ready`.
- `Vadronia.Editor.DemoSetup.ValidateDemo()` executado no Editor: 10 testes de lógica passaram; presença/filtro das texturas e pipeline Built-in aprovados.
- Cena `Assets/Scenes/Vadronia.unity` executada em Play Mode: Player, Conrad, câmera e 16 SpriteRenderers (chão, 13 objetos de cenário e dois atores) presentes.
- Verificação via C# no Editor exercitou os 32 quadros de caminhada (quatro por direção/ator), repouso com o atlas original, colisão do Player com o poço interrompendo os passos, acompanhamento da câmera e ordenação à frente/atrás do poço. Todos passaram. Script de verificação: `.local-backups/verify-unity-integration.cs`.
- Conrad foi observado em posições e quadros diferentes durante Play, usando o atlas `conrad-walk`.
- Capturas da Game view inspecionadas em 1280x720: `.local-backups/grunwald-play.png` e `.local-backups/grunwald-town.png`.
- Console consultado sem erros de compilação ou de execução. Play foi reiniciado após as verificações para restaurar o estado inicial e deixar a demo disponível para teste.

As verificações de movimento foram programáticas. A avaliação artística da fluidez e o teste físico das teclas pelo usuário continuam disponíveis no Editor. Nenhum sprite foi regenerado e nenhuma funcionalidade da etapa 2 foi adicionada.

## Continuidade

Passo 1 aplicado. Aguardar o teste e o próximo pedido de Marcos. Nas próximas alterações, manter os fontes em raiz do projeto Unity e sincronizar as mudanças com o projeto real do Editor; preservar os arquivos locais de configuração e a cena. O documento `PROJECT_STATE.md` importado é o registro histórico da entrega na nuvem; esta página registra a validação posterior no PC.

Não foi gerado executável nem pacote de distribuição Windows.

## Grande atualização autorizada e aplicada — 5 de outubro de 2026

Os pedidos posteriores substituem a restrição anterior de aguardar após o passo 1. Marcos autorizou a atualização gráfica/mecânica, escolheu visual e animações como prioridade e pediu alterações diretamente no Editor. Correção artística exigida: passos baixos, duas pernas alternadas e nenhum joelho exageradamente alto.

### Aplicado ao projeto real

- Novo chão detalhado e atlas de poses neutras para Player e Conrad; os PNGs originais continuam preservados.
- Pernas independentes com fases opostas e recorte lateral por geometria; ciclo ligado à distância, com ritmo de caminhada reduzido. A elevação adicional é de até 0,035 unidade, além da rotação moderada das pernas. Não interpretar esse valor como limite geométrico absoluto de todos os pixels da bota.
- Sombras, árvores nas bordas, folhas, fumaça, lanternas, poeira e câmera suave.
- HUD em UI Toolkit com fonte Inter incluída, objetivo, fôlego, moedas, diálogo e pausa.
- Corrida, esquiva com custo/recarga e colisão; missão de Conrad com três coletas, 25 moedas de recompensa única e save local. Poço/estalagem recuperam fôlego; guilda tem placa informativa.
- Backup prévio em `.local-backups/before-visual-update-20261004-220139/`. Cena e configurações locais preservadas; .meta gerados no Unity foram copiados para o repositório.

### Verificação local

Unity 6000.6.0f1 compilou os scripts. `DemoSetup.ValidateDemo()` passou 20 verificações; `AdventurePlayChecks.Run()` passou 10 verificações em Play, incluindo velocidades reais do motor, esquiva bloqueada pelo poço, parada dos pés, pausa, diálogo, missão, recompensa única, escrita/leitura de save e configuração de fonte/interface. O teste restaurou os arquivos de progresso anteriores.

Capturas da Game view inspecionadas: `adventure-first.png`, `adventure-hud.png`, `gait-poses.png`, `gait-final.png` e `adventure-dialog.png`, dentro de `.local-backups/`. A captura de poses é um arranjo temporário de verificação; os atores extras foram removidos ao reiniciar Play. Ajuste final reduziu a separação dos pivôs laterais para zero, mantendo a alternância simétrica.

Os testes de controle foram programáticos; não representam um teste físico de teclado por Marcos nem a aprovação artística dele. A versão continua limitada a exploração externa e uma missão, sem combate/interiores. Nenhum executável ou pacote Windows foi gerado. Detalhes e procedência da arte em `unity/VadroniaDemo/LEIA-ME.md`, `PROJECT_STATE.md` e `ARTWORK.md`.

### Fechamento da validação

A checagem final identificou a conversão incorreta de unidades em `Sprite.OverrideGeometry`. Corrigida para coordenadas em pixels no retângulo do sprite, conforme a documentação Unity: https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/sprite/overridegeometry. Acrescentada uma verificação da geometria lateral de ambos os atores: total final de **20 testes de lógica + 11 testes em Play**, aprovados.

O script temporário de múltiplas cópias de atores gerou mensagens por tentar redefinir geometria de sprites já usados durante a renderização. Ele não faz parte dos Assets e não deve ser reutilizado. A inspeção final utilizou o próprio Player, sem recriar sprites ativos; capturas `gait-side-verified.png` e `gait-contact-verified.png`. Novo Play e testes concluídos com zero erros e zero avisos no Console. GUIDs antigos preservados e 75 arquivos de Assets conferidos por SHA-256 entre workspace e Editor, sem diferenças antes do ajuste; CharacterView e AdventurePlayChecks ressincronizados após a correção.

Atualização publicada na branch `codex/unity-local-continuation` do repositório correto, sem merge na main.

## Organização do repositório — 5 de outubro de 2026

Em 05/10/2026, a organização do trabalho foi revista para centralizar o projeto jogável em raiz do projeto Unity; este registro foi consolidado em `docs/HISTORY.md` e a documentação central Unity em `docs/`.

Posteriormente, o repositório foi consolidado para manter apenas a implementação Unity. Lore, referências visuais, música e vídeos permanecem como fontes canônicas em `docs/` e `assets/`.

Esta reorganização não altera Assets, GUIDs ou lógica. Os documentos anteriores acima preservam caminhos históricos quando descrevem operações já realizadas. Nenhuma versão Windows foi gerada.

## Mudança da pasta local — 5 de outubro de 2026

Marcos reorganizou seus projetos locais dentro da pasta-pai `C:/Users/Marcos/Downloads/Projeto R`. O projeto do Vadronia passou a ficar em:

`C:/Users/Marcos/Downloads/Projeto R/Projeto R VadroniaDemo`

O caminho dentro do GitHub **não foi renomeado** e continua raiz do projeto Unity. Essa diferença é intencional: o nome local identifica melhor o jogo entre outros projetos no PC, enquanto o caminho do repositório permanece curto e estável para scripts, CI e documentação técnica.


## Migração para o repositório oficial do jogo — 6 de outubro de 2026

O repositório `MarcosPaulodaSilva/Projeto-R-2026-10-06_00-13-56` passou a ser a fonte oficial da implementação Unity. O conteúdo local mais recente deste repositório prevaleceu sobre cópias antigas do projeto. Do repositório anterior foram migrados apenas organização, documentação, lore, referências e automações que ainda eram úteis.

A raiz deste repositório é diretamente um projeto Unity (`Assets/`, `Packages/`, `ProjectSettings/`). O repositório antigo `MarcosPaulodaSilva/Projeto-R` permanece como histórico/origem da migração e não deve receber novas implementações do jogo.


## Limpeza estrutural — 6 de outubro de 2026

Removidos da árvore atual assets experimentais sem referências por nome ou GUID: `characters-neutral.png`, `player-walk.png`, `conrad-walk.png`, `NeutralAtlasLayout.cs` e `WalkAtlasLayout.cs`. O atlas ativo `Assets/Art/characters.png` não foi apagado: foi movido para `Assets/Resources/Vadronia/characters-original.png` preservando o mesmo GUID usado pela cena. `PROJECT_STATUS.md` foi incorporado a `PROJECT_STATE.md`; arte e instruções de Play passaram para `docs/`.
