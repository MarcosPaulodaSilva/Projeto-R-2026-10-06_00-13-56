# WORKFLOW — ChatGPT / Codex / humano

## Antes de trabalhar

1. partir da `main` atualizada;
2. ler `PROJECT_STATUS.md`;
3. verificar se já existe PR tocando nos mesmos arquivos;
4. criar uma branch para a tarefa.

Nomes recomendados: `codex/<tarefa>`, `chatgpt/<tarefa>`, `fix/<tarefa>`, `docs/<tarefa>`.

## Durante

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

Se o comportamento do jogo mudou, atualizar `PROJECT_STATUS.md` e `PROJECT_STATE.md` no mesmo PR.
