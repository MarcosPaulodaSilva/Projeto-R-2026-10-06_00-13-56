# Estado atual — 05/10/2026

Marcos autorizou uma grande atualização gráfica e mecânica, com visual/animação como prioridade. O pedido mais recente exige passos baixos e as duas pernas alternando. A restrição anterior de esperar após o passo 1 foi substituída por essa autorização.

Implementado e aplicado à pasta real do Editor: terreno novo, personagens neutros com pernas animadas independentemente, ambientação, câmera suave, HUD em UI Toolkit, corrida/fôlego, esquiva, interação com Conrad, coleta de três ervas, recompensa única de 25 moedas e save local.

Unity 6000.6.0f1: 20 verificações de lógica + 11 verificações em Play passaram. Capturas de cenário, HUD e poses de caminhada inspecionadas. Preservados cena, configuração local, arte original, GUIDs e progresso anterior aos testes. Backup anterior à atualização em `.local-backups/before-visual-update-20261004-220139` no workspace, fora do Git.

A documentação operacional está em LEIA-ME.md. O histórico da transferência da nuvem está em `../project-state/unity-local-2026-10-04.md`. A versão HTML foi separada em `../../html/`.

Limites: prédios externos, um NPC, sem combate/interiores; caminhada procedural com recortes. Validado por ferramentas e capturas; avaliação manual da sensação do movimento cabe ao teste em Game. Não gerar builds Windows sem novo pedido explícito.
