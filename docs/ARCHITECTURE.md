# Arquitetura técnica — Projeto R / Unity

## Código runtime

`Assets/Scripts/`

| Módulo | Responsabilidade |
|---|---|
| `Core/` | regras e estado sem dependência de cena |
| `Player/` | movimento, input e câmera |
| `World/` | layout, mundo, atmosfera e interações |
| `UI/` | HUD/interface |
| `Visual/` | renderização, atlas e visual dos personagens |
| `Bootstrap/` | composição e inicialização da demo |

Todos permanecem no namespace `Vadronia`.

## Editor

- `Assets/Editor/Checks/`: verificações.
- `Assets/Editor/Setup/`: preparação/configuração.

## Dependências

```text
Bootstrap
 ├─ Player ─┬─ Core
 │          ├─ Visual
 │          └─ World
 ├─ World ──┬─ Core
 │          └─ Visual
 └─ UI ─────── Core

Editor -> Runtime
Runtime -X-> Editor
```

Evite dependências circulares. `Core` deve permanecer o mais independente possível.

## Crescimento

Somente crie módulos novos quando houver implementação real: `Combat/`, `NPC/`, `Companions/`, `Inventory/`, `Persistence/`, `Dungeons/`.

Não adicionar `.asmdef` ou Addressables apenas por organização; adote quando houver ganho técnico mensurável.

## Validação

`.github/workflows/regression.yml` executa `Tests/Regression.csproj` sem Unity. Cena, renderização, input e integração ainda exigem o Editor/Play Mode.
