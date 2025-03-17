namespace Ilumisoft.VisualStateMachine
{
    using UnityEngine;

    [System.Serializable]
    public class ZoomSettings
    {
        public static readonly float MinZoomFactor = 0.5f;
        public static readonly float MaxZoomFactor = 2.0f;

        [SerializeField]
        private float zoomFactor = 1.0f;

        public float ZoomFactor
        {
            get => this.zoomFactor;
            set
            { //虽然这里有限定范围，但其实在赋值之前就已经确定会在范围内了。主要还是保护性代码。
                this.zoomFactor = Mathf.Clamp(value, MinZoomFactor, MaxZoomFactor);
            }
        }
    }
}