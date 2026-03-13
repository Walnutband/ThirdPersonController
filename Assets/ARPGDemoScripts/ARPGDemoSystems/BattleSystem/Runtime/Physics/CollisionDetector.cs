using System;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    [AddComponentMenu("ARPGDemo/BattleSystem/CollisionDetector")]
    public class CollisionDetector : MonoBehaviour
    {//TODO：回调需要加参数，应该是该检测器所在的个体对象。
        [SerializeField] private Collider m_Collider;
        /*Tip：加上add和remove之后显示“只能出现在 += 或 -= 的左边”错误警告，无法Invoke触发或赋值，一直想不通，然后发现TMD这样就是把原来的字段变成属性了，当然不行了，
        所以需要额外定义一个字段（backing field）才行。*/
        private Action<Collider> m_TriggerEnter;
        public event Action<Collider> triggerEnter
        {
            add
            {//TODO：暂定。比如连段攻击，可能第一段没有触发，但是注册了方法又没有清空，那就要出意外了。
                m_TriggerEnter = null;
                m_TriggerEnter += value;
            }
            remove
            {
                m_TriggerEnter -= value;
            }
        }
        public event Action<Collider> triggerStay;
        public event Action<Collider> triggerExit;

        //命中事件。
        private HitCallback hitEvent;

        private void Awake()
        {
            if (m_Collider == null) m_Collider = GetComponent<Collider>();
        }

        private void Start()
        {
            m_Collider.isTrigger = true; //默认就应该是触发器
        }

        public void SetHitCallback(HitCallback _action)
        {
            hitEvent = _action;
        }

        private void OnTriggerEnter(Collider other)
        {//TODO：需要进一步审视这里触发后就立刻置空的操作。还有如何不触发呢？
            Debug.Log("TriggerEnter");
            m_TriggerEnter?.Invoke(other);
            m_TriggerEnter = null;
            hitEvent?.Invoke(other.gameObject);
        }

        // private void OnTriggerStay(Collider other)
        // {
        //     triggerStay?.Invoke(other);
        //     triggerStay = null;
        // }

        // private void OnTriggerExit(Collider other)
        // {
        //     triggerExit?.Invoke(other);
        //     triggerExit = null;
        // }

        public void EnableDetector()
        {
            m_Collider.enabled = true;
        }

        public void DisableDetector()
        {
            m_Collider.enabled = false;
        }

    }

    public delegate void HitCallback(GameObject _target);
}