namespace Ilumisoft.VisualStateMachine.Editor
{
    using UnityEditor;
    using UnityEngine;

    public class StateMachineField : ToolbarElement
    {
        public StateMachineField(EditorWindow window) : base(window)
        {
            this.Width = 248;
        }

        public override void OnGUI(Rect rect)
        {
            var fieldRect = new Rect(2, rect.y + 1, this.Width, 16);

            EditorGUI.BeginChangeCheck();
            {
                //返回该字段引用的对象
                StateMachine stateMachine = (StateMachine)EditorGUI.ObjectField(fieldRect, Context.StateMachine, typeof(StateMachine), true);

                if (EditorGUI.EndChangeCheck())
                {//引用发生变化，则加载最新引用（包括空引用的情况）
                    Context.LoadStateMachine(stateMachine);
                }
            }
        }
    }
}