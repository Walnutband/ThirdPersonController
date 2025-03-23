using System;
using System.Text;
using UnityEngine;

namespace PnFramework
{
    public class VernierArray<T> //就是一个自定义的容器类，和内置容器类的结构类似
    {
        protected T[] data; //数据数组。其实所有容器类都可以看作是对原始数组的封装
        protected int first = 0; //头元素坐标
        protected int N = 0;

        public int Count => N; //当前元素数量
        public int Capacity => data.Length; //总容量
        /// <summary>
        /// 数组是否为空
        /// </summary>
        public bool IsEmpty => N == 0;

        //如果有默认值的话，没有传入参数时也会选择无参构造函数，因为编译器会选择最匹配的那个
        public VernierArray(int capacity) => data = new T[capacity == 0 ? 2 : capacity]; //漏洞，如果传入负数呢？确实一个小demo不用考虑那么多
        public VernierArray() : this(10) { } //调用有参数的构造函数，并且传入参数值为10

        /// <summary>
        /// 数组索引器 []
        /// </summary>
        /// <param name="index">索引位置</param>
        public virtual T this[int index]
        {//首先检查索引是否合法，
            get { IsLegal(index); return data[(first + index) % N]; }
            set { IsLegal(index); data[(first + index) % N] = value; }
        }
        /// <summary>
        /// 游标指针向左移动一个位置[相当于把尾部往头部移动]
        /// </summary>
        public virtual void LoopPos() => first = (N + first - 1) % N; //取余数用于实现循环（头尾相接）
        /// <summary>
        /// 游标指针向右移动一个位置[相当于把头部往尾部移动]
        /// </summary>
        public virtual void LoopNeg() => first = (++first) % N;
        /// <summary>
        /// 查看头部元素 
        /// </summary>
        public T GetFirst() { CheckIsEmpty(); return data[first]; } //检测为空则会直接抛出异常
        /// <summary>
        /// 查看尾部元素 
        /// </summary>
        public T GetLast() { CheckIsEmpty(); return this[N - 1]; }
        /// <summary>
        /// 在游标前添加[往末尾添加一个元素]
        /// </summary>
        public virtual void AddLast(T e)
        {
            if (N == data.Length) ResetCapacity(2 * data.Length); //如果已经达到了容量上限，则扩容（翻倍）
            for (int i = N - 1; i >= first; i--) data[i + 1] = data[i];
            data[first] = e; N++; LoopNeg();
        }
        /// <summary>
        /// 清空数组 []（使用Array.Clear清空从索引0开始的总长度个元素）
        /// </summary>
        public virtual void Clear() { Array.Clear(data, 0, data.Length); first = 0; N = 0; }

        public override string ToString()
        {
            StringBuilder res = new StringBuilder();
            res.Append($"{GetType().Name}:  Count = {N}   capacity = {data.Length} \n[");
            for (int i = first; i < first + N; i++)
            {
                res.Append(data[i % N]);
                if ((i + 1) % N != first) res.Append(",");
                Debug.Log(i);
            }
            return res.Append("]").ToString();
        }
        /// <summary>
        /// 重置数组容量 []
        /// </summary>
        /// <param name="newCapacity">新容量</param>
        protected virtual void ResetCapacity(int newCapacity)
        {//有个问题，要是newCapacity < N呢？
            T[] newData = new T[newCapacity];
            for (int i = 0; i < N; i++) newData[i] = data[i];
            data = newData;
        }
        /// <summary>
        /// 检查索引是否合法
        /// </summary>
        protected void IsLegal(int index) { if (index < 0 || index > N) throw new ArgumentException("数组索引越界"); }
        /// <summary>
        /// 检查数组是否为空
        /// </summary>
        protected void CheckIsEmpty() { if (N == 0) throw new InvalidOperationException("数组为空"); }
    }
}