# AGENTS — Projeto R / Vadronia

## Antes de editar

1. Leia `PROJECT_STATE.md`.
2. Consulte `docs/ARCHITECTURE.md` para escolher o módulo.
3. Use branch criada da `main` atual para mudanças relevantes.
4. Para decisões de sistema, consulte `docs/MASTER_PROMPT.md`.
5. Para cânone/lore, consulte `docs/lore/LORE_BIBLE.txt`.

## Regras

- Este repositório **já é a raiz do projeto Unity**.
- Engine: Unity 6000.6.0f1; C#; 2D top-down ortogonal; Built-in pipeline.
- Preserve `.meta`, GUIDs, cenas, saves e assets aprovados.
- Não recrie sistemas existentes sem inspecionar o código.
- Runtime não depende de `Assets/Editor`.
- Use `Core / Player / World / UI / Visual / Bootstrap`.
- Crie módulos novos somente quando houver implementação real.
- Não mantenha arte experimental sem uso dentro de `Assets/`; referências ficam em `docs/`.
- Não afirmar compilação, Play Mode ou validação Unity sem execução real.
- A regressão .NET não substitui Play Mode.
- Não gerar executável/pacote Windows sem pedido explícito.
- Projeto local atual: `C:/Users/Marcos/Downloads/Projeto R/Projeto R VadroniaDemo`.

Quando o estado do jogo ou da arquitetura mudar, atualize **somente** `PROJECT_STATE.md`; histórico datado vai para `docs/HISTORY.md`.
