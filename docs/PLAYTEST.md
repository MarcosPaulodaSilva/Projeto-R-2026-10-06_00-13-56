# Playtest — Vadronia / Grünwald

Unity 6.6 (6000.6.0f1), C#, 2D top-down ortogonal, pipeline Built-in. Projeto local atual aberto no Editor: `C:/Users/Marcos/Downloads/Projeto R/Projeto R VadroniaDemo`.

## Jogar no Editor

Abra a cena `Assets/Scenes/Vadronia.unity` e pressione Play. Em uma cópia nova, a cena é criada pelo inicializador do Editor. Os assets da interface acompanham o projeto; se necessário, execute **Vadronia > Preparar interface** antes de Play.

| Tecla | Ação |
|---|---|
| R / ↑ | Caminhar para cima |
| F / ↓ | Caminhar para baixo |
| D / ← | Caminhar para a esquerda |
| G / → | Caminhar para a direita |
| Shift | Correr, consumindo fôlego |
| Espaço | Esquiva com custo e recarga |
| E | Interagir / continuar diálogo |
| Esc | Pausar / fechar diálogo |
| F1 | Ocultar interface |
| F5 | Salvar progresso |

Converse com Konrad, colha as três porções de ervas e volte para receber 25 moedas. O poço e a estalagem recuperam fôlego; a guilda tem uma placa informativa. O progresso da missão é salvo automaticamente e pode ser salvo com F5. O arquivo fica em `Application.persistentDataPath/grunwald-adventure-v1.json`.

As teclas `W`, `S`, `X` e `2` não devem ser usadas como controles obrigatórios neste projeto, pois não funcionam no teclado de Marcos.

## Atualização

- Terreno detalhado, vegetação nas bordas, sombras de contato, folhas, fumaça e lanternas.
- Player usa quadros derivados do vídeo de referência; Konrad mantém o atlas original. A caminhada usa passos baixos e progressão pela distância percorrida.
- Câmera suave com antecipação e abertura ao correr; interface com fôlego, moedas, objetivo, diálogos e pausa.
- Corrida, esquiva com colisão, missão de coleta e salvamento local.

Esta é uma versão de exploração ampliada. Os prédios ainda são fachadas; não há interiores ou combate. A animação é procedural sobre recortes da arte, sem novos clipes desenhados quadro a quadro.

## Verificações

- **Vadronia > Verificar demo**: 20 verificações de lógica e conferência dos recursos/pipeline.
- Em Play, **Vadronia > Verificar exploração em Play**: 11 verificações de movimento, corrida, colisão na esquiva, repouso, pausa, diálogos, missão, recompensa, save em disco e interface. O teste restaura os arquivos de progresso anteriores; execute fora de uma sessão de jogo importante.
- As duas rotinas foram executadas no Editor local em 05/10/2026. Console sem erros de compilação/execução no fechamento da validação. Cenário, interface e poses de caminhada inspecionados em capturas do Editor. Testes de controles foram programáticos; sensação ao jogar precisa da avaliação de Marcos.

Nenhum executável ou pacote de distribuição Windows foi gerado. Este repositório é o projeto Unity aberto localmente em `C:/Users/Marcos/Downloads/Projeto R/Projeto R VadroniaDemo`.

A procedência e o estado da arte estão em `docs/ARTWORK.md`. As imagens originais e seus .meta foram preservados.
