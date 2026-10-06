# Arte — Projeto R / Vadronia

Este documento registra apenas o que interessa para a árvore atual. Experimentos removidos continuam recuperáveis no histórico Git.

## Assets ativos

- `Assets/Resources/Vadronia/terrain-v2.png`: terreno atual de Grünwald.
- `Assets/Resources/Vadronia/town.png`: atlas do cenário.
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
