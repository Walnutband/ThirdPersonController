///Credit ChoMPHi
///Sourced from - http://forum.unity3d.com/threads/accordion-type-layout.271818/

using UnityEngine;
using UnityEngine.Events;

namespace MyPlugins.GoodUI
{
    public struct FloatTween : ITweenValue
	{
		public class FloatTweenCallback : UnityEvent<float> {}
		public class FloatFinishCallback : UnityEvent {}
		
		private float m_StartFloat; 
		private float m_TargetFloat;
		private float m_Duration;
		private bool m_IgnoreTimeScale;
		
		private FloatTweenCallback m_Target; //在值改变时触发
		private FloatFinishCallback m_Finish; //在Tween动画结束时触发
		
		/// <summary>
		/// Gets or sets the starting float.
		/// </summary>
		/// <value>The start float.</value>
		public float startFloat
		{
			get { return m_StartFloat; }
			set { m_StartFloat = value; }
		}
		
		/// <summary>
		/// Gets or sets the target float.
		/// </summary>
		/// <value>The target float.</value>
		public float targetFloat
		{
			get { return m_TargetFloat; }
			set { m_TargetFloat = value; }
		}
		
		/// <summary>
		/// Gets or sets the duration of the tween.
		/// </summary>
		/// <value>The duration.</value>
		public float duration
		{
			get { return m_Duration; }
			set { m_Duration = value; }
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="UnityEngine.UI.Tweens.ColorTween"/> should ignore time scale.
		/// </summary>
		/// <value><c>true</c> if ignore time scale; otherwise, <c>false</c>.</value>
		public bool ignoreTimeScale
		{
			get { return m_IgnoreTimeScale; }
			set { m_IgnoreTimeScale = value; }
		}

		/// <summary>
		/// Tweens the float based on percentage.
		/// </summary>
		/// <param name="floatPercentage">Float percentage.</param>
		public void TweenValue(float floatPercentage)
		{
			if (!ValidTarget())
				return;
			//这里插值很关键，根据传入的当前时间比例来确定当前的具体数值，作为参数传入到注册的方法中，往往是直接赋值给相应属性
			m_Target.Invoke( Mathf.Lerp (m_StartFloat, m_TargetFloat, floatPercentage) );
		}
		
		public void AddOnChangedCallback(UnityAction<float> callback)
		{
			if (m_Target == null)
				m_Target = new FloatTweenCallback();
			
			m_Target.AddListener(callback);
		}
		
		public void AddOnFinishCallback(UnityAction callback)
		{
			if (m_Finish == null)
				m_Finish = new FloatFinishCallback();
			
			m_Finish.AddListener(callback);
		}
		
		public bool GetIgnoreTimescale()
		{
			return m_IgnoreTimeScale;
		}
		
		public float GetDuration()
		{
			return m_Duration;
		}
		
		public bool ValidTarget()
		{
			return m_Target != null;
		}
		
		/// <summary>
		/// Invokes the on finish callback.
		/// </summary>
		public void Finished()
		{
			if (m_Finish != null)
				m_Finish.Invoke();
		}
	}
}