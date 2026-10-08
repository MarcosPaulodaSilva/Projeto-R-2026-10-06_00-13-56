# Sword Combo v2 — integração segura com Vadronia

Esta atualização foi adaptada ao projeto Unity em `Projeto-R-2026-10-06_00-13-56`. O guia original `COMO_ADICIONAR.md` foi escrito para um bootstrap antigo, portanto **não** siga as substituições de linhas daquele guia sobre a branch `main`.

## Arquivos novos
- `Assets/Scripts/Combat/SwordController.cs`: entrada do combate, golpes, habilidades, alvo de treino, poses e movimento.
- `Assets/Scripts/Combat/SwordArt.cs`: sprite procedural da espada e efeitos.
- `Assets/Scripts/Combat/SwordFx.cs`: pooling dos efeitos visuais.
- `Assets/Scripts/Combat/SwordInput.cs`: teclado, mouse e entrada de ataque/mira por gamepad.
- `Assets/Scripts/Combat/TrainingDummy.cs`: boneco de treino com HP e knockback.
- `Assets/Scripts/Visual/PlayerPose.cs`: poses extras e poeira, preservando os frames existentes.

`Assets/Scripts/Combat/SwordCombo.cs` **já existe** no repositório e foi preservado, junto com seu GUID `.meta`. Não copie um segundo `SwordCombo.cs` para a pasta `Core`: isso geraria erro de classe duplicada.

## Alterações do projeto
- `Assets/Scripts/Bootstrap/VadroniaDemo.cs` passa a criar `SwordController` depois do HUD.
- `SwordController.Tick` aciona `ExplorerMotor.Tick` uma vez por frame, com RDFG/setas, sprint e esquiva.
- Ataques usam `ExplorerMotor.Lunge` para respeitar as colisões; pausa/diálogo interrompem o ataque.
- `PlayerSword.cs` antigo continua no repositório, mas não é instanciado em runtime.
- O boneco surge provisoriamente em `(2.4, -3.9)`. Depois do teste, remova ou mova `sword.SpawnDummy(...)` do bootstrap.

## Controles
| Ação | Entrada |
|---|---|
| Movimento | R/F/D/G ou setas |
| Correr | Z ou Shift |
| Combo de 3 golpes | Clique esquerdo ou botão sul do gamepad |
| Mira | Mouse ou analógico direito (com Input System) |
| Redemoinho de Aço | E, recarga de 6 s |
| Investida Cortante | H, recarga de 8 s e fôlego da esquiva |
| Esquiva | Espaço |
| Interagir | T |

## Validação local no Unity Hub
1. Atualize/abra a pasta local que contém `Assets`, `Packages` e `ProjectSettings` (a pasta raiz do repositório).
2. No Unity Hub: **Add project from disk** / Adicionar do disco → selecione essa pasta raiz.
3. Use o Editor **6000.6.0f1** configurado em `ProjectSettings/ProjectVersion.txt`.
4. Aguarde a importação, verifique o Console e execute `Vadronia > Verificar demo`.
5. Abra `Vadronia > Abrir demo`, entre em Play, execute `Vadronia > Verificar exploração em Play` e teste o combate manualmente.
6. Verifique ataques sobre o boneco, colisões junto ao poço, pausa/diálogo e animação nas 8 direções.

**Importante:** atualizar o GitHub não atualiza automaticamente a pasta aberta no Unity Hub. É necessário sincronizar os arquivos localmente com Git/GitHub Desktop. A regressão .NET só valida a lógica pura, não o Editor ou a cena.
