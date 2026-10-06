# SUPER MASTER PROMPT — PROJETO R / VADRONIA (UNITY)

Documento operacional único para qualquer agente que trabalhe neste repositório Unity. Versão 1 — 05/10/2026.

Consolidado a partir do histórico de design do Projeto R, da Bíblia de pré-produção Unity (`docs/PROJECT_BIBLE.txt`), de `AGENTS.md`, do estado validado e das decisões úteis preservadas de protótipos anteriores. Este documento é a referência operacional única para a implementação Unity.

Não conte a história dos prompts anteriores. Aplique as regras abaixo.

---

## 0. Função e autoridade

Você trabalha no **Projeto R / Vadronia — versão Unity**.

Hierarquia de decisão:

1. decisão explícita mais recente de Marcos;
2. `AGENTS.md`;
3. este Super Master;
4. `PROJECT_STATE.md` e `docs/HISTORY.md`;
5. comportamento estável comprovado no Editor;
6. Bíblia Unity e documentos auxiliares;
7. documentos históricos do Projeto R já consolidados, apenas quando ajudarem a explicar uma decisão preservada.

Não invente cânone ausente.

Sua função: INSPECIONAR, IMPLEMENTAR, INTEGRAR, EXECUTAR, TESTAR, TENTAR QUEBRAR, CORRIGIR, REGREDIR, DOCUMENTAR e CONTINUAR.

**Prioridade de trabalho:** a decisão mais recente de Marcos. Em 05/10/2026 ela é visual/animação. Sem decisão explícita, a ordem é MECÂNICA > INTEGRAÇÃO > ROBUSTEZ > IA > TESTABILIDADE > BALANCEAMENTO > PERFORMANCE > VISUAL.

### Dois modos de execução

**Modo COM Editor** (agente na máquina de Marcos, com o Unity aberto): cumpre o ciclo completo — editar, compilar, rodar `Vadronia > Verificar demo`, rodar o teste em Play, ler o Console, inspecionar capturas, sincronizar com `C:/Users/Marcos/Downloads/Projeto R/Projeto R VadroniaDemo`.

**Modo SEM Editor** (agente de nuvem/GitHub, sem Unity nem acesso ao PC):

- trabalhe em branch própria (`claude/unity-<tema>`), nunca direto na `main`, quando houver código C#;
- mudanças pequenas e revisáveis; um commit por ideia;
- declare **NÃO COMPILADO / NÃO TESTADO NO EDITOR** na mensagem do commit, no `PROJECT_STATE` e no relato final, até alguém com Editor verificar;
- inclua testes de lógica (EditMode/`DemoSetup`) que o agente local possa rodar, e uma lista curta de verificação manual;
- não fabrique arquivos `.meta` nem GUIDs: o Editor gera os `.meta` de arquivos novos, e eles são commitados depois;
- não altere cena, `ProjectSettings` nem `Packages` sem necessidade comprovada;
- registre em `PROJECT_STATE` o que falta para sincronizar com a pasta real.

Nunca diga que compilou, testou, executou ou sincronizou se não fez.

---

## 1. Regras fixas do repositório e do ambiente

- Repositório oficial do jogo: `MarcosPaulodaSilva/Projeto-R-2026-10-06_00-13-56`. A raiz é o próprio projeto Unity; lore e documentação ficam em `docs/`.
- Fontes Unity: `Assets/`, `Packages/` e `ProjectSettings/`. Unity **6000.6.0f1**, C#, 2D top-down ortogonal, pipeline Built-in.
- Projeto aberto por Marcos: `C:/Users/Marcos/Downloads/Projeto R/Projeto R VadroniaDemo`. Alterações que dependam do Editor devem chegar a essa pasta e ser validadas ali; não entregar apenas ZIPs ou código solto.
- A pasta-pai `C:/Users/Marcos/Downloads/Projeto R` funciona como contêiner local para vários jogos/projetos. **Não trate a pasta-pai inteira como o repositório do Vadronia e não misture arquivos de projetos irmãos.** O repositório deste jogo continua sendo `MarcosPaulodaSilva/Projeto-R-2026-10-06_00-13-56`, com o projeto Unity versionado em `raiz do repositório`.
- **Não gerar executáveis nem pacotes Windows** sem novo pedido explícito de Marcos.
- Preservar `.meta`, GUIDs, cena e trabalho existente. Antes de substituir Assets, manter backup local fora do versionamento (`.local-backups/`). Sincronização com a pasta do Editor: backup → cópia → conferência por SHA-256 → recompilar.
- Preservar versões antigas; não sobrescrever histórico.
- Nomes de arquivo em minúsculas, kebab-case, versões coerentes. (Arquivos C# seguem a convenção C#, PascalCase.)
- Não declarar conteúdo recuperado, importado, compilado ou testado sem verificação. Não regenerar sprites e chamar o resultado de “transferência” da arte original.
- Nunca commitar credenciais ou tokens.

---

## 2. Não recomeçar / preservar / linha de versão

Antes de editar: leia `PROJECT_STATE.md` e `docs/HISTORY.md`, examine `Assets/Scripts`, `Assets/Editor`, `Tests/` e `Tools/`, identifique sistemas existentes e preserve o trabalho funcional.

Não recrie do zero. Não reescreva por preferência. Não duplique sistemas (um Combat Core, um sistema de NPC, um inventário, um sistema de vida, um sistema de quests).

**Linha de versão Unity:** ainda não há uma linha oficial definida; registre propostas em `PROJECT_STATE` e aguarde a decisão de Marcos antes de criar uma nova convenção de versão.

Uma mecânica só está pronta se funciona, integra, é observável, é testável, cobre os edge cases principais e não quebra sistemas vizinhos.

Ciclo: INSPECIONAR → IMPLEMENTAR → COMPILAR → EXECUTAR → TESTAR → TENTAR QUEBRAR → CORRIGIR → TESTAR INTEGRAÇÃO → REGRESSÃO → PROJECT_STATE → CONTINUAR.

Autonomia técnica para arquitetura local, algoritmos, dados, ordem, correções e testes. Pergunte apenas por: decisão de design realmente importante, ação destrutiva, credencial/recurso ausente ou contradição canônica material.

---

## 3. Identidade e direção atual

Projeto R é Action RPG + Roguelike + RPG de exploração + simulação leve de mundo.

**Direção vigente (Unity):** 2D **top-down**, personagens de corpo visível, Unity 6.6. **Não** isométrico e **não** plataforma. Materiais históricos podem registrar direções anteriores; para o projeto atual, vale a decisão mais recente (top-down).

Profundidade vem de: SISTEMAS SIMPLES + BOA INTEGRAÇÃO + CONSEQUÊNCIAS + COMPORTAMENTO COERENTE. Não vença pela quantidade de sistemas; vença pela integração.

O mundo existia antes do protagonista. NPCs têm casa, trabalho, horários e perigos. O protagonista não é o escolhido: importância é conquistada (desconhecido → aventureiro → feitos locais → Guilda → reconhecimento regional → reino → corte → Grandes Casas → possível importância política). A campanha não obriga rota política ou romântica.

---

## 4. Escopo do mundo e cânone (resumo)

Mundo completo: Vadronia, Migriard, Vitehria. **Escopo atual: Vadronia, e dentro dela Grünwald.** Não implementar Migriard/Vitehria sem pedido explícito. Fechar primeiro o vertical slice (Grünwald + Cripta do Marco Partido + sistemas centrais).

Cânone que não pode ser reinterpretado silenciosamente:

- Vadronia: fantasia medieval germânica, calorosa e comunitária; não é um reino sombrio. Assentamentos: capital Vadronia, Eisenbruck, Grünwald, Nebelheim.
- Civilização antiga: nome verdadeiro desconhecido, língua indecifrável, sem tradução perfeita, sem revelação final, sem explicação universal. Não inventar nome, origem ou causa do desaparecimento.
- Casa Morigan: Heinrich (morreu aos 57, há ~2 anos, condição/maldição mágica incurável; origem e responsável desconhecidos — não inventar assassino); Beatrice 45, governante; Henry 23; Julian 19; Kian 19; Nicasia 18 (aniversário de lore 7/11). Protagonista: 18 (a documentação que cita 17 está obsoleta). Sem envelhecimento.
- Sucessão: Henry × Julian. Nenhum vira vilão por conveniência. Rota de Nicasia exige relação genuína, Beatrice, legitimidade, reconhecimento, feitos e apoio coerente de uma Grande Casa; não é prioridade.
- Grandes Casas: Falkenberg (militar), Eichenwald (agricultura/comércio), Silberhain (conhecimento/ruínas). Nenhuma é boa ou má.
- Migriard e Vitehria: apenas direção futura (ver Bíblia). Não expandir agora.

**Nome decidido por Marcos (05/10/2026):** o nome oficial do personagem é **Konrad**, com K. A demo ainda pode conter referências antigas a "Conrad". Ao tocar nesses arquivos, renomeie com cuidado (identificadores, textos de diálogo, nomes de atlas como `conrad-walk`) sem quebrar arquivos, `.meta` e GUIDs.

**Distinção de autoridade:** CANON (não reinterpretar), REQUISITO (precisa funcionar), DIREÇÃO (resultado desejado, implementação livre), SUGESTÃO (pode ser avaliada). Agentes **propõem**, não mudam pilares: nada de crafting complexo, mudança de permadeath, remoção da noite, companions controláveis, classes rígidas, alteração de sucessão ou substituição de sistemas centrais sem decisão de Marcos.

---

## 5. Estado implementado no Unity (preservar)

Salvo regressão comprovada ou decisão explícita, preserve:

- Grünwald exterior: ruas, praça, casas, estalagem, guilda, ferraria, poço, banca, árvores (prédios são fachadas);
- Player e Conrad com arte neutra nova; originais preservados;
- caminhada procedural: **passos baixos, duas pernas em fases opostas, sem joelho alto**; ciclo ligado à distância percorrida; recorte lateral por geometria (`Sprite.OverrideGeometry` em coordenadas de pixel);
- colisão nos pés com subpassos; câmera suave com antecipação;
- corrida com fôlego; esquiva com custo/recarga e colisão;
- HUD em UI Toolkit (fonte Inter), diálogo, pausa;
- missão de Conrad (três ervas, 25 moedas, recompensa única) e save local `grunwald-adventure-v1.json`;
- atmosfera: sombras, folhas, fumaça, lanternas, poeira;
- verificações: `Vadronia > Verificar demo` (20 lógicas) e `Vadronia > Verificar exploração em Play` (11 em Play).

Limites atuais: sem combate, sem interiores, um NPC, uma missão.

---

## 6. Roteiro sugerido (confirmar com Marcos antes de cada etapa)

Porte de **design**, não de código: implemente em C# idiomático usando a Bíblia Unity, este Super Master e o estado validado como referência de comportamento.

1. Combat Core no Player (ataque com fases, block/parry com o mesmo comando, riposte) + um inimigo de teste.
2. IA de inimigo mínima (percepção, investigar, perseguir, retornar/leash, sem atravessar paredes).
3. Relógio dia/noite ligado a luz e rotinas de NPC (manhã/trabalho/praça/casa; casa segura protege civil).
4. Guarda/companion (Konrad/Conrad), com down/revive e recuperação segura.
5. Inventário, equipamento, atributos, progressão, morte/level-down com resolver central; save versionado ampliado.
6. Cripta do Marco Partido (layout lógico por seed → módulos → validação) e Guardião Sem Nome.

Regra do “continue”: dentro do escopo já autorizado, escolha a melhoria de maior valor (robustez, testes, polimento, bugs). Para **abrir uma etapa nova de gameplay** da lista acima, pergunte uma vez a Marcos — é decisão de design importante.

---

## 7. Arquitetura Unity

- Sem `GameManager` gigante. Responsabilidades claras, dependências explícitas, contratos estáveis (interfaces, IDs, formato de save).
- **Dados separados do runtime.** ScriptableObjects para definições/configuração; nunca como depósito de estado mutável de save.
- Separar **dados do ataque**, **lógica de combate** e **apresentação/animação**. Janelas de hit e de gameplay **não dependem** da duração de AnimationClips.
- Separar `MoveDirection`, `AimDirection` e `FacingDirection`.
- Interfaces diretas quando a relação é clara; eventos só quando há desacoplamento um-para-muitos real. Sem event bus global por reflexo.
- Conteúdo data-driven: adicionar espada, NPC ou dungeon não deve exigir mexer em código central.
- IDs estáveis e persistentes para NPCs, itens importantes, assentamentos, quests, companions, POIs, dungeons e relações. Nunca usar referência de Scene/GameObject como identidade persistente.
- Pooling seletivo (projéteis, efeitos). Addressables só se resolverem problema concreto. Sem ECS próprio, DI framework, DSL de quests ou abstração hipotética.
- Também não subengenhar: combate, inventário, quests, save e NPCs não cabem em meia dúzia de scripts gigantes.
- Tempo: gameplay não varia com FPS. Stamina, IA, timers, parry e física usam tempo corretamente (`Time.deltaTime` / passo fixo).
- Evitar alocações por frame (GC) em Update.
- Overworld: regiões/chunks e níveis de simulação (FULL perto, REDUCED médio, DATA longe) devem nascer com o vertical slice, não como remendo tardio.

---

## 8. Controles, Player e câmera

- Controles atuais (`LEIA-ME.md`): WASD/setas caminham, Shift corre, Espaço esquiva, E interage, Esc pausa, F1 oculta interface, F5 salva. **Não altere mapeamentos sem pedido de Marcos.** Há histórico de uso de R/D/F/G por limitação de teclado; qualquer mudança de input deve ser confirmada com Marcos e registrada.
- Abstraia **ações de gameplay**, não teclas físicas (Input System ou camada própria), para permitir remapeamento e gamepad. Gamepad faz parte da visão: analógico esquerdo move 360°, direito mira; UI essencial utilizável sem mouse.
- Player: movimento, colisão, mira, facing, HP, fôlego/stamina, mana quando aplicável, ataque, block, parry, esquiva, interação, inventário, equipamento, atributos, progressão, morte.
- Movimento responsivo e previsível. Testar paredes, cantos, árvores, poço, entidades, esquiva, knockback e transições. Nunca atravessar sólido por bug.
- Câmera: follow suave com look-ahead; não gira o mundo.

---

## 9. Combat Core (quando a etapa for autorizada)

Player, inimigos, companions, NPCs de combate e bosses **reutilizam o mesmo Combat Core**:

Attack Intent → Attack State → Hit Detection → Validation → Damage → Defense → Modifiers/Status → Health → Reaction → Death/Down.

- Ataques data-driven: damage, range, startup, active, recovery, staminaCost, knockback, stagger, tags. Fases STARTUP/ACTIVE/RECOVERY; hitbox só causa dano na fase ativa; debug expõe fase/hitbox/hurtbox.
- Stamina: max/current/cost/regen/delay; controla ataque, esquiva e block. Block consome stamina por impacto; insuficiente = Guard Break.
- Parry usa o **mesmo comando** do block; timing gera vantagem real. Referência de design consolidada: PERFECT PARRY → RIPOSTE com janela aproximada de 1,35 s e benefício perceptível no próximo golpe válido (cerca de 35% de dano + stagger/knockback). Balancear por teste.
- Esquiva: duração, distância, custo, i-frames, recuperação. Testar parede, ataque, stamina zero, multi-hit, hazard e transição.
- À distância: projétil com owner/team/posição/velocidade/dano/vida/colisão; respeita cenário.
- Magia reutiliza o Combat Core (projétil/área/status/cooldown/mana). Não criar um segundo jogo.
- Input buffering e cancel windows controlados; telegraphs claros; sem leitura injusta de input.
- Dificuldade por comportamento e composição, não só HP/dano.

---

## 10. IA de inimigos, boss e hazards

- Estados possíveis: IDLE, PATROL, DETECT, INVESTIGATE, SEARCH, CHASE, POSITION, ATTACK, RECOVER, RETURN, DEAD. Nem todo inimigo usa todos.
- Percepção: raio/direção de visão, linha de visão, audição/ruído, última posição conhecida. **Sem conhecimento mágico.**
- Ouviu sem ver → INVESTIGATE → SEARCH temporário → RETURN. Perdeu a visão → vai à última posição, não segue através de parede.
- Leash por origem/zona. Não spawnar hostis na vila segura.
- Melee não atravessa obstáculo: contorna/reposiciona. Pathfinding sob demanda (nunca todo frame) com recuperação de stuck.
- Attack slots/spacing para limitar pileup.
- Poucos inimigos bons por ecossistema; evitar zumbis/esqueletos genéricos como identidade. Papéis de referência: bandido melee, lobo/charger, Veterano elite, Arqueiro Bandido, sombras noturnas, boss.
- Boss reutiliza o Combat Core, com fases, padrões, telegraphs e barra. Referência: **Guardião Sem Nome** (duas fases; runas antigas de área com telegraph longo e explosão curta; dano de chão não aparável como golpe comum).

---

## 11. NPCs, companions, tempo e noite

- NPC não é placa de diálogo: casa, rotina, trabalho, lazer, conversa, perigo, HP, memória, relações, função. Continua existindo após a quest.
- NPCs distantes existem como **dados**; não caminham fisicamente fora de vista. Calcule posição por horário e estado persistente.
- Rotinas: manhã tarefas, trabalho, fim de tarde praça, noite casa; chuva → abrigo; perigo → fuga.
- **REGRA ABSOLUTA:** casa/refúgio seguro protege civil. Ao entrar corretamente, ele deixa de ser alvo; a ameaça não ataca através da casa.
- Civis têm HP; 0 HP incapacita e recupera, sem permadeath civil obrigatório no slice.
- Companions: até 3 ativos; autônomos (IA em camadas: percepção → decisão → execução → navegação → recuperação); sem raciocínio pesado todo frame; detectam stuck e reposicionam/teleportam com segurança. O jogador nunca deve precisar resolver problemas de IA de companion.
- Estados do companion: Active → Incapacitated → Recovered ou Dead. 0 HP em dungeon = incapacitado com janela de resgate. **Permadeath nunca por bug de path, streaming, loading, personagem preso ou falha técnica.** Equipamento do jogador nunca desaparece sem política explícita.
- Dia/noite é pilar: amanhecer, dia, entardecer, noite, madrugada afetam NPCs, lojas, luz, ameaças, quests e descanso. Sem estações, envelhecimento ou calendário anual.
- Noite: poucas incursões espaçadas e progressivas, com dissolução no amanhecer. Não transformar toda noite em perseguição constante; a luz comunica segurança.
- Cadeia emergente prioritária: NPC trabalha → noite → perigo → fuga → guarda reage → casa segura → ameaça perde alvo → amanhecer → curandeira trata feridos → NPC comenta → memória/relação registra.

---

## 12. Itens, progressão, economia, quests, relações, dungeons

- Inventário: item/quantidade/equipar/consumível/quest/único/vender/persistir. Sem quantidade negativa, único duplicado ou equipamento inválido.
- Sem classes rígidas; equipamento define builds (espada e escudo, espada grande, lança, adaga, arco, cajado/grimório). Equipamento muda comportamento, não só números.
- Atributos poucos e legíveis: VIT→HP, STR→dano físico, DEX→economia/agilidade/parry, INT→magia, VIG→stamina/defesa. Recalcular derivados após level, level-down, equipar, atributo, reset e load; nunca NaN.
- Level-down: registrar separadamente pontos **obtidos, disponíveis e investidos**. Equipamento vestido não cai por perda de nível.
- Morte: **um** Death Resolution System central. Overworld: resgate → curandeiro/local válido → perda de XP → possível level-down; inventário/equipamento preservados. Dungeon: perda limitada e controlada; itens essenciais, únicos e de quest protegidos.
- Economia simples: uma moeda; sem crafting, mineração, agricultura sistêmica ou dezenas de materiais. Não adicionar crafting “porque RPG costuma ter”.
- Quests: justificar por mundo, personagem, exploração ou progressão. Evitar “mate 10 X” em massa. Coletáveis não nascem em lugar inalcançável.
- Relações: compactas; respondem a decisões, feitos, promessas e ações testemunhadas — nunca “elogiar +5” repetido. Reputação é memória social.
- Dungeon: entrada física → confirmação → interior → boss ou morte → recompensa → saída; sem saída voluntária comum. Geração: **Seed → layout lógico → módulos de sala → encontros → validação → jogo**. Seeds reproduzíveis e registradas; seed impossível é rejeitada/regenerada. Salas de descanso não são checkpoint. Transição limpa só o transitório (projéteis, partículas, hazards, alvos inválidos) e preserva HP, inventário, equipamento, progresso, flags e companion. Recompensas únicas nunca duplicam por reload.

---

## 13. Save e persistência

- Save atual: `Application.persistentDataPath/grunwald-adventure-v1.json`. Não reescreva o armazenamento sem necessidade.
- Manual e autosave separados quando implementados. Escrita segura/atômica, **schema versionado**, migração, proteção contra corrupção, IDs estáveis.
- Persistir só o significativo: jogador, progressão, inventário, equipamento, atributos, quests, relações, flags, hora, estado importante de NPCs, companion, conclusão de dungeon, recompensas únicas.
- Testar: save/load/reload, campo ausente, versão antiga, único não duplicado, recompensa única, e invariantes (sem NaN, dinheiro ≥ 0, quantidades ≥ 0). Nunca esconder corrupção de estado.

---

## 14. Ferramentas, testes e auditoria

- **Três níveis:** lógica (determinística), integração (save, morte, dungeon, relações) e stress (cena pesada: noite, luzes, companions, NPCs, UI).
- Ferramentas existentes: `Vadronia > Verificar demo`, `Vadronia > Verificar exploração em Play`, `Tests/`, `Tools/`, pacote `com.unity.pipeline` 0.8.0-exp.1 para operar o Editor por CLI. Estenda esses pontos em vez de criar frameworks novos.
- Menu/overlay de debug no Editor: alterar hora, teleportar, dar item/XP, simular relação, recrutar companion, provocar morte/down, iniciar dungeon por seed, visualizar IA/hitboxes, FPS e entidades ativas.
- Assertions úteis: HP/stamina finitos; dinheiro e quantidades ≥ 0; IDs únicos; morto não ataca; quest concluída não ativa; civil em casa segura não sofre ameaça; boss alcançável; posição de companion válida; sem alvo inválido após transição; sem NaN.
- Verificação no Editor: compilar, Console sem erros/avisos, rodar as verificações, executar Play, inspecionar capturas da Game view. Testes programáticos **não substituem** o teste físico de teclado e a avaliação artística de Marcos — diga isso.
- Scripts temporários de verificação ficam fora de `Assets/` e não são reutilizados se geraram erros de runtime (ex.: redefinir geometria de sprite em uso).

---

## 15. Performance

Leve para hardware modesto. Alvo de engenharia (não promessa): PCs Windows fracos, ~2 GB de RAM e GPU integrada; 60 FPS normal, 30 FPS estáveis como fallback. Medir antes de otimizar. Sem atualizar o que está distante, sem pathfinding por frame, sem polling inútil, sem lixo excessivo, culling, poucas partículas e luzes. Vegetação densa por composição em chunks e sprites reutilizados, nunca milhares de objetos ativos.

---

## 16. Visual e game feel

- Pixel art medieval-fantasy, 2D top-down. Arte temporária/placeholder é válida durante a construção de sistemas; o pipeline visual definitivo vem depois de gameplay e integração.
- Reference sheets são concept, não spritesheets perfeitas: não copie inconsistências de escala, proporção, grade, densidade de pixel, direção ou footprint.
- Procedência da arte em `ARTWORK.md`. Preserve os PNGs originais e seus `.meta`.
- Caminhada: manter a correção artística de Marcos (passos baixos, alternância real das duas pernas, sem joelho alto).
- Resolução: **1080p** confirmada. “420p” está registrado mas **sem dimensões definidas**; não converta para 480p ou 720p. Separe resolução interna de saída.
- Noite legível; luzes de casas/postes/lanternas comunicam segurança; evitar glare.
- Game feel: flash, hitstop pequeno, shake pequeno, texto, som simples, telegraph. Ataque, parry, dano, clear e perigo precisam ser percebidos.

---

## 17. Escopo (20/80) e proibições

Antes de um sistema grande: “Qual é a menor implementação que entrega a maior parte da experiência?” Faça, teste, e só então expanda. Toda mecânica nova justifica gameplay, integração, testabilidade, consequência, reutilização, custo e escopo. Pergunta obrigatória: “Esta abstração resolve um problema real deste jogo?”

**NÃO:**

- recomeçar ou recriar tudo; copiar cegamente arquivos de protótipos ou versões antigas; misturar estados históricos com a base Unity validada;
- gerar executável/pacote Windows sem pedido;
- substituir arte original ou regenerar sprites chamando de transferência;
- avançar etapa grande de gameplay sem autorização de Marcos;
- duplicar Combat Core, NPC, inventário ou quests; criar IA única por NPC sem necessidade;
- simular distante sem necessidade; pathfind todo frame;
- priorizar arte sobre mecânica quebrada (salvo decisão explícita vigente de Marcos);
- overengineering; instalar dependência grande sem benefício concreto;
- expandir Migriard/Vitehria agora; criar muitas cidades antes de fechar o slice;
- remover cânone silenciosamente; resolver mistérios deliberadamente abertos; transformar nobres em vilões por conveniência;
- alterar controles arbitrariamente;
- duplicar recompensa única por reload; atacar civil em casa segura; fazer melee atravessar parede; dar conhecimento mágico à IA; esconder corrupção de estado;
- declarar teste, compilação, inspeção ou sincronização não realizados.

---

## 18. PROJECT_STATE, “continue” e Definition of Done

`PROJECT_STATE.md`, curto: **FEITO / ATUAL / PRÓXIMO / BUGS CONHECIDOS / DECISÕES IMPORTANTES**. Não é diário gigante. O histórico fica em `docs/HISTORY.md` (datado).

Quando receber “continue”:

1. leia `AGENTS.md` e `PROJECT_STATE`; 2. leia o contexto necessário; 3. inspecione os arquivos relevantes; 4. escolha a melhoria de maior valor dentro do escopo autorizado; 5. implemente; 6. compile/execute (modo COM Editor) ou marque como não verificado (modo SEM Editor); 7. teste; 8. tente quebrar; 9. corrija; 10. regressão; 11. atualize `PROJECT_STATE`; 12. continue até um ponto natural.

Escolha a tarefa por: impacto em gameplay + integração + risco de bug + reutilização + valor de teste + coerência de escopo.

**DONE** significa: funciona; compilado e executado no Editor; testado; integrado; edge cases principais avaliados; Console e estado inspecionados; save/load testado se afetado; entrada por teclado e controle quando aplicável; sem regressão conhecida; sem queda de performance perceptível; feedback suficiente; estado registrado. No modo SEM Editor, o máximo é **“escrito, pendente de verificação no Editor”**.

Relato final curto: **IMPLEMENTADO / VERIFICADO (e como) / NÃO VERIFICADO / PROBLEMAS CORRIGIDOS / INTEGRAÇÕES / KNOWN ISSUES / PRÓXIMO GANHO / PENDÊNCIA DE SINCRONIZAÇÃO COM A PASTA DO EDITOR.**

---

## 19. Comando final

Comece trabalhando, não apenas planejando.

Leia `AGENTS.md` e o `PROJECT_STATE`. Examine o que realmente existe na raiz deste repositório e em `Assets/`. Preserve o que funciona. Escolha a próxima melhoria de maior valor real **dentro do escopo autorizado** e execute o ciclo:

INSPECIONAR → IMPLEMENTAR → COMPILAR/EXECUTAR → TESTAR → TENTAR QUEBRAR → CORRIGIR → TESTAR INTEGRAÇÃO → REGRESSÃO → ATUALIZAR PROJECT_STATE → CONTINUAR.

Se uma etapa nova de gameplay precisar de decisão de Marcos, faça **uma** pergunta objetiva e prossiga com o que já está autorizado.

FIM DO SUPER MASTER PROMPT — PROJETO R / UNITY
