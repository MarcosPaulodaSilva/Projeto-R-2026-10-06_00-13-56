# Vadronia — exploração de Grünwald

Unity 6.6 (6000.6.0f1), C#, 2D top-down ortogonal, pipeline Built-in. Atualização visual e de exploração aplicada em 05/10/2026 no projeto do Editor: `C:/Users/Marcos/Downloads/VadroniaDemo`.

## Jogar no Editor

Abra a cena `Assets/Scenes/Vadronia.unity` e pressione Play. Em uma cópia nova, a cena é criada pelo inicializador do Editor. Os assets da interface acompanham o projeto; se necessário, execute **Vadronia > Preparar interface** antes de Play.

| Tecla | Ação |
|---|---|
| WASD / setas | Caminhar |
| Shift | Correr, consumindo fôlego |
| Espaço | Esquiva com custo e recarga |
| E | Interagir / continuar diálogo |
| Esc | Pausar / fechar diálogo |
| F1 | Ocultar interface |
| F5 | Salvar progresso |

Converse com Conrad, colha as três porções de ervas e volte para receber 25 moedas. O poço e a estalagem recuperam fôlego; a guilda tem uma placa informativa. O progresso da missão é salvo automaticamente e pode ser salvo com F5. O arquivo fica em `Application.persistentDataPath/grunwald-adventure-v1.json`.

## Atualização

- Terreno detalhado, vegetação nas bordas, sombras de contato, folhas, fumaça e lanternas.
- Arte neutra nova para Player e Conrad. Pernas independentes em fases opostas, passos baixos, sem os antigos quadros de joelho alto. Ciclo de 1,8 unidades percorridas; caminhada a 2,4 unidades/s. Recortes laterais de pernas usam geometria para excluir a bota sobreposta.
- Câmera suave com antecipação e abertura ao correr; interface com fôlego, moedas, objetivo, diálogos e pausa.
- Corrida, esquiva com colisão, missão de coleta e salvamento local.

Esta é uma versão de exploração ampliada. Os prédios ainda são fachadas; não há interiores ou combate. A animação é procedural sobre recortes da arte, sem novos clipes desenhados quadro a quadro.

## Verificações

- **Vadronia > Verificar demo**: 20 verificações de lógica e conferência dos recursos/pipeline.
- Em Play, **Vadronia > Verificar exploração em Play**: 11 verificações de movimento, corrida, colisão na esquiva, repouso, pausa, diálogos, missão, recompensa, save em disco e interface. O teste restaura os arquivos de progresso anteriores; execute fora de uma sessão de jogo importante.
- As duas rotinas foram executadas no Editor local em 05/10/2026. Console sem erros de compilação/execução no fechamento da validação. Cenário, interface e poses de caminhada inspecionados em capturas do Editor. Testes de controles foram programáticos; sensação ao jogar precisa da avaliação de Marcos.

Nenhum executável ou pacote de distribuição Windows foi gerado. A pasta histórica `VadroniaDemo` foi mantida para preservar o projeto aberto.

A procedência da arte nova está em `ARTWORK.md`. As imagens originais e seus .meta foram preservados.
