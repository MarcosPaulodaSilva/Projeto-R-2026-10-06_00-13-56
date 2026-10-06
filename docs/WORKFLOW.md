# WORKFLOW — ChatGPT / Codex / humano

## Antes de trabalhar

1. partir da `main` atualizada;
2. ler `PROJECT_STATE.md`;
3. ler `docs/CONTEXT_GUIDE.md`;
4. localizar primeiro os arquivos de código envolvidos;
5. pesquisar somente as seções de documentação relacionadas;
6. verificar se já existe PR tocando nos mesmos arquivos;
7. criar uma branch para a tarefa.

Nomes recomendados: `codex/<tarefa>`, `chatgpt/<tarefa>`, `fix/<tarefa>`, `docs/<tarefa>`.

## Durante

- contexto sob demanda: não ler documentos grandes inteiros por padrão;
- mudanças pequenas e verificáveis;
- não misturar reorganização ampla com gameplay;
- preservar `.meta`, GUIDs, cenas e saves;
- não duplicar documentação canônica;
- não alterar cânone ou pilares de design silenciosamente.

## Finalização

O PR registra:

- objetivo;
- mudanças;
- arquivos principais;
- testes executados;
- o que não foi testado;
- riscos/pendências;
- próximo passo.

Se o comportamento do jogo mudou, atualizar `PROJECT_STATE.md` no mesmo PR.
