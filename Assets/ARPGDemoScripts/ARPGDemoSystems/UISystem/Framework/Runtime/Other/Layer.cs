using System;
using System.Collections.Generic;

namespace ARPGDemo.UISystem_Old
{
    //这里应该要对应于Unity编辑器中定义的对象层级，这种应该是可以自动生成的。
    public static class Layer
    {
        public const int Default = 0;
        public const int TransparentFX = 1;
        public const int IgnoreRaycast = 2;
        public const int Water = 4;
        public const int UI = 5;
        public const int UIRenderToTarget = 6;
    }
}
