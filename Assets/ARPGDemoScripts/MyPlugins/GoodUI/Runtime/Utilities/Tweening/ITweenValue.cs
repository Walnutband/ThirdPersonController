///Credit ChoMPHi
///Sourced from - http://forum.unity3d.com/threads/accordion-type-layout.271818/


namespace MyPlugins.GoodUI
{
	/// <summary>
	/// 实现Tween动画的接口
	/// </summary>
    internal interface ITweenValue
	{
		/// <summary>
		/// 被Tween协程所调用，通常用于插值并且触发值改变事件
		/// </summary>
		/// <param name="floatPercentage"></param>
		void TweenValue(float floatPercentage);
		bool ignoreTimeScale { get; }
		/// <summary>
		/// Tween过渡时间
		/// </summary>
		float duration { get; }
		bool ValidTarget();
		/// <summary>
		/// 在Tween动画结束时调用，通常用于触发结束事件
		/// </summary>
		void Finished();
	}
}