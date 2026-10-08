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
- `village.ogg` toca em loop na vila, com atenuação na pausa e mudo persistente no menu ESC; `city.ogg` está importado e reservado para um mapa de cidade.

## ORGANIZAÇÃO ATUAL

- runtime em `Assets/Scripts/Core|Player|Combat|World|UI|Visual|Bootstrap`;
- Editor em `Assets/Editor/Checks|Setup`;
- documentação fora de `Assets/`;
- referências que não entram no jogo ficam em `docs/`;
- arte experimental não usada foi removida da árvore atual e continua recuperável pelo histórico Git;
- `PROJECT_STATE.md` é a única fonte canônica para estado operacional/jogável atual.

## VALIDAÇÃO

Em 08/10/2026, revisão local sobre a main `11bef63`, incluindo praça, Sword Combo v2 e música restaurada pelo Cloud:

- 25 verificações puras passaram localmente com o SDK .NET 10 instalado, em projeto temporário fora dos Assets. A configuração versionada continua em .NET 8 para o CI; o SDK/reference pack 8 não está instalado neste PC.
- Unity 6000.6.0f1 compilou a revisão; `Vadronia > Verificar demo` passou 35 verificações de lógica e conferência dos assets.
- `Vadronia > Verificar exploração em Play` passou 36 verificações: duas quests, save JSON antigo, leitura repetida, relatos anteriores à oferta, recompensa única após recarga real do arquivo, cinco NPCs, oito direções do vídeo, repouso e colisão da investida da espada.
- `Vadronia > Verificar combate em Play` passou 16 verificações: investida em quatro taxas de atualização, dano/ângulo do combo, três pulsos do redemoinho, recargas, corte final, colisão com o poço e interrupção de golpes/efeitos. Teste usa personagem e alvos temporários, sem alterar saves.
- Progresso anterior e arquivos de save foram restaurados pelos testes de exploração. Praça e HUD foram inspecionados em captura real do Editor; isso não substitui aprovação artística de Marcos para a etapa 2.
- Música: uma fonte em reprodução e um AudioListener; sinal de saída medido por dois segundos com pico de aproximadamente 0,30. A primeira leitura isolada retornava zero; amostragem contínua confirmou sinal no motor de áudio. Não representa escuta nos alto-falantes do usuário.
- Existem avisos anteriores sobre Input Manager e APIs obsoletas; nenhuma migração de input foi feita nesta revisão. Nenhum executável Windows foi gerado.

Os controles foram verificados por código; não representa teste físico de teclado feito por Marcos nem aprovação artística dos NPCs.

## ATUAL

Praça da etapa 1, circulação de moradores, “Vozes da vila”, Sword Combo v2 e música do Cloud conferidos no Editor local. As correções desta revisão estabilizam a distância própria da habilidade H em 2,09 unidades no chão livre e limpam rastros/poeira/pose ao interromper combate. Antes, a investida chegava a 2,375 unidades em passos de 50 ms, contra 2,1375 em passos de 8,33 ms. Colisões ainda podem encurtar o avanço.

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

## ETAPA 1 — PRAÇA CENTRAL (08/10/2026)

- Reforma da praça para se aproximar da composição visual solicitada: poço central, corredor livre no eixo norte–sul, duas bancas na parte sul, bancos nas laterais e jardins simétricos nas margens.
- `VillageSquare.cs` gera uma única textura de pedra de tons quentes, com juntas de musgo, círculo discreto ao redor do poço e **bordas recortadas/dithered**. A praça se prolonga pelas quatro entradas, encontrando o chão de terra sem o retângulo de cor chapada anterior.
- `TownLayout.cs` reorganiza apenas os itens da praça. Edifícios e rotas externas ficam para a **etapa 2**, que depende de aprovação visual da etapa 1.
- Ponto da quest da banca acompanha a banca leste. A posição e o colisor do poço permanecem intactos.
- `CoreChecks` passa a testar simetria das bancas, acessos centrais e âncoras de quest contra os colisores.
- Nenhum asset antigo foi excluído; áudio, combates e saves não foram modificados. O resultado artístico precisa ser conferido no Unity Editor antes de começar a etapa 2.

## ETAPA 1 — ACABAMENTO DE PIXEL ART (08/10/2026)

- Corrigida a principal discrepância de escala: o calçamento tinha 64 pixels por unidade, enquanto `terrain-v3` tem **32 pixels por unidade**. Agora o pavimento usa a mesma densidade do terreno.
- Redesenhadas as fiadas de pedra com juntas mais finas, cantos gastos e pequenas variações de luz, mantendo a paleta quente, musgo discreto e caminhos orgânicos da reforma anterior.
- Sprite único de 544×448 pixels gerado no início do jogo (antes: 960×720), com redução do uso de memória de textura e sem atualização por frame.
- Casas, colisões, pontos de missão, música, NPCs, Sword Combo, rotas e saves preservados. Esta revisão complementa **somente a praça central**, não inicia a etapa 2.
- Revisão visual no Editor ainda necessária para confirmar o resultado estético das imagens escolhidas pelo usuário.

## ETAPA 1 — CORREÇÃO DO RETÂNGULO ANTIGO (08/10/2026)

- A revisão da captura real mostrou que o retângulo do calçamento antigo ainda aparecia atrás da nova textura. Trocar apenas pixels/cores na camada superior não apagava o piso original.
- `VillageSquare.cs` agora desenha **duas camadas procedurais de 32 PPU** em vez de só uma: uma base de terra que cobre integralmente a geometria do antigo retângulo e o novo calçamento quente por cima, com limite arredondado, irregular e caminhos conectados.
- A base mistura terra, grama e pequenos seixos onde as pedras se espaçam. O novo mosaico traz juntas, pedras de quinas gastas, variações entre fiadas e musgo discreto.
- Renderização em apenas dois sprites, gerados uma única vez na abertura da cena. Nenhum update por frame, nenhum colisor modificado; a integridade das casas, NPCs, quests, áudio, Sword Combo e saves foi mantida.
- Aprovação artística definitiva depende de captura da cena no Unity 6000.6.0f1 e comparação com as duas imagens de destino; **etapa 2 não foi iniciada**.

## ETAPA 1 — POLIMENTO DAS PEDRAS E JARDINS (08/10/2026)

- Após comparar a captura atual às duas composições pedidas, o piso uniforme e a borda de contorno ainda deixavam a praça com aspecto de placa grande.
- O piso passou a usar uma silhueta superelíptica com cantos orgânicos e quatro acessos, evitando os lados longos e retos anteriores; as bordas continuam se mesclando ao solo original coberto pela base de terra.
- O gerador de pedras usa fiadas e blocos com alturas e larguras variadas, mantendo pixel art a 32 PPU: reduz a repetição de fileiras idênticas visível nas capturas.
- Quatro novos canteiros baixos, puramente decorativos, destacam curvas da praça e deixam o entorno mais florido como nas imagens de destino.
- Casas, música, combate, poço, pontos de interação e scripts de NPCs permanecem intactos. A etapa 2 não foi iniciada.
- Validação final do visual só pode ocorrer após executar a cena no Unity; não confundir testes de regressão com aprovação estética.

## PRÓXIMO

1. obter a avaliação visual de Marcos sobre a praça da etapa 1 antes de modificar edifícios/rotas da etapa 2;
2. revisar com Marcos a arte temporária dos NPCs: ela ainda difere do visual original de Konrad e do player;
3. testar fisicamente RDFG, Z, T, clique esquerdo, E/H e o botão de música no Editor;
4. continuar a próxima mecânica autorizada, sem substituir sistemas já existentes. A faixa de cidade aguarda o mapa correspondente.

## LIMITES / DECISÕES

- prédios ainda externos;
- Konrad mantém o atlas original; os 5 NPCs novos usam pixel art procedural temporária própria, ainda sujeita a substituição por arte final;
- combate atualizado com combo e efeitos/skills E e H e boneco de treino com HP; ainda sem inimigos reais, block/parry nem HP integrado aos NPCs;
- sem interiores;
- casas em diagonal exigem arte nova (o atlas só tem fachadas de frente; "de costas" foi derivado dela);
- `W`, `S`, `X` e `2` não podem ser usados como controles obrigatórios; o movimento atual é RDFG + setas;
- não gerar build Windows sem pedido explícito;
- não voltar a usar o repositório antigo `MarcosPaulodaSilva/Projeto-R` como implementação ativa.
