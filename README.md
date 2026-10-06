# Projeto R — Vadronia Demo

Repositório oficial do jogo **Projeto R / Vadronia**, em Unity 6.6 (6000.6.0f1), C# e 2D top-down ortogonal.

Este repositório é um projeto Unity completo: abra **a raiz** no Unity Hub.

## Começar rápido

1. Leia [PROJECT_STATUS.md](PROJECT_STATUS.md).
2. Leia [AGENTS.md](AGENTS.md).
3. Consulte [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) para localizar o código.
4. Abra `Assets/Scenes/Vadronia.unity` e pressione Play.

Projeto local atual de Marcos: `C:/Users/Marcos/Downloads/Projeto R/Projeto R VadroniaDemo`.

## Estrutura

```text
Assets/            # código, cenas e assets importados pelo Unity
Packages/          # dependências Unity
ProjectSettings/   # configuração do projeto
Tests/             # regressão de lógica fora do Editor
Tools/             # ferramentas auxiliares
docs/              # arquitetura, lore, design, histórico e referências
.github/           # CI e template de PR
```

Não copie manualmente `Library/`, `Temp/`, `Logs/` ou builds. O GitHub deve ser sincronizado por Git; o Unity reimporta as mudanças locais.
