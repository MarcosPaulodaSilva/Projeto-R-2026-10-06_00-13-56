# AGENTS — Projeto R / Vadronia

## Antes de editar

1. Leia `PROJECT_STATE.md`.
2. Leia `docs/CONTEXT_GUIDE.md`.
3. Consulte `docs/ARCHITECTURE.md` para escolher o módulo.
4. Use branch criada da `main` atual para mudanças relevantes.
5. Para documentos grandes, **pesquise primeiro por assunto e leia só a seção necessária**.
6. Para decisões de sistema, busque termos relevantes em `docs/MASTER_PROMPT.md` / `docs/PROJECT_BIBLE.txt`.
7. Para cânone/lore, busque o nome/tema específico em `docs/lore/LORE_BIBLE.txt`.

## Regras

- Este repositório **já é a raiz do projeto Unity**.
- Engine: Unity 6000.6.0f1; C#; 2D top-down ortogonal; Built-in pipeline.
- Preserve `.meta`, GUIDs, cenas, saves e assets aprovados.
- Não recrie sistemas existentes sem inspecionar o código.
- `MASTER_PROMPT`, `PROJECT_BIBLE`, `HISTORY` e `LORE_BIBLE` são bases pesquisáveis: não ler por inteiro por padrão.
- Adquira contexto sob demanda: buscar → ler trecho → implementar; expandir somente se faltar informação ou houver conflito.
- Runtime não depende de `Assets/Editor`.
- Use `Core / Player / Combat / World / UI / Visual / Bootstrap`.
- Crie módulos novos somente quando houver implementação real.
- Não mantenha arte experimental sem uso dentro de `Assets/`; referências ficam em `docs/`.
- Não afirmar compilação, Play Mode ou validação Unity sem execução real.
- A regressão .NET não substitui Play Mode.
- Não gerar executável/pacote Windows sem pedido explícito.
- Controles obrigatórios não podem usar `W`, `S`, `X` ou `2`; movimento atual: `R` cima, `F` baixo, `D` esquerda, `G` direita, com setas como alternativa.
- Projeto local atual: `C:/Users/Marcos/Downloads/Projeto R/Projeto R VadroniaDemo`.

Quando o estado do jogo ou da arquitetura mudar, atualize **somente** `PROJECT_STATE.md`; histórico datado vai para `docs/HISTORY.md`.
