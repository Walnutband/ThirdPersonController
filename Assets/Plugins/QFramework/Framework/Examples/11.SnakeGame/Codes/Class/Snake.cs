using UnityEngine;
using PnFramework;

namespace SnakeGame
{
    public interface ISnake
    {
        int HeadIndex { get; }
        int TailIndex { get; }

        Vector3 NextMoveDir { get; }

        void GetMoveDir(int hor, int ver);
        void Bigger(int index);
        void Move();
    }
    public class Snake : ISnake
    {
        private VernierArray<int> mBodys;
        private Vector3 mNextMoveDir = Vector3.right;

        public Snake() => mBodys = new VernierArray<int>(); //一行代码的无参数构造函数。C#不能像C++那样使用初始化列表

        Vector3 ISnake.NextMoveDir => mNextMoveDir;

        int ISnake.HeadIndex => mBodys.GetFirst();
        int ISnake.TailIndex => mBodys.GetLast();

        void ISnake.Move()
        {
            mBodys.LoopPos();
            Debug.Log(mBodys.ToString());
        }
        void ISnake.Bigger(int index) => mBodys.AddLast(index);
        /// <summary>
        /// 获取下一次移动的方向
        /// </summary>
        /// <param name="hor"></param>
        /// <param name="ver"></param>
        void ISnake.GetMoveDir(int hor, int ver)
        {
            if (mBodys.Count > 1)
            {//有不只本体的长度时，因为会自动前进，而只有在输入垂直方向时才会改变方向，避免回头撞自己。
                if (mNextMoveDir.x != 0 && mNextMoveDir.y == 0 && ver != 0)
                {
                    mNextMoveDir = Vector3.up * ver; //向量化
                }
                else if (mNextMoveDir.y != 0 && mNextMoveDir.x == 0 && hor != 0)
                {
                    mNextMoveDir = Vector3.right * hor;
                }
            }//只有自己本体的时候水平输入优先（这个倒是没啥影响，主要是对同时按下的处理）。显然只有自己本体的时候可以回头。
            else if (hor != 0) mNextMoveDir = Vector3.right * hor;
            else if (ver != 0) mNextMoveDir = Vector3.up * ver;
        }
    }
}