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
- Konrad usando o atlas original preservado por GUID; direção estável em diagonais (histerese) e leve respiração/passo por código, já que o atlas só tem 4 quadros de caminhada;
- caminhada, corrida/fôlego e esquiva;
- Sword Combo inicial do player: 3 golpes com windup/active/recovery, buffer de encadeamento, lunge e apresentação simples de espada; ataque no clique esquerdo;
- movimento de teclado em **RDFG** (`R` cima, `F` baixo, `D` esquerda, `G` direita), mantendo as setas como alternativa; **Z** corre e **T** interage/continua diálogo;
- câmera suave;
- HUD em UI Toolkit;
- interação com NPC;
- 5 NPCs extras (Helga/Bruno/Maren/Lúcia/Tomás) com rotas, funções e falas dependentes da missão, agora com pixel art procedural temporária própria (32x64, 4 direções, 4 quadros);
- missão de três ervas;
- missão “Vozes da vila”: mural, banca e placa da guilda, diário, flags persistentes e 15 moedas de recompensa única;
- saves antigos sem flags continuam carregando; relatos ouvidos antes da oferta também contam;
- recompensa única de 25 moedas;
- save local;
- cena real, Packages e ProjectSettings preservados;
- vila de Grünwald reformulada (2026-10-06): casas de frente e de costas, viela dos fundos ao sul com casas voltadas para ela, jardim noroeste, quintais, praça com poço, três bancas, bancos, mural, barris, canteiros, arbustos, pedras e fumaça saindo da chaminé da ferraria;
- chão `terrain-v3` (gerado por `Tools/build_terrain.py`): caminhos de terra até cada porta visível, viela, pátios pisados e sombra de assentamento sob as casas; bosquetes e borda de floresta com cinco tipos de árvore;
- ronda de Konrad ampliada (10 pontos com pausa própria): praça, bancas, rua sul e quintal oeste;
- `village.ogg` e `city.ogg` importados como assets de áudio; **a reprodução ainda não está conectada ao runtime**.

## ORGANIZAÇÃO ATUAL

- runtime em `Assets/Scripts/Core|Player|Combat|World|UI|Visual|Bootstrap`;
- Editor em `Assets/Editor/Checks|Setup`;
- documentação fora de `Assets/`;
- referências que não entram no jogo ficam em `docs/`;
- arte experimental não usada foi removida da árvore atual e continua recuperável pelo histórico Git;
- `PROJECT_STATE.md` é a única fonte canônica para estado operacional/jogável atual.

## VALIDAÇÃO

Em 07/10/2026, após integração da pasta `Downloads/grunwald-quests-vozes-da-vila` sobre a main `e843b2b`:

- 24 verificações puras passaram localmente com o SDK .NET 10 instalado, em projeto temporário fora dos Assets. A configuração versionada continua em .NET 8 para o CI; o SDK/reference pack 8 não está instalado neste PC.
- Unity 6000.6.0f1 compilou a integração; `Vadronia > Verificar demo` passou 34 verificações de lógica e conferência dos assets.
- `Vadronia > Verificar exploração em Play` passou 36 verificações: duas quests, save JSON antigo, leitura repetida, relatos anteriores à oferta, recompensa única após recarga real do arquivo, cinco NPCs, oito direções do vídeo, repouso e colisão da investida da espada.
- Progresso anterior e arquivos de save foram restaurados pelos testes. Mapa e diálogo longo da oferta foram inspecionados em capturas do Editor.
- Uma chamada do Pipeline excedeu 5 segundos; o teste terminou e seu resultado completo foi confirmado no Console. Isso não foi erro do jogo.

Os controles foram verificados por código; não representa teste físico de teclado feito por Marcos nem aprovação artística dos NPCs.

## ATUAL

“Vozes da vila” integrada sem substituir as mudanças mais recentes do Cloud: mapa, controles RDFG/Z/T, Sword Combo e cinco NPCs permanecem. O pacote original estava baseado numa versão anterior e removeria as interações dos NPCs e testes de combate se fosse copiado integralmente; esses pontos foram combinados manualmente.

As três fontes de relato são pontos de interação, descritos em `docs/QUESTS.md`. A quest não revela tradução nem origem definitiva da civilização antiga e não define nova biografia para Konrad.

## ATUALIZAÇÃO — Sword Combo v2 (08/10/2026)

- `PlayerSword.cs` atualizado preservando a API pública utilizada por `VadroniaDemo` (`Combo`, `Aim`, `Tick`, `Interrupt`, `Dispose` e construtor).
- Espada procedural, poses adicionais de corrida e golpes, trilhas, poeira, três cortes encadeados, Redemoinho de Aço em E e Investida Cortante em H.
- Botões sul/oeste/norte do gamepad para ataque/habilidades quando o Input System estiver ativo; a mira por analógico direito já está na classe `PlayerSword`.
- Boneco de treino gerado temporariamente perto do jogador e avisos de recarga ligados ao HUD.
- Mantidos `SwordCombo.cs`, `ExplorerMotor.cs`, cenas, GUIDs existentes, NPCs, mapa e persistência.
- Integração na `main` solicitada sem testes; compilação/Play Mode no Unity **não foram executados nesta atualização**.
- Após `git pull` ou Pull no GitHub Desktop dentro da pasta do projeto principal, o Unity Hub poderá abrir os arquivos atualizados.

## AUDITORIA — revisão estática de 08/10/2026

- Conferidos no GitHub os arquivos do projeto Unity, seus pares `.meta`, a configuração de Git e os scripts de combate, movimento, interface, cenário e NPCs. Nenhum `.meta` sem asset nem asset sem `.meta` foi encontrado na árvore versionada.
- `VadroniaDemo.Update` agora interrompe o combate ao pausar, evitando retomar uma habilidade incompleta após sair do menu.
- `PlayerPose.Reset` restaura também posição visual, deslocamento acumulado, mistura da corrida e estado interno de passos.
- `PlayerSword.Interrupt` limpa rastros e deslocamentos transitórios ao interromper.
- Texto da HUD atualizado com as habilidades E/H existentes.
- Nenhum asset foi removido: `terrain-v2` ainda é fallback do terreno; `docs/references` documenta a arte do projeto; `.meta` e GUIDs são preservados.
- A pasta local `Library` e caches de build já estão excluídos do versionamento pelo `.gitignore`.
- Inspeção estática apenas: consumo de RAM, FPS, Unity/Play Mode e controles físicos não foram medidos nesta revisão.

## REVISÃO VISUAL DE GRÜNWALD — praça e vida cotidiana (08/10/2026)

- `VillageSquare.cs` desenha calçamento único com pedras de mesma escala e paleta, bordas irregulares e musgo, integrando visualmente a praça ao terreno existente. Um sprite único, gerado apenas no carregamento; zero pedras como GameObjects individuais.
- `TownLayout.cs` ganhou canteiros, bancos e cercas decorativas nas margens dos percursos, sem novos colisores ou mudanças em NPCs de quest.
- `TownWorld.cs` diminui/disciplina as sombras projetadas por sprites; props pequenos deixam de ter manchas desproporcionais.
- `VillageCrowd.cs` adiciona dois moradores circulando na feira com a mesma pipeline de sprites dos NPCs existentes; sem interferir nas falas, missões e progressão.
- Tudo funciona com os atlases atuais. Para igualar inteiramente a arte conceitual ainda serão necessários assets finais próprios para novas fachadas/objetos e refazer o atlas do terreno fora do runtime.
- Alterações adicionadas inicialmente numa branch de revisão; a validação de lógica automatizada não substitui avaliação estética ou Play Mode no Unity.

## RESTAURAÇÃO DA MÚSICA (08/10/2026)

- Diagnóstico: a main tinha `village.ogg` e `city.ogg` importados, mas faltavam `AudioSource` de runtime e `AudioListener` na câmera gerada por `VadroniaDemo`; por isso não havia reprodução ambiente em Play.
- `VillageMusic.cs` toca `village.ogg` em loop 2D, com volume moderado e atenuação suave na pausa. O mudo fica salvo no PlayerPrefs.
- O menu ESC ganhou botão para ligar/desligar música. O restante do HUD, as cenas, missões, ataques, sprites e efeitos do vilarejo foram mantidos.
- `Vadronia > Verificar demo` passa a conferir que a faixa da vila existe. O CI .NET não substitui teste com áudio/Unity Editor.
- A branch local `codex/village-soundtrack` não havia sido publicada no GitHub; esta restauração usa as faixas já versionadas, não afirma recuperar exatamente a implementação inédita daquela branch.

## PRÓXIMO

1. conectar `village.ogg` e `city.ogg` à reprodução no jogo, com controle de volume;
2. revisar com Marcos a arte temporária dos NPCs: ela ainda difere do visual original de Konrad e do player;
3. testar fisicamente RDFG, Z, T e o clique esquerdo no Editor;
4. continuar a próxima mecânica autorizada, sem substituir sistemas já existentes.

## LIMITES / DECISÕES

- prédios ainda externos;
- Konrad mantém o atlas original; os 5 NPCs novos usam pixel art procedural temporária própria, ainda sujeita a substituição por arte final;
- combate atualizado com combo e efeitos/skills E e H e boneco de treino com HP; ainda sem inimigos reais, block/parry nem HP integrado aos NPCs;
- sem interiores;
- casas em diagonal exigem arte nova (o atlas só tem fachadas de frente; "de costas" foi derivado dela);
- `W`, `S`, `X` e `2` não podem ser usados como controles obrigatórios; o movimento atual é RDFG + setas;
- não gerar build Windows sem pedido explícito;
- não voltar a usar o repositório antigo `MarcosPaulodaSilva/Projeto-R` como implementação ativa.
