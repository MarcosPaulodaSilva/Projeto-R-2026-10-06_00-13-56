using UnityEngine;
namespace Vadronia
{
    internal static class WalkAtlasLayout
    {
        internal const float PlayerPPU = 178.901515f;
        internal static readonly Rect[] Player =
        {
            new Rect(141, 947, 141, 296),
            new Rect(419, 947, 127, 296),
            new Rect(708, 947, 126, 297),
            new Rect(992, 947, 131, 297),
            new Rect(124, 637, 194, 296),
            new Rect(434, 637, 115, 297),
            new Rect(684, 637, 187, 296),
            new Rect(1013, 637, 114, 297),
            new Rect(139, 319, 137, 296),
            new Rect(420, 323, 122, 292),
            new Rect(711, 319, 121, 296),
            new Rect(1002, 323, 132, 292),
            new Rect(87, 13, 207, 292),
            new Rect(412, 13, 113, 294),
            new Rect(660, 13, 197, 294),
            new Rect(986, 12, 118, 295),
        };
        internal static readonly Vector2[] PlayerPivots =
        {
            new Vector2(0.492908f, 0),
            new Vector2(0.480315f, 0),
            new Vector2(0.511905f, 0),
            new Vector2(0.507634f, 0),
            new Vector2(0.543814f, 0),
            new Vector2(0.565217f, 0),
            new Vector2(0.537433f, 0),
            new Vector2(0.561404f, 0),
            new Vector2(0.529197f, 0),
            new Vector2(0.512295f, 0),
            new Vector2(0.512397f, 0),
            new Vector2(0.473485f, 0),
            new Vector2(0.425121f, 0),
            new Vector2(0.438053f, 0),
            new Vector2(0.434010f, 0),
            new Vector2(0.419492f, 0),
        };
        internal const float ConradPPU = 184.431818f;
        internal static readonly Rect[] Conrad =
        {
            new Rect(129, 937, 157, 307),
            new Rect(403, 938, 141, 306),
            new Rect(695, 937, 162, 307),
            new Rect(991, 938, 135, 306),
            new Rect(110, 630, 213, 302),
            new Rect(423, 632, 117, 301),
            new Rect(694, 630, 197, 303),
            new Rect(1007, 631, 121, 302),
            new Rect(116, 316, 165, 308),
            new Rect(403, 320, 144, 303),
            new Rect(691, 316, 175, 308),
            new Rect(992, 320, 142, 303),
            new Rect(84, 10, 218, 303),
            new Rect(412, 11, 120, 303),
            new Rect(659, 11, 216, 301),
            new Rect(1000, 9, 112, 306),
        };
        internal static readonly Vector2[] ConradPivots =
        {
            new Vector2(0.490446f, 0),
            new Vector2(0.496454f, 0),
            new Vector2(0.503086f, 0),
            new Vector2(0.496296f, 0),
            new Vector2(0.485915f, 0),
            new Vector2(0.517094f, 0),
            new Vector2(0.482234f, 0),
            new Vector2(0.495868f, 0),
            new Vector2(0.545455f, 0),
            new Vector2(0.506944f, 0),
            new Vector2(0.520000f, 0),
            new Vector2(0.503521f, 0),
            new Vector2(0.504587f, 0),
            new Vector2(0.504167f, 0),
            new Vector2(0.493056f, 0),
            new Vector2(0.504464f, 0),
        };
    }
}
