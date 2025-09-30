using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ARPGDemo.Test.Timeline
{
    /// <summary>
    /// 可在 Unity 2022.3 中运行的时间尺（Ruler）VisualElement。
    /// 功能：根据 visibleStartTime 和 pixelsPerSecond 绘制主/次刻度与标签，支持鼠标滚轮缩放（以鼠标为锚）和中键/右键平移。
    /// 将此类实例化并把回调注入以读取/写入 visibleStartTime 与 pixelsPerSecond（见示例 TimelineRulerWindow）。
    /// </summary>
    public class RulerElement : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<RulerElement, UxmlTraits> { }

        // 外部注入的访问器与回调，用于从父窗口获取/设置可见时间起点与缩放（px/sec）。
        /*Tip：突然想到，这种由外部传入方法决定如何获取某些数值，是一种很巧妙的程序技巧。*/
        // readonly Func<double> GetVisibleStartTime;
        private Func<double> m_GetVisibleStartTime;
        /*Tip：公开一个属性的起因就是SkillEditorWindow中需要在初始化时调用OnAdjustEndFlag，需要这两个基本量，而又不适合把逻辑放在RulerElement中，也不适合设置一个初始化回调、因为
        RulerElement初始化早于SkillEditorWindow、而且这样本来就有点冗余。*/
        public double visibleStartTime => m_GetVisibleStartTime();
        private Func<float> m_GetPixelsPerSecond;
        public float pixelsPerSecond => m_GetPixelsPerSecond();
        private Action<double> m_SetVisibleStartTime;
        private Action<float, float> m_SetScaleWithAnchor; //第一个是新的pixelsPerSecond，第二个是缩放锚点的像素位置

        // public Action<double, float> rulerInitialized;
        public Action<double, float> rulerGeometryChanged;
        public Action<double, float> preRulerScaled;
        public Action<double, float> postRulerScaled; //参数就是visibleStartTime和pixelsPerSecond
        public Action<double, float> rulerMoved; 

        private double m_VisibleStartTime = 0.0;        // seconds at left edge
        private float m_PixelsPerSecond = 100f;         // scale: px / sec

        // 平移交互状态（鼠标中键按住移动）
        private bool m_IsPanning = false;
        private Vector2 m_LastPointerPos; //用于处理鼠标移动的交互逻辑

        // 构造函数：注入 getter/setter，注册绘制与事件回调
        public RulerElement(
            Func<double> _getVisibleStartTime,
            Func<float> _getPixelsPerSecond,
            Action<double> _setVisibleStartTime,
            Action<float, float> _setScaleWithAnchor)
        {
            Init(_getVisibleStartTime, _getPixelsPerSecond, _setVisibleStartTime, _setScaleWithAnchor);
        }

        public RulerElement()
        {
            Init(() => m_VisibleStartTime, () => m_PixelsPerSecond,
                                     (newStart) => m_VisibleStartTime = newStart,
                                     (newScale, anchorPx) =>
                                     {
                                        // adjust visibleStartTime to keep anchor time fixed
                                        //anchorTime代表鼠标位置对应的时刻，anchorPx就是鼠标位置对应的像素
                                        double anchorTime = PxToTime(anchorPx, this.m_VisibleStartTime, m_PixelsPerSecond);
                                        m_PixelsPerSecond = newScale;
                                        /*Tip：以鼠标位置为缩放锚点体现在，以鼠标位置时刻不变为前提，计算缩放后的鼠标位置与可视区域左边界的距离所代表的时长，减去即可得到此时
                                        左边界的开始时刻。这就是一个简单的解方程，从逻辑关系来看原本应该是visibleStartTime + anchorPx / pixelsPerSecond = anchorTime，而
                                        根据anchorTime、anchorPx、pixelsPerSecond已知，就可以求出visibleStartTime*/
                                        m_VisibleStartTime = anchorTime - (anchorPx / m_PixelsPerSecond);
                                        //  Debug.Log($"visibleStartTime: {m_VisibleStartTime}");
                                        // Debug.Log($"锚点时刻：{anchorTime}, 锚点像素: {anchorPx}");
                                     });

            SetRegularStyle();
        }

        private void Init(
            Func<double> getVisibleStartTime,
            Func<float> getPixelsPerSecond,
            Action<double> setVisibleStartTime,
            Action<float, float> setScaleWithAnchor)
        {
            m_GetVisibleStartTime = getVisibleStartTime;
            m_GetPixelsPerSecond = getPixelsPerSecond;
            m_SetVisibleStartTime = setVisibleStartTime;
            m_SetScaleWithAnchor = setScaleWithAnchor;

            // 注册用于自定义几何绘制的委托（UI Toolkit retained-mode）
            // generateVisualContent 在需要重绘时被调用，内部使用 MeshGenerationContext 来绘制线与文本.
            generateVisualContent += OnGenerateVisualContent;

            // 注册鼠标滚轮事件，用来缩放（以鼠标位置为锚）
            RegisterCallback<WheelEvent>(OnWheel);
            // 纯粹为SkillEditorWindow提供回调事件，因为要用到这里的visibleStartTime和pixelsPerSecond基本量，而我又不想将其公开，所以就通过观察者模式实现。
            RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);

            // 注册指针按下/移动/抬起事件以实现平移（中键或右键）
            // RegisterCallback<PointerDownEvent>(OnPointerDown);
            // RegisterCallback<PointerMoveEvent>(OnPointerMove);
            // RegisterCallback<PointerUpEvent>(OnPointerUp);

            // 使元素可接收焦点和指针交互
            focusable = true;
            pickingMode = PickingMode.Position; // 接收精确位置事件

            // rulerInitialized?.Invoke(m_GetVisibleStartTime(), m_GetPixelsPerSecond());
        }

        private void OnGeometryChangedEvent(GeometryChangedEvent evt)
        {
            rulerGeometryChanged?.Invoke(m_GetVisibleStartTime(), m_GetPixelsPerSecond());
        }


        //常规样式，就是固定好的，应该不会变、因为会牵涉到时间轴中的其他部分
        public void SetRegularStyle()
        {
            style.height = 40;
            style.backgroundColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f));
            // style.marginLeft = 10; //在左边界留出一定空间主要是左边界为0时刻时更方便观察。片段容器设置marginRight也是同理
        }

        //主要用于求得缩放锚点（鼠标位置、时间线位置、左边界）所在位置对应的时刻。
        static double PxToTime(float px, double visibleStartTime, float pixelsPerSecond)
        {
            return px / pixelsPerSecond + visibleStartTime;
        }

        // ---- 辅助映射函数：时间 <-> 像素 ----
        double TimeAtPixel(float px)
        {
            // visibleStartTime + px / pixelsPerSecond
            return px / m_GetPixelsPerSecond() + m_GetVisibleStartTime();
        }

        //将（相对于可视区域左边界）时间长度转换为像素数量（坐标位置）
        public float PixelOfTime(double t)
        {//以刻度尺可见区域的左边界所代表的时刻为起点，计算当前时刻在可视区域中的位置。
         // (t - visibleStartTime) * pixelsPerSecond
            return (float)((t - m_GetVisibleStartTime()) * m_GetPixelsPerSecond());
        }

        // ---- 事件处理：滚轮缩放 ----
        void OnWheel(WheelEvent evt)
        {
            // Debug.Log("触发WheelEvent");
            //触发缩放前回调
            preRulerScaled?.Invoke(m_GetVisibleStartTime(), m_GetPixelsPerSecond());

            // 取本地鼠标 x 作为锚点
            /*Tip：这里的参考系就是目标元素的本地坐标系，以目标元素的*内容区域*的左上角为原点，向右向下为正方向。*/
            float mouseX = evt.localMousePosition.x;

            // 当前缩放值（px / sec）
            //Tip：缩放值完全等同于每秒所占的像素值。
            float currentScale = m_GetPixelsPerSecond();

            // wheel.delta: 若向上滚动通常为负/正因平台不同，取 -e.delta.y 让向上变 zoom in
            /*效果上看，向下滚动通常是收缩，向上滚动通常是放大*/
            // float delta = -evt.delta.y;
            float delta;
            /*BugFix: 我草了，Unity是默认按下Shift时滑动鼠标滚轮就是使用的delta.x，因为鼠标滚轮只有y竖直方向、所以通过此方式来模拟水平方向的滚轮输入*/
            // 缩放灵敏度调节
            // float zoomFactor = 1.0f + delta * 0.0015f;
            float zoomFactor;
            if ((evt.modifiers & EventModifiers.Shift) != 0)
            {
                delta = -evt.delta.x;
                zoomFactor = 1.0f + delta * 0.015f;
            }
            else
            {
                delta = -evt.delta.y;
                zoomFactor = 1.0f + delta * 0.005f;
            }
            // Debug.Log("zoomFactor: " + zoomFactor + "delta: " + delta);
            // float newScale = Mathf.Clamp(currentScale * zoomFactor, 10f, 3000f); //限定缩放的上下限，开发者正常使用几乎是不可能遇到这个界限的。
            float newScale = Mathf.Clamp(currentScale * zoomFactor, 20, 200f); //限定缩放的上下限，开发者正常使用几乎是不可能遇到这个界限的。

            //Tip：分情况确定选择哪个位置作为缩放锚点。

            // 回调父窗口，以鼠标像素位置作为锚点更新缩放（父实现会保持锚点时间不变）
            /*Tip：更新两个关键数据：可视区域的起始时间visibleStartTime、每秒换算的像素数pixelsPerSecond*/
            // SetScaleWithAnchor(newScale, mouseX);
            // if (Math.Abs(m_GetVisibleStartTime() - 0.0) < 0.01) //如果左边起始时间为0时刻
            // if (Math.Abs(m_GetVisibleStartTime() - 0.0) < 0.001) //如果左边起始时间为0时刻
            if (m_GetVisibleStartTime() <= 0.001) //如果左边起始时间为0时刻
            {
                m_SetVisibleStartTime(0.0); //修正为0
                m_SetScaleWithAnchor(newScale, 0.0f); //以左边起始位置为锚点
            }
            else //起始非0时刻，并且没有时间线（没有处于预览模式），就将鼠标位置作为缩放锚点。
            {
                // Debug.Log("以鼠标位置为缩放锚点");
                m_SetScaleWithAnchor(newScale, mouseX);
                /*BugFix：在以鼠标位置为缩放锚点时，可能会出现 visibleStartTime 为负数的情况，在这里进行修正。*/
                // if (m_GetVisibleStartTime() <= 0.0) m_SetVisibleStartTime(0.0);
                // Debug.Log("鼠标为锚点");
            }

            //触发缩放后回调
            postRulerScaled?.Invoke(m_GetVisibleStartTime(), m_GetPixelsPerSecond());

            // 请求重绘
            MarkDirtyRepaint();

            // 阻止冒泡，避免 ScrollView 同时处理滚动
            evt.StopImmediatePropagation();
        }

        // ---- 事件处理：指针按下 开始平移 ----
        void OnPointerDown(PointerDownEvent e)
        {
            // 只用中键或右键作为平移触发（常见编辑器约定）
            if (e.button == (int)MouseButton.MiddleMouse || e.button == (int)MouseButton.RightMouse)
            {
                m_IsPanning = true;
                m_LastPointerPos = e.localPosition;
                // 捕获指针以持续接收 move/up
                this.CapturePointer(e.pointerId);
                e.StopImmediatePropagation();
            }
        }

        // ---- 事件处理：移动 平移逻辑 ----
        void OnPointerMove(PointerMoveEvent e)
        {
            if (!m_IsPanning) return;
            //注意向量方向、考虑到正负，即可。
            Vector2 cur = e.localPosition;
            float dx = cur.x - m_LastPointerPos.x; //鼠标移动距离，以像素为单位，所以下方可以直接除以GetPixelsPerSecond就能得到这段距离代表的时长。
            m_LastPointerPos = cur;

            // Debug.Log($"移动距离: {dx}");

            DoMoveRuler(dx);
            // // 平移时更新 visibleStartTime：左移(正dx) -> 时间减少
            // // visibleStartTime = visibleStartTime - dx / pixelsPerSecond
            // double newStart = m_GetVisibleStartTime() - dx / m_GetPixelsPerSecond();
            // /*Tip: 避免移动时左边超出0时刻（变为负）*/
            // newStart = Math.Max(0.0, newStart);

            // m_SetVisibleStartTime(newStart);

            // MarkDirtyRepaint(); //下一帧重绘，就会触发generateVisualContent回调。
            e.StopImmediatePropagation();
        }

        /// <summary>
        /// 传入移动距离（带方向，负为左移、正为右移）实现移动时间尺的效果
        /// </summary>
        /// <param name="_dx"></param>
        public void DoMoveRuler(float _dx)
        {
            double newStart = m_GetVisibleStartTime() - _dx / m_GetPixelsPerSecond();
            // Debug.Log($"_dx: {_dx},  m_GetPixelsPerSecond: {m_GetPixelsPerSecond()},  _dx / m_GetPixelsPerSecond: {_dx / m_GetPixelsPerSecond()}");
            /*Tip: 避免移动时左边超出0时刻（变为负）,以及留出0.01的误差范围*/
            // newStart = Math.Max(0.0, newStart);
            if (newStart <= 0.001) newStart = 0.0;

            m_SetVisibleStartTime(newStart);

            rulerMoved?.Invoke(m_GetVisibleStartTime(), m_GetPixelsPerSecond());

            MarkDirtyRepaint(); //下一帧重绘，就会触发generateVisualContent回调，然后才会调用OnGenerateVisualContent来更新时间尺画面内容。

        }

        // ---- 事件处理：指针抬起 结束平移 ----
        void OnPointerUp(PointerUpEvent e)
        {
            if (m_IsPanning && (e.button == (int)MouseButton.MiddleMouse || e.button == (int)MouseButton.RightMouse))
            {
                m_IsPanning = false;
                this.ReleasePointer(e.pointerId);
                e.StopImmediatePropagation();
            }
        }

        /*Tip：在写好了这里的OnGenerateVisualContent逻辑之后，其他的交互方法Wheel、PointerMove等等就只是读取交互行为的相关数据来更新所要用到的一些基本数据即可。*/

        // ---- 主绘制函数：在 generateVisualContent 回调中被调用 ----
        // 其实这个方法非常类似于IMGUI的OnGUI。
        void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            // 获取绘制区域（contentRect 是元素内容区域的本地矩形）
            Rect rect = contentRect;
            if (rect.width < 1f || rect.height < 1f) return; // 过小则跳过。就是在极端情况下，总要有个处理结果，但是开发者除非故意为之，是不可能出现这种极端情况的。

            // 本地化常用变量
            float width = rect.width;
            float height = rect.height;
            float pxPerSec = m_GetPixelsPerSecond();
            double visibleStart = m_GetVisibleStartTime();

            /*Tip：该值只是一个参考值，要看下面ChooseNiceInternal的返回值才是真正用于绘制编辑器UI的数据。*/
            // 目标主刻度像素间隔（希望主刻度约为此像素间距）
            //Tip：刻度尺就是由主刻度（大刻度）和次刻度（小刻度）来构成刻度线的，而确定了主刻度的间隔长度之后，只要知道需要绘制几个小刻度就能直接均分求出次刻度的间隔长度了。
            const float targetPxPerTick = 100f;

            // 将像素间隔转换为时间间隔（秒），并选取“优美”间隔（1,2,5 * 10^n）
            /*Tip：该变量指的是在相邻两个主刻度之间的时间长度。之前没看懂ChooseNiceInternal的返回值到底代表什么含义，这样来看的话，
            该方法就是对主刻度的间隔时间进行修正，这就是所谓的NiceInterval，传入的就是理论上的间隔时间，但是因为会直接影响到次刻度的绘制，而这直接决定了刻度尺的可读性，
            而且理论上的两个值targetPxPerTick和pxPerSec是自由设定的，并不能保证具有好的可读性，所以就要通过ChooseNiceInterval中的逻辑对此结果值进行修正。*/
            double secondsPerTick = ChooseNiceInterval(targetPxPerTick / pxPerSec);

            // 计算第一个大刻度的时间（>= visibleStart）
            /*Tip：计算逻辑是首先求出当前visibleStart与0点之间包含了多少个主刻度间隔时间，由于visibleStart可能位于两个主刻度的中间，并且它又在可视区域的左边，所以
            通过Ceiling向上取整就能实现向右偏移的效果，也就找到了从左边界向右的第一个主刻度相对于0点的长度相当于多少个主刻度间隔，
            然后再乘以每个间隔的时长即secondsPerTick就可以得到可视区域内的第一个大刻度所代表的时刻了。*/
            double firstTickTime = Math.Ceiling(visibleStart / secondsPerTick) * secondsPerTick; //Ceiling向上取整
            // Debug.Log($"visibleStartTime: {visibleStart}\n secondPerTick时间间隔: {secondsPerTick}");


            double lastStartTickTime = Math.Floor(visibleStart / secondsPerTick) * secondsPerTick;

            // 通过 mgc.painter2D 绘制线条（Painter2D 提供矢量绘制能力）
            var painter = mgc.painter2D; //该Painter2D应该就是为该VisualElement专用的，在ContentRect内容区域中进行绘制。

            // --- 绘制主刻度（较长） ---
            painter.lineWidth = 1.0f; //1个像素的宽
            painter.strokeColor = new Color(1f, 1f, 1f, 0.75f); //纯白色

            painter.BeginPath();
            for (double t = firstTickTime; ; t += secondsPerTick)
            {
                float x = PixelOfTime(t);
                if (x > width) break; // 超出右侧可视区域就停止，即需要渲染的内容已经结束了。

                // 从顶部画到 60% 高度的长刻度线
                /*Tip：注意是以VisualElement的内容区域作为矩形空间，左上角为原点。*/
                // painter.MoveTo(new Vector2(x, 0f));
                // painter.LineTo(new Vector2(x, height * 0.6f)); 
                //这样就是从下到上的刻度
                painter.MoveTo(new Vector2(x, height));
                // painter.LineTo(new Vector2(x, height * (1f - 0.6f)));
                painter.LineTo(new Vector2(x, height * (1f - 0.5f)));
            }
            painter.Stroke(); //就是绘制所定义的路径

            // --- 绘制次刻度（短线），比如分成 4 段 ---
            // int subdivisions = 4;
            int subdivisions = 5;
            painter.lineWidth = 1.0f;
            painter.strokeColor = new Color(1f, 1f, 1f, 0.35f);
            painter.BeginPath();
            // 从 firstTickTime - secondsPerTick 开始，以保证在视窗左侧的次刻度也被绘制
            /*Tip：这里细节很重要，因为刻度尺可以被平移、缩放，而本来就没有数据记录、只是纯粹的渲染，必须呈现出连续的效果，这就对渲染数据的相关计算提出了高要求。*/
            for (double t = firstTickTime - secondsPerTick; ; t += secondsPerTick / subdivisions)
            {
                float x = PixelOfTime(t);
                if (x < 0f) continue;          // 若在左侧可见范围外，跳过（但不能 break，因为左侧可能在下一次才进入）
                if (x > width) break;
                // painter.MoveTo(new Vector2(x, 0f));
                // painter.LineTo(new Vector2(x, height * 0.35f));
                painter.MoveTo(new Vector2(x, height));
                // painter.LineTo(new Vector2(x, height * (1f - 0.35f)));
                painter.LineTo(new Vector2(x, height * (1f - 0.25f)));
            }
            painter.Stroke();

            // --- 绘制刻度标签（使用 mgc.DrawText，UI Toolkit 在 2022.3 支持 DrawText）---
            // 为了避免频繁创建 GUIStyle 或字体，这里使用固定颜色与小字号。
            // DrawText 接口会在内部处理文本渲染到 panel；其签名在 2022.3 的 MeshGenerationContext 提供.
            // 我们将标签画在主刻度下方一点的位置。

            // float labelY = height * 0.62f;
            // float labelY = height * 0.8f;
            float labelY = height * 0.7f;
            Color labelColor = Color.white;

            /*Tip：在左边界上方显示上一个可见第一个主刻度的时间，这是个很实用的功能。而在起始时间大于0的时候才显示，还是特殊化处理。*/
            // if (Math.Abs(GetVisibleStartTime() - 0.0) < 0.01)
            if (m_GetVisibleStartTime() - 0.0 > 0.001)
            {
                string lastStartTickLabel = FormatTimeLabel(lastStartTickTime);
                // mgc.DrawText(lastStartTickLabel, new Vector2(2f, 0f), 14, labelColor);
                //Tip：字体小一点避免与第一个主刻度的标签文本重叠。
                mgc.DrawText(lastStartTickLabel, new Vector2(2f, 0f), 10, labelColor);
            }

            for (double t = firstTickTime; ; t += secondsPerTick)
            {
                float x = PixelOfTime(t);
                if (x > width) break;

                string label = FormatTimeLabel(t); //这里传入的t按照相关的计算过程来看，就应该是以浮点表示的整数值。

                // DrawText( string text, Vector2 pos, Color color ) 可用性取决具体小版本；MeshGenerationContext 在 2022.3 提供 DrawText 方法.
                // 若你遇到编译问题（极少见），可以替代为在 ruler 之上放一组 Label 元素（代价略高），或用 IMGUIContainer 绘制文本。
                // mgc.DrawText(label, new Vector2(x + 2f, labelY), labelColor);
                // mgc.DrawText(label, new Vector2(x + 2f, labelY), 14, labelColor);
                // mgc.DrawText(label, new Vector2(x + 2f, height - labelY), 14, labelColor);
                mgc.DrawText(label, new Vector2(x + 2f, height - labelY), 12, labelColor);
            }

            // --- 绘制示例播放头Playhead（示范如何绘制一条竖线） ---
            // 真实项目中播放头的时间应该由外部驱动（传入 currentTime），此处示范固定 playTime = visibleStart + 2s
            // double playTime = visibleStart + 2.0;
            // float playX = PixelOfTime(playTime);
            // if (playX >= 0f && playX <= width) //在可视范围之内
            // {
            //     painter.lineWidth = 2.0f;
            //     painter.strokeColor = Color.red;
            //     painter.BeginPath();
            //     painter.MoveTo(new Vector2(playX, 0f));
            //     painter.LineTo(new Vector2(playX, height));
            //     painter.Stroke();
            // }
        }

        // 选取“优美”的时间间隔（1,2,5 * 10^n）以得到整齐的刻度分布
        //返回值就是相邻刻度之间代表的间隔时间，即刻度单位。
        //Tip：乘以10^n是因为1、2、5的公倍数就是10
        static double ChooseNiceInterval(double targetSeconds) //传入的就是在两个相邻主刻度（单位为像素pixel）上对应的时间长度（单位为秒s）
        {
            //通常是只要保证非负就行了，这里还判断IsNaN就更加严谨一些。
            if (double.IsNaN(targetSeconds) || targetSeconds <= 0.0) return 1.0;

            double exponent = Math.Pow(10.0, Math.Floor(Math.Log10(targetSeconds)));
            /*BugFix：这里将1作为最小值，可以避免出现exponent为0.1的情况，*/
            exponent = Math.Max(1.0, exponent); //Tip：偶然发现，Unity的Mathf不支持double类型，而C#的Math支持double类型。

            double[] choices = { 1.0, 2.0, 5.0 }; //刻度基准1、2、5，可以绘制出具有非常好的可读性的刻度尺。
                                                  //初始化候选“最佳间隔”为 1·10^n 的值，作为起始比较项
            double best = choices[0] * exponent;
            //记录初始候选与目标之间的差值，用于比较哪个 choices·exponent 更接近 targetSeconds
            double bestDiff = Math.Abs(best - targetSeconds);
            //遍历剩余候选（2·10n、5·10n），比较并选择距离 targetSeconds 最近的那个
            for (int i = 1; i < choices.Length; ++i)
            {
                double val = choices[i] * exponent;
                double d = Math.Abs(val - targetSeconds);
                if (d < bestDiff)
                {
                    best = val;
                    bestDiff = d;
                }
            }
            // Debug.Log($"targetSeconds: {targetSeconds}, exponent: {exponent}, 优美间隔: {best}");

            return best;
        }

        // 简单时间标签格式化（mm:ss），可扩展为帧显示或带毫秒
        static string FormatTimeLabel(double seconds)
        {
            int totalSec = Mathf.FloorToInt((float)seconds);
            /*Tip：之前出现的缩放bug中会导致左边界起始时间已经小于0，但是此处修正为了0，所以就会看到一串都是00:00*/
            if (totalSec < 0) totalSec = 0;
            //商为分，余为秒。
            int m = totalSec / 60;
            int s = totalSec % 60;
            return $"{m:D2}:{s:D2}";
        }
    }
}
