# Equipamentos — arte e implementação

Gerados em 09/10/2026 com a ferramenta nativa de geração de imagens. Os PNGs foram copiados sem editar pixels. As caixas de recorte em JSON foram calculadas pela transparência dentro das células. Importação Unity: Point, sem mipmaps, sem compressão, alpha preservado.

## Briefs de geração (resumo dos prompts)

- **swords.png**: atlas transparente de oito espadas de fantasia medieval, grade 4×2, lâminas verticais completas e separadas, sem texto. Ferro, aço, florete dourado, falcata, guarda azul/prata, gelo, guilda azul/dourado, montante. Pixel art detalhada, paleta coerente com Vadronia.
- **armor.png**: atlas transparente de 24 peças, grade 6×4; cada linha é um conjunto (viajante azul, batedor couro, guarda prata/azul, guilda azul/dourado). Colunas: cabeça, torso, luvas, calças, botas, capa. Peças isoladas, frontal, sem corpos nem texto.
- **accessories.png**: atlas transparente 4×2 de oito acessórios medievais: anéis de cobre/safira/âmbar, pingente de cristal, talismã de couro, broche de folha, medalhão da guilda, amuleto de ferro. Mesmo estilo, sem texto.

## Fontes locais da geração

Diretório: `C:/Users/Marcos/.codex/generated_images/01a103c8-0006-75b0-a2a2-cae8c8a4973b/`

- Espadas: `exec-bd6a3761-3516-4d35-a7d9-fdd7891e7213.png`
- Armaduras: `exec-27f868ef-c313-49dd-9c0c-aa4c6de3d703.png`
- Acessórios: `exec-509bb805-3dee-4679-bf19-d5b01d3a103d.png`

Arquivos entregues em `Assets/Resources/Vadronia/Equipment/`, com seus metadados Unity.

## Uso final após correção de Marcos

Os sprites são usados somente como ícones no inventário/equipamentos. Marcos pediu a retirada das sobreposições do personagem. A prévia e o mundo preservam o visual original; equipar afeta apenas os atributos. Ataques não foram implementados. Nenhum personagem ou NPC foi substituído por outro modelo.
