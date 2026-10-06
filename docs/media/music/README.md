# Músicas — Projeto R

Esta pasta reúne as trilhas musicais oficiais do Projeto R e registra o contexto de uso de cada faixa.

| Arquivo | Contexto de uso |
|---|---|
| `Castle_Music_Legacy_in_the_Stone_Walls.mp4` | **Castelo / áreas reais.** Trilha de exploração e ambientação em castelos, especialmente espaços ligados à realeza de Vadrônia. Priorizar momentos sem combate e cenas de presença, tradição e imponência. |
| `Dungeon_Music_Crowns_in_the_Cold_Mist.mp4` | **Dungeons.** Trilha-base para exploração de masmorras, corredores, salas e áreas de tensão antes do confronto principal. |
| `Boss_Fight_Music_Iron_Weeps_for_Stone.mp4` | **Batalhas contra bosses.** Usar em confrontos de chefe e momentos de combate de alta intensidade. |
| `Tema_Project_Music_Beyond_the_Castle_Threshold.mp4` | **Tema principal do Projeto R.** Faixa de identidade geral do projeto, indicada para apresentação/tela principal e momentos especiais em que o tema central do jogo deve ser reforçado. |
| `Vilage_Music_Sunlight_on_the_Thatch.mp4` | **Vilarejos.** Trilha de ambientação tranquila para o vilarejo inicial e outros vilarejos em momentos cotidianos, principalmente fora de situações de ameaça. |
| `CIty_Music_Morning_at_the_Citadel_Gates.mp4` | **Cidades.** Trilha urbana para cidades e áreas próximas a cidadelas/portões, com foco em exploração e rotina civil. |

## Regras de integração

- A música de **boss** deve substituir temporariamente a trilha de dungeon/área durante o confronto e devolver a trilha adequada após o término.
- As faixas de **vilarejo**, **cidade** e **castelo** funcionam como ambientação de exploração; eventos de ameaça ou combate podem interrompê-las conforme o sistema de áudio do jogo.
- O **tema principal** deve ser tratado como faixa de identidade do Projeto R, e não como música ambiente comum em loop contínuo.
- Manter os nomes dos arquivos estáveis para facilitar a futura ligação com o sistema de áudio do jogo.
