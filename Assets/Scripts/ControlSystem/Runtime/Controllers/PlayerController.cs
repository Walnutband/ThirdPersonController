using Animancer;
using Cinemachine;
using MyPlugins.GoodUI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPGDemo.ControlSystem
{
    public class PlayerController : CommandConsumer, IPlayerConsumer
    {
        /*TODO：到底是使用更抽象的CommandConsumer还是这里同类型的PlayerController，其实具体的纠结点就在于相机，感觉最好还是用CommandConsumer，因为可以使用类型转换检测其实际类型，如果
        不是的话那就少执行一些逻辑，这样看来也是完全兼容的，不需要特殊考虑。当然也要看具体的游戏设计，总之不是什么难题，纯粹是与具体游戏设计紧密相关的问题*/
        [SerializeField] private CommandConsumer targetConsumer;
        // [SerializeField] private PlayerController targetConsumer;
        [SerializeField] private CinemachineFreeLook freelook;
        [SerializeField] private float maxZoom = 2f;
        [SerializeField] private float minZoom = 0.5f;
        /*TODO：肯定要能够自由改变灵敏度，不过在实际游戏中，镜头的灵敏度应该是全局的，也就是独立于玩家所控制的角色（控制器）的，所以此处的deltaZoom
        可能应该改为一个只有getter的属性，并且将实际存储灵敏度数值的字段放在专门的静态类中，然后在角色控制器中的属性的getter中直接返回该字段值即可。
        这样的字段应该就属于游戏基础设置中的各项数值之一，可以通过设置界面的UI进行自由调整，不过UI对象肯定是非静态类，所以存放这些
        设置数值的类可能也会设置为非静态类，到时候只要在游戏开始时向UI元素的回调委托上注册方法、改变对应的字段值，即可实现通过UI
        实时调整相关的设置项了。*/
        //TODO：这个表示灵敏度的字段到底应该放在控制器中，还是放在FreeLookZoomer结构体中呢？
        [SerializeField] private float deltaZoom = 0.0005f; 

        [SerializeField] private AnimancerComponent animPlayer;
        [SerializeField] private AnimationList animationList; //用于animPlayer播放，其实就代表该控制器所控制的角色可以使用的所有动画。


        //TODO:可以尝试用一个结构体来存储Rig高度和半径的初始值，这样可以避免用一系列字段。
        // private float zoomCounter = 1f;

        private FreeLookZoomer zoomer;
        private Transform camTransform;
        private Vector2 moveInput;
        private Vector3 moveDir;
        private float moveSpeed = 3f;

        private void Awake()
        {
            //TODO：这种控制鼠标显隐的任务都不应该由控制器来做，应该要有专门的管理器。
            Cursor.lockState = CursorLockMode.Locked;
            // freelook ??= GameObject.FindObjectOfType<CinemachineFreeLook>(); //freelook为空才会查找赋值。
            // animPlayer ??= GetComponent<AnimancerComponent>();
            camTransform = Camera.main.transform;
        }

        private void Start()
        {
            /*BUG: 在我加入场景加载的逻辑之后，在加载MainScene也就是该组件所在场景之后，下面这一行代码获取不到，而后面一行直接用GameObject查找的方法才能获取到，可以猜想
            就是Cinemachine的生命周期管理上出现的问题，或者说是兼容性问题，但是这需要去阅读Cinemachine源码才能知道了。*/
            // freelook = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera as CinemachineFreeLook;
            // freelook ??= GameObject.FindObjectOfType<CinemachineFreeLook>(); //freelook为空才会查找赋值。
        }

        private void OnEnable()
        {
            // Tip：在运行模式下可以简单地通过禁用再启用来切换（更新）FreeLook相机配置。
            zoomer = new FreeLookZoomer(freelook, minZoom, maxZoom);
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"Trigger: {other.gameObject.name}");
            targetConsumer = other.GetComponent<CommandConsumer>();
        }

        private void OnTriggerExit(Collider other)
        {
            targetConsumer = null;
        }


        public void GetMoveInput(InputAction.CallbackContext ctx)
        {
            moveInput = ctx.ReadValue<Vector2>();
        }

        public void GetCursorState(InputAction.CallbackContext ctx)
        {//使用UIMainView控制鼠标状态的显隐之后，就必须限制只有在MainView时才可以手动控制显隐。
        //并非，其实应该从输入的角度来看，即切换ActionMap，即Playing和UI分别使用一个ActionMap。
            // if (!UIManager.Instance.InMainView)
            //     return;

            switch (ctx.phase)
            {
                case InputActionPhase.Started:
                    Cursor.lockState = CursorLockMode.None;
                    //Cursor.visible = false;
                    break;
                case InputActionPhase.Canceled:
                    Cursor.lockState = CursorLockMode.Locked;
                    break;
            }
        }

        public void GetZoomInput(InputAction.CallbackContext ctx)
        {//Zoom动作所绑定的鼠标滚轮，每一次滚动都是在同一帧触发started和performed，在下一帧触发canceled，无论滚动的频率如何，都是这样一组一组地固定触发。
            Vector2 zoomInput = ctx.ReadValue<Vector2>();
            float delta = (-zoomInput.y) * deltaZoom; 
            zoomer.Zoom(delta); //传入处理之后的变化值，就是可以直接使用的值。
        }

        //相机复位
        public void GetResetCMD(InputAction.CallbackContext ctx)
        {
            if (ctx.phase == InputActionPhase.Started) //按下时复位。
            {
                zoomer.Reset();
                // zoomCounter = 1f; //别忘了复原计数器
            }
        }

        public void GetAttackCMD(InputAction.CallbackContext ctx)
        {
            if (ctx.phase == InputActionPhase.Started)
            {
                
            }
        }

        public void OnUpdate()
        {
            MoveAndRotate();
        }

        
        private void OnDestroy()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        /*BUG: 在持续移动的同时转动镜头，会发现人物旋转有所滞后，因为明显的视觉残留看到幻影，估计是监测方法的更新频率和更新顺序方面的问题，Update与FixedUpdate？？？*/
        private void MoveAndRotate()
        {
            Vector3 camFoward = new Vector3(camTransform.forward.x, 0, camTransform.forward.z).normalized;
            moveDir = camFoward * moveInput.y + camTransform.right * moveInput.x;
            //有输入才转向，否则每次停止输入后都会回到原方向。
            if (moveInput != Vector2.zero) transform.rotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }

        public bool Move(Vector2 _moveInput)
        {
            Debug.Log("Move");
            moveInput = _moveInput;
            return true;
        }

        public bool Jump()
        {
            Debug.Log("Jump");
            return true;
        }

        public bool Dodge()
        {
            Debug.Log("Dodge");
            return true;
        }

        public bool LightAttack()
        {
            Debug.Log("LightAttack");
            return true;
        }

        public bool HeavyAttack(bool _charging, bool? _full)
        {
            Debug.Log("HeavyAttack");
            return true;
        }

        public bool Zoom(Vector2 _zoomInput)
        {
            Vector2 zoomInput = _zoomInput;
            float delta = (-zoomInput.y) * deltaZoom;
            zoomer.Zoom(delta); //传入处理之后的变化值，就是可以直接使用的值。
            return true;
        }

        public bool ResetCamera()
        {
            zoomer.Reset();
            return true;
        }

        /*TODO：这里应该会是一个通用方法而不是专用方法，总之就是在改变控制对象时要对原对象进行一些善后处理，然后再改变，因为只有控制器挂载在原对象上，才能够获取到原对象的许多信息，
        从而执行所需逻辑*/
        public bool ChangePlayer(CommandProducer producer)
        {
            if (targetConsumer == null) return false; //没目标换不了。

            // freelook.LookAt = freelook.Follow = targetConsumer.GetComponent<Transform>();
            ControlSystem.Instance.ChangeConsumer(producer, targetConsumer);
            return true;
        }

        /*Tip：通过私有的静态方法，使得相同Class的不同实例能够进行交流。其实还可以将freelook字段设置为静态，感觉其实更好。
        甚至更加特殊化一点，直接为每个要掌控相机的对象都准备一个相机（虚拟相机），到时候直接切换就完事了，还方便做相机过渡之类的效果*/
        private static void DeliverFreeLook(PlayerController current, CommandConsumer target, CinemachineFreeLook freelook, FreeLookZoomer zoomer)
        {
            PlayerController controller = target as PlayerController;
            if (controller != null) //确定要把相机交出去了。
            {
                controller.freelook = freelook; //类内部的私有静态方法，可以访问类的私有成员？
                controller.zoomer = zoomer;
                /*Tip：规定个体对象的根对象的直接子对象中有一个名为“Target”的对象专门作为相机的Follow和LookAt目标对象。
                为了兼容性（鲁棒性？），如果没找到的话就直接将其根对象作为目标对象*/
                Transform cameraTarget = controller.transform.Find("Target");
                freelook.LookAt = freelook.Follow = cameraTarget != null ? cameraTarget : controller.transform; 
                current.freelook = null;
            }
        }

        public override void OnStart()
        {
            if (zoomer.Equals(default(FreeLookZoomer))) //值类型，default(T)就代表其未赋值状态，可以当做引用类型的null来使用。
            {
                zoomer = new FreeLookZoomer(freelook, minZoom, maxZoom);
            }
        }

        public override void OnEnd()
        {
            DeliverFreeLook(this, targetConsumer, freelook, zoomer);
            moveInput = Vector2.zero;
        }
    }
}