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
- câmera suave;
- HUD em UI Toolkit;
- interação com NPC;
- missão de três ervas;
- recompensa única de 25 moedas;
- save local;
- cena real, Packages e ProjectSettings preservados.

## ORGANIZAÇÃO ATUAL

- runtime em `Assets/Scripts/Core|Player|World|UI|Visual|Bootstrap`;
- Editor em `Assets/Editor/Checks|Setup`;
- documentação fora de `Assets/`;
- referências que não entram no jogo ficam em `docs/`;
- arte experimental não usada foi removida da árvore atual e continua recuperável pelo histórico Git;
- `PROJECT_STATE.md` é a única fonte canônica para estado operacional/jogável atual.

## VALIDAÇÃO

Último registro local anterior a esta limpeza: **20 verificações de lógica + 11 verificações em Play aprovadas**.

A regressão .NET deve permanecer verde. Como esta limpeza move um asset Unity preservando GUID e remove assets mortos, é necessário fazer um novo pull no PC e validar compilação/Play Mode no Editor antes de chamar a reorganização de validada localmente.

## ATUAL

Limpeza estrutural do repositório: remoção de arte/layouts não utilizados, eliminação da pasta solitária `Assets/Art/`, simplificação da documentação e da ferramenta de atlas.

## PRÓXIMO

1. sincronizar a `main` com a pasta local;
2. aguardar o Unity reimportar/recompilar;
3. executar `Vadronia > Verificar demo`;
4. executar `Vadronia > Verificar exploração em Play`;
5. continuar a próxima mecânica autorizada.

## LIMITES / DECISÕES

- prédios ainda externos;
- um NPC;
- sem combate;
- sem interiores;
- não gerar build Windows sem pedido explícito;
- não voltar a usar o repositório antigo `MarcosPaulodaSilva/Projeto-R` como implementação ativa.
