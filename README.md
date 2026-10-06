# Projeto R — Vadronia Demo

Repositório oficial do jogo **Projeto R / Vadronia**, em Unity 6000.6.0f1, C# e 2D top-down ortogonal.

A raiz deste repositório já é um projeto Unity completo. No Unity Hub, abra a própria raiz clonada.

## Começar rápido

1. Leia [PROJECT_STATE.md](PROJECT_STATE.md).
2. Leia [AGENTS.md](AGENTS.md).
3. Use [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) para localizar código.
4. Para testar, siga [docs/PLAYTEST.md](docs/PLAYTEST.md).
5. Abra `Assets/Scenes/Vadronia.unity` e pressione Play.

Projeto local atual: `C:/Users/Marcos/Downloads/Projeto R/Projeto R VadroniaDemo`.

## Estrutura

```text
Assets/            # somente conteúdo que o Unity precisa importar
Packages/          # dependências Unity
ProjectSettings/   # configuração Unity
Tests/             # regressão de lógica sem Editor
Tools/             # ferramentas técnicas pequenas
docs/              # arquitetura, estado histórico, lore e referências
.github/           # CI e template de PR
README.md
PROJECT_STATE.md   # fonte canônica do estado atual
AGENTS.md
```

`Library/`, `Temp/`, `Logs/`, `UserSettings/` e builds não pertencem ao Git.

## Regra de organização

- assets usados pelo jogo ficam em `Assets/`;
- referências/conceitos ficam em `docs/`;
- um sistema novo só ganha pasta quando houver código real;
- não manter versões antigas soltas em `Assets/`: o histórico Git já cumpre esse papel.
