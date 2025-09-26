
namespace ARPGDemo.BattleSystem
{
    public interface IBattleHandler
    {
        void OnFixedUpdate(); //被周期方法FixedUpdate所调用，主要是为了能够手动控制调用的顺序，而不是让Unity自动调用。
    }

    // public interface IInteractable<T> //传入的泛型类型就是交互对象的实际类型、也就是交互内容。
    public interface IInteractable<T> : IInteractable//传入的泛型类型就是交互对象的实际类型、也就是交互内容。
    {
        // int priority { get; }

        /*Tip：有个隐含规定，因为物理检测是基于组件的，所以交互对象的实际类型应该是组件，但是它也只是提供一个外壳、以便能够被视为交互对象，最终参与逻辑时就要返回自己内部存储的内容，
        比如PickUp拾取行为对应的内容就应该是Item，但是会有一个组件类封装Item比如PickableItem*/
        // InteractionType interactionType { get; }

        T InteractableTarget();
    }
    /*Tip：通过IInteractable可以引用所有IInteractable<T>类型，根据interactionType得知其实际类型并且将其转换，比如类型为PickUp，那么就转换为IInteractable<Item>，然后就可以
    调用InteractableTarget方法获取到交互对象了。*/
    public interface IInteractable
    {
        int priority { get; }
        InteractionType type { get; }

        void InteractionEnd(); //交互行为结束
    }
}