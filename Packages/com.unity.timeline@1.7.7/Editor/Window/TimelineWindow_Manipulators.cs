using UnityEngine;

namespace UnityEditor.Timeline
{
    partial class TimelineWindow
    {
        /*在绘制轨道列表（TreeView）和时间尺之前，优先拦截并响应用户输入的“前置”交互。主要用于对标尺空白区、剪辑本身或全局缩放／平移等操作。*/
        readonly Control m_PreTreeViewControl = new Control(); //在绘制轨道列表（TreeView）之前抢先处理输入
        /*在轨道内容（TreeView + 剪辑、曲线行内控件）渲染完成后，再次分发事件。用于那些要“覆盖”在轨道之上的交互，比如上下文菜单、快捷键清空选中等。*/
        readonly Control m_PostTreeViewControl = new Control();

        /*用于在轨道区域做框选（Marquee Select），通常与某个 Manipulator（如 SelectAndMoveItem）配合，实现按住拖拽选中多个剪辑／轨道*/
        readonly RectangleSelect m_RectangleSelect = new RectangleSelect();
        /*支持在标尺或轨道上按键＋拖拽做时间轴缩放（类似“区域缩放”），配合 TimelineZoomManipulator 或 TrackZoom 使用。*/
        readonly RectangleZoom m_RectangleZoom = new RectangleZoom();

        void InitializeManipulators()
        {
            // Order is important! 因为会按照顺序调用方法来尝试响应当前的输入事件

            // Manipulators that needs to be processed BEFORE the treeView (mainly anything clip related)

            //Tip：这里调用的是构造方法，所以在类名上写的Xml注释在这里就不会显示。
            m_PreTreeViewControl.AddManipulator(new HeaderSplitterManipulator());
            m_PreTreeViewControl.AddManipulator(new TimelinePanManipulator());
            m_PreTreeViewControl.AddManipulator(new TrackResize());
            m_PreTreeViewControl.AddManipulator(new InlineCurveResize());
            m_PreTreeViewControl.AddManipulator(new TrackZoom());
            m_PreTreeViewControl.AddManipulator(new Jog());
            m_PreTreeViewControl.AddManipulator(TimelineZoomManipulator.Instance);
            m_PreTreeViewControl.AddManipulator(new ContextMenuManipulator());

            m_PreTreeViewControl.AddManipulator(new EaseClip());
            m_PreTreeViewControl.AddManipulator(new TrimClip());
            m_PreTreeViewControl.AddManipulator(new SelectAndMoveItem());
            m_PreTreeViewControl.AddManipulator(new TrackDoubleClick());
            m_PreTreeViewControl.AddManipulator(new DrillIntoClip());
            m_PreTreeViewControl.AddManipulator(new InlineCurvesShortcutManipulator());

            // Manipulators that needs to be processed AFTER the treeView or any GUI element able to use event (like inline curves)
            //这组只在前面所有 GUI 渲染完后才抢输入，确保轨道和行内控件先“自己用”事件。
            m_PostTreeViewControl.AddManipulator(new MarkerHeaderTrackManipulator());
            m_PostTreeViewControl.AddManipulator(new TimeAreaContextMenu());
            m_PostTreeViewControl.AddManipulator(new TrackShortcutManipulator());
            m_PostTreeViewControl.AddManipulator(new TimelineShortcutManipulator());
            m_PostTreeViewControl.AddManipulator(new ClearSelection());
        }
    }
}
