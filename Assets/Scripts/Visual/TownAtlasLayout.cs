using UnityEngine;
namespace Vadronia
{
    internal static class TownAtlasLayout
    {
        internal static readonly Rect[] Frames =
        {
            new Rect(15, 525, 310, 456),
            new Rect(349, 524, 427, 467),
            new Rect(780, 522, 390, 490),
            new Rect(1179, 521, 347, 435),
            new Rect(19, 39, 394, 445),
            new Rect(442, 39, 315, 455),
            new Rect(804, 47, 272, 322),
            new Rect(1139, 41, 383, 369),
        };
        // Resources/Vadronia/town-extra.png, built by Tools/build_town_extra.py.
        // Index: 0 casa de costas · 1 estalagem de costas · 2 casa telhado azul · 3 casa telhado verde
        // 4 estalagem telhado vermelho · 5 casa de costas azul · 6 estalagem de costas vermelha
        // 7 barril · 8 caixotes · 9 feno · 10 cerca · 11 banco · 12 mural · 13 canteiro · 14 carroça · 15 caminho de terra
        // 16 carvalho dourado · 17 carvalho claro · 18 pinheiro azulado · 19 arbusto · 20 arbusto florido · 21 pedras
        internal static readonly Rect[] Extra =
        {
            new Rect(0, 964, 305, 438), // 0 cottage_back
            new Rect(313, 946, 427, 456), // 1 inn_back
            new Rect(748, 946, 310, 456), // 2 cottage_blue
            new Rect(1066, 946, 310, 456), // 3 cottage_green
            new Rect(1384, 935, 427, 467), // 4 inn_red
            new Rect(0, 489, 305, 438), // 5 cottage_back_blue
            new Rect(313, 471, 427, 456), // 6 inn_back_red
            new Rect(748, 866, 54, 61), // 7 barrel
            new Rect(810, 844, 94, 83), // 8 crates
            new Rect(912, 870, 84, 57), // 9 hay
            new Rect(1004, 867, 118, 60), // 10 fence
            new Rect(1130, 867, 104, 60), // 11 bench
            new Rect(1242, 810, 96, 117), // 12 notice_board
            new Rect(1346, 882, 118, 45), // 13 flowerbed
            new Rect(1472, 837, 139, 90), // 14 cart
            new Rect(1619, 837, 130, 90), // 15 dirt_patch
            new Rect(0, 18, 394, 445), // 16 oak_gold
            new Rect(402, 18, 394, 445), // 17 oak_light
            new Rect(804, 8, 315, 455), // 18 pine_blue
            new Rect(1127, 404, 84, 59), // 19 bush
            new Rect(1219, 404, 84, 59), // 20 bush_flowers
            new Rect(1311, 423, 84, 40), // 21 rocks
        };
    }
}
