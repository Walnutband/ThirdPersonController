using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MyPlugins.GoodUI
{
    [AddComponentMenu("GoodUI/Controls/SimpleButton")]
    //挂载在UIMask上，替代Button，因为通常只需要响应一下点击事件，其他逻辑都不需要。
    public class SimpleButton : UIBehaviour, IPointerClickHandler
    {
        private event Action onClick;

        protected SimpleButton() { }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke();
        }

        public void AddListener(Action action)
        {
            onClick += action;
        }

        public void RemoveListener(Action action)
        {
            onClick -= action;
        }
    }
}