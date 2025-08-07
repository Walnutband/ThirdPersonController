using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;
// using System.Diagnostics;

namespace MyTools.BehaviourTreeTool
{
    public class InspectorView : VisualElement {
        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }

        //Editor editor;
        //自定义控件必须提供默认的构造器
        public InspectorView() {

        }

        //internal限制只能在同一程序集中访问
        /// <summary>
        /// 更新检视视图为当前选中节点的检视面板
        /// </summary>
        /// <param name="nodeView"></param>
        internal void UpdateSelection(NodeView nodeView) {
            Clear(); //清空检视视图元素下的所有元素

            /*Tip：DestroyImmediate应当只在编辑模式下使用，因为使用普通的Destroy方法时，对象会在当前帧结束后（其实仍在同一帧执行）销毁，这是 Unity 的延迟销毁机制，而
            在编辑模式下，延迟销毁机制不会被调用（因为游戏逻辑和场景更新的行为与运行时不同）。而DestroyImmediate方法它能够立即销毁一个对象，
            并且不会等到当前帧结束再执行，但在运行时，DestroyImmediate 的即时销毁可能导致数据不一致或引发运行时错误，所以运行时建议使用
            Destroy方法来进行销毁操作。
            DestroyImmediate的第二个参数是布尔值，决定是否允许销毁素材，即资产文件，此处必须谨慎，因为它可以永久性销毁。还有不要在遍历数组
            时进行销毁操作，可能造成严重问题，这是个一般原则。
            */
            //UnityEngine.Object.DestroyImmediate(editor); 
            //这里应该也可以直接赋值，而不需要手动调用销毁操作，因为有GC，但是大概为了安全起见。
            //Ques:经测试，在这里用局部变量，也会始终执行if内的部分，应该跟内存释放的机制有关。
            Editor editor = Editor.CreateEditor(nodeView.node); //为数据节点创建一个编辑器，就相当于CustomEditor特性
            IMGUIContainer container = new IMGUIContainer(() => { //存放数据节点的检视面板
                // Debug.Log("dd");
                if (editor && editor.target) {
                    // Debug.Log("editor");
                    editor.OnInspectorGUI(); //创建检视面板（似乎是默认进行了数据绑定，这是IMGUI的数据绑定，注意与UI Toolkit的数据绑定区分，后者更加复杂，也更加丰富）
                }
            }); //IMGUIContainer就是一个单独的元素，用来存放IMGUI创建的元素，并不会在层级结构中显示出来，当然其实也没必要，因为IMGUI本来就是纯代码操作
            Add(container);
        }
    }
}