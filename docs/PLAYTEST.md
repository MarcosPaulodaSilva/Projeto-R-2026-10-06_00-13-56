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
| Z | Correr, consumindo fôlego |
| Espaço | Esquiva com custo e recarga |
| Mouse esquerdo | Combo de espada de 3 golpes |
| E | Redemoinho de Aço |
| H | Investida Cortante |
| T | Interagir / continuar diálogo |
| Esc | Pausar / fechar diálogo |
| F1 | Ocultar interface |
| F5 | Salvar progresso |

Converse com Konrad, colha as três porções de ervas e volte para receber 25 moedas. Grünwald também tem Helga (estalajadeira), Bruno (ferreiro), Maren (herbalista), Lúcia e Tomás; aproxime-se e use T para conversar. Helga recupera o fôlego, Bruno explica controles/combo e Maren adapta a dica ao progresso da missão. O poço e a estalagem também recuperam fôlego; a guilda tem uma placa informativa. O progresso da missão é salvo automaticamente e pode ser salvo com F5. O arquivo fica em `Application.persistentDataPath/grunwald-adventure-v1.json`.

As teclas `W`, `S`, `X` e `2` não devem ser usadas como controles obrigatórios neste projeto, pois não funcionam no teclado de Marcos.

## Atualização

- Terreno detalhado, vegetação nas bordas, sombras de contato, folhas, fumaça e lanternas.
- Player usa quadros derivados do vídeo de referência; Konrad mantém o atlas original. A caminhada usa passos baixos e progressão pela distância percorrida.
- Câmera suave com antecipação e abertura ao correr; interface com fôlego, moedas, objetivo, diálogos e pausa.
- Corrida, esquiva com colisão, missão de coleta e salvamento local.
- Cinco NPCs extras com rotas próprias, conversa e funções simples ligadas à vila/missão; cada um usa pixel art procedural temporária própria em vez de reutilizar visualmente o atlas de Konrad.

Os prédios ainda são fachadas e não há interiores. O combo e as habilidades E/H podem atingir o boneco de treino perto do ponto inicial, que mostra vida e se recupera. Ainda não há inimigos reais nem HP dos NPCs. A caminhada usa quadros completos extraídos do vídeo, em oito direções; sudoeste espelha sudeste. Poses adicionais de combate/corrida são aplicadas sobre esses quadros. No menu ESC, o botão de música alterna o mudo e salva a preferência.

## Verificações

- **Vadronia > Verificar demo**: lógica e conferência de recursos/pipeline.
- Em Play, **Vadronia > Verificar exploração em Play**: movimento, missões, saves, NPCs e oito direções. Restaura os arquivos de progresso anteriores; execute fora de uma sessão de jogo importante.
- Em Play, **Vadronia > Verificar combate em Play**: distância da investida em diferentes taxas de atualização, dano e ângulo dos golpes, recargas, colisões e interrupção. Usa personagem/alvos temporários e não grava saves.
- Os resultados e limites da validação mais recente ficam em `PROJECT_STATE.md`. Os testes usam entradas programáticas; sensação ao jogar e aprovação do visual precisam da avaliação de Marcos.

Nenhum executável ou pacote de distribuição Windows foi gerado. Este repositório é o projeto Unity aberto localmente em `C:/Users/Marcos/Downloads/Projeto R/Projeto R VadroniaDemo`.

A procedência e o estado da arte estão em `docs/ARTWORK.md`. As imagens originais e seus .meta foram preservados.
