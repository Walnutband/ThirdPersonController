namespace ARPGDemo.UISystem_Old
{
    //TODO：这里是假定每个UI视图只会有一个实例，虽然好像也够用了，但总感觉不太合适。
    //在UIType中定义了的成员，就应该在UIConfig文件中编写相关配置信息，当然还要有对应的预制体。
    public enum UIViewType
    {
        //启动界面（网游中可能叫做登录界面）
        UIStartView, 
        //选择是否退出游戏（区别于是否退回到启动界面）
        UIExitView, 
        //加载界面（这个界面应该说是贯穿游戏始终，不过注意这是一个专门的加载界面，通常用于加载时间较长的地方，如果是加载时间很短，甚至只是一个过渡效果的话，就直接调用UIManager的FadeOut和FadeIn方法即可）
        UILoadingView,
        UIMainView,
        UISettingsView,
        UIAttributeView,
        UICharacterView,
        //最大值用于稍微规定常量上限，如果是按照枚举常量名来使用的话，其实一般用不上，但是如果使用枚举常量值，那可能就会作为判断条件了
        Max, 
    }
}
