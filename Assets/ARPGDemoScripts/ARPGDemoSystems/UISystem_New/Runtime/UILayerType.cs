
namespace ARPGDemo.UISystem_New
{
    public enum UILayerType
    {
        Scene = 1000, //位于场景的3DUI。
        Background = 2000,  // 底层，主界面、背景UI
        Normal = 3000,      // 中层，普通功能界面，大部分UI面板都放在这里，遵循一致的“后打开先关闭”的顺序,
        PopUp = 4000,       // 弹窗层，弹窗、提示框，属于游戏内容本身，但是要在普通UI面板之上。
        System = 5000       // 系统层，系统提示、加载界面，不属于游戏内容本身的一些UI。
    }

    //这里应该要对应于Unity编辑器中定义的对象层级，这种应该是可以自动生成的。
    public static class Layer
    {
        public const int Default = 0;
        public const int TransparentFX = 1;
        public const int IgnoreRaycast = 2;
        public const int Water = 4;
        public const int UI = 5;
    }
}