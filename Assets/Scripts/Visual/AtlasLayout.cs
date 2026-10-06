using UnityEngine;
namespace Vadronia
{
    // Alpha bounds measured from the supplied atlas; avoids clipping uneven cell spacing.
    internal static class AtlasLayout
    {
        internal static readonly Rect[] Frames =
        {
            new Rect(18, 936, 130, 272),
            new Rect(174, 932, 129, 277),
            new Rect(334, 936, 121, 271),
            new Rect(490, 931, 123, 277),
            new Rect(637, 931, 141, 281),
            new Rect(794, 931, 141, 285),
            new Rect(951, 931, 136, 281),
            new Rect(1103, 931, 138, 280),
            new Rect(16, 629, 140, 276),
            new Rect(172, 628, 140, 277),
            new Rect(327, 628, 138, 277),
            new Rect(479, 629, 139, 278),
            new Rect(636, 628, 144, 281),
            new Rect(791, 627, 146, 283),
            new Rect(945, 627, 145, 281),
            new Rect(1101, 629, 143, 278),
            new Rect(14, 331, 135, 276),
            new Rect(172, 331, 131, 275),
            new Rect(327, 331, 131, 273),
            new Rect(481, 331, 128, 277),
            new Rect(636, 331, 139, 278),
            new Rect(790, 331, 142, 278),
            new Rect(947, 331, 138, 278),
            new Rect(1101, 331, 140, 276),
            new Rect(15, 34, 136, 272),
            new Rect(172, 31, 131, 277),
            new Rect(326, 31, 142, 271),
            new Rect(482, 31, 131, 272),
            new Rect(637, 32, 138, 276),
            new Rect(790, 32, 143, 277),
            new Rect(947, 31, 142, 277),
            new Rect(1101, 31, 140, 274),
        };
    }
}
