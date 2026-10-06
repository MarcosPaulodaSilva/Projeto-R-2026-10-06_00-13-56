# Arte da atualização — 05/10/2026

Produzida com a ferramenta nativa image_gen, sem CLI de geração, com fundo transparente. Os arquivos originais recebidos da nuvem não foram substituídos. Abaixo estão os briefings resumidos, não uma transcrição literal dos prompts.

## terrain-v2.png

Saída original: `C:/Users/Marcos/.codex/generated_images/01a103c8-0006-75b0-a2a2-cae8c8a4973b/exec-feda40c3-c558-4624-9012-c9e649acc933.png`.

Briefing: chão de uma vila medieval 2D top-down ortogonal, proporção do mapa 28x22, praça central de pedra e ruas de terra norte/sul e transversais. Grama detalhada, flores pequenas, trevos e musgo; luz quente de fim de tarde. Apenas terreno, sem prédios ou personagens. Textura usada no chão da vila.

## characters-neutral.png

Saída original: `C:/Users/Marcos/.codex/generated_images/01a103c8-0006-75b0-a2a2-cae8c8a4973b/exec-5aa1627c-9875-4680-9c58-ff94534d3a9e.png`.

Briefing: oito figuras neutras, quatro colunas por duas linhas, frente/direita/costas/esquerda. Player de roupa azul e creme, calças e botas marrons; Conrad barbado, azul e dourado. Corpo visível, pernas retas, botas apoiadas e sem joelhos elevados. A animação final articula os recortes no Unity e alterna as pernas pela distância percorrida.

As duas tentativas anteriores de atlas de caminhada foram descartadas após a crítica de Marcos ao joelho alto; não são usadas pelo jogo. As imagens novas foram inspecionadas; o canal alpha foi confirmado no Unity. PNGs importados sem compressão, filtro Point, sem mipmaps.

## Fonte da interface

Inter-Regular.ttf foi copiada dos recursos locais do Unity 6000.6.0f1. A licença SIL Open Font License do projeto Inter acompanha o arquivo em `Assets/Resources/Vadronia/UI/Inter-LICENSE.txt`, obtida de https://github.com/rsms/inter/blob/master/LICENSE.txt. Os assets TextCore e PanelSettings foram criados pela API do Editor, com GUIDs gerados pelo Unity.
