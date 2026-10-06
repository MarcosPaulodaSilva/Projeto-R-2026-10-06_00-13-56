# CONTEXT_GUIDE — procurar antes de ler

Este arquivo existe para economizar contexto de agentes locais e Cloud.

## Regra principal

Documentos grandes são **bases pesquisáveis**, não leitura obrigatória do começo ao fim.

Antes de abrir `docs/MASTER_PROMPT.md`, `docs/PROJECT_BIBLE.txt`, `docs/HISTORY.md` ou `docs/lore/LORE_BIBLE.txt` inteiro:

1. identifique o sistema/personagem/local afetado;
2. inspecione primeiro os arquivos de código relevantes;
3. pesquise palavras-chave no documento adequado;
4. leia apenas a seção encontrada e, se necessário, as seções vizinhas;
5. amplie a leitura somente se houver conflito, lacuna ou mudança cross-system.

Leitura integral de documento grande é último recurso.

## Roteamento rápido

| Tarefa | Procurar primeiro | Termos úteis |
|---|---|---|
| movimento/câmera | `Assets/Scripts/Player/`, `Core/`, `docs/ARCHITECTURE.md` | movimento, camera, stamina, dodge |
| mundo/vila/NPC | `Assets/Scripts/World/` | NPC, rotina, vila, guarda, ameaça |
| UI | `Assets/Scripts/UI/`, `docs/PLAYTEST.md` | HUD, diálogo, menu, input |
| visual/animação | `Assets/Scripts/Visual/`, `docs/ARTWORK.md` | sprite, atlas, animation, Konrad, Player |
| combate | código relacionado + busca em `MASTER_PROMPT.md` / `PROJECT_BIBLE.txt` | combate, parry, block, stamina, ataque |
| inventário/equipamento | busca seletiva em design | inventory, inventário, item, equipment, raridade |
| save | código + busca seletiva | save, persistência, schema, ID estável |
| dungeon | busca seletiva em design | dungeon, procedural, seed, boss, loot, morte |
| lore/personagem/local | `docs/lore/LORE_BIBLE.txt` | nome exato do personagem/local/tema |
| decisão antiga | `docs/HISTORY.md` | sistema + data/termo relacionado |

## Exemplos

Tarefa: corrigir câmera.
- abrir `AdventureCamera.cs`;
- procurar "camera" em `ARCHITECTURE.md` e, se necessário, `MASTER_PROMPT.md`;
- não carregar lore, política ou dungeon.

Tarefa: trabalhar em Konrad.
- procurar "Konrad", "Conrad", "guarda", "companion" e "NPC";
- ler somente os trechos relevantes.

Tarefa: implementar dungeon.
- procurar "dungeon", "procedural", "seed", "boss", "loot", "morte" e "save";
- não ler capítulos sem relação com a implementação.

## Quando expandir contexto

Leia mais somente quando:
- duas regras conflitam;
- uma decisão de design está ambígua;
- a alteração cruza vários sistemas;
- há risco para save, GUIDs, cenas ou cânone;
- a seção encontrada não basta.

Fluxo recomendado:

`INSPECIONAR CÓDIGO → BUSCAR CONTEXTO → LER TRECHO → IMPLEMENTAR`

Não:

`LER TUDO → TENTAR DESCOBRIR O QUE IMPORTA`
