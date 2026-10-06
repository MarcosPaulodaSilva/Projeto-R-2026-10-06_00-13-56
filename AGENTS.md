# AGENTS — Projeto R / Vadronia

## Antes de editar

1. Leia `PROJECT_STATUS.md`.
2. Leia `PROJECT_STATE.md`.
3. Consulte `docs/ARCHITECTURE.md` para escolher o módulo.
4. Use branch criada da `main` atual.
5. Para decisões de sistema, consulte `docs/MASTER_PROMPT.md`.
6. Para cânone/lore, consulte `docs/lore/LORE_BIBLE.txt`.

## Regras

- Este repositório **já é a raiz do projeto Unity**.
- Engine: Unity 6000.6.0f1; C#; 2D top-down ortogonal; Built-in pipeline.
- Preserve `.meta`, GUIDs, cenas, saves e assets aprovados.
- Não recrie sistemas existentes sem inspecionar o código.
- Runtime não depende de `Assets/Editor`.
- Use os módulos `Core / Player / World / UI / Visual / Bootstrap`.
- Crie novos módulos como `Combat/`, `NPC/` ou `Inventory/` somente quando existir código real.
- Não afirmar compilação, Play Mode ou teste Unity sem verificação real.
- A regressão .NET não substitui Play Mode.
- Não gerar executável/pacote Windows sem pedido explícito de Marcos.
- Projeto local atual: `C:/Users/Marcos/Downloads/Projeto R/Projeto R VadroniaDemo`.

Se o estado jogável mudar, atualize `PROJECT_STATUS.md` e `PROJECT_STATE.md` no mesmo PR.
