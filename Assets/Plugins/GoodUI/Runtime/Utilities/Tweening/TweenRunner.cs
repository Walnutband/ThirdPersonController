///Credit ChoMPHi
///Sourced from - http://forum.unity3d.com/threads/accordion-type-layout.271818/

using System.Collections;
using UnityEngine;

namespace MyPlugins.GoodUI
{
	/// <summary>
	/// TweenRunner该类用于通过执行协程来实现Tween动画，协程会存在于指定的MonoBehaviour类型的容器中，每一个TweenRunner实例都为一个特定的MonoBehaviour实例服务
	/// </summary>
	/// <typeparam name="T"></typeparam>
    internal class TweenRunner<T> where T : struct, ITweenValue
	{
		protected MonoBehaviour m_CoroutineContainer; //协程容器，因为协程本来就是依附于MonoBehaviour组件的
		protected IEnumerator m_Tween; //当前的协程

		//实现Tween动画的协程
		private static IEnumerator Start(T tweenInfo)
		{
			if (!tweenInfo.ValidTarget())
				yield break;

			float elapsedTime = 0.0f;
			while (elapsedTime < tweenInfo.duration)
			{
				elapsedTime += tweenInfo.ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
				var percentage = Mathf.Clamp01(elapsedTime / tweenInfo.duration); //时间与值等比
				tweenInfo.TweenValue(percentage);
				yield return null;
			}
			//while循环会在最后一段退出，在此补上执行。其实也不对，while循环中由于是在执行前累加的时间，所以最后一段也会执行，这里的根本目的是保证达到准确目标
			tweenInfo.TweenValue(1.0f);
			tweenInfo.Finished();
		}

		public void Init(MonoBehaviour coroutineContainer)
		{
			m_CoroutineContainer = coroutineContainer;
		}

		/// <summary>
		/// 调用该方法开启协程，实际是启动Tween动画
		/// </summary>
		/// <param name="info"></param>
		public void StartTween(T info)
		{
			if (m_CoroutineContainer == null)
			{
				Debug.LogWarning("Coroutine container not configured... did you forget to call Init?");
				return;
			}

			//如果正在进行，就先退出。这是个关键功能，因为应当保证玩家能够随便乱按，当然也不一定是乱按，可能按错了马上想要取消，就可以这样实现。
			if (m_Tween != null)
			{
				m_CoroutineContainer.StopCoroutine(m_Tween);
				m_Tween = null;
			}

			if (!m_CoroutineContainer.gameObject.activeInHierarchy)
			{
				Debug.Log("!active");
				info.TweenValue(1.0f);
				return;
			}
			//注意对于协程的理解，这里调用Start就是获取到了一个协程，但是要通过MonoBehaviour的StartCoroutine方法才会开始执行协程里面的程序，
			//这也反映出为何m_CoroutineContainer是MonoBehaviour类型，以及协程会存在于m_CoroutineContainer其中。
			m_Tween = Start(info); //获取到协程实例以便随时可以指定停止，不需要通过StopAllCoroutine来停止
			m_CoroutineContainer.StartCoroutine(m_Tween);
		}
	}
}