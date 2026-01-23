using UnityEngine;

namespace UnityHFSM
{
	/// <summary>
	/// Default timer that calculates the elapsed time based on <c>Time.time</c>.
	/// </summary>
	public class Timer : ITimer
	{
		public float startTime;
		public float Elapsed => Time.time - startTime;

		//这里的Reset重置意为“重新开始计时”。
		public void Reset()
		{
			startTime = Time.time;
		}

		public static bool operator >(Timer timer, float duration)
			=> timer.Elapsed > duration;

		public static bool operator <(Timer timer, float duration)
			=> timer.Elapsed < duration;

		public static bool operator >=(Timer timer, float duration)
			=> timer.Elapsed >= duration;

		public static bool operator <=(Timer timer, float duration)
			=> timer.Elapsed <= duration;
	}
}
