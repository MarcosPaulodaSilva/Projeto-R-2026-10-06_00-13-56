# PROJECT_STATE — Projeto R / Vadronia

## Operação

- **Repositório oficial:** `MarcosPaulodaSilva/Projeto-R-2026-10-06_00-13-56`
- **Branch integrada:** `main`
- **Projeto Unity:** raiz do repositório
- **Projeto local:** `C:/Users/Marcos/Downloads/Projeto R/Projeto R VadroniaDemo`
- **Engine:** Unity 6000.6.0f1
- **Direção:** C#, 2D top-down ortogonal, Built-in pipeline
- **Arquitetura:** `docs/ARCHITECTURE.md`
- **Play/teste:** `docs/PLAYTEST.md`
- **Arte:** `docs/ARTWORK.md`
- **Prompt operacional:** `docs/MASTER_PROMPT.md`
- **Bíblia de design:** `docs/PROJECT_BIBLE.txt`
- **Lore:** `docs/lore/LORE_BIBLE.txt`

## FEITO

- Grünwald exterior com terreno, cenário e ambientação;
- player com quadros de animação derivados do vídeo de referência;
- Konrad usando o atlas original preservado por GUID;
- caminhada, corrida/fôlego e esquiva;
- movimento de teclado em **RDFG** (`R` cima, `F` baixo, `D` esquerda, `G` direita), mantendo as setas como alternativa;
- câmera suave;
- HUD em UI Toolkit;
- interação com NPC;
- missão de três ervas;
- recompensa única de 25 moedas;
- save local;
- cena real, Packages e ProjectSettings preservados;
- vila de Grünwald reformulada (2026-10-06): casas de frente e de costas ao longo das ruas, jardim noroeste, quintais, praça com poço, três bancas, bancos, mural, barris e canteiros, e fumaça saindo da chaminé da ferraria;
- ronda de Konrad ampliada (10 pontos com pausa própria): praça, bancas, rua sul e quintal oeste;
- `village.ogg` e `city.ogg` importados como assets de áudio; **a reprodução ainda não está conectada ao runtime**.

## ORGANIZAÇÃO ATUAL

- runtime em `Assets/Scripts/Core|Player|World|UI|Visual|Bootstrap`;
- Editor em `Assets/Editor/Checks|Setup`;
- documentação fora de `Assets/`;
- referências que não entram no jogo ficam em `docs/`;
- arte experimental não usada foi removida da árvore atual e continua recuperável pelo histórico Git;
- `PROJECT_STATE.md` é a única fonte canônica para estado operacional/jogável atual.

## VALIDAÇÃO

Última validação registrada no Editor local, em 05/10/2026: **20 verificações de lógica + 11 verificações em Play aprovadas**. Essa validação é anterior à reformulação mais recente de Grünwald.

A integração do novo mapa/rota/assets passou no **Logic Regression** do GitHub Actions em 06/10/2026. A reformulação visual ainda precisa ser validada no Unity Editor e em Play Mode no PC depois de sincronizar a `main`.

## ATUAL

Reformulação visual do mapa de Grünwald (`TownLayout`, `TownWorld`, atlas `town-extra.png`) e nova rota de Konrad. **Não validada no Unity ainda**: foi escrita sem Editor; só a lógica de colisão/patrulha foi conferida offline com `Tools/preview_town.py`. O teste de patrulha em `CoreChecks` agora exige todos os pontos alcançados e ≥10 voltas (a rota é mais longa que a antiga). Os arquivos `village.ogg` e `city.ogg` estão importados, mas ainda não há sistema de música/playback ligado a eles. O movimento foi remapeado de WASD para **RDFG**; a implementação está no repositório, mas precisa do teste físico de teclado de Marcos no Editor.

## PRÓXIMO

1. sincronizar a `main` com a pasta local;
2. abrir no Unity e aguardar a importação/recompilação de `town-extra.png` e dos demais assets;
3. conferir visualmente o mapa e ajustar `TownLayout.cs` somente se houver sobreposição;
4. testar fisicamente o movimento: `R` cima, `F` baixo, `D` esquerda e `G` direita;
5. executar `Vadronia > Verificar demo` e `Vadronia > Verificar exploração em Play`;
6. continuar a próxima mecânica autorizada.

## LIMITES / DECISÕES

- prédios ainda externos;
- um NPC;
- sem combate;
- sem interiores;
- casas em diagonal exigem arte nova (o atlas só tem fachadas de frente; "de costas" foi derivado dela);
- `W`, `S`, `X` e `2` não podem ser usados como controles obrigatórios; o movimento atual é RDFG + setas;
- não gerar build Windows sem pedido explícito;
- não voltar a usar o repositório antigo `MarcosPaulodaSilva/Projeto-R` como implementação ativa.
