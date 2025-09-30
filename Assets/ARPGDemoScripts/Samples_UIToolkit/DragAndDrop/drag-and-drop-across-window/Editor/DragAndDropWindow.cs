 using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    namespace Samples.Editor.General
    {
        public partial class DragAndDropWindow : EditorWindow
        {
            // This is the visual tree that contains the UI structure of the window.
            [SerializeField]
            VisualTreeAsset uxmlAsset;

            // This manipulator contains all of the event logic for this window.
            DragAndDropManipulator manipulator;

            // This is the minimum size of both windows.
            readonly static Vector2 windowMinSize = new(300, 180);

            // These are the starting positions of the windows.
            readonly static Vector2 windowAPosition = new(50, 50);
            readonly static Vector2 windowBPosition = new(450, 100);

            // These are the titles of the windows.
            const string windowATitle = "Drag and Drop A";
            const string windowBTitle = "Drag and Drop B";

            // This method opens two DragAndDropWindows when a user selects the specified menu item.
            [MenuItem("Window/UI Toolkit/示例/Drag And Drop (Editor)")]
            public static void OpenDragAndDropWindows()
            {
                // Create the windows.
                var windowA = CreateInstance<DragAndDropWindow>();
                var windowB = CreateInstance<DragAndDropWindow>();

                // Define the attributes of the windows and display them.
                windowA.minSize = windowMinSize;
                windowB.minSize = windowMinSize;
                windowA.Show();
                windowB.Show();
                windowA.titleContent = new(windowATitle);
                windowB.titleContent = new(windowBTitle);
                windowA.position = new(windowAPosition, windowMinSize);
                windowB.position = new(windowBPosition, windowMinSize);
            }

            void OnEnable()
            {
                if (uxmlAsset != null)
                {
                    uxmlAsset.CloneTree(rootVisualElement);
                }

                // Instantiate manipulator.
                manipulator = new(rootVisualElement); //这是简写的new创建实例
            }

            void OnDisable()
            {
                // The RemoveManipulator() method calls the Manipulator's UnregisterCallbacksFromTarget() method.
                //这里主要是为了注销事件，避免遗留而浪费内存，不过更重要的是避免逻辑重复，从而导致错误。
                manipulator.target.RemoveManipulator(manipulator);
            }
        }
    }