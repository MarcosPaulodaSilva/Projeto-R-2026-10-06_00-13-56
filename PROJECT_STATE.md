# Estado atual — 06/10/2026

Este repositório é agora a fonte oficial da implementação Unity do Projeto R / Vadronia.

## FEITO

- Unity 6000.6.0f1, 2D top-down ortogonal, Built-in pipeline.
- Grünwald exterior com terreno, cenário e ambientação.
- caminhada procedural com passos baixos e pernas alternadas;
- corrida/fôlego e esquiva;
- câmera suave;
- HUD em UI Toolkit;
- interação com NPC;
- missão de três ervas;
- recompensa única de 25 moedas;
- save local;
- cena real, Packages e ProjectSettings preservados do projeto local mais novo.

## VALIDADO

Último registro local: **20 verificações de lógica + 11 verificações em Play aprovadas**. Isso antecede a reorganização de pastas desta migração; após puxar a migração no PC, o Unity deve recompilar e Play Mode deve ser revalidado.

## ATUAL

O código foi organizado por responsabilidade sem substituir as versões locais mais novas nem os seus `.meta`.

Projeto local: `C:/Users/Marcos/Downloads/Projeto R/Projeto R VadroniaDemo`.

## PRÓXIMO

1. sincronizar esta `main` com a pasta local;
2. abrir o Unity e aguardar reimportação/recompilação;
3. executar as verificações do menu Vadronia e Play Mode;
4. continuar a próxima mecânica autorizada.

## LIMITES

Prédios externos, um NPC, sem combate e sem interiores. Não gerar build Windows sem pedido explícito.
