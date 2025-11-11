using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo
{
    public interface ICanMoveWithPlatform
    {
        //由于MonoBehaviour本来就实现了属性transform，所以这里不应该命名为transform
        // Transform transform { get; } 

        Transform root { get; }
        bool onPlatform { set; }
    }
    
    [AddComponentMenu("ARPGDemo/Other/MovablePlatform")]
    public class MovablePlatform : MonoBehaviour
    {
        // public Rigidbody m_Rb;
        public Vector3 moveDir = new Vector3(0f, 1f, 0f);
        public float moveSpeed = 1f;
        public bool isMoving;
        public Transform upperSurface; //上表面

        public Vector3 lastPos;
        public Vector3 deltaPos;
        private Coroutine preUpdate;
        // public List<ICanMoveWithPlatform> m_Targets = new List<ICanMoveWithPlatform>();
        public Dictionary<int, ICanMoveWithPlatform> m_Targets = new Dictionary<int, ICanMoveWithPlatform>();

        private void Start()
        {
            lastPos = transform.position;
            // preUpdate = StartCoroutine(PreUpdate());
            // m_Rb ??= GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (isMoving)
            {
                transform.Translate(moveDir * moveSpeed * Time.fixedDeltaTime);
                // m_Rb.MovePosition(transform.position + moveDir * moveSpeed * Time.fixedDeltaTime);
                deltaPos = transform.position - lastPos;
                lastPos = transform.position;
                Vector3 delta = deltaPos;
                // delta.y = ;
                foreach (var target in m_Targets.Values)
                {
                    Debug.Log($"移动平台上对象{target.root.name}, deltaPos: {deltaPos}");
                    // target.root.Translate(deltaPos);
                    delta.y = target.root.position.y - upperSurface.position.y;
                    target.root.Translate(delta);
                }
            }

        }
        // /*TODO：似乎设置了接口之后，从逻辑上就不用限制碰撞层级了，但是限制层级应该对性能有所帮助，总之实现方式不固定*/
        // private void OnTriggerEnter(Collider other)
        // {
        //     if (other.TryGetComponent<ICanMoveWithPlatform>(out var target))
        //     {
        //         m_Targets.Add(target.root.GetInstanceID(), target);
        //         CaptureTarget(target);
        //     }
        // }

        // private void OnTriggerExit(Collider other)
        // {
        //     if (other.TryGetComponent<ICanMoveWithPlatform>(out var target) && m_Targets.TryGetValue(target.root.GetInstanceID(), out var _target))
        //     {
        //         target.onPlatform = false;
        //         m_Targets.Remove(target.root.GetInstanceID());
        //         Debug.Log($"失去目标： {target.root.name}");
        //     }
        // }

        public void CaptureTarget(ICanMoveWithPlatform target)
        {
            Debug.Log($"捕获目标：{target.root.name}");
            if (!m_Targets.ContainsKey(target.root.GetInstanceID())) m_Targets.Add(target.root.GetInstanceID(), target);
            //首先将目标移动（吸附）到上表面
            var pos = target.root.position;
            pos.y = upperSurface.position.y;
            target.root.position = pos;
            //通知已经在平台上了，因为这其实属于一个状态信息，而个体就可能因此改变一些状态逻辑
            target.onPlatform = true;
            // isMoving = false;
        }

        public void ReleaseTarget(ICanMoveWithPlatform target)
        {
            Debug.Log($"释放目标：{target.root.name}");
            target.onPlatform = false;
            if (m_Targets.ContainsKey(target.root.GetInstanceID())) m_Targets.Remove(target.root.GetInstanceID());
        }

        // private void OnTriggerEnter(Collider other)
        // {
        //     isFollowing = false;
        // } 

        // private IEnumerator PreUpdate()
        // {
        //     while (true)
        //     {
        //         yield return new WaitForFixedUpdate();
        //         deltaPos = transform.position - lastPos;
        //         lastPos = transform.position;
        //     }
        // }
        //Tip: 由于FixedUpdate与Update频率不同，而人物是在Update中移动、平台却是在FixedUpdate中移动，所以在没有调用FixedUpdate的帧内都应该在最后LateUpdate将deltaPos归零
        // private void LateUpdate()
        // {
        //     deltaPos = Vector3.zero;
        //     // deltaPos = transform.position - lastPos;
        //     // lastPos = transform.position;
        // }
    }
}