using System;
using System.Collections.Generic;
using UnityEngine;

namespace QFramework.Example
{
    public class ArchitectureInHeartExample : MonoBehaviour
    {

        #region Framework

        public interface ICommand
        {
            void Execute();
        }

        public class BindableProperty<T>
        {
            private T mValue = default; //该类型的默认值

            public T Value
            {
                get => mValue;
                set
                {
                    if (mValue != null && mValue.Equals(value)) return;
                    mValue = value;
                    OnValueChanged?.Invoke(mValue);
                }
            }

            public event Action<T> OnValueChanged = _ => { }; //简写空事件
        }

        #endregion


        #region 定义 Model

        public static class CounterModel
        {
            //注意BindableProperty的构造函数是默认的空参数，此处是通过大括号包含的初始化列表来初始化Value的
            public static BindableProperty<int> Counter = new BindableProperty<int>()
            {
                Value = 0
            };
        }

        #endregion

        #region 定义 Command
        public struct IncreaseCountCommand : ICommand
        {
            public void Execute()
            {
                CounterModel.Counter.Value++; //自增++即Value = Value + 1
            }
        }

        public struct DecreaseCountCommand : ICommand
        {
            public void Execute()
            {
                CounterModel.Counter.Value--;
            }
        }
        #endregion


        private void OnGUI()
        {
            if (GUILayout.Button("+"))
            {
                new IncreaseCountCommand().Execute();
            }

            GUILayout.Label(CounterModel.Counter.Value.ToString());

            if (GUILayout.Button("-"))
            {
                new DecreaseCountCommand().Execute();
            }
        }
    }
}