// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2024 Kybernetik //

using UnityEngine;

namespace Animancer.Samples.FineControl
{
    /// <summary>
    /// Attempts to interact with whatever <see cref="IInteractable"/>
    /// the cursor is pointing at when the user clicks the mouse.
    /// </summary>
    /// 
    /// <remarks>
    /// <strong>Sample:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/samples/fine-control/doors">
    /// Doors</see>
    /// </remarks>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer.Samples.FineControl/ClickToInteract
    /// 
    [AddComponentMenu(Strings.SamplesMenuPrefix + "Fine Control - Click To Interact")]
    [AnimancerHelpUrl(typeof(ClickToInteract))]
    public class ClickToInteract : MonoBehaviour
    {
        /************************************************************************************************************************/
#if UNITY_PHYSICS_3D
        /************************************************************************************************************************/

        protected virtual void Update()
        {
            if (!SampleInput.LeftMouseUp)
                return;
            //按下了鼠标左键，发射射线，检测到（门的）碰撞体
            Ray ray = Camera.main.ScreenPointToRay(SampleInput.MousePosition);

            if (Physics.Raycast(ray, out RaycastHit raycastHit))
            {//Tip：并非依赖，通常是依赖于某个具体类型才叫做依赖，而这样依赖抽象的接口其实并非依赖。
                //获取碰撞体所在游戏对象或其上层对象所挂载的带有IInteractable接口的组件，调用其实现的Interact方法
                IInteractable interactable = raycastHit.collider.GetComponentInParent<IInteractable>(); 
                interactable?.Interact();
            }
        }

        /************************************************************************************************************************/
#else
        /************************************************************************************************************************/

        protected virtual void Awake()
        {
            SampleReadMe.LogMissingPhysics3DModuleError(this);
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}
