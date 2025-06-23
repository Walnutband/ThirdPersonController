

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MyPlugins.GoodUI
{
    public static class ToolOptions
    {
        // [MenuItem("GoodUI/适应子对象高度")]
        static void FitChildrenHeight()
        {
            GameObject go = Selection.activeGameObject;
            RectTransform rect = Selection.activeGameObject.GetComponent<RectTransform>();
            if (rect == null) return;
            float totalHeight = 0f;
            int childCount = rect.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = rect.GetChild(i) as RectTransform;
                if (child != null) totalHeight += child.rect.height;
            }
            Vector2 prePivot = rect.pivot;
            rect.pivot = new Vector2(0f, 1f);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
            rect.pivot = prePivot;
        }

        [MenuItem("GameObject/GoodUI/选中同名子对象", false, -1000)]
        static void RenameLowerGO()
        {
            // Selection.objects = new Object[] { Selection.activeGameObject.transform.GetChild(0) };
            string name = Selection.activeGameObject.name; //因为不能传参，所以就将选中对象的名字作为信息了。
            List<Object> objects = new List<Object>();
            Main(Selection.activeGameObject.transform); //填充objects
            Selection.objects = objects.ToArray(); //将列表转换为数组，使用列表主要是为了动态性。
            void Main(Transform transform)
            {
                int count = transform.childCount;
                if (count <= 0) return;
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        Transform child = transform.GetChild(i);
                        //由于递归层之间逻辑无关，所以放在前放在后都无所谓，只要放了就行，不过区别也是有的，就是添加到容器中的顺序不同。放在前就是从子对象开始添加，放在后就是从父对象开始添加
                        // Main(child); 
                        if (child.name == name)
                        {/*Tip:卧槽，Selection.objects的功能比我想象的强大得多了，它会根据数组元素的实际类型来选中，从而在检视面板中显示，就可以进行统一编辑了，
                        也就是说可以选中GameObject，也可以是挂载的各种组件*/
                            objects.Add(child.gameObject);
                        }
                        Main(child);
                    }
                }
            }
        }
    }

    public static class UIObjectsCreateMenu
    {
        [MenuItem("GameObject/UI/GoodUI/UIBasicView", false, 1000)]
        private static void CreateUIBasicView()
        {
            Canvas canvas = Selection.activeGameObject.GetComponent<Canvas>();
            // if (canvas != null)
            // {
            //     Create(canvas.transform);
            // }
            // else
            // {
            //     canvas = GameObject.FindObjectOfType<Canvas>();
            //     Create(canvas.transform);
            // }
            if (canvas == null) canvas = GameObject.FindObjectOfType<Canvas>();
            Create(canvas.transform);


        }

        private static void Create(Transform canvasTrans)
        {//注意这里的创建就揭示了很多默认条件、默认情况
            // GameObject.Instantiate(new GameObject(), canvas.transform);
            GameObject root = new GameObject("UIBasicView");
            root.AddComponent<RectTransform>();
            root.AddComponent<UIControlData>();
            root.transform.SetParent(canvasTrans);
            (root.transform as RectTransform).SetAnchor(AnchorPresets.StretchAll);
            GameObject uiMask = new GameObject("UIMask");
            uiMask.AddComponent<RectTransform>();
            //TODO：Image用于接收射线检测，传递给SimpleButton组件调用点击方法，但是Image其实也可以被更加特化的类替换。
            uiMask.AddComponent<Image>().color = new Color32(0, 0, 0, 128); //Color32可以直接指定rgba值，注意参数值是0~255，而不是Color的0~1。这里默认半透明纯黑色
            uiMask.AddComponent<SimpleButton>();
            uiMask.transform.SetParent(root.transform);
            (uiMask.transform as RectTransform).SetAnchor(AnchorPresets.StretchAll);
            GameObject uiRoot = new GameObject("UIContent");
            uiRoot.AddComponent<RectTransform>();
            uiRoot.transform.SetParent(root.transform);
            (uiRoot.transform as RectTransform).SetAnchor(AnchorPresets.MiddleCenter);
        }

    }
}