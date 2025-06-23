using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SimpleDemo
{
    public class PlayerController : MonoBehaviour
    {
        private struct FreeLookRig
        {
            public Vector2 topRig;
            public Vector2 middleRig;
            public Vector2 bottomRig;
        }

        public float maxZoom;
        public float minZoom = 1f;
        public float deltaZoom = 0.0001f;

        //TODO:可以尝试用一个结构体来存储Rig高度和半径的初始值，这样可以避免用一系列字段。        
        private float zoomCounter = 1f;

        private SimpleAnimation animPlayer;
        private Transform camTransform;
        private CinemachineFreeLook freelook;
        private Vector2 inputMoveDir;
        private Vector3 moveDir;
        private FreeLookRig rigData;
        private float moveSpeed = 3f;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            animPlayer = GetComponent<SimpleAnimation>();
            camTransform = Camera.main.transform;
            // freelook = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera as CinemachineFreeLook;
            
        }

        private void Start()
        {
            /*BUG: 在我加入场景加载的逻辑之后，在加载MainScene也就是该组件所在场景之后，下面这一行代码获取不到，而后面一行直接用GameObject查找的方法才能获取到，可以猜想
            就是Cinemachine的生命周期管理上出现的问题，或者说是兼容性问题，但是这需要去阅读Cinemachine源码才能知道了。*/
            // freelook = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera as CinemachineFreeLook;
            freelook = GameObject.FindObjectOfType<CinemachineFreeLook>();
            zoomCounter = 1f;
            //记录FreeLook相机初始高度和半径数值。
            rigData = new FreeLookRig()
            {
                topRig = new Vector2(freelook.m_Orbits[0].m_Height, freelook.m_Orbits[0].m_Radius),
                middleRig = new Vector2(freelook.m_Orbits[1].m_Height, freelook.m_Orbits[1].m_Radius),
                bottomRig = new Vector2(freelook.m_Orbits[2].m_Height, freelook.m_Orbits[2].m_Radius)
            };
        }

        public void GetMoveInput(InputAction.CallbackContext ctx)
        {
            inputMoveDir = ctx.ReadValue<Vector2>();

        }

        public void GetCursorState(InputAction.CallbackContext ctx)
        {
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
        {
            Vector2 zoomInput = ctx.ReadValue<Vector2>();
            // ICinemachineCamera vm = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera;
            // if (vm != null && vm is CinemachineFreeLook freelook)
            // {
            // Debug.Log($"zoomInput: {zoomInput}");
            float delta = (0f - zoomInput.y) * deltaZoom;
            zoomCounter += delta; //zoomCounter记录了当前相对于初始值的倍数，因为没有记录初始值。
            if (zoomCounter <= minZoom) zoomCounter = minZoom;
            else if (zoomCounter >= maxZoom) zoomCounter = maxZoom;
            else
            { //Ques：等比例缩放，效果很好。但是有一说一，我真没想通为何效果这么好，因为商业游戏就是这样的效果。
                freelook.m_Orbits[0].m_Height *= 1f + delta;
                freelook.m_Orbits[0].m_Radius *= 1f + delta;
                freelook.m_Orbits[1].m_Height *= 1f + delta;
                freelook.m_Orbits[1].m_Radius *= 1f + delta;
                freelook.m_Orbits[2].m_Height *= 1f + delta;
                freelook.m_Orbits[2].m_Radius *= 1f + delta;
            }
            // }
        }

        //相机复位
        public void GetResetCMD(InputAction.CallbackContext ctx)
        {
            if (ctx.phase == InputActionPhase.Started) //按下时复位。
            {
                // freelook.m_Orbits[0].m_Height /= zoomCounter;
                // freelook.m_Orbits[0].m_Radius /= zoomCounter;
                // freelook.m_Orbits[1].m_Height /= zoomCounter;
                // freelook.m_Orbits[1].m_Radius /= zoomCounter;
                // freelook.m_Orbits[2].m_Height /= zoomCounter;
                // freelook.m_Orbits[2].m_Radius /= zoomCounter;
                freelook.m_Orbits[0].m_Height = rigData.topRig[0];
                freelook.m_Orbits[0].m_Radius = rigData.topRig[1];
                freelook.m_Orbits[1].m_Height = rigData.middleRig[0];
                freelook.m_Orbits[1].m_Radius = rigData.middleRig[1];
                freelook.m_Orbits[2].m_Height = rigData.bottomRig[0];
                freelook.m_Orbits[2].m_Radius = rigData.bottomRig[1];
                zoomCounter = 1f; //别忘了复原计数器
            }
        }

        public void GetAttackCMD(InputAction.CallbackContext ctx)
        {
            if (ctx.phase == InputActionPhase.Started)
            {
                // animPlayer.Play("Attack");
                // animPlayer.CrossFade()
                // if (animPlayer.IsPlaying("Default"))
                // {
                //     animPlayer.Play("Attack");
                //     animPlayer.PlayQueued("Default", QueueMode.CompleteOthers);
                // }
            }
        }

        private void Update()
        {
            MoveAndRotate();
        }

        private void OnDestroy()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        private void MoveAndRotate()
        {
            Vector3 camFoward = new Vector3(camTransform.forward.x, 0, camTransform.forward.z).normalized;
            moveDir = camFoward * inputMoveDir.y + camTransform.right * inputMoveDir.x;
            //有输入才转向，否则每次停止输入后都会回到原方向。
            if (inputMoveDir != Vector2.zero) transform.rotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }
    }

}