using UnityEngine;
namespace Vadronia
{
    // Pixel bounds of the approved-reference atlas, in source top-left coordinates.
    internal static class BlueTownAtlas
    {
        static Rect Cut(int x,int y,int w,int h) => new Rect(x,1086-y-h,w,h);
        internal static readonly Rect[] Frames = {
            Cut(15,125,335,333), Cut(355,93,350,369), Cut(705,36,390,428), Cut(1099,124,342,341),
            Cut(30,494,310,314), Cut(360,468,360,332), Cut(725,494,365,315), Cut(1148,580,250,228),
            Cut(66,840,235,213), Cut(425,914,235,139), Cut(722,822,374,236), Cut(1225,824,170,230)
        };
        static Rect Prop(int col,int row,int x,int y,int w,int h) => new Rect(col*418+x,1254-(row*418+y+h),w,h);
        internal static readonly Rect[] Props = {
            Prop(0,0,15,108,390,295),Prop(1,0,10,100,394,318),Prop(2,0,12,108,395,306),
            Prop(0,1,10,140,398,256),Prop(1,1,0,110,414,285),Prop(2,1,0,82,412,321),
            Prop(0,2,22,61,383,322),Prop(1,2,7,33,404,345),Prop(2,2,3,88,399,300)
        };
        internal static int PropReplacement(TownProp p)
        {
            if(p.Atlas!=1)return -1;
            switch(p.Sprite){case 7:return 0;case 8:return 1;case 9:return 2;case 10:return 3;
                case 13:return 4;case 14:return 5;case 19:return 6;case 20:return 7;case 21:return 8;default:return -1;}
        }
        internal static int Replacement(TownProp p)
        {
            if(p.Atlas==2)return p.Sprite;
            if(p.Atlas==0){if(p.Sprite<=3)return p.Sprite;if(p.Sprite==6)return 7;if(p.Sprite==7)return 10;}
            if(p.Atlas==1){if(p.Sprite<=6)return p.Sprite==4?5:(p.Sprite%2==0?0:4);if(p.Sprite==11)return 9;if(p.Sprite==12)return 8;}
            return -1;
        }
    }
}
