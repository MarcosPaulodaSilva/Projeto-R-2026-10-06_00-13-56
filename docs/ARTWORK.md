# Arte — Projeto R / Vadronia

Este documento registra apenas o que interessa para a árvore atual. Experimentos removidos continuam recuperáveis no histórico Git.

## Assets ativos

- `Assets/Resources/Vadronia/terrain-v3.png`: terreno atual de Grünwald (v2 + caminhos até as portas, viela, pátios e sombra de assentamento). É **gerado** por `Tools/build_terrain.py` a partir do v2 e do `TownLayout.cs`; se faltar, o jogo usa o v2.
- `Assets/Resources/Vadronia/terrain-v2.png`: terreno pintado base (duas ruas e praça); fonte do v3.
- `Assets/Resources/Vadronia/town.png`: atlas do cenário.
- `Assets/Resources/Vadronia/town-extra.png`: atlas extra da vila (casas de costas, telhados recoloridos, barril, caixotes, feno, cerca, banco, mural, canteiro, carroça, caminho de terra, carvalho dourado, carvalho claro, pinheiro azulado, arbustos e pedras). É **gerado** por `Tools/build_town_extra.py` a partir de `town.png` e de desenho procedural; os quadros ficam em `TownAtlasLayout.Extra`. O jogo funciona sem ele (avisa no Console e omite os adereços extras).
- `Tools/preview_town.py`: prévia offline do mapa lida direto de `TownLayout.cs`; também confere spawn, ervas, colisões e a patrulha de Konrad (`--blocks` desenha colisões e rota).
- `docs/references/grunwald-map-preview-2026-10-06.png`: prévia estática gerada para revisão visual; não é importada pelo Unity.
- `Assets/Resources/Vadronia/player-video/`: quadros atuais do Player derivados do vídeo de referência.
- `Assets/Resources/Vadronia/characters-original.png`: atlas original ainda usado por Konrad. Foi movido de `Assets/Art/characters.png` mantendo o mesmo `.meta`/GUID.
- `Assets/Resources/Vadronia/UI/Inter-Regular.ttf`: fonte da interface, com licença em `Inter-LICENSE.txt`.

## Arte aposentada da árvore atual

Os seguintes arquivos não tinham referência por nome nem por GUID e foram removidos para não serem importados pelo Unity:

- `characters-neutral.png`;
- `player-walk.png`;
- `conrad-walk.png`;
- `NeutralAtlasLayout.cs`;
- `WalkAtlasLayout.cs`.

Esses materiais não precisam de uma pasta de “backup” dentro do projeto: o histórico Git preserva versões anteriores.

## Referências

Imagens conceituais ficam em `docs/references/`. Vídeos e outros materiais que não precisam ser importados pelo Unity ficam em `docs/media/`.

A fonte Inter é distribuída sob a SIL Open Font License; a licença acompanha o arquivo no projeto.
