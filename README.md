## 开发内容记录
### 动画系统
主要参考Animancer插件https://assetstore.unity.com/packages/tools/animation/animancer-pro-v8-293522

基于Unity Playables系统开发、代码驱动的动画系统，用以替代AnimatorContorller，实现更加灵活的动画播放逻辑，支持线性混合动画和分层动画。

Assets/ARPGDemoScripts/MyPlugins/AnimationPlayer

### 行为树编辑器
主要参考：

https://github.com/thekiwicoder0/UnityBehaviourTreeEditor.git

https://github.com/HalfADog/Unity-ARPGGameDemo-TheDawnAbyss.git

支持节点的图形化编辑、运行时状态监控、操作撤销。

Assets/ARPGDemoScripts/MyPlugins/BehaviourTree

### 角色控制
实现了一个简单的可以进行奔跑、连段普通攻击、普攻衔接重击的角色，以及简单的伤害和Buff逻辑。

## 随记
大多系统开发的尝试都基本中道崩殂，导致这个项目好像成了一大坨Shit。

动画系统和行为树编辑器勉强能用，一些基于UGUI扩展的组件功能合格，但还只是UI层面的功能，没有经过逻辑层的测试，而且还没有一个像样的UI系统来管理游戏运行时的UI。

本来尝试开发的技能系统参考虚幻引擎的GAS，AbilitySystemComponent、AttributeSet、GameplayEffect的逻辑大概能用，不过到了AbilityTask，没有成熟的编辑器或者说技能编辑器，导致技能的表现层和一些区间逻辑（比如连段区间）非常难以编辑，而且现在写的AbilityTaskEditor也还不支持循环执行，不好做蓄力攻击之类的行为。而之前对于技能编辑器开发的尝试，只弄出来了一个刻度尺，虽然UI交互效果不错，不过没有后续了，现在看来，得先把技能系统搞清楚再说。
